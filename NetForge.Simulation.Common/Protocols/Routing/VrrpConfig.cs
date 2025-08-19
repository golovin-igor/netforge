namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a VRRP configuration
    /// </summary>
    public class VrrpConfig
    {
        public Dictionary<int, VrrpGroup> Groups { get; set; } = new Dictionary<int, VrrpGroup>();
        public bool IsEnabled { get; set; }
        public int HelloInterval { get; set; } = 1; // Default hello interval in seconds
        public int DeadInterval { get; set; } = 3; // Default dead interval in seconds
        
        public VrrpConfig()
        {
            IsEnabled = true;
        }

        public void AddGroup(int groupId, string virtualIp, int priority, string interfaceName)
        {
            if (!Groups.ContainsKey(groupId))
            {
                Groups[groupId] = new VrrpGroup(groupId, virtualIp, priority, interfaceName);
            }
        }
    }

    public class VrrpGroup
    {
        public int GroupId { get; set; }
        public string VirtualIp { get; set; }
        public int Priority { get; set; } = 100; // Default priority
        public string Interface { get; set; }
        public string State { get; set; } = "Initialize"; // Initialize, Backup, Master
        public bool Preempt { get; set; } = true;
        public int PreemptDelay { get; set; } = 0;
        public int HelloInterval { get; set; } = 1; // Advertisement interval in seconds
        public int MasterDownInterval { get; set; } = 3; // Time to wait before becoming master
        public string AuthType { get; set; } = "none"; // none, simple, md5
        public string AuthKey { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public DateTime LastAdvertisement { get; set; } = DateTime.Now;
        public string MasterIp { get; set; } = "";
        public int Version { get; set; } = 2; // VRRP version (2 or 3)

        public VrrpGroup(int groupId, string virtualIp, int priority, string interfaceName)
        {
            GroupId = groupId;
            VirtualIp = virtualIp;
            Priority = priority;
            Interface = interfaceName;
        }
    }
} 
