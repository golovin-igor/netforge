namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a BGP community configuration
    /// </summary>
    public class BgpCommunity(string name)
    {
        public string Name { get; set; } = name;
        public List<string> Communities { get; set; } = new List<string>();
        public string Action { get; set; } = "permit";
        public List<string> Members { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents an AS Path Group configuration
    /// </summary>
    public class AsPathGroup(string name)
    {
        public string Name { get; set; } = name;
        public Dictionary<string, string> Paths { get; set; } = new Dictionary<string, string>();
    }
}
