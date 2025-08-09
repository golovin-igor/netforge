namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents an IGRP configuration
    /// </summary>
    public class IgrpConfig
    {
        public int AsNumber { get; set; }
        public int AutonomousSystemNumber { get; set; }
        public string RouterId { get; set; } = "";
        public List<string> Networks { get; set; } = new List<string>();
        public bool AutoSummary { get; set; } = true;
        public List<IgrpNeighbor> Neighbors { get; set; } = new List<IgrpNeighbor>();
        public Dictionary<string, int> Metrics { get; set; } = new Dictionary<string, int>();
        public List<string> Redistribution { get; set; } = new List<string>();
        public bool IsEnabled { get; set; }
        public int Bandwidth { get; set; } = 1544; // Default T1 bandwidth in kbps
        public int Delay { get; set; } = 20000; // Default delay in microseconds
        public int Reliability { get; set; } = 255; // Default reliability (255 = 100%)
        public int Load { get; set; } = 1; // Default load (1 = no load)
        public int Mtu { get; set; } = 1500; // Default MTU
        
        public IgrpConfig(int asNumber)
        {
            AsNumber = asNumber;
            AutonomousSystemNumber = asNumber;
            IsEnabled = true;
        }
    }

    public class IgrpNeighbor
    {
        public string IpAddress { get; set; }
        public int AsNumber { get; set; }
        public string Interface { get; set; }
        public string State { get; set; } = "Up";
        public int Metric { get; set; } = 0;
        public int Bandwidth { get; set; } = 1544;
        public int Delay { get; set; } = 20000;
        public int Reliability { get; set; } = 255;
        public int Load { get; set; } = 1;
        public int Mtu { get; set; } = 1500;

        public IgrpNeighbor(string ipAddress, int asNumber, string interfaceName)
        {
            IpAddress = ipAddress;
            AsNumber = asNumber;
            Interface = interfaceName;
        }
    }
} 
