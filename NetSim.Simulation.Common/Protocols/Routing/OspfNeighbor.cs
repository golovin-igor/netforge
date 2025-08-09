namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents an OSPF neighbor
    /// </summary>
    public class OspfNeighbor
    {
        public string NeighborId { get; set; }
        public string IpAddress { get; set; }
        public string State { get; set; } = "INIT";
        public string Interface { get; set; }
        public int Priority { get; set; } = 1;
        public DateTime StateTime { get; set; } = DateTime.Now;
        
        public OspfNeighbor(string neighborId, string ipAddress, string interfaceName)
        {
            NeighborId = neighborId;
            IpAddress = ipAddress;
            Interface = interfaceName;
        }
    }
} 
