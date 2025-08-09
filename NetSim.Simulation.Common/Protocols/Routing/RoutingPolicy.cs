namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a routing policy configuration
    /// </summary>
    public class RoutingPolicy
    {
        public string Name { get; set; }
        public List<RoutingPolicyStatement> Statements { get; set; } = new List<RoutingPolicyStatement>();
        public List<string> MatchConditions { get; set; } = new List<string>();
        public List<string> SetActions { get; set; } = new List<string>();
        public Dictionary<string, object> Nodes { get; set; } = new Dictionary<string, object>();
        public List<string> Entries { get; set; } = new List<string>();
        public List<string> Terms { get; set; } = new List<string>();

        public RoutingPolicy(string name)
        {
            Name = name;
        }
    }

    public class RoutingPolicyStatement
    {
        public string Name { get; set; }
        public string Action { get; set; } = "permit";
        public List<string> MatchConditions { get; set; } = new List<string>();
        public List<string> SetActions { get; set; } = new List<string>();

        public RoutingPolicyStatement(string name)
        {
            Name = name;
        }
    }
} 
