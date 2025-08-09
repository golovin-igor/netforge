namespace NetSim.Simulation.Configuration
{
    /// <summary>
    /// Port channel configuration
    /// </summary>
    public class PortChannel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsUp { get; set; }
        public List<string> MemberPorts { get; set; }
        public List<string> MemberInterfaces { get; set; }
        public string Mode { get; set; }
        public string Protocol { get; set; }

        public PortChannel(int id)
        {
            Id = id;
            Description = "";
            IsUp = true;
            MemberPorts = new List<string>();
            MemberInterfaces = new List<string>();
            Mode = "on";
            Protocol = "lacp";
        }
    }
} 
