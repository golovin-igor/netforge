using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Cisco
{
    /// <summary>
    /// Utility class for handling Cisco interface name aliases and expansions
    /// </summary>
    public static class CiscoInterfaceAliasHandler
    {
        private static readonly Dictionary<string, string> InterfaceAliasMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // GigabitEthernet aliases
            { "gi", "GigabitEthernet" },
            { "gig", "GigabitEthernet" },
            { "gigabit", "GigabitEthernet" },
            
            // FastEthernet aliases
            { "fa", "FastEthernet" },
            { "fast", "FastEthernet" },
            
            // TenGigabitEthernet aliases
            { "te", "TenGigabitEthernet" },
            { "ten", "TenGigabitEthernet" },
            
            // Ethernet aliases
            { "eth", "Ethernet" },
            
            // Loopback aliases
            { "lo", "Loopback" },
            { "loop", "Loopback" },
            
            // Port-channel aliases
            { "po", "Port-channel" },
            { "port", "Port-channel" },
            
            // VLAN aliases
            { "vl", "Vlan" },
            
            // Serial aliases
            { "se", "Serial" },
            
            // Tunnel aliases
            { "tu", "Tunnel" }
        };

        /// <summary>
        /// Expands interface aliases to full interface names
        /// </summary>
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return interfaceName;

            var normalized = interfaceName.ToLowerInvariant().Trim();
            
            foreach (var kvp in InterfaceAliasMap)
            {
                var alias = kvp.Key;
                var fullName = kvp.Value;
                
                if (ShouldExpandAlias(normalized, alias, fullName))
                {
                    return interfaceName.Replace(alias, fullName, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Handle special case for vlan/Vlan capitalization
            if (normalized.StartsWith("vlan", StringComparison.OrdinalIgnoreCase) && 
                !char.IsUpper(interfaceName[0]))
            {
                return interfaceName.Replace("vlan", "Vlan", StringComparison.OrdinalIgnoreCase);
            }

            // Handle special case for tunnel/Tunnel capitalization
            if (normalized.StartsWith("tunnel", StringComparison.OrdinalIgnoreCase) && 
                !char.IsUpper(interfaceName[0]))
            {
                return interfaceName.Replace("tunnel", "Tunnel", StringComparison.OrdinalIgnoreCase);
            }

            return interfaceName;
        }

        private static bool ShouldExpandAlias(string normalized, string alias, string fullName)
        {
            if (!normalized.StartsWith(alias, StringComparison.OrdinalIgnoreCase))
                return false;

            // Don't expand if it already starts with a longer form
            var lowerFullName = fullName.ToLowerInvariant();
            
            // Special handling for hierarchical aliases (gi -> gig -> gigabit -> gigabitethernet)
            return alias switch
            {
                "gi" => !normalized.StartsWith("gig", StringComparison.OrdinalIgnoreCase),
                "gig" => !normalized.StartsWith("gigabit", StringComparison.OrdinalIgnoreCase),
                "gigabit" => !normalized.StartsWith("gigabitethernet", StringComparison.OrdinalIgnoreCase),
                "fa" => !normalized.StartsWith("fast", StringComparison.OrdinalIgnoreCase),
                "fast" => !normalized.StartsWith("fastethernet", StringComparison.OrdinalIgnoreCase),
                "te" => !normalized.StartsWith("ten", StringComparison.OrdinalIgnoreCase),
                "ten" => !normalized.StartsWith("tengigabitethernet", StringComparison.OrdinalIgnoreCase),
                "eth" => !normalized.StartsWith("ethernet", StringComparison.OrdinalIgnoreCase),
                "lo" => !normalized.StartsWith("loop", StringComparison.OrdinalIgnoreCase),
                "loop" => !normalized.StartsWith("loopback", StringComparison.OrdinalIgnoreCase),
                "po" => !normalized.StartsWith("port", StringComparison.OrdinalIgnoreCase),
                "port" => !normalized.StartsWith("port-channel", StringComparison.OrdinalIgnoreCase),
                "vl" => !normalized.StartsWith("vlan", StringComparison.OrdinalIgnoreCase),
                "se" => !normalized.StartsWith("serial", StringComparison.OrdinalIgnoreCase),
                "tu" => !normalized.StartsWith("tunnel", StringComparison.OrdinalIgnoreCase),
                _ => !normalized.StartsWith(lowerFullName, StringComparison.OrdinalIgnoreCase)
            };
        }

        /// <summary>
        /// Compresses interface names to shorter aliases
        /// </summary>
        public static string CompressInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            // Convert full names to shortest common abbreviations
            // Order matters: longer strings first to avoid partial matches
            return interfaceName
                .Replace("TenGigabitEthernet", "te", StringComparison.OrdinalIgnoreCase)
                .Replace("GigabitEthernet", "gi", StringComparison.OrdinalIgnoreCase)
                .Replace("FastEthernet", "fa", StringComparison.OrdinalIgnoreCase)
                .Replace("Port-channel", "po", StringComparison.OrdinalIgnoreCase)
                .Replace("Ethernet", "eth", StringComparison.OrdinalIgnoreCase)
                .Replace("Loopback", "lo", StringComparison.OrdinalIgnoreCase)
                .Replace("Serial", "se", StringComparison.OrdinalIgnoreCase)
                .Replace("Tunnel", "tu", StringComparison.OrdinalIgnoreCase)
                .Replace("Vlan", "vl", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates if an interface name is valid
        /// </summary>
        public static bool IsValidInterfaceName(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return false;

            var normalized = interfaceName.ToLower().Trim();
            
            // Check for valid interface patterns with proper numbering
            // Limit to reasonable depth (max 3 levels: x/y/z)
            var patterns = new[]
            {
                @"^gi\d+(/\d+)?(/\d+)?(\.\d+)?$",                    // gi0/1, gi1/0/1 (max 3 levels)
                @"^gig\d+(/\d+)?(/\d+)?(\.\d+)?$",                   // gig0/1
                @"^gigabit\d+(/\d+)?(/\d+)?(\.\d+)?$",               // gigabit0/1
                @"^gigabitethernet\d+(/\d+)?(/\d+)?(\.\d+)?$",       // gigabitethernet0/1
                @"^fa\d+(/\d+)?(/\d+)?(\.\d+)?$",                    // fa0/1
                @"^fast\d+(/\d+)?(/\d+)?(\.\d+)?$",                  // fast0/1
                @"^fastethernet\d+(/\d+)?(/\d+)?(\.\d+)?$",          // fastethernet0/1
                @"^te\d+(/\d+)?(/\d+)?(\.\d+)?$",                    // te0/1
                @"^ten\d+(/\d+)?(/\d+)?(\.\d+)?$",                   // ten0/1
                @"^tengigabitethernet\d+(/\d+)?(/\d+)?(\.\d+)?$",    // tengigabitethernet0/1
                @"^eth\d+(/\d+)?(/\d+)?(\.\d+)?$",                   // eth0/1
                @"^ethernet\d+(/\d+)?(/\d+)?(\.\d+)?$",              // ethernet0/1
                @"^lo\d+$",                                          // lo0
                @"^loop\d+$",                                        // loop0
                @"^loopback\d+$",                                    // loopback0
                @"^po\d+$",                                          // po1
                @"^port\d+$",                                        // port1
                @"^port-channel\d+$",                                // port-channel1
                @"^se\d+(/\d+)?(/\d+)?(\.\d+)?$",                    // se0/0/0
                @"^serial\d+(/\d+)?(/\d+)?(\.\d+)?$",                // serial0/0/0
                @"^tu\d+$",                                          // tu0
                @"^tunnel\d+$",                                      // tunnel0
                @"^vl\d+$",                                          // vl100
                @"^vlan\d+$"                                         // vlan100
            };
            
            return patterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(normalized, pattern));
        }

        /// <summary>
        /// Checks if two interface names are equivalent (considering aliases)
        /// </summary>
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
                return true;
            
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            var expanded1 = ExpandInterfaceAlias(name1);
            var expanded2 = ExpandInterfaceAlias(name2);
            
            return string.Equals(expanded1, expanded2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets all possible aliases for an interface name
        /// </summary>
        public static List<string> GetInterfaceAliases(string interfaceName)
        {
            var aliases = new List<string>();
            
            if (string.IsNullOrEmpty(interfaceName))
                return aliases;

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();
            
            // Always include the original and expanded forms
            aliases.Add(interfaceName);
            aliases.Add(expanded);
            
            // Generate all possible aliases based on interface type
            if (normalized.StartsWith("gigabitethernet"))
            {
                var number = expanded.Substring("GigabitEthernet".Length);
                aliases.Add($"gi{number}");
                aliases.Add($"gig{number}");
                aliases.Add($"gigabit{number}");
                aliases.Add($"gigabiteth{number}");
                aliases.Add($"GigabitEthernet{number}");
            }
            else if (normalized.StartsWith("fastethernet"))
            {
                var number = expanded.Substring("FastEthernet".Length);
                aliases.Add($"fa{number}");
                aliases.Add($"fast{number}");
                aliases.Add($"FastEthernet{number}");
            }
            else if (normalized.StartsWith("tengigabitethernet"))
            {
                var number = expanded.Substring("TenGigabitEthernet".Length);
                aliases.Add($"te{number}");
                aliases.Add($"ten{number}");
                aliases.Add($"TenGigabitEthernet{number}");
            }
            else if (normalized.StartsWith("ethernet"))
            {
                var number = expanded.Substring("Ethernet".Length);
                aliases.Add($"eth{number}");
                aliases.Add($"Ethernet{number}");
            }
            else if (normalized.StartsWith("loopback"))
            {
                var number = expanded.Substring("Loopback".Length);
                aliases.Add($"lo{number}");
                aliases.Add($"loop{number}");
                aliases.Add($"Loopback{number}");
            }
            else if (normalized.StartsWith("port-channel"))
            {
                var number = expanded.Substring("Port-channel".Length);
                aliases.Add($"po{number}");
                aliases.Add($"port{number}");
                aliases.Add($"Port-channel{number}");
            }
            
            return aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Gets the interface type from the name
        /// </summary>
        public static string GetInterfaceType(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "Unknown";

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            if (normalized.StartsWith("gigabitethernet"))
                return "GigabitEthernet";
            if (normalized.StartsWith("fastethernet"))
                return "FastEthernet";
            if (normalized.StartsWith("tengigabitethernet"))
                return "TenGigabitEthernet";
            if (normalized.StartsWith("ethernet"))
                return "Ethernet";
            if (normalized.StartsWith("loopback"))
                return "Loopback";
            if (normalized.StartsWith("serial"))
                return "Serial";
            if (normalized.StartsWith("port-channel"))
                return "Port-channel";
            if (normalized.StartsWith("tunnel"))
                return "Tunnel";
            if (normalized.StartsWith("vlan"))
                return "VLAN";

            return "Unknown";
        }

        /// <summary>
        /// Gets the interface number from the name
        /// </summary>
        public static string GetInterfaceNumber(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var expanded = ExpandInterfaceAlias(interfaceName);
            
            // Extract number part - handle special cases like Port-channel
            if (expanded.StartsWith("Port-channel", StringComparison.OrdinalIgnoreCase))
            {
                var match = System.Text.RegularExpressions.Regex.Match(expanded, @"Port-channel(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            
            // For other interfaces, extract everything after the alphabetic prefix
            var generalMatch = System.Text.RegularExpressions.Regex.Match(expanded, @"^[a-zA-Z-]+(.*)");
            return generalMatch.Success ? generalMatch.Groups[1].Value : "";
        }

        /// <summary>
        /// Gets the canonical interface name (expanded and normalized)
        /// </summary>
        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            return ExpandInterfaceAlias(interfaceName);
        }
    }

    /// <summary>
    /// Utility class for processing Cisco command shortcuts and history
    /// </summary>
    public static class HistoryCommandProcessor
    {
        /// <summary>
        /// Processes command shortcuts and abbreviations
        /// </summary>
        public static string ProcessShortcuts(string command, NetworkDevice device)
        {
            if (string.IsNullOrEmpty(command))
                return command;

            // For now, return the original command
            // TODO: Implement Cisco-specific command shortcuts and abbreviations
            return command;
        }

        /// <summary>
        /// Expands command abbreviations to full commands
        /// </summary>
        public static string ExpandCommandAbbreviation(string command)
        {
            if (string.IsNullOrEmpty(command))
                return command;

            // Common Cisco command abbreviations
            var abbreviations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "sh", "show" },
                { "conf", "configure" },
                { "config", "configure" },
                { "int", "interface" },
                { "ip", "ip" },
                { "no", "no" },
                { "exit", "exit" },
                { "end", "end" }
            };

            var parts = command.Split(' ');
            if (parts.Length > 0 && abbreviations.ContainsKey(parts[0]))
            {
                parts[0] = abbreviations[parts[0]];
                return string.Join(" ", parts);
            }

            return command;
        }
    }
} 
