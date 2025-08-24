namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a route in the routing table
    /// </summary>
    public class Route
    {
        public string Network { get; set; }
        public string Mask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public string Protocol { get; set; } // Connected, Static, OSPF, BGP, RIP
        public int Metric { get; set; }
        public int AdminDistance { get; set; }

        public Route(string network, string mask, string nextHop, string interfaceName, string protocol)
        {
            Network = network;
            Mask = mask;
            NextHop = nextHop;
            Interface = interfaceName;
            Protocol = protocol;

            // Set default administrative distances
            AdminDistance = protocol switch
            {
                "Connected" => 0,
                "Static" => 1,
                "OSPF" => 110,
                "RIP" => 120,
                "BGP" => 20,
                _ => 255
            };
        }
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
