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
            if (normalized.StartsWith("mgmt"))
                return $"Management{input.Substring(4)}";
            if (normalized.StartsWith("vlan."))
                return $"VLAN.{input.Substring(5)}";
            if (normalized.StartsWith("vlan"))
                return $"VLAN{input.Substring(4)}";
            if (normalized.StartsWith("reth"))
                return $"RedundantEthernet{input.Substring(4)}";
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
            // Always use the shortest, most common Juniper alias
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
                return $"ae{number}";
            if (normalized.StartsWith("agg"))
                return $"ae{number}";
            if (normalized.StartsWith("mgmt") || normalized.StartsWith("management"))
                return $"me{number}";
            if (normalized.StartsWith("irb"))
                return $"irb{number}";
            if (normalized.StartsWith("loopback"))
                return $"lo{number}";
            if (normalized.StartsWith("vlan."))
                return $"vlan{number}";
            if (normalized.StartsWith("vlan"))
                return $"vlan{number}";
            if (normalized.StartsWith("redundantethernet"))
                return $"reth{number}";
            if (normalized.StartsWith("reth"))
                return $"reth{number}";
            if (normalized.StartsWith("gre-") || normalized.StartsWith("gr-"))
                return $"gr-{number}";
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
                return $"st{number}";
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
            string cleanNumber = number.TrimStart('/', '-');
            // For high-speed interfaces, ensure cleanNumber is always x/y/z
            if ((normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-") ||
                 normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-") ||
                 normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-")) && cleanNumber.Count(c => c == '/') == 1)
            {
                // If only x/y, try to extract x/y/z from the original string
                var m = System.Text.RegularExpressions.Regex.Match(expanded, @"([0-9]+/[0-9]+/[0-9]+)");
                if (m.Success) cleanNumber = m.Groups[1].Value;
            }
            // 25GE
            if (normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-"))
            {
                aliases.Add($"25ge-{cleanNumber}");
                aliases.Add($"25g{cleanNumber}");
                aliases.Add($"TwentyFiveGigabitEthernet-{cleanNumber}");
                aliases.Add($"twentyfivegigabit-{cleanNumber}");
            }
            // 100GE
            if (normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-"))
            {
                aliases.Add($"100ge-{cleanNumber}");
                aliases.Add($"100g{cleanNumber}");
                aliases.Add($"HundredGigabitEthernet-{cleanNumber}");
                aliases.Add($"hundredgigabit-{cleanNumber}");
            }
            // 40GE
            if (normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-"))
            {
                aliases.Add($"40ge-{cleanNumber}");
                aliases.Add($"40g{cleanNumber}");
                aliases.Add($"FortyGigabitEthernet-{cleanNumber}");
                aliases.Add($"fortygigabit-{cleanNumber}");
            }
            // GE
            if (normalized.StartsWith("ge-") || normalized.StartsWith("gigabitethernet-"))
            {
                aliases.Add($"ge-{cleanNumber}");
                aliases.Add($"gig{cleanNumber}");
                aliases.Add($"gig-{cleanNumber}");
                aliases.Add($"GigabitEthernet-{cleanNumber}");
                aliases.Add($"gigabit-{cleanNumber}");
            }
            // XE
            if (normalized.StartsWith("xe-") || normalized.StartsWith("tengigabitethernet-"))
            {
                aliases.Add($"xe-{cleanNumber}");
                aliases.Add($"xge-{cleanNumber}");
                aliases.Add($"ten{cleanNumber}");
                aliases.Add($"tengig-{cleanNumber}");
                aliases.Add($"TenGigabitEthernet-{cleanNumber}");
                aliases.Add($"tengigabit-{cleanNumber}");
            }
            // ET
            if (normalized.StartsWith("et-") || normalized.StartsWith("ethernetinterface-"))
            {
                aliases.Add($"et-{cleanNumber}");
                aliases.Add($"ether-{cleanNumber}");
                aliases.Add($"EthernetInterface-{cleanNumber}");
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
                aliases.Add($"aggregated{number}");
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
                aliases.Add($"vl{number}");
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

        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var expanded = ExpandInterfaceAlias(interfaceName);
            var type = GetInterfaceType(expanded);
            var number = GetInterfaceNumber(expanded); // Always cleaned

            switch (type)
            {
                case "GigabitEthernet":
                    return !string.IsNullOrEmpty(number) ? $"GigabitEthernet-{number}" : "GigabitEthernet";
                case "TenGigabitEthernet":
                    return !string.IsNullOrEmpty(number) ? $"TenGigabitEthernet-{number}" : "TenGigabitEthernet";
                case "TwentyFiveGigabitEthernet":
                    return !string.IsNullOrEmpty(number) ? $"TwentyFiveGigabitEthernet-{number}" : "TwentyFiveGigabitEthernet";
                case "FortyGigabitEthernet":
                    return !string.IsNullOrEmpty(number) ? $"FortyGigabitEthernet-{number}" : "FortyGigabitEthernet";
                case "HundredGigabitEthernet":
                    return !string.IsNullOrEmpty(number) ? $"HundredGigabitEthernet-{number}" : "HundredGigabitEthernet";
                case "EthernetInterface":
                    return !string.IsNullOrEmpty(number) ? $"EthernetInterface-{number}" : "EthernetInterface";
                case "AggregatedEthernet":
                    return !string.IsNullOrEmpty(number) ? $"AggregatedEthernet{number}" : "AggregatedEthernet";
                case "Loopback":
                    return !string.IsNullOrEmpty(number) ? $"Loopback{number}" : "Loopback";
                case "Management":
                    return !string.IsNullOrEmpty(number) ? $"Management{number}" : "Management";
                case "VLAN":
                    return !string.IsNullOrEmpty(number) ? $"VLAN{number}" : "VLAN";
                case "IRB":
                    return !string.IsNullOrEmpty(number) ? $"IRB{number}" : "IRB";
                case "GRE":
                    return !string.IsNullOrEmpty(number) ? $"gr-{number}" : "gr";
                case "STunnel":
                    return !string.IsNullOrEmpty(number) ? $"st{number}" : "st";
                case "FXP":
                    return !string.IsNullOrEmpty(number) ? $"FXP{number}" : "FXP";
                case "ME":
                    return !string.IsNullOrEmpty(number) ? $"ME{number}" : "ME";
                case "EM":
                    return !string.IsNullOrEmpty(number) ? $"EM{number}" : "EM";
                case "RedundantEthernet":
                    return !string.IsNullOrEmpty(number) ? $"reth{number}" : "reth";
                default:
                    return expanded;
            }
        }

        public static string GetInterfaceNumber(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            // For high-speed interfaces: ge-, xe-, et-, 25ge-, 40ge-, 100ge-, gr-
            var m = System.Text.RegularExpressions.Regex.Match(expanded, @"(?:ge|xe|et|25ge|40ge|100ge|gr)-([0-9]+/[0-9]+/[0-9]+(?:\.[0-9]+)?)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            // For high-speed interfaces, if only x/y is found, try to extract x/y/z from the original string
            if (System.Text.RegularExpressions.Regex.IsMatch(expanded, @"(?:ge|xe|et|25ge|40ge|100ge|gr)-[0-9]+/[0-9]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                var m2 = System.Text.RegularExpressions.Regex.Match(interfaceName, @"([0-9]+/[0-9]+/[0-9]+)");
                if (m2.Success) return m2.Groups[1].Value;
            }

            // For types with dash and numbers (fallback, e.g., st-, gre-)
            var dashIdx = expanded.IndexOf('-');
            if (dashIdx > 0 && dashIdx < expanded.Length - 1)
            {
                var numPart = expanded.Substring(dashIdx + 1);
                numPart = System.Text.RegularExpressions.Regex.Replace(numPart, "^[^0-9]+", "");
                if (System.Text.RegularExpressions.Regex.IsMatch(numPart, @"^\d+(/\d+){0,2}(\.\d+)?$"))
                    return numPart;
            }

            // For types with no dash but numbers at the end (e.g., ae0, lo0, me0, em0, fxp0, IRB0, VLAN.100, st0, reth0)
            m = System.Text.RegularExpressions.Regex.Match(expanded, @"[a-zA-Z]+[.\-]?([0-9][0-9./]*)$");
            if (m.Success)
                return m.Groups[1].Value.TrimStart('.', '/');

            // For VLAN.100 or similar
            m = System.Text.RegularExpressions.Regex.Match(expanded, @"vlan[.](\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            // For IRB.100 or similar
            m = System.Text.RegularExpressions.Regex.Match(expanded, @"irb[.](\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            return "";
        }

        public static string GetInterfaceType(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "Unknown";

            var normalized = interfaceName.ToLower();
            if (normalized.StartsWith("ge-") || normalized.StartsWith("gigabitethernet-"))
                return "GigabitEthernet";
            if (normalized.StartsWith("xe-") || normalized.StartsWith("tengigabitethernet-"))
                return "TenGigabitEthernet";
            if (normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-"))
                return "TwentyFiveGigabitEthernet";
            if (normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-"))
                return "FortyGigabitEthernet";
            if (normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-"))
                return "HundredGigabitEthernet";
            if (normalized.StartsWith("et-") || normalized.StartsWith("ethernetinterface-"))
                return "EthernetInterface";
            if (normalized.StartsWith("ae") || normalized.StartsWith("aggregatedethernet"))
                return "AggregatedEthernet";
            if (normalized.StartsWith("lo") || normalized.StartsWith("loopback"))
                return "Loopback";
            if (normalized.StartsWith("me") || normalized.StartsWith("fxp") || normalized.StartsWith("em") || normalized.StartsWith("mgmt") || normalized.StartsWith("management"))
                return "Management";
            if (normalized.StartsWith("vlan"))
                return "VLAN";
            if (normalized.StartsWith("irb"))
                return "IRB";
            if (normalized.StartsWith("gr-") || normalized.StartsWith("gre-"))
                return "GRE";
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
                return "STunnel";
            if (normalized.StartsWith("fxp"))
                return "FXP";
            if (normalized.StartsWith("me"))
                return "ME";
            if (normalized.StartsWith("em"))
                return "EM";
            if (normalized.StartsWith("reth") || normalized.StartsWith("redundantethernet"))
                return "RedundantEthernet";
            return "Unknown";
        }
    }
}
