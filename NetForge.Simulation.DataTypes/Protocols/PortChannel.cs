namespace NetForge.Simulation.Common.Configuration
{
    /// <summary>
    /// Port channel configuration
    /// </summary>
    public class PortChannel(int id)
    {
        public int Id { get; set; } = id;
        public string Description { get; set; } = "";
        public bool IsUp { get; set; } = true;
        public List<string> MemberPorts { get; set; } = new();
        public List<string> MemberInterfaces { get; set; } = new();
        public string Mode { get; set; } = "on";
        public string Protocol { get; set; } = "lacp";
    }
}
