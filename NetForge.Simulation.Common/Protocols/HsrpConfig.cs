namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents an HSRP configuration
    /// </summary>
    public class HsrpConfig
    {
        public Dictionary<int, HsrpGroup> Groups { get; set; } = new Dictionary<int, HsrpGroup>();
        public bool IsEnabled { get; set; }
        public int HelloInterval { get; set; } = 3; // Default hello interval in seconds
        public int HoldTime { get; set; } = 10; // Default hold time in seconds

        public HsrpConfig()
        {
            IsEnabled = true;
        }

        public void AddGroup(int groupId, string virtualIp, int priority, string interfaceName)
        {
            if (!Groups.ContainsKey(groupId))
            {
                Groups[groupId] = new HsrpGroup(groupId, virtualIp, priority, interfaceName);
            }
        }
    }

    public class HsrpGroup
    {
        public int GroupId { get; set; }
        public string VirtualIp { get; set; }
        public string VirtualMac { get; set; }
        public int Priority { get; set; } = 100; // Default priority
        public string Interface { get; set; }
        public string State { get; set; } = "Initial"; // Initial, Learn, Listen, Speak, Standby, Active
        public bool Preempt { get; set; } = false; // HSRP does not preempt by default
        public int PreemptDelay { get; set; } = 0;
        public int HelloInterval { get; set; } = 3; // Hello timer in seconds
        public int HoldTime { get; set; } = 10; // Hold timer in seconds
        public string AuthType { get; set; } = "none"; // none, text, md5
        public string AuthKey { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public DateTime LastHello { get; set; } = DateTime.Now;
        public string ActiveRouter { get; set; } = "";
        public string StandbyRouter { get; set; } = "";
        public int Version { get; set; } = 1; // HSRP version (1 or 2)
        public bool UseVia { get; set; } = false; // Use VIA (Virtual IP Address) learning
        public bool Track { get; set; } = false; // Interface tracking enabled

        public HsrpGroup(int groupId, string virtualIp, int priority, string interfaceName)
        {
            GroupId = groupId;
            VirtualIp = virtualIp;
            Priority = priority;
            Interface = interfaceName;
            // Generate virtual MAC address for HSRP group
            VirtualMac = $"00:00:0c:07:ac:{groupId:x2}"; // Cisco HSRP virtual MAC format
        }
    }
}
