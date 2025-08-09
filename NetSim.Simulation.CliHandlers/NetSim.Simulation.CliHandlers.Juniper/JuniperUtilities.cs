using NetSim.Simulation.Common;
using System.Text.RegularExpressions;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Utility class for handling Juniper interface name aliases and expansions
    /// </summary>
    public static class JuniperInterfaceAliasHandler
    {
        /// <summary>
        /// Expands interface aliases to full interface names
        /// </summary>
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var input = interfaceName.Trim();
            var normalized = input.ToLower();

            // If already in canonical form, return as-is
            if (normalized.Contains("gigabitethernet") || normalized.Contains("tengigabitethernet") ||
                normalized.Contains("twentyfivegigabitethernet") || normalized.Contains("hundredgigabitethernet") ||
                normalized.Contains("fortygigabitethernet") || normalized.Contains("ethernetinterface") ||
                normalized.Contains("management") || normalized.Contains("loopback") ||
                normalized.Contains("aggregatedethernet") || normalized.Contains("redundantethernet") ||
                normalized.Contains("stunnel") || normalized.Contains("gre-") || normalized.Contains("irb") ||
                normalized.Contains("vlan"))
                return interfaceName;

            // Map short Juniper aliases to canonical names (for test expectations)

            if (normalized.StartsWith("ge-"))
                return $"GigabitEthernet-{input.Substring(3)}";
            if (normalized.StartsWith("xe-"))
                return $"TenGigabitEthernet-{input.Substring(3)}";
            if (normalized.StartsWith("et-"))
                return $"EthernetInterface-{input.Substring(3)}";
            if (normalized.StartsWith("ae"))
                return $"AggregatedEthernet{input.Substring(2)}";
            if (normalized.StartsWith("irb"))
                return $"IRB{input.Substring(3)}".ToUpper();
            if (normalized.StartsWith("lo"))
                return $"Loopback{input.Substring(2)}";
            if (normalized.StartsWith("me"))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("fxp"))
                return $"Management{input.Substring(3)}";
            if (normalized.StartsWith("em"))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("mgmt"))
                return $"Management{input.Substring(4)}";
            if (normalized.StartsWith("vlan."))
                return $"VLAN.{input.Substring(5)}".ToUpper();
            if (normalized.StartsWith("vlan"))
                return $"VLAN{input.Substring(4)}".ToUpper();
            if (normalized.StartsWith("reth"))
                return $"RedundantEthernet{input.Substring(4)}";
            if (normalized.StartsWith("irb"))
                return $"IRB{input.Substring(3)}".ToUpper();
            if (normalized.StartsWith("vlan."))
                return $"VLAN.{input.Substring(5)}".ToUpper();
            if (normalized.StartsWith("vlan"))
                return $"VLAN{input.Substring(4)}".ToUpper();
            if (normalized.StartsWith("100ge-"))
                return $"HundredGigabitEthernet-{input.Substring(7)}";
            if (normalized.StartsWith("25ge-"))
                return $"TwentyFiveGigabitEthernet-{input.Substring(6)}";
            if (normalized.StartsWith("40ge-"))
                return $"FortyGigabitEthernet-{input.Substring(6)}";

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

            var input = interfaceName.Trim();
            var normalized = input.ToLower();
            
            // Map canonical names to short aliases
            // Use GetInterfaceNumber for all high-speed and standard Ethernet types
            string number = GetInterfaceNumber(interfaceName);
            if ((normalized.StartsWith("twentyfivegigabitethernet-") || normalized.StartsWith("25ge-")) && !string.IsNullOrEmpty(number))
                return $"25ge-{number}";
            if ((normalized.StartsWith("hundredgigabitethernet-") || normalized.StartsWith("100ge-")) && !string.IsNullOrEmpty(number))
                return $"100ge-{number}";
            if ((normalized.StartsWith("fortygigabitethernet-") || normalized.StartsWith("40ge-")) && !string.IsNullOrEmpty(number))
                return $"40ge-{number}";
            if ((normalized.StartsWith("gigabitethernet-") || normalized.StartsWith("ge-")) && !string.IsNullOrEmpty(number))
                return $"ge-{number}";
            if ((normalized.StartsWith("tengigabitethernet-") || normalized.StartsWith("xe-")) && !string.IsNullOrEmpty(number))
                return $"xe-{number}";
            if ((normalized.StartsWith("ethernetinterface-") || normalized.StartsWith("et-")) && !string.IsNullOrEmpty(number))
                return $"et-{number}";
            if (normalized.StartsWith("aggregatedethernet"))
                return $"ae{input.Substring(18)}";
            if (normalized.StartsWith("agg"))
                return $"ae{input.Substring(3)}";
            if (normalized.StartsWith("mgmt"))
                return $"me{input.Substring(4)}";
            if (normalized.StartsWith("irb"))
                return $"irb{input.Substring(3)}";
            if (normalized.StartsWith("loopback"))
                return $"lo{input.Substring(8)}";
            if (normalized.StartsWith("management"))
                return $"me{input.Substring(10)}";
            if (normalized.StartsWith("vlan."))
                return $"vlan{input.Substring(5)}".Replace(".", "");
            if (normalized.StartsWith("vlan"))
                return $"vlan{input.Substring(4)}";
            if (normalized.StartsWith("redundantethernet"))
                return $"reth{input.Substring(17)}";
            if (normalized.StartsWith("gre-"))
                return $"gr-{input.Substring(4)}";
            if (normalized.StartsWith("stunnel"))
                return $"st{input.Substring(7)}";
            // If already a short alias, return as-is
            if (normalized.StartsWith("ge-") || normalized.StartsWith("xe-") || normalized.StartsWith("et-") || normalized.StartsWith("ae") || normalized.StartsWith("lo") || normalized.StartsWith("me") || normalized.StartsWith("fxp") || normalized.StartsWith("em") || normalized.StartsWith("vlan") || normalized.StartsWith("reth") || normalized.StartsWith("gr-") || normalized.StartsWith("st") || normalized.StartsWith("100ge-") || normalized.StartsWith("25ge-") || normalized.StartsWith("40ge-"))
                return interfaceName;
            return interfaceName;
        }

        /// <summary>
        /// Checks if the interface name is valid
        /// </summary>
        public static bool IsValidInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return false;

            var normalized = interfaceName.ToLower().Trim();
            
            // Check for valid Juniper interface patterns
            var patterns = new[]
            {
                @"^ge-\d+/\d+/\d+(\.\d+)?$",                        // ge-0/0/0
                @"^xe-\d+/\d+/\d+(\.\d+)?$",                        // xe-0/0/0
                @"^et-\d+/\d+/\d+(\.\d+)?$",                        // et-0/0/0
                @"^25ge-\d+/\d+/\d+(\.\d+)?$",                      // 25ge-0/0/0
                @"^100ge-\d+/\d+/\d+(\.\d+)?$",                     // 100ge-0/0/0
                @"^40ge-\d+/\d+/\d+(\.\d+)?$",                      // 40ge-0/0/0
                @"^em\d+(\.\d+)?$",                                 // em0
                @"^lo\d+(\.\d+)?$",                                 // lo0
                @"^ae\d+(\.\d+)?$",                                 // ae0
                @"^irb(\.\d+)?$",                                   // irb.100
                @"^vlan(\.\d+)?$",                                  // vlan.100
                @"^st\d+(\.\d+)?$",                                 // st0 (secure tunnel)
                @"^gr-\d+/\d+/\d+(\.\d+)?$",                        // gr-0/0/0 (GRE tunnel)
                @"^ip-\d+/\d+/\d+(\.\d+)?$",                        // ip-0/0/0 (IP-over-IP tunnel)
                @"^lt-\d+/\d+/\d+(\.\d+)?$",                        // lt-0/0/0 (logical tunnel)
                @"^mt-\d+/\d+/\d+(\.\d+)?$",                        // mt-0/0/0 (multilink trunk)
                @"^pd-\d+/\d+/\d+(\.\d+)?$",                        // pd-0/0/0 (packet-over-SONET/SDH)
                @"^pe-\d+/\d+/\d+(\.\d+)?$",                        // pe-0/0/0 (packet-over-Ethernet)
                @"^pp\d+(\.\d+)?$",                                 // pp0 (point-to-point)
                @"^rbeb\d+(\.\d+)?$",                               // rbeb0 (routing bridge)
                @"^tap\d+(\.\d+)?$",                                // tap0 (TAP interface)
                @"^vtep(\.\d+)?$",                                  // vtep (VXLAN tunnel endpoint)
                @"^fxp\d+(\.\d+)?$",                                // fxp0 (management)
                @"^me\d+(\.\d+)?$",                                 // me0 (management)
                @"^reth\d+(\.\d+)?$"                                // reth0 (redundant ethernet)
            };
            
            return patterns.Any(pattern => Regex.IsMatch(normalized, pattern));
        }

        /// <summary>
        /// Checks if two interface names are equivalent (considering aliases)
        /// </summary>
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
                return false;
            
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            var aliases1 = GetInterfaceAliases(name1);
            var aliases2 = GetInterfaceAliases(name2);
            return aliases1.Any(a1 => aliases2.Any(a2 => string.Equals(a1, a2, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Gets all possible aliases for an interface name
        /// </summary>
        public static List<string> GetInterfaceAliases(string interfaceName)
        {
            var aliases = new List<string>();
            
            if (string.IsNullOrEmpty(interfaceName))
            {
                aliases.Add("");
                return aliases;
            }

            var canonical = GetCanonicalInterfaceName(interfaceName);
            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();
            
            // Always include the original and canonical forms
            aliases.Add(interfaceName);
            aliases.Add(canonical);
            
            // Generate all possible aliases based on interface type
            string number = GetInterfaceNumber(expanded);
            // 25GE
            if (normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-"))
            {
                aliases.Add($"25ge-{number}");
                aliases.Add($"25g{number}");
                aliases.Add($"TwentyFiveGigabitEthernet-{number}");
            }
            // 100GE
            if (normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-"))
            {
                aliases.Add($"100ge-{number}");
                aliases.Add($"100g{number}");
                aliases.Add($"HundredGigabitEthernet-{number}");
            }
            // 40GE
            if (normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-"))
            {
                aliases.Add($"40ge-{number}");
                aliases.Add($"40g{number}");
                aliases.Add($"FortyGigabitEthernet-{number}");
            }
            // GE
            if (normalized.StartsWith("ge-") || normalized.StartsWith("gigabitethernet-"))
            {
                aliases.Add($"ge-{number}");
                aliases.Add($"gig{number}");
                aliases.Add($"GigabitEthernet-{number}");
            }
            // XE
            if (normalized.StartsWith("xe-") || normalized.StartsWith("tengigabitethernet-"))
            {
                aliases.Add($"xe-{number}");
                aliases.Add($"xge-{number}");
                aliases.Add($"ten{number}");
                aliases.Add($"TenGigabitEthernet-{number}");
            }
            // ET
            if (normalized.StartsWith("et-") || normalized.StartsWith("ethernetinterface-"))
            {
                aliases.Add($"et-{number}");
                aliases.Add($"ether-{number}");
                aliases.Add($"EthernetInterface-{number}");
            }
            // Management
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt"))
            {
                aliases.Add($"em{number}");
                aliases.Add($"fxp{number}");
                aliases.Add($"me{number}");
                aliases.Add($"mgmt{number}");
                aliases.Add($"Management{number}");
            }
            // Loopback
            if (normalized.StartsWith("lo"))
            {
                aliases.Add($"lo{number}");
                aliases.Add($"loop{number}");
                aliases.Add($"Loopback{number}");
            }
            // AggregatedEthernet
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg"))
            {
                aliases.Add($"ae{number}");
                aliases.Add($"agg{number}");
                aliases.Add($"AggregatedEthernet{number}");
            }
            // RedundantEthernet
            if (normalized.StartsWith("reth") || normalized.StartsWith("redundant"))
            {
                aliases.Add($"reth{number}");
                aliases.Add($"RedundantEthernet{number}");
                aliases.Add($"redundant{number}");
            }
            // STunnel
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
            {
                aliases.Add($"st{number}");
                aliases.Add($"tunnel{number}");
                aliases.Add($"STunnel{number}");
            }
            // GRE
            if (normalized.StartsWith("gr-") || normalized.StartsWith("gre-"))
            {
                aliases.Add($"gr-{number}");
                aliases.Add($"GRE-{number}");
            }
            // IRB
            if (normalized.StartsWith("irb"))
            {
                aliases.Add($"irb{number}");
                aliases.Add($"IRB{number}");
            }
            // VLAN
            if (normalized.StartsWith("vlan"))
            {
                aliases.Add($"vlan{number}");
                aliases.Add($"VLAN{number}");
            }
            // Ether short alias for et-
            if (normalized.StartsWith("et-"))
            {
                aliases.Add($"ether-{number}");
            }
            // Add tunnel0 for stunnel
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
            {
                aliases.Add($"tunnel{number}");
            }
            // Add loop0 for lo
            if (normalized.StartsWith("lo"))
            {
                aliases.Add($"loop{number}");
            }
            // Add agg0 for ae
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg"))
            {
                aliases.Add($"agg{number}");
            }
            // Add mgmt0 for management
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt"))
            {
                aliases.Add($"mgmt{number}");
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

            if (normalized.StartsWith("twentyfivegigabitethernet-") || normalized.StartsWith("25ge-")) return "TwentyFiveGigabitEthernet";
            if (normalized.StartsWith("hundredgigabitethernet-") || normalized.StartsWith("100ge-")) return "HundredGigabitEthernet";
            if (normalized.StartsWith("fortygigabitethernet-") || normalized.StartsWith("40ge-")) return "FortyGigabitEthernet";
            if (normalized.StartsWith("gigabitethernet-") || normalized.StartsWith("ge-")) return "GigabitEthernet";
            if (normalized.StartsWith("tengigabitethernet-") || normalized.StartsWith("xe-")) return "TenGigabitEthernet";
            if (normalized.StartsWith("ethernetinterface-") || normalized.StartsWith("et-")) return "EthernetInterface";
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt")) return "Management";
            if (normalized.StartsWith("lo")) return "Loopback";
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg")) return "AggregatedEthernet";
            if (normalized.StartsWith("reth") || normalized.StartsWith("redundant")) return "RedundantEthernet";
            if (normalized.StartsWith("irb")) return "IRB";
            if (normalized.StartsWith("vlan")) return "VLAN";
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st")) return "STunnel";
            if (normalized.StartsWith("gre-") || normalized.StartsWith("gr-")) return "GRE";
            if (normalized.StartsWith("ip-")) return "IPTunnel";
            if (normalized.StartsWith("lt-")) return "LogicalTunnel";

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
            var normalized = expanded.ToLower();

            // For types with dash and numbers (e.g., ge-0/0/0, xe-0/0/0, et-0/0/0, 25ge-0/0/0, 40ge-0/0/0, 100ge-0/0/0)
            var dashIdx = expanded.IndexOf('-');
            if (dashIdx > 0 && dashIdx < expanded.Length - 1)
            {
                var numPart = expanded.Substring(dashIdx + 1);
                if (Regex.IsMatch(numPart, @"^\d+/\d+/\d+"))
                    return numPart;
            }

            // For types with no dash but numbers at the end (e.g., ae0, lo0, me0, em0, fxp0, IRB0, VLAN.100)
            var m = Regex.Match(expanded, @"[a-zA-Z]+[.\-]?([0-9][0-9./]*)$");
            if (m.Success)
                return m.Groups[1].Value.TrimStart('.', '/');

            return "";
        }

        /// <summary>
        /// Gets the canonical interface name (expanded and normalized)
        /// </summary>
        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            string number = GetInterfaceNumber(expanded);
            // Remove any leading dots or slashes from number for all types except VLAN (which may have a dot)
            string cleanNumber = number;
            if (!normalized.StartsWith("vlan."))
            {
                cleanNumber = number.TrimStart('.', '/');
            }

            if ((normalized.StartsWith("twentyfivegigabitethernet-") || normalized.StartsWith("25ge-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"TwentyFiveGigabitEthernet-{cleanNumber.TrimStart('/')}";
            if ((normalized.StartsWith("hundredgigabitethernet-") || normalized.StartsWith("100ge-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"HundredGigabitEthernet-{cleanNumber.TrimStart('/')}";
            if ((normalized.StartsWith("fortygigabitethernet-") || normalized.StartsWith("40ge-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"FortyGigabitEthernet-{cleanNumber.TrimStart('/')}";
            if ((normalized.StartsWith("gigabitethernet-") || normalized.StartsWith("ge-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"GigabitEthernet-{cleanNumber.TrimStart('/')}";
            if ((normalized.StartsWith("tengigabitethernet-") || normalized.StartsWith("xe-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"TenGigabitEthernet-{cleanNumber.TrimStart('/')}";
            if ((normalized.StartsWith("ethernetinterface-") || normalized.StartsWith("et-")) && !string.IsNullOrEmpty(cleanNumber))
                return $"EthernetInterface-{cleanNumber.TrimStart('/')}";
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt"))
                return "Management" + cleanNumber;
            if (normalized.StartsWith("lo"))
                return "Loopback" + cleanNumber;
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg"))
                return "AggregatedEthernet" + cleanNumber;
            if (normalized.StartsWith("reth") || normalized.StartsWith("redundant"))
                return "RedundantEthernet" + cleanNumber;
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
                return "STunnel" + cleanNumber;
            if (normalized.StartsWith("gr-"))
                return "GRE-" + cleanNumber;
            if (normalized.StartsWith("irb"))
                return ("IRB" + cleanNumber).ToUpper();
            if (normalized.StartsWith("vlan."))
                return ("VLAN." + number).ToUpper();
            if (normalized.StartsWith("vlan"))
                return ("VLAN" + cleanNumber).ToUpper();

            // Return the expanded form if no canonical mapping found
            return expanded;
        }
    }
} 
