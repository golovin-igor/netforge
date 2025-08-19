namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of a BGP protocol instance
    /// </summary>
    public class BgpState
    {
        /// <summary>
        /// Dictionary of BGP peer sessions indexed by peer IP
        /// </summary>
        public Dictionary<string, BgpPeerSession> PeerSessions { get; set; } = new();
        
        /// <summary>
        /// Last time each peer was contacted
        /// </summary>
        public Dictionary<string, DateTime> PeerLastContact { get; set; } = new();
        
        /// <summary>
        /// Track if routing policy has changed
        /// </summary>
        public bool PolicyChanged { get; set; } = true;
        
        /// <summary>
        /// BGP RIB (Routing Information Base)
        /// </summary>
        public Dictionary<string, BgpRoute> Rib { get; set; } = new();
        
        /// <summary>
        /// BGP routes received from peers (Adj-RIB-In)
        /// </summary>
        public Dictionary<string, Dictionary<string, BgpRoute>> AdjRibIn { get; set; } = new();
        
        /// <summary>
        /// BGP routes advertised to peers (Adj-RIB-Out)
        /// </summary>
        public Dictionary<string, Dictionary<string, BgpRoute>> AdjRibOut { get; set; } = new();
        
        /// <summary>
        /// Last time route selection was run
        /// </summary>
        public DateTime LastRouteSelection { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Mark routing policy as changed
        /// </summary>
        public void MarkPolicyChanged()
        {
            PolicyChanged = true;
        }
        
        /// <summary>
        /// Get or create BGP peer session
        /// </summary>
        public BgpPeerSession GetOrCreatePeerSession(string peerIp, int peerAs)
        {
            if (!PeerSessions.ContainsKey(peerIp))
            {
                PeerSessions[peerIp] = new BgpPeerSession(peerIp, peerAs);
                AdjRibIn[peerIp] = new Dictionary<string, BgpRoute>();
                AdjRibOut[peerIp] = new Dictionary<string, BgpRoute>();
            }
            return PeerSessions[peerIp];
        }
        
        /// <summary>
        /// Remove BGP peer session
        /// </summary>
        public void RemovePeerSession(string peerIp)
        {
            if (PeerSessions.Remove(peerIp))
            {
                PeerLastContact.Remove(peerIp);
                AdjRibIn.Remove(peerIp);
                AdjRibOut.Remove(peerIp);
                MarkPolicyChanged();
            }
        }
        
        /// <summary>
        /// Check for peers that haven't been contacted recently
        /// </summary>
        public List<string> GetStalePeers(int holdTime = 180)
        {
            var stalePeers = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in PeerLastContact)
            {
                if ((now - kvp.Value).TotalSeconds > holdTime)
                {
                    stalePeers.Add(kvp.Key);
                }
            }
            
            return stalePeers;
        }
    }
    
    /// <summary>
    /// Represents a BGP peer session
    /// </summary>
    public class BgpPeerSession
    {
        public string PeerIp { get; set; }
        public int PeerAs { get; set; }
        public BgpSessionState State { get; set; } = BgpSessionState.Idle;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public int ConnectRetryCounter { get; set; } = 0;
        public int KeepaliveTimer { get; set; } = 60;
        public int HoldTimer { get; set; } = 180;
        public string LocalIp { get; set; } = string.Empty;
        public bool IsIbgp { get; set; } = false;
        
        public BgpPeerSession(string peerIp, int peerAs)
        {
            PeerIp = peerIp;
            PeerAs = peerAs;
        }
        
        public void ChangeState(BgpSessionState newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChangeTime = DateTime.Now;
            }
        }
        
        public TimeSpan GetTimeInCurrentState()
        {
            return DateTime.Now - StateChangeTime;
        }
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
    /// BGP route entry
    /// </summary>
    public class BgpRoute
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string Origin { get; set; } = "IGP";
        public List<int> AsPath { get; set; } = new();
        public int LocalPreference { get; set; } = 100;
        public int Med { get; set; } = 0;
        public bool IsValid { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string PeerIp { get; set; } = string.Empty;
        
        public BgpRoute(string network, string subnetMask, string nextHop)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
        }
        
        /// <summary>
        /// Calculate BGP route preference for best path selection
        /// </summary>
        public int CalculatePreference()
        {
            // Simplified BGP best path selection
            // In reality, this would be much more complex
            return LocalPreference * 1000 + (1000 - AsPath.Count * 100) - Med;
        }
    }
} 
