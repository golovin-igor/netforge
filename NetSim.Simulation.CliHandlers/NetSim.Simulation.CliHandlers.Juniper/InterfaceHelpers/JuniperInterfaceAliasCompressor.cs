using System;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Compresses Juniper interface names to short aliases.
    /// </summary>
    public static class JuniperInterfaceAliasCompressor
    {
        public static string CompressInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            // Convert full names to shortest common abbreviations (lowercase)
            var result = interfaceName;
            var subInterface = "";
            if (interfaceName.Contains("."))
            {
                var parts = interfaceName.Split('.');
                result = parts[0];
                subInterface = "." + parts[1];
            }

            result = result
                .Replace("HundredGigabitEthernet", "100ge", System.StringComparison.OrdinalIgnoreCase)
                .Replace("FortyGigabitEthernet", "40ge", System.StringComparison.OrdinalIgnoreCase)
                .Replace("TwentyFiveGigabitEthernet", "25ge", System.StringComparison.OrdinalIgnoreCase)
                .Replace("TenGigabitEthernet", "xe", System.StringComparison.OrdinalIgnoreCase)
                .Replace("GigabitEthernet", "ge", System.StringComparison.OrdinalIgnoreCase)
                .Replace("EthernetInterface", "et", System.StringComparison.OrdinalIgnoreCase)
                .Replace("AggregatedEthernet", "ae", System.StringComparison.OrdinalIgnoreCase)
                .Replace("RedundantEthernet", "reth", System.StringComparison.OrdinalIgnoreCase)
                .Replace("Loopback", "lo", System.StringComparison.OrdinalIgnoreCase)
                .Replace("Management", "mgmt", System.StringComparison.OrdinalIgnoreCase)
                .Replace("VLAN", "vlan", System.StringComparison.OrdinalIgnoreCase)
                .Replace("IRB", "irb", System.StringComparison.OrdinalIgnoreCase)
                .Replace("GRE", "gr", System.StringComparison.OrdinalIgnoreCase)
                .Replace("STunnel", "st", System.StringComparison.OrdinalIgnoreCase);

            return result.ToLower() + subInterface;
        }
    }
}
