namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a RIP route
    /// </summary>
    public class RipRoute
    {
        public string Network { get; set; }
        public string Mask { get; set; }
        public int Metric { get; set; }
        public string NextHop { get; set; }

        public RipRoute(string network, string mask, int metric, string nextHop)
        {
            Network = network;
            Mask = mask;
            Metric = metric;
            NextHop = nextHop;
        }
    }
}
