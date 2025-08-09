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
                normalized.Contains("stunnel", StringComparison.OrdinalIgnoreCase))
                return input; // Preserve original case

            // Management aliases (fxp, me, em, mgmt)
            if (normalized.StartsWith("fxp", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("me", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("em", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("mgmt", StringComparison.OrdinalIgnoreCase))
            {
                int prefixLen = normalized.StartsWith("mgmt") ? 4 : 2;
                if (normalized.StartsWith("fxp")) prefixLen = 3;
                var number = input.Substring(prefixLen);
                if (string.IsNullOrEmpty(number)) number = "0";
                return $"Management{number}";
            }

            // Helper for AE, LO, RETH, STUNNEL, etc.
            string NoExtraZeros(string prefix, string rest)
            {
                rest = rest.TrimStart('-','/');
                if (string.IsNullOrEmpty(rest)) return prefix + "0";
                return prefix + rest;
            }

            if (normalized.StartsWith("ae", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("AggregatedEthernet", input.Substring(2));
            if (normalized.StartsWith("lo", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("Loopback", input.Substring(2));
            if (normalized.StartsWith("reth", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("RedundantEthernet", input.Substring(4));
            if (normalized.StartsWith("stunnel", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("STunnel", input.Substring(7));
            if (normalized.StartsWith("st", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("STunnel", input.Substring(2));

            if (normalized.StartsWith("irb", StringComparison.OrdinalIgnoreCase))
                return $"IRB{input.Substring(3)}".ToUpper();
            if (normalized.StartsWith("vlan.", StringComparison.OrdinalIgnoreCase))
                return $"VLAN.{input.Substring(5)}".ToUpper();
            if (normalized.StartsWith("vlan", StringComparison.OrdinalIgnoreCase))
                return $"VLAN{input.Substring(4)}".ToUpper();

            // High-speed interfaces: always ensure x/y/z, default to 0/0/0 if missing
            string EnsureFullOrDefault(string s)
            {
                s = s.TrimStart('-','/');
                if (string.IsNullOrEmpty(s)) return "0/0/0";
                var parts = s.Split('/');
                while (parts.Length < 3) s += "/0";
                return s;
            }
            if (normalized.StartsWith("ge-", StringComparison.OrdinalIgnoreCase))
                return $"GigabitEthernet-{EnsureFullOrDefault(input.Substring(3))}";
            if (normalized.StartsWith("xe-", StringComparison.OrdinalIgnoreCase))
                return $"TenGigabitEthernet-{EnsureFullOrDefault(input.Substring(3))}";
            if (normalized.StartsWith("et-", StringComparison.OrdinalIgnoreCase))
                return $"EthernetInterface-{EnsureFullOrDefault(input.Substring(3))}";
            if (normalized.StartsWith("100ge-", StringComparison.OrdinalIgnoreCase))
                return $"HundredGigabitEthernet-{EnsureFullOrDefault(input.Substring(7))}";
            if (normalized.StartsWith("25ge-", StringComparison.OrdinalIgnoreCase))
                return $"TwentyFiveGigabitEthernet-{EnsureFullOrDefault(input.Substring(6))}";
            if (normalized.StartsWith("40ge-", StringComparison.OrdinalIgnoreCase))
                return $"FortyGigabitEthernet-{EnsureFullOrDefault(input.Substring(6))}";

            if (normalized.StartsWith("agg", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("AggregatedEthernet", input.Substring(3));
            if (normalized.StartsWith("redundantethernet", StringComparison.OrdinalIgnoreCase))
                return NoExtraZeros("RedundantEthernet", input.Substring(16));
            if (normalized.StartsWith("gr-", StringComparison.OrdinalIgnoreCase))
                return $"GRE-{EnsureFullOrDefault(input.Substring(3))}";
            if (normalized.StartsWith("gre-", StringComparison.OrdinalIgnoreCase))
                return $"GRE-{EnsureFullOrDefault(input.Substring(4))}";

            // Return original name if no alias expansion needed
            return input;
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
