namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a routing policy configuration
    /// </summary>
    public class RoutingPolicy(string name)
    {
        public string Name { get; set; } = name;
        public List<RoutingPolicyStatement> Statements { get; set; } = new List<RoutingPolicyStatement>();
        public List<string> MatchConditions { get; set; } = new List<string>();
        public List<string> SetActions { get; set; } = new List<string>();
        public Dictionary<string, object> Nodes { get; set; } = new Dictionary<string, object>();
        public List<string> Entries { get; set; } = new List<string>();
        public List<string> Terms { get; set; } = new List<string>();
    }

    public class RoutingPolicyStatement(string name)
    {
        public string Name { get; set; } = name;
        public string Action { get; set; } = "permit";
        public List<string> MatchConditions { get; set; } = new List<string>();
        public List<string> SetActions { get; set; } = new List<string>();
    }
}
