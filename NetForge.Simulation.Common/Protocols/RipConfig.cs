namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a RIP configuration
    /// </summary>
    public class RipConfig
    {
        public int Version { get; set; } = 2;
        public List<string> Networks { get; set; } = new List<string>();
        public List<RipNeighbor> Neighbors { get; set; } = new List<RipNeighbor>();
        public bool AutoSummary { get; set; } = false;
        public bool Summary { get; set; } = false;
        public Dictionary<string, RipGroup> Groups { get; set; } = new Dictionary<string, RipGroup>();
        public string AuthenticationType { get; set; } = "none";
        public bool IsEnabled { get; set; }

        public RipConfig(int version = 2)
        {
            Version = version;
            IsEnabled = true;
        }
    }

    public class RipGroup
    {
        public string Name { get; set; }
        public List<string> Members { get; set; } = new List<string>();
        public List<string> ImportPolicies { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public List<string> Neighbors { get; set; } = new List<string>();

        public RipGroup(string name)
        {
            Name = name;
        }
    }
}
