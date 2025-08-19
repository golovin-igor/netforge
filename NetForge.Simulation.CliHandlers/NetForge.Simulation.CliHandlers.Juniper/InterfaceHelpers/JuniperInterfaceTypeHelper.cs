namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Provides helpers for Juniper interface type detection.
    /// </summary>
    public static class JuniperInterfaceTypeHelper
    {
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
