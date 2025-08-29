namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents an EIGRP configuration
    /// </summary>
    public class EigrpConfig(int asNumber)
    {
        public int AsNumber { get; set; } = asNumber;
        public string RouterId { get; set; } = "";
        public List<string> Networks { get; set; } = new List<string>();
        public bool AutoSummary { get; set; } = true;
        public List<EigrpNeighbor> Neighbors { get; set; } = new List<EigrpNeighbor>();
        public Dictionary<string, int> Metrics { get; set; } = new Dictionary<string, int>();
        public List<string> Redistribution { get; set; } = new List<string>();
        public bool IsEnabled { get; set; } = true;
    }

    public class EigrpNeighbor(string ipAddress, int asNumber, string interfaceName)
    {
        public string IpAddress { get; set; } = ipAddress;
        public int AsNumber { get; set; } = asNumber;
        public string Interface { get; set; } = interfaceName;
        public string State { get; set; } = "Up";
        public int Metric { get; set; } = 0;
    }
}
