using NetForge.Simulation.Common;

namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of an OSPF protocol instance
    /// </summary>
    public class OspfState
    {
        /// <summary>
        /// Dictionary of neighbor adjacencies indexed by neighbor router ID
        /// </summary>
        public Dictionary<string, OspfNeighborAdjacency> Neighbors { get; set; } = new();
        
        /// <summary>
        /// Last time each neighbor was seen (for dead timer)
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();
        
        /// <summary>
        /// Track if topology has changed and SPF needs to be run
        /// </summary>
        public bool TopologyChanged { get; set; } = true;
        
        /// <summary>
        /// Last time SPF was calculated
        /// </summary>
        public DateTime LastSpfCalculation { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Current LSA database (simplified)
        /// </summary>
        public Dictionary<string, OspfLsa> LsaDatabase { get; set; } = new();
        
        /// <summary>
        /// Current routing table from last SPF calculation
        /// </summary>
        public Dictionary<string, OspfRoute> RoutingTable { get; set; } = new();
        
        /// <summary>
        /// Interface states for each OSPF-enabled interface
        /// </summary>
        public Dictionary<string, OspfInterfaceState> InterfaceStates { get; set; } = new();
        
        /// <summary>
        /// Sequence number for LSA generation
        /// </summary>
        public uint SequenceNumber { get; set; } = 0x80000001;
        
        /// <summary>
        /// Mark topology as changed (triggers SPF recalculation)
        /// </summary>
        public void MarkTopologyChanged()
        {
            TopologyChanged = true;
        }
        
        /// <summary>
        /// Get or create neighbor adjacency
        /// </summary>
        public OspfNeighborAdjacency GetOrCreateNeighbor(string neighborId, string ipAddress, string interfaceName)
        {
            if (!Neighbors.ContainsKey(neighborId))
            {
                Neighbors[neighborId] = new OspfNeighborAdjacency(neighborId, ipAddress, interfaceName);
            }
            return Neighbors[neighborId];
        }
        
        /// <summary>
        /// Remove neighbor adjacency
        /// </summary>
        public void RemoveNeighbor(string neighborId)
        {
            if (Neighbors.Remove(neighborId))
            {
                NeighborLastSeen.Remove(neighborId);
                MarkTopologyChanged();
            }
        }
        
        /// <summary>
        /// Check for dead neighbors based on hold time
        /// </summary>
        public List<string> GetDeadNeighbors(int holdTime = 40)
        {
            var deadNeighbors = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in NeighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > holdTime)
                {
                    deadNeighbors.Add(kvp.Key);
                }
            }
            
            return deadNeighbors;
        }
    }
    
    /// <summary>
    /// Represents an OSPF neighbor adjacency with state tracking
    /// </summary>
    public class OspfNeighborAdjacency
    {
        public string NeighborId { get; set; }
        public string IpAddress { get; set; }
        public string InterfaceName { get; set; }
        public OspfNeighborState State { get; set; } = OspfNeighborState.Init;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public int Priority { get; set; } = 1;
        public string DesignatedRouter { get; set; } = "0.0.0.0";
        public string BackupDesignatedRouter { get; set; } = "0.0.0.0";
        public bool IsDr { get; set; } = false;
        public bool IsBdr { get; set; } = false;
        
        public OspfNeighborAdjacency(string neighborId, string ipAddress, string interfaceName)
        {
            NeighborId = neighborId;
            IpAddress = ipAddress;
            InterfaceName = interfaceName;
        }
        
        public void ChangeState(OspfNeighborState newState)
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
    /// OSPF neighbor states according to RFC 2328
    /// </summary>
    public enum OspfNeighborState
    {
        Down,
        Init,
        TwoWay,
        ExStart,
        Exchange,
        Loading,
        Full
    }
    
    /// <summary>
    /// Interface state for OSPF
    /// </summary>
    public class OspfInterfaceState
    {
        public string InterfaceName { get; set; }
        public OspfInterfaceStateType State { get; set; } = OspfInterfaceStateType.Down;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public string DesignatedRouter { get; set; } = "0.0.0.0";
        public string BackupDesignatedRouter { get; set; } = "0.0.0.0";
        public int HelloInterval { get; set; } = 10;
        public int DeadInterval { get; set; } = 40;
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        
        public OspfInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
        
        public void ChangeState(OspfInterfaceStateType newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChangeTime = DateTime.Now;
            }
        }
    }
    
    /// <summary>
    /// OSPF interface states
    /// </summary>
    public enum OspfInterfaceStateType
    {
        Down,
        Loopback,
        Waiting,
        PointToPoint,
        DROther,
        Backup,
        DR
    }
    
    /// <summary>
    /// Simplified OSPF LSA for state tracking
    /// </summary>
    public class OspfLsa
    {
        public string Id { get; set; }
        public OspfLsaType Type { get; set; }
        public string AdvertisingRouter { get; set; }
        public uint SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Age { get; set; } = 0;
        
        public OspfLsa(string id, OspfLsaType type, string advertisingRouter, uint sequenceNumber)
        {
            Id = id;
            Type = type;
            AdvertisingRouter = advertisingRouter;
            SequenceNumber = sequenceNumber;
        }
    }
    
    /// <summary>
    /// OSPF LSA types
    /// </summary>
    public enum OspfLsaType
    {
        Router = 1,
        Network = 2,
        Summary = 3,
        ASBRSummary = 4,
        ASExternal = 5
    }
    
    /// <summary>
    /// OSPF route entry from SPF calculation
    /// </summary>
    public class OspfRoute
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string Interface { get; set; }
        public int Cost { get; set; }
        public OspfRouteType Type { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        public OspfRoute(string network, string subnetMask, string nextHop, string interfaceName, int cost, OspfRouteType type)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
            Interface = interfaceName;
            Cost = cost;
            Type = type;
        }
    }
    
    /// <summary>
    /// OSPF route types
    /// </summary>
    public enum OspfRouteType
    {
        IntraArea,
        InterArea,
        External1,
        External2
    }
} 
