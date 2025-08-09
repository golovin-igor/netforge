using System.Text.RegularExpressions;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Validates Juniper interface names.
    /// </summary>
    public static class JuniperInterfaceNameValidator
    {
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
                @"^stunnel\d+(\.\d+)?$",                            // stunnel0
                @"^gr-\d+/\d+/\d+(\.\d+)?$",                        // gr-0/0/0 (GRE tunnel)
                @"^gre-\d+/\d+/\d+(\.\d+)?$",                       // gre-0/0/0 (GRE tunnel)
                @"^GRE-\d+/\d+/\d+(\.\d+)?$",                       // GRE-0/0/0 PascalCase
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
                @"^mgmt\d+(\.\d+)?$",                               // mgmt0 (management)
                @"^management\d+(\.\d+)?$",                         // management0
                @"^reth\d+(\.\d+)?$",                               // reth0 (redundant ethernet)
                @"^RedundantEthernet\d+(\.\d+)?$",                  // RedundantEthernet0
                @"^AggregatedEthernet\d+(\.\d+)?$",                 // AggregatedEthernet0
                @"^Loopback\d+(\.\d+)?$",                           // Loopback0
                @"^Management\d+(\.\d+)?$",                         // Management0
                @"^STunnel\d+(\.\d+)?$",                            // STunnel0
                @"^IRB\d+(\.\d+)?$",                                // IRB0
                @"^VLAN(\.\d+)?$",                                   // VLAN.100
            };
            return patterns.Any(pattern => Regex.IsMatch(interfaceName, pattern, RegexOptions.IgnoreCase));
        }
    }
}
