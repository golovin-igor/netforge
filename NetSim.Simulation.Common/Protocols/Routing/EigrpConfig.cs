namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents an EIGRP configuration
    /// </summary>
    public class EigrpConfig
    {
        public int AsNumber { get; set; }
        public string RouterId { get; set; } = "";
        public List<string> Networks { get; set; } = new List<string>();
        public bool AutoSummary { get; set; } = true;
        public List<EigrpNeighbor> Neighbors { get; set; } = new List<EigrpNeighbor>();
        public Dictionary<string, int> Metrics { get; set; } = new Dictionary<string, int>();
        public List<string> Redistribution { get; set; } = new List<string>();
        public bool IsEnabled { get; set; }
        
        public EigrpConfig(int asNumber)
        {
            AsNumber = asNumber;
            IsEnabled = true;
        }
    }

    public class EigrpNeighbor
    {
        public string IpAddress { get; set; }
        public int AsNumber { get; set; }
        public string Interface { get; set; }
        public string State { get; set; } = "Up";
        public int Metric { get; set; } = 0;

        public EigrpNeighbor(string ipAddress, int asNumber, string interfaceName)
        {
            IpAddress = ipAddress;
            AsNumber = asNumber;
            Interface = interfaceName;
        }
    }
} 
