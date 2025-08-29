namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a route in the routing table
    /// </summary>
    public class Route(string network, string mask, string nextHop, string interfaceName, string protocol)
    {
        public string Network { get; set; } = network;
        public string Mask { get; set; } = mask;
        public string NextHop { get; set; } = nextHop;
        public string Interface { get; set; } = interfaceName;
        public string Protocol { get; set; } = protocol; // Connected, Static, OSPF, BGP, RIP
        public int Metric { get; set; }
        public int AdminDistance { get; set; } = protocol switch
        {
            "Connected" => 0,
            "Static" => 1,
            "OSPF" => 110,
            "RIP" => 120,
            "BGP" => 20,
            _ => 255
        };

        // Set default administrative distances
    }

     /// <summary>
    /// Represents a static route entry during configuration
    /// </summary>
    public class StaticRouteEntry
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string Gateway { get; set; } = "";
        public string Device { get; set; } = "";
    }
}
