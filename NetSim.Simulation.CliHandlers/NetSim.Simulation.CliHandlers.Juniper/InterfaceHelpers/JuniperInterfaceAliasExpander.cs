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
            if (normalized.Contains("gigabitethernet") || normalized.Contains("tengigabitethernet") ||
                normalized.Contains("twentyfivegigabitethernet") || normalized.Contains("hundredgigabitethernet") ||
                normalized.Contains("fortygigabitethernet") || normalized.Contains("ethernetinterface") ||
                normalized.Contains("management") || normalized.Contains("loopback") ||
                normalized.Contains("aggregatedethernet") || normalized.Contains("redundantethernet") ||
                normalized.Contains("vlan") || normalized.Contains("irb") || normalized.Contains("gre") ||
                normalized.Contains("stunnel") || normalized.Contains("fxp") || normalized.Contains("me") ||
                normalized.Contains("em"))
                return interfaceName;

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
            if (normalized.StartsWith("redundantethernet"))
                return $"RedundantEthernet{input.Substring(16)}";
            if (normalized.StartsWith("agg"))
                return $"AggregatedEthernet{input.Substring(3)}";
            if (normalized.StartsWith("stunnel"))
                return $"STunnel{input.Substring(7)}";
            if (normalized.StartsWith("st"))
                return $"STunnel{input.Substring(2)}";
            if (normalized.StartsWith("gr-"))
                return $"GRE-{input.Substring(3)}";
            if (normalized.StartsWith("gre-"))
                return $"GRE-{input.Substring(4)}";
            if (normalized.StartsWith("100ge-"))
                return $"HundredGigabitEthernet-{input.Substring(7)}";
            if (normalized.StartsWith("25ge-"))
                return $"TwentyFiveGigabitEthernet-{input.Substring(6)}";
            if (normalized.StartsWith("40ge-"))
                return $"FortyGigabitEthernet-{input.Substring(6)}";

            // Return original name if no alias expansion needed
            return interfaceName;
        }
    }
}
