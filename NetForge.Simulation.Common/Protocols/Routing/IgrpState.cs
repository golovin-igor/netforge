namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of an IGRP protocol instance
    /// </summary>
    public class IgrpState
    {
        /// <summary>
        /// Dictionary of IGRP neighbors indexed by neighbor IP
        /// </summary>
        public Dictionary<string, IgrpNeighborAdjacency> Neighbors { get; set; } = new();
        
        /// <summary>
        /// Dictionary of IGRP adjacencies indexed by neighbor IP (alias for Neighbors)
        /// </summary>
        public Dictionary<string, IgrpNeighborAdjacency> Adjacencies => Neighbors;
        
        /// <summary>
        /// Last time each neighbor was seen
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();
        
        /// <summary>
        /// Last time routes were calculated
        /// </summary>
        public DateTime LastRouteCalculation { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Track if routes have changed
        /// </summary>
        public bool RoutesChanged { get; set; } = true;
        
        /// <summary>
        /// Last time routes were advertised
        /// </summary>
        public DateTime LastAdvertisement { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// IGRP routing table
        /// </summary>
        public Dictionary<string, IgrpRouteEntry> Routes { get; set; } = new();
        
        /// <summary>
        /// Interface states for each IGRP-enabled interface
        /// </summary>
        public Dictionary<string, IgrpInterfaceState> InterfaceStates { get; set; } = new();
        
        /// <summary>
        /// Mark routes as changed
        /// </summary>
        public void MarkRoutesChanged()
        {
            RoutesChanged = true;
        }
        
        /// <summary>
        /// Get or create IGRP neighbor
        /// </summary>
        public IgrpNeighborAdjacency GetOrCreateNeighbor(string neighborIp, string interfaceName)
        {
            if (!Neighbors.ContainsKey(neighborIp))
            {
                Neighbors[neighborIp] = new IgrpNeighborAdjacency(neighborIp, interfaceName);
            }
            return Neighbors[neighborIp];
        }
        
        /// <summary>
        /// Get or create IGRP adjacency (alias for GetOrCreateNeighbor)
        /// </summary>
        public IgrpNeighborAdjacency GetOrCreateAdjacency(string neighborIp, string interfaceName)
        {
            return GetOrCreateNeighbor(neighborIp, interfaceName);
        }
        
        /// <summary>
        /// Remove IGRP neighbor
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
        /// Remove IGRP adjacency (alias for RemoveNeighbor)
        /// </summary>
        public void RemoveAdjacency(string neighborIp)
        {
            RemoveNeighbor(neighborIp);
        }
        
        /// <summary>
        /// Check for neighbors that haven't been seen recently
        /// </summary>
        public List<string> GetStaleNeighbors(int timeout = 270)
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
        public List<string> GetTimedOutRoutes(int timeout = 270)
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
        public List<string> GetFlushableRoutes(int flushTime = 630)
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
    /// Represents an IGRP neighbor adjacency with runtime state
    /// </summary>
    public class IgrpNeighborAdjacency
    {
        public string IpAddress { get; set; }
        public string InterfaceName { get; set; }
        public string NeighborId { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public int UpdatesReceived { get; set; } = 0;
        public int UpdatesSent { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public IgrpNeighborState State { get; set; } = IgrpNeighborState.Down;
        
        public IgrpNeighborAdjacency(string ipAddress, string interfaceName)
        {
            IpAddress = ipAddress;
            InterfaceName = interfaceName;
            NeighborId = ipAddress; // Use IP address as neighbor ID for simplicity
        }
        
        public TimeSpan GetTimeSinceLastUpdate()
        {
            return DateTime.Now - LastUpdate;
        }
        
        public bool HasTimedOut(int timeout = 270)
        {
            return GetTimeSinceLastUpdate().TotalSeconds > timeout;
        }
        
        /// <summary>
        /// Change the state of the neighbor
        /// </summary>
        public void ChangeState(IgrpNeighborState newState)
        {
            if (State != newState)
            {
                State = newState;
                LastUpdate = DateTime.Now;
            }
        }
    }
    
    /// <summary>
    /// Interface state for IGRP
    /// </summary>
    public class IgrpInterfaceState
    {
        public string InterfaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastUpdateSent { get; set; } = DateTime.MinValue;
        public int UpdateInterval { get; set; } = 90;
        public bool SplitHorizon { get; set; } = true;
        
        public IgrpInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
        
        public bool ShouldSendUpdate()
        {
            return (DateTime.Now - LastUpdateSent).TotalSeconds >= UpdateInterval;
        }
    }
    
    /// <summary>
    /// IGRP route entry with timers
    /// </summary>
    public class IgrpRouteEntry
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public int Delay { get; set; } = 1000;
        public int Bandwidth { get; set; } = 1000000;
        public int Reliability { get; set; } = 255;
        public int Load { get; set; } = 1;
        public int Mtu { get; set; } = 1500;
        public int HopCount { get; set; } = 0;
        public int Metric { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        public IgrpRouteState State { get; set; } = IgrpRouteState.Valid;
        public string Source { get; set; } = "connected";
        public bool IsChanged { get; set; } = true;
        
        public IgrpRouteEntry(string network, string subnetMask, string nextHop, string interfaceName)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
            Interface = interfaceName;
        }
        
        /// <summary>
        /// Calculate IGRP composite metric
        /// </summary>
        public int CalculateMetric()
        {
            // IGRP metric calculation: (Bandwidth + Delay) * 24
            return (Bandwidth + Delay) * 24;
        }
        
        /// <summary>
        /// Update route information
        /// </summary>
        public void UpdateRoute(string nextHop, string interfaceName, int delay, int bandwidth, int reliability, int load, int mtu, int hopCount)
        {
            if (NextHop != nextHop || Interface != interfaceName || Delay != delay || Bandwidth != bandwidth)
            {
                NextHop = nextHop;
                Interface = interfaceName;
                Delay = delay;
                Bandwidth = bandwidth;
                Reliability = reliability;
                Load = load;
                Mtu = mtu;
                HopCount = hopCount;
                Metric = CalculateMetric();
                LastUpdated = DateTime.Now;
                IsChanged = true;
                State = IgrpRouteState.Valid;
            }
        }
        
        /// <summary>
        /// Mark route as invalid
        /// </summary>
        public void MarkInvalid()
        {
            State = IgrpRouteState.Invalid;
            IsChanged = true;
        }
        
        /// <summary>
        /// Check if route has timed out
        /// </summary>
        public bool IsTimedOut(int timeout = 270)
        {
            return (DateTime.Now - LastUpdated).TotalSeconds > timeout;
        }
        
        /// <summary>
        /// Check if route should be flushed
        /// </summary>
        public bool ShouldFlush(int flushTime = 630)
        {
            return (DateTime.Now - LastUpdated).TotalSeconds > flushTime;
        }
    }
    
    /// <summary>
    /// IGRP route states
    /// </summary>
    /// <summary>
    /// Simple IGRP route representation (alias for IgrpRouteEntry)
    /// </summary>
    public class IgrpRoute
    {
        public string Network { get; set; }
        public string Mask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public int Metric { get; set; }
        public int HopCount { get; set; }
        public IgrpRouteState State { get; set; } = IgrpRouteState.Valid;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        
        public IgrpRoute(string network, string mask, string nextHop, string interfaceName, int metric)
        {
            Network = network;
            Mask = mask;
            NextHop = nextHop;
            Interface = interfaceName;
            Metric = metric;
            HopCount = 1;
        }
        
        /// <summary>
        /// Convert to IgrpRouteEntry
        /// </summary>
        public IgrpRouteEntry ToRouteEntry()
        {
            return new IgrpRouteEntry(Network, Mask, NextHop, Interface)
            {
                Metric = Metric,
                HopCount = HopCount,
                State = State,
                LastUpdateTime = LastUpdateTime
            };
        }
    }
    
    /// <summary>
    /// IGRP neighbor states
    /// </summary>
    public enum IgrpNeighborState
    {
        Down,
        Up,
        Pending,
        Initializing
    }
    
    /// <summary>
    /// IGRP route states
    /// </summary>
    public enum IgrpRouteState
    {
        Valid,
        Invalid,
        Holddown,
        Flushing
    }
} 
