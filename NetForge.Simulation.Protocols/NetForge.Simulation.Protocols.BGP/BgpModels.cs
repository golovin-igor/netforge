using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Protocols.BGP
{
    /// <summary>
    /// BGP protocol state following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class BgpState : BaseProtocolState
    {
        public int LocalAs { get; set; } = 0;
        public string RouterId { get; set; } = "";
        public Dictionary<string, BgpPeer> Peers { get; set; } = new();
        
        /// <summary>
        /// Access to neighbors collection for base protocol compatibility
        /// </summary>
        public Dictionary<string, object> Neighbors => _neighbors;
        public Dictionary<string, BgpRouteEntry> BestRoutes { get; set; } = new();
        public List<BgpRouteEntry> AdvertisedRoutes { get; set; } = new();
        public bool PolicyChanged { get; set; } = true;
        public DateTime LastRouteSelection { get; set; } = DateTime.MinValue;
        public int RouteSelectionCount { get; set; } = 0;
        
        /// <summary>
        /// Get or create BGP peer with type safety
        /// </summary>
        public BgpPeer GetOrCreateBgpPeer(string peerKey, Func<BgpPeer> factory)
        {
            return GetOrCreateNeighbor<BgpPeer>(peerKey, factory);
        }
        
        /// <summary>
        /// Mark routing policy as changed to trigger route selection
        /// </summary>
        public void MarkPolicyChanged()
        {
            PolicyChanged = true;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Record successful route selection
        /// </summary>
        public void RecordRouteSelection()
        {
            LastRouteSelection = DateTime.Now;
            RouteSelectionCount++;
            PolicyChanged = false;
        }
        
        /// <summary>
        /// Check if route selection is needed
        /// </summary>
        public bool ShouldRunRouteSelection()
        {
            return PolicyChanged || (DateTime.Now - LastRouteSelection).TotalMinutes > 5;
        }
        
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["LocalAs"] = LocalAs;
            baseData["RouterId"] = RouterId;
            baseData["Peers"] = Peers;
            baseData["BestRoutes"] = BestRoutes;
            baseData["AdvertisedRoutes"] = AdvertisedRoutes;
            baseData["PolicyChanged"] = PolicyChanged;
            baseData["LastRouteSelection"] = LastRouteSelection;
            baseData["RouteSelectionCount"] = RouteSelectionCount;
            return baseData;
        }
    }
    
    /// <summary>
    /// Represents a BGP peer session with enhanced state tracking
    /// </summary>
    public class BgpPeer
    {
        public string PeerIp { get; set; } = "";
        public int PeerAs { get; set; } = 0;
        public bool IsIbgp { get; set; } = false;
        public string State { get; set; } = "Idle";
        public DateTime StateTime { get; set; } = DateTime.Now;
        public int HoldTime { get; set; } = 180;
        public int KeepaliveTime { get; set; } = 60;
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = "";
        public int AdvertisedRouteCount { get; set; } = 0;
        public int ReceivedRouteCount { get; set; } = 0;
        public int UpdatesSent { get; set; } = 0;
        public int UpdatesReceived { get; set; } = 0;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public List<string> AddressFamilies { get; set; } = new() { "IPv4-Unicast" };
        
        public BgpPeer(string peerIp, int peerAs, bool isIbgp)
        {
            PeerIp = peerIp;
            PeerAs = peerAs;
            IsIbgp = isIbgp;
        }
        
        /// <summary>
        /// Get time spent in current state
        /// </summary>
        public TimeSpan GetTimeInCurrentState()
        {
            return DateTime.Now - StateTime;
        }
        
        /// <summary>
        /// Check if peer session is established
        /// </summary>
        public bool IsEstablished => State == "Established";
        
        /// <summary>
        /// Get peer type description
        /// </summary>
        public string PeerType => IsIbgp ? "IBGP" : "EBGP";
    }
    
    /// <summary>
    /// Represents a BGP route entry in the RIB
    /// </summary>
    public class BgpRouteEntry
    {
        public string Network { get; set; } = "";
        public int PrefixLength { get; set; } = 0;
        public string NextHop { get; set; } = "";
        public List<int> AsPath { get; set; } = new();
        public int LocalPreference { get; set; } = 100;
        public int Med { get; set; } = 0; // Multi-Exit Discriminator
        public string Origin { get; set; } = "IGP"; // IGP, EGP, INCOMPLETE
        public List<string> Communities { get; set; } = new();
        public bool IsValid { get; set; } = true;
        public bool IsBest { get; set; } = false;
        public string PeerIp { get; set; } = "";
        public DateTime ReceivedTime { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Get route key for identification
        /// </summary>
        public string RouteKey => $"{Network}/{PrefixLength}";
        
        /// <summary>
        /// Get AS path length for best path selection
        /// </summary>
        public int AsPathLength => AsPath.Count;
        
        /// <summary>
        /// Check if route is from EBGP peer
        /// </summary>
        public bool IsEbgpRoute => AsPath.Count > 0;
        
        /// <summary>
        /// Get route age in seconds
        /// </summary>
        public int AgeInSeconds => (int)(DateTime.Now - ReceivedTime).TotalSeconds;
        
        /// <summary>
        /// Create AS path string representation
        /// </summary>
        public string AsPathString => string.Join(" ", AsPath);
    }
    
    /// <summary>
    /// BGP session states according to RFC 4271
    /// </summary>
    public enum BgpSessionState
    {
        Idle,
        Connect,
        Active,
        OpenSent,
        OpenConfirm,
        Established
    }
    
    /// <summary>
    /// BGP message types
    /// </summary>
    public enum BgpMessageType
    {
        Open = 1,
        Update = 2,
        Notification = 3,
        Keepalive = 4,
        RouteRefresh = 5
    }
    
    /// <summary>
    /// BGP route origin types
    /// </summary>
    public enum BgpOriginType
    {
        IGP = 0,
        EGP = 1,
        Incomplete = 2
    }
    
    /// <summary>
    /// BGP well-known communities
    /// </summary>
    public static class BgpCommunities
    {
        public const string NoExport = "65535:65281";
        public const string NoAdvertise = "65535:65282";
        public const string NoExportSubconfed = "65535:65283";
        public const string PlannedShutdown = "65535:0";
    }
    
    /// <summary>
    /// BGP address family identifiers
    /// </summary>
    public enum BgpAddressFamily
    {
        IPv4Unicast = 1,
        IPv6Unicast = 2,
        IPv4Multicast = 3,
        IPv6Multicast = 4,
        IPv4Labeled = 5,
        IPv6Labeled = 6,
        VpnIPv4 = 128,
        VpnIPv6 = 129
    }
}
