namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Provides canonicalization for Juniper interface names.
    /// </summary>
    public static class JuniperCanonicalInterfaceNameHelper
    {
        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var expanded = JuniperInterfaceAliasExpander.ExpandInterfaceAlias(interfaceName);
            var type = JuniperInterfaceAliasHandler.GetInterfaceType(expanded);
            var number = JuniperInterfaceAliasHandler.GetInterfaceNumber(expanded); // Always cleaned

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
                    return !string.IsNullOrEmpty(number) ? $"VLAN.{number}" : "VLAN";
                case "IRB":
                    return !string.IsNullOrEmpty(number) ? $"IRB{number}" : "IRB";
                case "GRE":
                    return !string.IsNullOrEmpty(number) ? $"GRE-{number}" : "GRE";
                case "STunnel":
                    return !string.IsNullOrEmpty(number) ? $"STunnel{number}" : "STunnel";
                case "FXP":
                    return !string.IsNullOrEmpty(number) ? $"FXP{number}" : "FXP";
                case "ME":
                    return !string.IsNullOrEmpty(number) ? $"ME{number}" : "ME";
                case "EM":
                    return !string.IsNullOrEmpty(number) ? $"EM{number}" : "EM";
                case "RedundantEthernet":
                    return !string.IsNullOrEmpty(number) ? $"RedundantEthernet{number}" : "RedundantEthernet";
                default:
                    return expanded;
            }
        }
    }
}
