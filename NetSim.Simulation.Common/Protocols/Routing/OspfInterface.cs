namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents an OSPF interface configuration
    /// </summary>
    public class OspfInterface
    {
        public string Name { get; set; }
        public int Area { get; set; }
        public int Cost { get; set; } = 10;
        public int Priority { get; set; } = 1;
        public string NetworkType { get; set; } = "broadcast";
        public string InterfaceType { get; set; } = "broadcast";
        
        public OspfInterface(string name, int area)
        {
            Name = name;
            Area = area;
        }
    }
} 
