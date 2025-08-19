namespace NetForge.Simulation.Configuration
{
    /// <summary>
    /// Spanning Tree Protocol configuration
    /// </summary>
    public class StpConfig
    {
        public bool IsEnabled { get; set; }
        public string BridgeId { get; set; }
        public bool IsRoot { get; set; }
        public Dictionary<int, int> VlanPriorities { get; set; }
        public string Mode { get; set; }
        public int DefaultPriority { get; set; }
        public int HelloTime { get; set; } = 2;
        public int MaxAge { get; set; } = 20;
        public int ForwardDelay { get; set; } = 15;
        public int MessageAge { get; set; } = 0;

        public StpConfig()
        {
            IsEnabled = true;
            BridgeId = "";
            IsRoot = false;
            VlanPriorities = new Dictionary<int, int>();
            Mode = "mstp";
            DefaultPriority = 32768;
            HelloTime = 2;
            MaxAge = 20;
            ForwardDelay = 15;
            MessageAge = 0;
        }

        public int GetPriority(int vlan)
        {
            return VlanPriorities.ContainsKey(vlan) ? VlanPriorities[vlan] : DefaultPriority;
        }

        public void SetPriority(int vlan, int priority)
        {
            VlanPriorities[vlan] = priority;
        }
    }
} 
