using System;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Expands Juniper interface aliases to full interface names.
    /// </summary>
    public static class JuniperInterfaceAliasExpander
    {
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var input = interfaceName.Trim();
            var normalized = input.ToLower();

            // If already in canonical form, return as-is
            if (normalized.Contains("gigabitethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("tengigabitethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("twentyfivegigabitethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("hundredgigabitethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("fortygigabitethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("ethernetinterface", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("management", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("loopback", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("aggregatedethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("redundantethernet", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("vlan", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("irb", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("gre", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("stunnel", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("fxp", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("me", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("em", StringComparison.OrdinalIgnoreCase))
                return interfaceName;

            if (normalized.StartsWith("ge-", StringComparison.OrdinalIgnoreCase))
                return $"GigabitEthernet-{EnsureFullStructure(input.Substring(3))}";
            if (normalized.StartsWith("xe-", StringComparison.OrdinalIgnoreCase))
                return $"TenGigabitEthernet-{EnsureFullStructure(input.Substring(3))}";
            if (normalized.StartsWith("et-", StringComparison.OrdinalIgnoreCase))
                return $"EthernetInterface-{EnsureFullStructure(input.Substring(3))}";
            if (normalized.StartsWith("ae", StringComparison.OrdinalIgnoreCase))
                return $"AggregatedEthernet{input.Substring(2)}";
            if (normalized.StartsWith("irb", StringComparison.OrdinalIgnoreCase))
                return $"IRB{input.Substring(3)}";
            if (normalized.StartsWith("lo", StringComparison.OrdinalIgnoreCase))
                return $"Loopback{input.Substring(2)}";
            if (normalized.StartsWith("me", StringComparison.OrdinalIgnoreCase))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("fxp", StringComparison.OrdinalIgnoreCase))
                return $"Management{input.Substring(3)}";
            if (normalized.StartsWith("em", StringComparison.OrdinalIgnoreCase))
                return $"Management{input.Substring(2)}";
            if (normalized.StartsWith("mgmt", StringComparison.OrdinalIgnoreCase))
                return $"Management{input.Substring(4)}";
            if (normalized.StartsWith("vlan.", StringComparison.OrdinalIgnoreCase))
                return $"VLAN.{input.Substring(5)}";
            if (normalized.StartsWith("vlan", StringComparison.OrdinalIgnoreCase))
                return $"VLAN{input.Substring(4)}";
            if (normalized.StartsWith("reth", StringComparison.OrdinalIgnoreCase))
                return $"RedundantEthernet{input.Substring(4)}";
            if (normalized.StartsWith("redundantethernet", StringComparison.OrdinalIgnoreCase))
                return $"RedundantEthernet{input.Substring(16)}";
            if (normalized.StartsWith("agg", StringComparison.OrdinalIgnoreCase))
                return $"AggregatedEthernet{input.Substring(3)}";
            if (normalized.StartsWith("stunnel", StringComparison.OrdinalIgnoreCase))
                return $"STunnel{input.Substring(7)}";
            if (normalized.StartsWith("st", StringComparison.OrdinalIgnoreCase))
                return $"STunnel{input.Substring(2)}";
            if (normalized.StartsWith("gr-", StringComparison.OrdinalIgnoreCase))
                return $"GRE-{input.Substring(3)}";
            if (normalized.StartsWith("gre-", StringComparison.OrdinalIgnoreCase))
                return $"GRE-{input.Substring(4)}";
            if (normalized.StartsWith("100ge-", StringComparison.OrdinalIgnoreCase))
                return $"HundredGigabitEthernet-{input.Substring(7)}";
            if (normalized.StartsWith("25ge-", StringComparison.OrdinalIgnoreCase))
                return $"TwentyFiveGigabitEthernet-{input.Substring(6)}";
            if (normalized.StartsWith("40ge-", StringComparison.OrdinalIgnoreCase))
                return $"FortyGigabitEthernet-{input.Substring(6)}";

            // Return original name if no alias expansion needed
            return interfaceName;
        }

        private static string EnsureFullStructure(string input)
        {
            // Ensure the structure is in the form 'x/y/z'
            var parts = input.Split('/');
            while (parts.Length < 3)
            {
                input += "/0";
                parts = input.Split('/');
            }
            return input;
        }
    }
}
