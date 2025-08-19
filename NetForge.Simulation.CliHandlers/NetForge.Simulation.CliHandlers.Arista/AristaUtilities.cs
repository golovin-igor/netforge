using NetForge.Simulation.Common;
using System.Text.RegularExpressions;

namespace NetForge.Simulation.CliHandlers.Arista
{
    /// <summary>
    /// Utility class for handling Arista interface name aliases and expansions
    /// </summary>
    public static class AristaInterfaceAliasHandler
    {
        /// <summary>
        /// Expands interface aliases to full interface names
        /// </summary>
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var normalized = interfaceName.ToLower().Trim();
            
            // Extract sub-interface part (e.g., ".200" from "ma1.200")
            var subInterfacePart = "";
            var dotIndex = normalized.IndexOf('.');
            if (dotIndex != -1)
            {
                subInterfacePart = normalized.Substring(dotIndex);
                normalized = normalized.Substring(0, dotIndex);
            }
            
            // Ethernet patterns
            if (normalized.StartsWith("et") && !normalized.StartsWith("eth"))
            {
                var number = normalized.Substring(2);
                return $"Ethernet{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("eth") && !normalized.StartsWith("ethernet"))
            {
                var number = normalized.Substring(3);
                return $"Ethernet{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("ethernet"))
            {
                var number = normalized.Substring(8);
                return $"Ethernet{number}{subInterfacePart}";
            }
            
            // Management patterns
            if (normalized.StartsWith("ma") && !normalized.StartsWith("man"))
            {
                var number = normalized.Substring(2);
                return $"Management{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("man") && !normalized.StartsWith("management"))
            {
                var number = normalized.Substring(3);
                return $"Management{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("mgmt") && !normalized.StartsWith("management"))
            {
                var number = normalized.Substring(4);
                return $"Management{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("management"))
            {
                var number = normalized.Substring(10);
                return $"Management{number}{subInterfacePart}";
            }
            
            // Loopback patterns
            if (normalized.StartsWith("lo") && !normalized.StartsWith("loop"))
            {
                var number = normalized.Substring(2);
                return $"Loopback{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("loop") && !normalized.StartsWith("loopback"))
            {
                var number = normalized.Substring(4);
                return $"Loopback{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("loopback"))
            {
                var number = normalized.Substring(8);
                return $"Loopback{number}{subInterfacePart}";
            }
            
            // Port-Channel patterns
            if (normalized.StartsWith("po") && !normalized.StartsWith("port"))
            {
                var number = normalized.Substring(2);
                return $"Port-Channel{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("port") && !normalized.StartsWith("port-channel"))
            {
                var number = normalized.Substring(4);
                return $"Port-Channel{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("port-channel"))
            {
                var number = normalized.Substring(12);
                return $"Port-Channel{number}{subInterfacePart}";
            }
            
            // Tunnel patterns
            if (normalized.StartsWith("tu") && !normalized.StartsWith("tunnel"))
            {
                var number = normalized.Substring(2);
                return $"Tunnel{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("tunnel"))
            {
                var number = normalized.Substring(6);
                return $"Tunnel{number}{subInterfacePart}";
            }
            
            // VLAN patterns
            if (normalized.StartsWith("vl") && !normalized.StartsWith("vlan"))
            {
                var number = normalized.Substring(2);
                return $"Vlan{number}{subInterfacePart}";
            }
            if (normalized.StartsWith("vlan"))
            {
                var number = normalized.Substring(4);
                return $"Vlan{number}{subInterfacePart}";
            }

            // Return original name if no alias expansion needed
            return interfaceName;
        }

        /// <summary>
        /// Compresses interface names to shorter aliases
        /// </summary>
        public static string CompressInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            // Extract sub-interface part (e.g., ".200" from "Management1.200")
            var subInterfacePart = "";
            var dotIndex = interfaceName.IndexOf('.');
            var baseInterface = interfaceName;
            if (dotIndex != -1)
            {
                subInterfacePart = interfaceName.Substring(dotIndex);
                baseInterface = interfaceName.Substring(0, dotIndex);
            }

            // Convert full names to shortest common abbreviations (lowercase)
            var result = baseInterface;
            if (result.StartsWith("Ethernet", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(8);
                result = $"et{number}";
            }
            else if (result.StartsWith("Management", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(10);
                result = $"ma{number}";
            }
            else if (result.StartsWith("Port-Channel", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(12);
                result = $"po{number}";
            }
            else if (result.StartsWith("Loopback", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(8);
                result = $"lo{number}";
            }
            else if (result.StartsWith("Tunnel", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(6);
                result = $"tu{number}";
            }
            else if (result.StartsWith("Vlan", StringComparison.OrdinalIgnoreCase))
            {
                var number = result.Substring(4);
                result = $"vl{number}";
            }

            return result + subInterfacePart;
        }

        /// <summary>
        /// Validates if an interface name is valid
        /// </summary>
        public static bool IsValidInterfaceName(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return false;

            var normalized = interfaceName.ToLower().Trim();
            
            // Special case: "Port1" (without hyphen) is invalid, but "Port-Channel1" is valid
            if (interfaceName.StartsWith("Port") && char.IsUpper(interfaceName[0]) && !interfaceName.StartsWith("Port-Channel"))
            {
                return false; // "Port1" is invalid, but "Port-Channel1" is valid
            }
            
            // Check for valid Arista interface patterns
            var patterns = new[]
            {
                @"^et\d+(/\d+)?(\.\d+)?$",                          // Et1, Et1/1, Et1.100
                @"^eth\d+(/\d+)?(\.\d+)?$",                         // Eth1, Eth1/1, Eth1.100
                @"^ethernet\d+(/\d+)?(\.\d+)?$",                    // Ethernet1, Ethernet1/1, Ethernet1.100
                @"^ma\d+(\.\d+)?$",                                 // Ma1, Ma1.200
                @"^man\d+(\.\d+)?$",                                // Man1, Man1.200
                @"^mgmt\d+(\.\d+)?$",                               // Mgmt1, Mgmt1.200
                @"^management\d+(\.\d+)?$",                         // Management1, Management1.200
                @"^lo\d+$",                                         // Lo0
                @"^loop\d+$",                                       // Loop0
                @"^loopback\d+$",                                   // Loopback0
                @"^po\d+$",                                         // Po1
                @"^port\d+$",                                       // port1 (lowercase, alias for Port-Channel)
                @"^port-channel\d+$",                               // Port-Channel1
                @"^tu\d+$",                                         // Tu1
                @"^tunnel\d+$",                                     // Tunnel1
                @"^vl\d+$",                                         // Vl100
                @"^vlan\d+$"                                        // Vlan100
            };
            
            return patterns.Any(pattern => Regex.IsMatch(normalized, pattern));
        }

        /// <summary>
        /// Checks if two interface names are equivalent (considering aliases)
        /// </summary>
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
                return false; // Both empty should be false, not true
            
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
            
            // Generate all possible aliases based on interface type
            if (normalized.StartsWith("ethernet"))
            {
                var number = expanded.Substring("Ethernet".Length);
                aliases.Add($"Ethernet{number}");
                aliases.Add($"et{number}");
                aliases.Add($"eth{number}");
                aliases.Add($"ethernet{number}");
            }
            else if (normalized.StartsWith("management"))
            {
                var number = expanded.Substring("Management".Length);
                aliases.Add($"Management{number}");
                aliases.Add($"ma{number}");
                aliases.Add($"mgmt{number}");
                aliases.Add($"management{number}");
            }
            else if (normalized.StartsWith("loopback"))
            {
                var number = expanded.Substring("Loopback".Length);
                aliases.Add($"Loopback{number}");
                aliases.Add($"lo{number}");
                aliases.Add($"loop{number}");
                aliases.Add($"loopback{number}");
            }
            else if (normalized.StartsWith("port-channel"))
            {
                var number = expanded.Substring("Port-Channel".Length);
                aliases.Add($"Port-Channel{number}");
                aliases.Add($"po{number}");
                aliases.Add($"port{number}");
                aliases.Add($"port-channel{number}");
            }
            else if (normalized.StartsWith("tunnel"))
            {
                var number = expanded.Substring("Tunnel".Length);
                aliases.Add($"Tunnel{number}");
                aliases.Add($"tu{number}");
                aliases.Add($"tunnel{number}");
            }
            else if (normalized.StartsWith("vlan"))
            {
                var number = expanded.Substring("Vlan".Length);
                aliases.Add($"Vlan{number}");
                aliases.Add($"vl{number}");
                aliases.Add($"vlan{number}");
            }
            else
            {
                // For invalid interfaces, return the original name
                aliases.Add(interfaceName);
            }
            
            // Remove duplicates while preserving original case - don't use case-insensitive comparison
            var result = aliases.Distinct().ToList();
            return result;
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

            if (normalized.StartsWith("ethernet"))
                return "Ethernet";
            if (normalized.StartsWith("management"))
                return "Management";
            if (normalized.StartsWith("loopback"))
                return "Loopback";
            if (normalized.StartsWith("port-channel"))
                return "Port-Channel";
            if (normalized.StartsWith("tunnel"))
                return "Tunnel";
            if (normalized.StartsWith("vlan"))
                return "Vlan";

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
            
            // Extract number part - handle special cases like Port-Channel
            if (expanded.StartsWith("Port-Channel", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"Port-Channel(.*)");
                return match.Success ? match.Groups[1].Value : "";
            }
            
            // For other interfaces, extract everything after the alphabetic prefix
            var generalMatch = Regex.Match(expanded, @"^[a-zA-Z-]+(.*)");
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
} 
