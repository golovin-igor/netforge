namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of a RIP protocol instance
    /// </summary>
    public class RipState
    {
        /// <summary>
        /// Dictionary of RIP routes indexed by network/mask
        /// </summary>
        public Dictionary<string, RipRouteEntry> Routes { get; set; } = new();
        
        /// <summary>
        /// Dictionary of RIP neighbors indexed by neighbor IP
        /// </summary>
        public Dictionary<string, RipNeighborAdjacency> Neighbors { get; set; } = new();
        
        /// <summary>
        /// Last time each neighbor was seen
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();
        
        /// <summary>
        /// Track if routes have changed
        /// </summary>
        public bool RoutesChanged { get; set; } = true;
        
        /// <summary>
        /// Last time routes were advertised
        /// </summary>
        public DateTime LastAdvertisement { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Interface states for each RIP-enabled interface
        /// </summary>
        public Dictionary<string, RipInterfaceState> InterfaceStates { get; set; } = new();
        
        /// <summary>
        /// Mark routes as changed (triggers route advertisement)
        /// </summary>
        public void MarkRoutesChanged()
        {
            RoutesChanged = true;
        }
        
        /// <summary>
        /// Get or create RIP neighbor
        /// </summary>
        public RipNeighborAdjacency GetOrCreateNeighbor(string neighborIp, string interfaceName)
        {
            if (!Neighbors.ContainsKey(neighborIp))
            {
                Neighbors[neighborIp] = new RipNeighborAdjacency(neighborIp, interfaceName);
            }
            return Neighbors[neighborIp];
        }
        
        /// <summary>
        /// Remove RIP neighbor
        /// </summary>
        public void RemoveNeighbor(string neighborIp)
        {
            if (Neighbors.Remove(neighborIp))
            {
                NeighborLastSeen.Remove(neighborIp);
                MarkRoutesChanged();
            }
        }
        
        /// <summary>
        /// Check for neighbors that haven't been seen recently
        /// </summary>
        public List<string> GetStaleNeighbors(int timeout = 180)
        {
            var staleNeighbors = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in NeighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > timeout)
                {
                    staleNeighbors.Add(kvp.Key);
                }
            }
            
            return staleNeighbors;
        }
        
        /// <summary>
        /// Check for routes that have timed out
        /// </summary>
        public List<string> GetTimedOutRoutes(int timeout = 180)
        {
            var timedOutRoutes = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in Routes)
            {
                if ((now - kvp.Value.LastUpdated).TotalSeconds > timeout)
                {
                    timedOutRoutes.Add(kvp.Key);
                }
            }
            
            return timedOutRoutes;
        }
        
        /// <summary>
        /// Check for routes that should be flushed
        /// </summary>
        public List<string> GetFlushableRoutes(int flushTime = 240)
        {
            var flushableRoutes = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in Routes)
            {
                if ((now - kvp.Value.LastUpdated).TotalSeconds > flushTime)
                {
                    flushableRoutes.Add(kvp.Key);
                }
            }
            
            return flushableRoutes;
        }
    }
    
    /// <summary>
    /// Represents a RIP neighbor adjacency
    /// </summary>
    public class RipNeighborAdjacency
    {
        public string IpAddress { get; set; }
        public string InterfaceName { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public int UpdatesReceived { get; set; } = 0;
        public int UpdatesSent { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        
        public RipNeighborAdjacency(string ipAddress, string interfaceName)
        {
            IpAddress = ipAddress;
            InterfaceName = interfaceName;
        }
        
        public TimeSpan GetTimeSinceLastUpdate()
        {
            return DateTime.Now - LastUpdate;
        }
    }
    
    /// <summary>
    /// Interface state for RIP
    /// </summary>
    public class RipInterfaceState
    {
        public string InterfaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastUpdateSent { get; set; } = DateTime.MinValue;
        public int UpdateInterval { get; set; } = 30;
        public bool SplitHorizon { get; set; } = true;
        public bool PoisonReverse { get; set; } = false;
        
        public RipInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
    }
    
    /// <summary>
    /// RIP route entry with timers
    /// </summary>
    public class RipRouteEntry
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public int Metric { get; set; } = 1;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public RipRouteState State { get; set; } = RipRouteState.Valid;
        public string Source { get; set; } = "connected";
        public bool IsChanged { get; set; } = true;
        
        public RipRouteEntry(string network, string subnetMask, string nextHop, string interfaceName)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
            Interface = interfaceName;
        }
        
        /// <summary>
        /// Update route information
        /// </summary>
        public void UpdateRoute(string nextHop, string interfaceName, int metric)
        {
            if (NextHop != nextHop || Interface != interfaceName || Metric != metric)
            {
                NextHop = nextHop;
                Interface = interfaceName;
                Metric = metric;
                LastUpdated = DateTime.Now;
                IsChanged = true;
                State = RipRouteState.Valid;
            }
        }
        
        /// <summary>
        /// Mark route as invalid (holddown)
        /// </summary>
        public void MarkInvalid()
        {
            State = RipRouteState.Invalid;
            Metric = 16; // RIP infinity
            IsChanged = true;
        }
        
        /// <summary>
        /// Check if route has timed out
        /// </summary>
        public bool IsTimedOut(int timeout = 180)
        {
            return (DateTime.Now - LastUpdated).TotalSeconds > timeout;
        }
        
        /// <summary>
        /// Check if route should be flushed
        /// </summary>
        public bool ShouldFlush(int flushTime = 240)
        {
            return (DateTime.Now - LastUpdated).TotalSeconds > flushTime;
        }
    }
    
    /// <summary>
    /// RIP route states
    /// </summary>
    public enum RipRouteState
    {
        Valid,
        Invalid,
        Holddown,
        Flushing
    }
} 
