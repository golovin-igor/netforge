namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents the runtime state of an EIGRP protocol instance
    /// </summary>
    public class EigrpState
    {
        /// <summary>
        /// Dictionary of EIGRP neighbors indexed by neighbor IP
        /// </summary>
        public Dictionary<string, EigrpNeighborAdjacency> Neighbors { get; set; } = new();

        /// <summary>
        /// Last time each neighbor was seen (for hold timer)
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();

        /// <summary>
        /// Track if topology has changed and DUAL needs to be run
        /// </summary>
        public bool TopologyChanged { get; set; } = true;

        /// <summary>
        /// Last time DUAL was run
        /// </summary>
        public DateTime LastDualCalculation { get; set; } = DateTime.MinValue;

        /// <summary>
        /// EIGRP topology table
        /// </summary>
        public Dictionary<string, EigrpTopologyEntry> TopologyTable { get; set; } = new();

        /// <summary>
        /// EIGRP routing table (successor routes)
        /// </summary>
        public Dictionary<string, EigrpRoute> RoutingTable { get; set; } = new();

        /// <summary>
        /// Interface states for each EIGRP-enabled interface
        /// </summary>
        public Dictionary<string, EigrpInterfaceState> InterfaceStates { get; set; } = new();

        /// <summary>
        /// Sequence number for EIGRP updates
        /// </summary>
        public uint SequenceNumber { get; set; } = 1;

        /// <summary>
        /// Mark topology as changed (triggers DUAL recalculation)
        /// </summary>
        public void MarkTopologyChanged()
        {
            TopologyChanged = true;
        }

        /// <summary>
        /// Get or create EIGRP neighbor
        /// </summary>
        public EigrpNeighborAdjacency GetOrCreateNeighbor(string neighborIp, string interfaceName)
        {
            if (!Neighbors.ContainsKey(neighborIp))
            {
                Neighbors[neighborIp] = new EigrpNeighborAdjacency(neighborIp, interfaceName);
            }
            return Neighbors[neighborIp];
        }

        /// <summary>
        /// Remove EIGRP neighbor
        /// </summary>
        public void RemoveNeighbor(string neighborIp)
        {
            if (Neighbors.Remove(neighborIp))
            {
                NeighborLastSeen.Remove(neighborIp);
                MarkTopologyChanged();
            }
        }

        /// <summary>
        /// Check for neighbors that haven't been seen recently
        /// </summary>
        public List<string> GetStaleNeighbors(int holdTime = 15)
        {
            var staleNeighbors = new List<string>();
            var now = DateTime.Now;

            foreach (var kvp in NeighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > holdTime)
                {
                    staleNeighbors.Add(kvp.Key);
                }
            }

            return staleNeighbors;
        }
    }

    /// <summary>
    /// Represents an EIGRP neighbor adjacency with runtime state
    /// </summary>
    public class EigrpNeighborAdjacency
    {
        public string IpAddress { get; set; }
        public string InterfaceName { get; set; }
        public EigrpNeighborState State { get; set; } = EigrpNeighborState.Down;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public int HoldTime { get; set; } = 15;
        public DateTime LastHello { get; set; } = DateTime.MinValue;
        public uint SequenceNumber { get; set; } = 0;
        public bool IsBlocked { get; set; } = false;

        public EigrpNeighborAdjacency(string ipAddress, string interfaceName)
        {
            IpAddress = ipAddress;
            InterfaceName = interfaceName;
        }

        public void ChangeState(EigrpNeighborState newState)
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
    /// EIGRP neighbor states
    /// </summary>
    public enum EigrpNeighborState
    {
        Down,
        Pending,
        Up
    }

    /// <summary>
    /// Interface state for EIGRP
    /// </summary>
    public class EigrpInterfaceState
    {
        public string InterfaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        public int HelloInterval { get; set; } = 5;
        public int HoldTime { get; set; } = 15;

        public EigrpInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
    }

    /// <summary>
    /// EIGRP topology table entry
    /// </summary>
    public class EigrpTopologyEntry
    {
        public string Network { get; set; }
        public string SubnetMask { get; set; }
        public List<EigrpRoute> Routes { get; set; } = new();
        public EigrpRoute Successor { get; set; }
        public List<EigrpRoute> FeasibleSuccessors { get; set; } = new();
        public bool IsActive { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public EigrpTopologyEntry(string network, string subnetMask)
        {
            Network = network;
            SubnetMask = subnetMask;
        }
    }

    /// <summary>
    /// EIGRP route entry
    /// </summary>
    public class EigrpRoute
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
        public bool IsSuccessor { get; set; } = false;
        public bool IsFeasibleSuccessor { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public EigrpRoute(string network, string subnetMask, string nextHop, string interfaceName)
        {
            Network = network;
            SubnetMask = subnetMask;
            NextHop = nextHop;
            Interface = interfaceName;
        }

        /// <summary>
        /// Calculate EIGRP composite metric
        /// </summary>
        public int CalculateMetric(int k1 = 1, int k2 = 0, int k3 = 1, int k4 = 0, int k5 = 0)
        {
            // Standard EIGRP metric calculation
            int metric = 0;

            if (k5 == 0)
            {
                metric = (int)((k1 * Bandwidth + (k2 * Bandwidth) / (256 - Load) + k3 * Delay) * 256);
            }
            else
            {
                metric = (int)((k1 * Bandwidth + (k2 * Bandwidth) / (256 - Load) + k3 * Delay) * (k5 / (Reliability + k4)) * 256);
            }

            return metric;
        }
    }
}
