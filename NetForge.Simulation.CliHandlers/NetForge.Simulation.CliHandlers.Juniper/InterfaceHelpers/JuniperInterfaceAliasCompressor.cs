using System;

namespace NetForge.Simulation.CliHandlers.Juniper
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
                .Replace("HundredGigabitEthernet", "100ge", StringComparison.OrdinalIgnoreCase)
                .Replace("FortyGigabitEthernet", "40ge", StringComparison.OrdinalIgnoreCase)
                .Replace("TwentyFiveGigabitEthernet", "25ge", StringComparison.OrdinalIgnoreCase)
                .Replace("TenGigabitEthernet", "xe", StringComparison.OrdinalIgnoreCase)
                .Replace("GigabitEthernet", "ge", StringComparison.OrdinalIgnoreCase)
                .Replace("EthernetInterface", "et", StringComparison.OrdinalIgnoreCase)
                .Replace("AggregatedEthernet", "ae", StringComparison.OrdinalIgnoreCase)
                .Replace("RedundantEthernet", "reth", StringComparison.OrdinalIgnoreCase)
                .Replace("Loopback", "lo", StringComparison.OrdinalIgnoreCase)
                .Replace("Management", "mgmt", StringComparison.OrdinalIgnoreCase)
                .Replace("VLAN", "vlan", StringComparison.OrdinalIgnoreCase)
                .Replace("IRB", "irb", StringComparison.OrdinalIgnoreCase)
                .Replace("GRE", "gr", StringComparison.OrdinalIgnoreCase)
                .Replace("STunnel", "st", StringComparison.OrdinalIgnoreCase);

            return result.ToLower() + subInterface;
        }
    }
}
