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
            
            // Handle specific interface patterns in order of specificity
            
            // 25GE interfaces - more precise patterns
            if (Regex.IsMatch(normalized, @"^25g\d") || Regex.IsMatch(normalized, @"^25g-\d"))
            {
                return Regex.Replace(input, @"^25g(\S+)", "25ge-$1", RegexOptions.IgnoreCase);
            }
            if (Regex.IsMatch(normalized, @"^twentyfive\d") || Regex.IsMatch(normalized, @"^twentyfive-\d"))
            {
                return Regex.Replace(input, @"^twentyfive(\S+)", "25ge-$1", RegexOptions.IgnoreCase);
            }
            
            // 100GE interfaces
            if (Regex.IsMatch(normalized, @"^100g\d") || Regex.IsMatch(normalized, @"^100g-\d"))
            {
                return Regex.Replace(input, @"^100g(\S+)", "100ge-$1", RegexOptions.IgnoreCase);
            }
            if (Regex.IsMatch(normalized, @"^hundred\d") || Regex.IsMatch(normalized, @"^hundred-\d"))
            {
                return Regex.Replace(input, @"^hundred(\S+)", "100ge-$1", RegexOptions.IgnoreCase);
            }
            
            // 40GE interfaces
            if (Regex.IsMatch(normalized, @"^40g\d") || Regex.IsMatch(normalized, @"^40g-\d"))
            {
                return Regex.Replace(input, @"^40g(\S+)", "40ge-$1", RegexOptions.IgnoreCase);
            }
            if (Regex.IsMatch(normalized, @"^forty\d") || Regex.IsMatch(normalized, @"^forty-\d"))
            {
                return Regex.Replace(input, @"^forty(\S+)", "40ge-$1", RegexOptions.IgnoreCase);
            }
            
            // GigabitEthernet patterns - more precise
            if (Regex.IsMatch(normalized, @"^gig\d") || Regex.IsMatch(normalized, @"^gig-\d"))
            {
                return Regex.Replace(input, @"^gig(\S+)", "ge-$1", RegexOptions.IgnoreCase);
            }
            
            // 10 GigabitEthernet patterns
            if (Regex.IsMatch(normalized, @"^ten\d") || Regex.IsMatch(normalized, @"^ten-\d"))
            {
                return Regex.Replace(input, @"^ten(\S+)", "xe-$1", RegexOptions.IgnoreCase);
            }
            
            // Management patterns
            if (Regex.IsMatch(normalized, @"^mgmt\d"))
            {
                return Regex.Replace(input, @"^mgmt(\d+)", "em$1", RegexOptions.IgnoreCase);
            }
            
            // Loopback patterns
            if (Regex.IsMatch(normalized, @"^loopback\d"))
            {
                return Regex.Replace(input, @"^loopback(\d+)", "lo$1", RegexOptions.IgnoreCase);
            }
            
            // Aggregated Ethernet patterns
            if (Regex.IsMatch(normalized, @"^agg\d"))
            {
                return Regex.Replace(input, @"^agg(\d+)", "ae$1", RegexOptions.IgnoreCase);
            }
            
            // GRE patterns
            if (Regex.IsMatch(normalized, @"^gre\d") || Regex.IsMatch(normalized, @"^gre-\d"))
            {
                return Regex.Replace(input, @"^gre(\S+)", "gr-$1", RegexOptions.IgnoreCase);
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

            var input = interfaceName.Trim();
            var normalized = input.ToLower();
            
            // Convert to shortest form
            if (normalized.StartsWith("25ge-"))
                return Regex.Replace(input, @"^25ge-(.+)", "25g$1", RegexOptions.IgnoreCase);
            if (normalized.StartsWith("100ge-"))
                return Regex.Replace(input, @"^100ge-(.+)", "100g$1", RegexOptions.IgnoreCase);
            if (normalized.StartsWith("40ge-"))
                return Regex.Replace(input, @"^40ge-(.+)", "40g$1", RegexOptions.IgnoreCase);
            if (normalized.StartsWith("ge-"))
                return Regex.Replace(input, @"^ge-(.+)", "g$1", RegexOptions.IgnoreCase);
            if (normalized.StartsWith("xe-"))
                return Regex.Replace(input, @"^xe-(.+)", "x$1", RegexOptions.IgnoreCase);
            if (normalized.StartsWith("et-"))
                return Regex.Replace(input, @"^et-(.+)", "e$1", RegexOptions.IgnoreCase);
            
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

            if (normalized.StartsWith("25ge-"))
                return "TwentyFiveGigabitEthernet";
            if (normalized.StartsWith("100ge-"))
                return "HundredGigabitEthernet";
            if (normalized.StartsWith("40ge-"))
                return "FortyGigabitEthernet";
            if (normalized.StartsWith("ge-"))
                return "GigabitEthernet";
            if (normalized.StartsWith("xe-"))
                return "TenGigabitEthernet";
            if (normalized.StartsWith("et-"))
                return "EthernetInterface";
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me"))
                return "Management";
            if (normalized.StartsWith("lo"))
                return "Loopback";
            if (normalized.StartsWith("ae"))
                return "AggregatedEthernet";
            if (normalized.StartsWith("reth"))
                return "RedundantEthernet";
            if (normalized.StartsWith("irb"))
                return "IRB";
            if (normalized.StartsWith("vlan"))
                return "VLAN";
            if (normalized.StartsWith("st"))
                return "STunnel";
            if (normalized.StartsWith("gr-"))
                return "GRE";
            if (normalized.StartsWith("ip-"))
                return "IPTunnel";
            if (normalized.StartsWith("lt-"))
                return "LogicalTunnel";

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
            
            // Extract number part for different Juniper interface types
            if (expanded.StartsWith("25ge-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"25ge-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("100ge-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"100ge-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("40ge-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"40ge-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("ge-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"ge-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("xe-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"xe-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("et-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"et-(.+)", RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : "";
            }
            if (expanded.StartsWith("gr-", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(expanded, @"gr-(.+)", RegexOptions.IgnoreCase);
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
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            // Return the full canonical name based on interface type
            if (normalized.StartsWith("25ge-"))
            {
                var number = expanded.Substring("25ge-".Length);
                return $"TwentyFiveGigabitEthernet-{number}";
            }
            if (normalized.StartsWith("100ge-"))
            {
                var number = expanded.Substring("100ge-".Length);
                return $"HundredGigabitEthernet-{number}";
            }
            if (normalized.StartsWith("40ge-"))
            {
                var number = expanded.Substring("40ge-".Length);
                return $"FortyGigabitEthernet-{number}";
            }
            if (normalized.StartsWith("ge-"))
            {
                var number = expanded.Substring("ge-".Length);
                return $"GigabitEthernet-{number}";
            }
            if (normalized.StartsWith("xe-"))
            {
                var number = expanded.Substring("xe-".Length);
                return $"TenGigabitEthernet-{number}";
            }
            if (normalized.StartsWith("et-"))
            {
                var number = expanded.Substring("et-".Length);
                return $"EthernetInterface-{number}";
            }
            if (normalized.StartsWith("em"))
            {
                var number = expanded.Substring("em".Length);
                return $"Management{number}";
            }
            if (normalized.StartsWith("fxp"))
            {
                var number = expanded.Substring("fxp".Length);
                return $"Management{number}";
            }
            if (normalized.StartsWith("me"))
            {
                var number = expanded.Substring("me".Length);
                return $"Management{number}";
            }
            if (normalized.StartsWith("lo"))
            {
                var number = expanded.Substring("lo".Length);
                return $"Loopback{number}";
            }
            if (normalized.StartsWith("ae"))
            {
                var number = expanded.Substring("ae".Length);
                return $"AggregatedEthernet{number}";
            }
            if (normalized.StartsWith("reth"))
            {
                var number = expanded.Substring("reth".Length);
                return $"RedundantEthernet{number}";
            }
            if (normalized.StartsWith("st"))
            {
                var number = expanded.Substring("st".Length);
                return $"STunnel{number}";
            }
            if (normalized.StartsWith("gr-"))
            {
                var number = expanded.Substring("gr-".Length);
                return $"GRE-{number}";
            }
            if (normalized.StartsWith("irb"))
            {
                var number = expanded.Substring("irb".Length);
                return $"IRB{number}";
            }
            if (normalized.StartsWith("vlan"))
            {
                var number = expanded.Substring("vlan".Length);
                return $"VLAN{number}";
            }

            // Return the expanded form if no canonical mapping found
            return expanded;
        }
    }
} 
