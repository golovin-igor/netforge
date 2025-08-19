namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of an IS-IS protocol instance
    /// </summary>
    public class IsisState
    {
        /// <summary>
        /// Dictionary of IS-IS adjacencies indexed by system ID
        /// </summary>
        public Dictionary<string, IsisAdjacency> Adjacencies { get; set; } = new();
        
        /// <summary>
        /// Last time each adjacency was seen
        /// </summary>
        public Dictionary<string, DateTime> AdjacencyLastSeen { get; set; } = new();
        
        /// <summary>
        /// Track if topology has changed and SPF needs to be run
        /// </summary>
        public bool TopologyChanged { get; set; } = true;
        
        /// <summary>
        /// Last time SPF was calculated
        /// </summary>
        public DateTime LastSpfCalculation { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// IS-IS LSP database
        /// </summary>
        public Dictionary<string, IsisLsp> LspDatabase { get; set; } = new();
        
        /// <summary>
        /// IS-IS routing table
        /// </summary>
        public Dictionary<string, IsisRoute> RoutingTable { get; set; } = new();
        
        /// <summary>
        /// Interface states for each IS-IS-enabled interface
        /// </summary>
        public Dictionary<string, IsisInterfaceState> InterfaceStates { get; set; } = new();
        
        /// <summary>
        /// Sequence number for LSP generation
        /// </summary>
        public uint SequenceNumber { get; set; } = 1;
        
        /// <summary>
        /// Mark topology as changed (triggers SPF recalculation)
        /// </summary>
        public void MarkTopologyChanged()
        {
            TopologyChanged = true;
        }
        
        /// <summary>
        /// Get or create IS-IS adjacency
        /// </summary>
        public IsisAdjacency GetOrCreateAdjacency(string systemId, string interfaceName)
        {
            if (!Adjacencies.ContainsKey(systemId))
            {
                Adjacencies[systemId] = new IsisAdjacency(systemId, interfaceName);
            }
            return Adjacencies[systemId];
        }
        
        /// <summary>
        /// Remove IS-IS adjacency
        /// </summary>
        public void RemoveAdjacency(string systemId)
        {
            if (Adjacencies.Remove(systemId))
            {
                AdjacencyLastSeen.Remove(systemId);
                MarkTopologyChanged();
            }
        }
        
        /// <summary>
        /// Check for adjacencies that haven't been seen recently
        /// </summary>
        public List<string> GetStaleAdjacencies(int holdTime = 30)
        {
            var staleAdjacencies = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in AdjacencyLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > holdTime)
                {
                    staleAdjacencies.Add(kvp.Key);
                }
            }
            
            return staleAdjacencies;
        }
        
        /// <summary>
        /// Check for LSPs that have aged out
        /// </summary>
        public List<string> GetAgedLsps(int maxAge = 1200)
        {
            var agedLsps = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in LspDatabase)
            {
                if ((now - kvp.Value.Timestamp).TotalSeconds > maxAge)
                {
                    agedLsps.Add(kvp.Key);
                }
            }
            
            return agedLsps;
        }
    }
    
    /// <summary>
    /// Represents an IS-IS adjacency
    /// </summary>
    public class IsisAdjacency
    {
        public string SystemId { get; set; }
        public string InterfaceName { get; set; }
        public IsisAdjacencyState State { get; set; } = IsisAdjacencyState.Down;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public IsisLevel Level { get; set; } = IsisLevel.Level1;
        public DateTime LastHello { get; set; } = DateTime.MinValue;
        public int HoldTime { get; set; } = 30;
        public int Priority { get; set; } = 64;
        public bool IsDesignatedRouter { get; set; } = false;
        
        public IsisAdjacency(string systemId, string interfaceName)
        {
            SystemId = systemId;
            InterfaceName = interfaceName;
        }
        
        public void ChangeState(IsisAdjacencyState newState)
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
    /// IS-IS adjacency states
    /// </summary>
    public enum IsisAdjacencyState
    {
        Down,
        Initializing,
        Up
    }
    
    /// <summary>
    /// IS-IS levels
    /// </summary>
    public enum IsisLevel
    {
        Level1,
        Level2,
        Level1And2
    }
    
    /// <summary>
    /// Interface state for IS-IS
    /// </summary>
    public class IsisInterfaceState
    {
        public string InterfaceName { get; set; }
        public IsisLevel Level { get; set; } = IsisLevel.Level1And2;
        public bool IsEnabled { get; set; } = true;
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        public int HelloInterval { get; set; } = 10;
        public int HoldTime { get; set; } = 30;
        public int Priority { get; set; } = 64;
        public bool IsDesignatedRouter { get; set; } = false;
        
        public IsisInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
    }
    
    /// <summary>
    /// IS-IS LSP (Link State PDU)
    /// </summary>
    public class IsisLsp
    {
        public string LspId { get; set; }
        public string SystemId { get; set; }
        public uint SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Age { get; set; } = 0;
        public IsisLevel Level { get; set; } = IsisLevel.Level1;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        
        public IsisLsp(string lspId, string systemId, uint sequenceNumber)
        {
            LspId = lspId;
            SystemId = systemId;
            SequenceNumber = sequenceNumber;
        }
        
        /// <summary>
        /// Check if LSP has aged out
        /// </summary>
        public bool IsAgedOut(int maxAge = 1200)
        {
            return (DateTime.Now - Timestamp).TotalSeconds > maxAge;
        }
    }
    
    /// <summary>
    /// IS-IS route entry
    /// </summary>
    public class IsisRoute
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public int Metric { get; set; } = 10;
        public IsisLevel Level { get; set; } = IsisLevel.Level1;
        public IsisRouteType Type { get; set; } = IsisRouteType.Internal;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        public IsisRoute(string network, string subnetMask, string nextHop, string interfaceName)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
            Interface = interfaceName;
        }
    }
    
    /// <summary>
    /// IS-IS route types
    /// </summary>
    public enum IsisRouteType
    {
        Internal,
        External
    }
} 
