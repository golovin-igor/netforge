namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a route map configuration
    /// </summary>
    public class RouteMap
    {
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public string Action { get; set; } = "permit";
        public List<string> Statements { get; set; } = new List<string>();
        public List<string> MatchConditions { get; set; } = new List<string>();
        public List<string> SetActions { get; set; } = new List<string>();

        public RouteMap(string name, int sequenceNumber, string action = "permit")
        {
            Name = name;
            SequenceNumber = sequenceNumber;
            Action = action;
        }
    }
} 
