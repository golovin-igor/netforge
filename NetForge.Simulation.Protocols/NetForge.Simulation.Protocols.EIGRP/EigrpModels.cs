using System;
using System.Collections.Generic;
using NetForge.Simulation.Protocols.Common;
// Use the existing EigrpConfig from Common project
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Protocols.EIGRP
{
    // Extended EIGRP configuration for advanced features (compatible with existing EigrpConfig)
    public class EigrpAdvancedConfig : EigrpConfig
    {
        public int HelloInterval { get; set; } = 5; // seconds
        public int HoldTime { get; set; } = 15; // seconds
        public int Variance { get; set; } = 1;
        public int MaxPaths { get; set; } = 4;
        public Dictionary<string, EigrpInterfaceConfig> Interfaces { get; set; } = new();
        
        public EigrpAdvancedConfig(int asNumber) : base(asNumber)
        {
        }
    }

    public class EigrpNetworkConfig
    {
        public string Network { get; set; } = "";
        public string WildcardMask { get; set; } = "";
        public bool IsAdvertised { get; set; } = true;
        public bool AutoSummary { get; set; } = true;
    }

    public class EigrpInterfaceConfig
    {
        public string InterfaceName { get; set; } = "";
        public bool IsPassive { get; set; } = false;
        public int HelloInterval { get; set; } = 5;
        public int HoldTime { get; set; } = 15;
        public int Bandwidth { get; set; } = 1544; // kbps
        public int Delay { get; set; } = 20000; // microseconds
        public int Reliability { get; set; } = 255;
        public int Load { get; set; } = 1;
        public int Mtu { get; set; } = 1500;
        public Dictionary<string, string> AuthenticationKeys { get; set; } = new();
    }

    // EIGRP Protocol State
    public class EigrpState : BaseProtocolState
    {
        public string RouterId { get; set; } = "";
        public int AsNumber { get; set; } = 1;
        public Dictionary<string, EigrpNeighbor> Neighbors { get; set; } = new();
        public Dictionary<string, EigrpTopologyEntry> TopologyTable { get; set; } = new();
        public Dictionary<string, EigrpRoute> RoutingTable { get; set; } = new();
        public List<EigrpRoute> CalculatedRoutes { get; set; } = new();
        public bool TopologyChanged { get; set; } = true;
        public Dictionary<string, DateTime> InterfaceTimers { get; set; } = new();
        public long SequenceNumber { get; set; } = 1;
        public Dictionary<string, EigrpQuery> ActiveQueries { get; set; } = new();

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["RouterId"] = RouterId;
            baseData["AsNumber"] = AsNumber;
            baseData["Neighbors"] = Neighbors;
            baseData["TopologyTable"] = TopologyTable;
            baseData["RoutingTable"] = RoutingTable;
            baseData["TopologyChanged"] = TopologyChanged;
            baseData["ActiveQueries"] = ActiveQueries.Count;
            return baseData;
        }

        public EigrpNeighbor GetOrCreateNeighbor(string neighborId, Func<EigrpNeighbor> factory)
        {
            if (!Neighbors.ContainsKey(neighborId))
            {
                Neighbors[neighborId] = factory();
                MarkStateChanged();
            }
            return Neighbors[neighborId];
        }

        public override void RemoveNeighbor(string neighborId)
        {
            if (Neighbors.Remove(neighborId))
            {
                // Remove topology entries via this neighbor
                var toRemove = new List<string>();
                foreach (var kvp in TopologyTable)
                {
                    if (kvp.Value.ViaNeighbor == neighborId)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
                
                foreach (var key in toRemove)
                {
                    TopologyTable.Remove(key);
                }
                
                MarkStateChanged();
                TopologyChanged = true;
            }
        }
    }

    // EIGRP Neighbor
    public class EigrpNeighbor
    {
        public string RouterId { get; set; } = "";
        public string InterfaceName { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public EigrpNeighborState State { get; set; } = EigrpNeighborState.Down;
        public DateTime LastHello { get; set; } = DateTime.Now;
        public int HoldTime { get; set; } = 15;
        public long SequenceNumber { get; set; } = 0;
        public int Srtt { get; set; } = 0; // Smooth Round Trip Time
        public int Rto { get; set; } = 5000; // Retransmission Timeout
        public Dictionary<string, object> Capabilities { get; set; } = new();
        public int AsNumber { get; set; } = 1;
        public Queue<EigrpPacket> ReliableQueue { get; set; } = new();
    }

    public enum EigrpNeighborState
    {
        Down,
        Pending,
        Up
    }

    // EIGRP Topology Entry
    public class EigrpTopologyEntry
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string ViaNeighbor { get; set; } = "";
        public string NextHop { get; set; } = "";
        public string Interface { get; set; } = "";
        public long FeasibleDistance { get; set; } = long.MaxValue;
        public long ReportedDistance { get; set; } = long.MaxValue;
        public EigrpMetric Metric { get; set; } = new();
        public EigrpRouteState RouteState { get; set; } = EigrpRouteState.Passive;
        public bool IsSuccessor { get; set; } = false;
        public bool IsFeasibleSuccessor { get; set; } = false;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public int QueryCount { get; set; } = 0;
    }

    public enum EigrpRouteState
    {
        Passive,
        Active0, // Active, no replies outstanding
        Active1, // Active, replies outstanding
        Active2, // Active, SIA (Stuck in Active) timer running
        Active3  // Active, SIA condition detected
    }

    // EIGRP Route
    public class EigrpRoute
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string NextHop { get; set; } = "";
        public string Interface { get; set; } = "";
        public long Metric { get; set; } = 0;
        public EigrpMetric CompositeMetric { get; set; } = new();
        public int AdminDistance { get; set; } = 90;
        public string RouteSource { get; set; } = "EIGRP";
        public DateTime InstallTime { get; set; } = DateTime.Now;
        public bool IsInternal { get; set; } = true;
        public string Tag { get; set; } = "";
    }

    // EIGRP Metric Components
    public class EigrpMetric
    {
        public int Bandwidth { get; set; } = 1544; // kbps
        public int Delay { get; set; } = 20000; // microseconds
        public int Reliability { get; set; } = 255; // 0-255
        public int Load { get; set; } = 1; // 0-255
        public int Mtu { get; set; } = 1500;
        public int HopCount { get; set; } = 0;
        
        // K-values for metric calculation
        public int K1 { get; set; } = 1; // Bandwidth
        public int K2 { get; set; } = 0; // Load
        public int K3 { get; set; } = 1; // Delay
        public int K4 { get; set; } = 0; // Reliability
        public int K5 { get; set; } = 0; // MTU

        public long CalculateCompositeMetric()
        {
            // EIGRP Composite Metric = 256 * ((K1 * Bandwidth) + ((K2 * Bandwidth)/(256 - Load)) + (K3 * Delay)) * (K5/(Reliability + K4))
            
            long metric = 0;
            
            if (K5 == 0)
            {
                // Simple formula when K5 = 0
                metric = K1 * (10000000 / Bandwidth) + K3 * (Delay / 10);
                
                if (K2 > 0)
                {
                    metric += K2 * (10000000 / Bandwidth) / (256 - Load);
                }
            }
            else
            {
                // Complex formula when K5 > 0
                long basePart = K1 * (10000000 / Bandwidth) + K3 * (Delay / 10);
                
                if (K2 > 0)
                {
                    basePart += K2 * (10000000 / Bandwidth) / (256 - Load);
                }
                
                metric = basePart * K5 / (Reliability + K4);
            }
            
            return metric * 256;
        }
    }

    // EIGRP Packet Types
    public class EigrpPacket
    {
        public EigrpPacketType PacketType { get; set; }
        public string SourceRouter { get; set; } = "";
        public string DestinationRouter { get; set; } = "";
        public long SequenceNumber { get; set; } = 0;
        public long AckNumber { get; set; } = 0;
        public int AsNumber { get; set; } = 1;
        public Dictionary<string, object> Tlvs { get; set; } = new();
        public DateTime SentTime { get; set; } = DateTime.Now;
        public bool RequiresAck { get; set; } = false;
        public int RetransmitCount { get; set; } = 0;
    }

    public enum EigrpPacketType
    {
        Update = 1,
        Query = 3,
        Reply = 4,
        Hello = 5,
        Ack = 6,
        SiaQuery = 10,
        SiaReply = 11
    }

    // EIGRP Query for DUAL algorithm
    public class EigrpQuery
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string QueryingNeighbor { get; set; } = "";
        public DateTime QueryTime { get; set; } = DateTime.Now;
        public HashSet<string> RepliesOutstanding { get; set; } = new();
        public bool IsSiaQuery { get; set; } = false;
        public int SiaCount { get; set; } = 0;
    }

    // EIGRP Interface Statistics
    public class EigrpInterfaceStats
    {
        public string InterfaceName { get; set; } = "";
        public int HellosSent { get; set; } = 0;
        public int HellosReceived { get; set; } = 0;
        public int UpdatesSent { get; set; } = 0;
        public int UpdatesReceived { get; set; } = 0;
        public int QueriesSent { get; set; } = 0;
        public int QueriesReceived { get; set; } = 0;
        public int RepliesSent { get; set; } = 0;
        public int RepliesReceived { get; set; } = 0;
        public int AcksSent { get; set; } = 0;
        public int AcksReceived { get; set; } = 0;
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        public DateTime LastUpdateSent { get; set; } = DateTime.MinValue;
    }

    // EIGRP Network Summary
    public class EigrpNetworkSummary
    {
        public string SummaryAddress { get; set; } = "";
        public string SummaryMask { get; set; } = "";
        public List<string> ComponentNetworks { get; set; } = new();
        public long SummaryMetric { get; set; } = 0;
        public bool IsAutoSummary { get; set; } = false;
        public string SummaryInterface { get; set; } = "";
    }
}