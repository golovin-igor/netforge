namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a BGP configuration
    /// </summary>
    public class BgpConfig
    {
        public int LocalAs { get; set; }
        public string RouterId { get; set; }
        public Dictionary<string, BgpNeighbor> Neighbors { get; set; } = new Dictionary<string, BgpNeighbor>();
        public Dictionary<string, BgpNeighbor> Peers { get; set; } = new Dictionary<string, BgpNeighbor>();
        public Dictionary<string, BgpGroup> Groups { get; set; } = new Dictionary<string, BgpGroup>();
        public List<string> Networks { get; set; } = new List<string>();
        public bool IsEnabled { get; set; }
        
        public BgpConfig(int localAs)
        {
            LocalAs = localAs;
            RouterId = "";
            IsEnabled = true;
        }

        public void AddNeighbor(string ipAddress, int remoteAs, string description = "", bool isEnabled = true)
        {
            var neighbor = new BgpNeighbor(ipAddress, remoteAs, description, isEnabled);
            Neighbors[ipAddress] = neighbor;
            Peers[ipAddress] = neighbor;
        }
    }

    public class BgpNeighbor
    {
        public string IpAddress { get; set; }
        public int RemoteAs { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public int HoldTime { get; set; } = 180;
        public Dictionary<string, string> AddressFamilies { get; set; } = new Dictionary<string, string>();
        public List<string> ImportPolicies { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public string State { get; set; } = "Idle";
        public System.DateTime StateTime { get; set; } = System.DateTime.Now;
        public List<string> ReceivedRoutes { get; set; } = new List<string>();
        public int MessagesReceived { get; set; } = 0;
        public int MessagesSent { get; set; } = 0;
        public string UpdateSource { get; set; } = "";
        public bool IsActive { get; set; } = false;
        public string RouteMapIn { get; set; } = "";
        public string RouteMapOut { get; set; } = "";
        public bool SendCommunity { get; set; } = false;
        public bool SendCommunityExtended { get; set; } = false;
        public bool AdvertiseCommunity { get; set; } = false;
        public bool AdvertiseExtCommunity { get; set; } = false;

        public BgpNeighbor(string ipAddress, int remoteAs, string description = "", bool isEnabled = true)
        {
            IpAddress = ipAddress;
            RemoteAs = remoteAs;
            Description = description;
            IsEnabled = isEnabled;
            State = "Idle";
            StateTime = System.DateTime.Now;
            ReceivedRoutes = new List<string>();
            MessagesReceived = 0;
            MessagesSent = 0;
            UpdateSource = "";
            IsActive = false;
            RouteMapIn = "";
            RouteMapOut = "";
            SendCommunity = false;
            SendCommunityExtended = false;
            AdvertiseCommunity = false;
            AdvertiseExtCommunity = false;
        }
    }

    public class BgpGroup
    {
        public string Name { get; set; }
        public int RemoteAs { get; set; }
        public string Description { get; set; } = "";
        public List<string> Members { get; set; } = new List<string>();
        public List<string> ImportPolicies { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public int PeerAs { get; set; }
        public Dictionary<string, BgpNeighbor> Neighbors { get; set; } = new Dictionary<string, BgpNeighbor>();
        public string Type { get; set; } = "external";

        public BgpGroup(string name, int remoteAs)
        {
            Name = name;
            RemoteAs = remoteAs;
            PeerAs = remoteAs;
        }
    }
} 
