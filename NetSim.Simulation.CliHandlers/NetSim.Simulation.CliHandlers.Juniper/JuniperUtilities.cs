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
                return $"IRB{input.Substring(3)}";
            if (normalized.StartsWith("lo"))
                return $"Loopback{input.Substring(2)}";
            if (normalized.StartsWith("me"))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("fxp"))
                return $"Management{input.Substring(3)}";
            if (normalized.StartsWith("em"))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("vlan"))
                return $"VLAN{input.Substring(4)}";
            if (normalized.StartsWith("reth"))
                return $"RedundantEthernet{input.Substring(4)}";
            if (normalized.StartsWith("gr-"))
                return $"GRE-{input.Substring(3)}";
            if (normalized.StartsWith("st"))
                return $"STunnel{input.Substring(2)}";
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
            if (normalized.StartsWith("twentyfivegigabitethernet-"))
                return $"25ge-{input.Substring(26)}";
            if (normalized.StartsWith("hundredgigabitethernet-"))
                return $"100ge-{input.Substring(21)}";
            if (normalized.StartsWith("fortygigabitethernet-"))
                return $"40ge-{input.Substring(19)}";
            if (normalized.StartsWith("gigabitethernet-"))
                return $"ge-{input.Substring(16)}";
            if (normalized.StartsWith("tengigabitethernet-"))
                return $"xe-{input.Substring(18)}";
            if (normalized.StartsWith("ethernetinterface-"))
                return $"et-{input.Substring(17)}";
            if (normalized.StartsWith("aggregatedethernet"))
                return $"ae{input.Substring(18)}";
            if (normalized.StartsWith("irb"))
                return $"irb{input.Substring(3)}";
            if (normalized.StartsWith("loopback"))
                return $"lo{input.Substring(8)}";
            if (normalized.StartsWith("management"))
                return $"me{input.Substring(10)}";
            if (normalized.StartsWith("vlan."))
                return $"vlan{input.Substring(4).Replace(".","")}";
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

            var canonical1 = GetCanonicalInterfaceName(name1);
            var canonical2 = GetCanonicalInterfaceName(name2);
            
            return string.Equals(canonical1, canonical2, StringComparison.OrdinalIgnoreCase);
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
            if (normalized.StartsWith("25ge-"))
            {
                var number = expanded.Substring("25ge-".Length);
                aliases.Add($"25ge-{number}");
                aliases.Add($"25g{number}");
                aliases.Add($"TwentyFiveGigabitEthernet-{number}");
            }
            else if (normalized.StartsWith("100ge-"))
            {
                var number = expanded.Substring("100ge-".Length);
                aliases.Add($"100ge-{number}");
                aliases.Add($"100g{number}");
                aliases.Add($"HundredGigabitEthernet-{number}");
            }
            else if (normalized.StartsWith("40ge-"))
            {
                var number = expanded.Substring("40ge-".Length);
                aliases.Add($"40ge-{number}");
                aliases.Add($"40g{number}");
                aliases.Add($"FortyGigabitEthernet-{number}");
            }
            else if (normalized.StartsWith("ge-"))
            {
                var number = expanded.Substring("ge-".Length);
                aliases.Add($"ge-{number}");
                aliases.Add($"gig{number}");
                aliases.Add($"GigabitEthernet-{number}");
            }
            else if (normalized.StartsWith("xe-"))
            {
                var number = expanded.Substring("xe-".Length);
                aliases.Add($"xe-{number}");
                aliases.Add($"ten{number}");
                aliases.Add($"TenGigabitEthernet-{number}");
            }
            else if (normalized.StartsWith("et-"))
            {
                var number = expanded.Substring("et-".Length);
                aliases.Add($"et-{number}");
                aliases.Add($"EthernetInterface-{number}");
            }
            else if (normalized.StartsWith("em"))
            {
                var number = expanded.Substring("em".Length);
                aliases.Add($"em{number}");
                aliases.Add($"mgmt{number}");
                aliases.Add($"Management{number}");
            }
            else if (normalized.StartsWith("fxp"))
            {
                var number = expanded.Substring("fxp".Length);
                aliases.Add($"fxp{number}");
                aliases.Add($"Management{number}");
            }
            else if (normalized.StartsWith("me"))
            {
                var number = expanded.Substring("me".Length);
                aliases.Add($"me{number}");
                aliases.Add($"Management{number}");
            }
            else if (normalized.StartsWith("lo"))
            {
                var number = expanded.Substring("lo".Length);
                aliases.Add($"lo{number}");
                aliases.Add($"Loopback{number}");
            }
            else if (normalized.StartsWith("ae"))
            {
                var number = expanded.Substring("ae".Length);
                aliases.Add($"ae{number}");
                aliases.Add($"agg{number}");
                aliases.Add($"AggregatedEthernet{number}");
            }
            else if (normalized.StartsWith("reth"))
            {
                var number = expanded.Substring("reth".Length);
                aliases.Add($"reth{number}");
                aliases.Add($"RedundantEthernet{number}");
            }
            else if (normalized.StartsWith("st"))
            {
                var number = expanded.Substring("st".Length);
                aliases.Add($"st{number}");
                aliases.Add($"STunnel{number}");
            }
            else if (normalized.StartsWith("gr-"))
            {
                var number = expanded.Substring("gr-".Length);
                aliases.Add($"gr-{number}");
                aliases.Add($"GRE-{number}");
            }
            else if (normalized.StartsWith("irb"))
            {
                var number = expanded.Substring("irb".Length);
                aliases.Add($"irb{number}");
                aliases.Add($"IRB{number}");
            }
            else if (normalized.StartsWith("vlan"))
            {
                var number = expanded.Substring("vlan".Length);
                aliases.Add($"vlan{number}");
                aliases.Add($"VLAN{number}");
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

            if (normalized.StartsWith("25ge-")) return "TwentyFiveGigabitEthernet";
            if (normalized.StartsWith("hundredgigabitethernet-")) return "HundredGigabitEthernet";
            if (normalized.StartsWith("100ge-")) return "HundredGigabitEthernet";
            if (normalized.StartsWith("fortygigabitethernet-")) return "FortyGigabitEthernet";
            if (normalized.StartsWith("40ge-")) return "FortyGigabitEthernet";
            if (normalized.StartsWith("gigabitethernet-")) return "GigabitEthernet";
            if (normalized.StartsWith("ge-")) return "GigabitEthernet";
            if (normalized.StartsWith("tengigabitethernet-")) return "TenGigabitEthernet";
            if (normalized.StartsWith("xe-")) return "TenGigabitEthernet";
            if (normalized.StartsWith("ethernetinterface-")) return "EthernetInterface";
            if (normalized.StartsWith("et-")) return "EthernetInterface";
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me")) return "Management";
            if (normalized.StartsWith("lo")) return "Loopback";
            if (normalized.StartsWith("ae")) return "AggregatedEthernet";
            if (normalized.StartsWith("reth")) return "RedundantEthernet";
            if (normalized.StartsWith("irb")) return "IRB";
            if (normalized.StartsWith("vlan")) return "VLAN";
            if (normalized.StartsWith("st")) return "STunnel";
            if (normalized.StartsWith("gr-")) return "GRE";
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

            // Extract number part for different Juniper interface types (fix for correct numbers)
            if (normalized.StartsWith("25ge-"))
                return expanded.Substring(6);
            if (normalized.StartsWith("100ge-"))
                return expanded.Substring(7);
            if (normalized.StartsWith("40ge-"))
                return expanded.Substring(6);
            if (normalized.StartsWith("ge-"))
                return expanded.Substring(3);
            if (normalized.StartsWith("xe-"))
                return expanded.Substring(3);
            if (normalized.StartsWith("et-"))
                return expanded.Substring(3);
            if (normalized.StartsWith("gr-"))
                return expanded.Substring(3);
            if (normalized.StartsWith("ae"))
                return expanded.Substring(2);
            if (normalized.StartsWith("reth"))
                return expanded.Substring(4);
            if (normalized.StartsWith("lo"))
                return expanded.Substring(2);
            if (normalized.StartsWith("me"))
                return expanded.Substring(2);
            if (normalized.StartsWith("fxp"))
                return expanded.Substring(3);
            if (normalized.StartsWith("em"))
                return expanded.Substring(2);
            if (normalized.StartsWith("irb"))
                return expanded.Substring(3);
            if (normalized.StartsWith("vlan."))
                return expanded.Substring(5);
            if (normalized.StartsWith("vlan"))
                return expanded.Substring(4);
            // For other interfaces, extract everything after the alphabetic prefix
            var generalMatch = Regex.Match(expanded, @"^[a-zA-Z-]+(.*)");
            return generalMatch.Success ? generalMatch.Groups[1].Value : "";
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

            // Return the full canonical name based on interface type, fix substring logic for correct numbers
            if (normalized.StartsWith("25ge-"))
                return $"TwentyFiveGigabitEthernet-{expanded.Substring(6)}";
            if (normalized.StartsWith("100ge-"))
                return $"HundredGigabitEthernet-{expanded.Substring(7)}";
            if (normalized.StartsWith("40ge-"))
                return $"FortyGigabitEthernet-{expanded.Substring(6)}";
            if (normalized.StartsWith("ge-"))
                return $"GigabitEthernet-{expanded.Substring(3)}";
            if (normalized.StartsWith("xe-"))
                return $"TenGigabitEthernet-{expanded.Substring(3)}";
            if (normalized.StartsWith("et-"))
                return $"EthernetInterface-{expanded.Substring(3)}";
            if (normalized.StartsWith("em"))
                return $"Management{expanded.Substring(2)}";
            if (normalized.StartsWith("fxp"))
                return $"Management{expanded.Substring(3)}";
            if (normalized.StartsWith("me"))
                return $"Management{expanded.Substring(2)}";
            if (normalized.StartsWith("lo"))
                return $"Loopback{expanded.Substring(2)}";
            if (normalized.StartsWith("ae"))
                return $"AggregatedEthernet{expanded.Substring(2)}";
            if (normalized.StartsWith("reth"))
                return $"RedundantEthernet{expanded.Substring(4)}";
            if (normalized.StartsWith("st"))
                return $"STunnel{expanded.Substring(2)}";
            if (normalized.StartsWith("gr-"))
                return $"GRE-{expanded.Substring(3)}";
            if (normalized.StartsWith("irb"))
                return $"IRB{expanded.Substring(3)}";
            if (normalized.StartsWith("vlan"))
                return $"VLAN{expanded.Substring(4)}";

            // Return the expanded form if no canonical mapping found
            return expanded;
        }
    }
} 
