namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a RIP neighbor
    /// </summary>
    public class RipNeighbor
    {
        public string IpAddress { get; set; }
        public string Interface { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public List<RipRoute> Routes { get; set; } = new List<RipRoute>();

        public RipNeighbor(string ipAddress, string interfaceName)
        {
            IpAddress = ipAddress;
            Interface = interfaceName;
        }
    }
}
