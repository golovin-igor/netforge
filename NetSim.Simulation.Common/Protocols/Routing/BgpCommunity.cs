namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a BGP community configuration
    /// </summary>
    public class BgpCommunity
    {
        public string Name { get; set; }
        public List<string> Communities { get; set; } = new List<string>();
        public string Action { get; set; } = "permit";
        public List<string> Members { get; set; } = new List<string>();

        public BgpCommunity(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Represents an AS Path Group configuration
    /// </summary>
    public class AsPathGroup
    {
        public string Name { get; set; }
        public Dictionary<string, string> Paths { get; set; } = new Dictionary<string, string>();

        public AsPathGroup(string name)
        {
            Name = name;
        }
    }
} 
