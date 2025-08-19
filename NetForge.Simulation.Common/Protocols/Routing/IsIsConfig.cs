namespace NetForge.Simulation.Protocols.Routing
{
    public class IsIsConfig
    {
        public string NetworkEntity { get; set; }
        public string SystemId { get; set; } = "";
        public string IsType { get; set; } = "level-2";
        public string IsLevel { get; set; } = "level-2";
        public string LevelCapability { get; set; } = "level-2";
        public Dictionary<string, IsIsInterface> Interfaces { get; set; } = new Dictionary<string, IsIsInterface>();
        public List<string> Areas { get; set; } = new List<string>();
        public bool IsEnabled { get; set; }
        public List<string> PassiveInterfaces { get; set; } = new List<string>();
        public bool Level1Enabled { get; set; } = false;

        public IsIsConfig()
        {
            NetworkEntity = "";
            IsEnabled = true;
        }
    }

    public class IsIsInterface
    {
        public string Name { get; set; }
        public string Type { get; set; } = "point-to-point";
        public bool Passive { get; set; } = false;
        public int Priority { get; set; } = 64;

        public IsIsInterface(string name)
        {
            Name = name;
        }
    }
} 
