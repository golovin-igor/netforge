using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Protocols.OSPF
{
    /// <summary>
    /// OSPF protocol state following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class OspfState : BaseProtocolState
    {
        public string RouterId { get; set; } = "";
        public int ProcessId { get; set; } = 1;
        public Dictionary<int, OspfArea> Areas { get; set; } = new();
        public Dictionary<string, OspfNeighbor> Neighbors { get; set; } = new();
        public Dictionary<string, OspfInterface> Interfaces { get; set; } = new();
        public Dictionary<string, OspfRoute> RoutingTable { get; set; } = new();
        public List<OspfRoute> CalculatedRoutes { get; set; } = new();
        public bool TopologyChanged { get; set; } = true;
        public DateTime LastSpfCalculation { get; set; } = DateTime.MinValue;
        public int SpfCalculationCount { get; set; } = 0;
        public Dictionary<string, LinkStateAdvertisement> LinkStateDatabase { get; set; } = new();
        
        /// <summary>
        /// Get or create OSPF neighbor with type safety
        /// </summary>
        public OspfNeighbor GetOrCreateOspfNeighbor(string neighborKey, Func<OspfNeighbor> factory)
        {
            return GetOrCreateNeighbor<OspfNeighbor>(neighborKey, factory);
        }
        
        /// <summary>
        /// Mark topology as changed to trigger SPF calculation
        /// </summary>
        public void MarkTopologyChanged()
        {
            TopologyChanged = true;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Record successful SPF calculation
        /// </summary>
        public void RecordSpfCalculation()
        {
            LastSpfCalculation = DateTime.Now;
            SpfCalculationCount++;
            TopologyChanged = false;
        }
        
        /// <summary>
        /// Check if SPF calculation is needed
        /// </summary>
        public bool ShouldRunSpfCalculation()
        {
            return TopologyChanged || (DateTime.Now - LastSpfCalculation).TotalMinutes > 10;
        }
        
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["RouterId"] = RouterId;
            baseData["ProcessId"] = ProcessId;
            baseData["Areas"] = Areas;
            baseData["Neighbors"] = Neighbors;
            baseData["Interfaces"] = Interfaces;
            baseData["RoutingTable"] = RoutingTable;
            baseData["TopologyChanged"] = TopologyChanged;
            baseData["LastSpfCalculation"] = LastSpfCalculation;
            baseData["SpfCalculationCount"] = SpfCalculationCount;
            baseData["LinkStateDatabase"] = LinkStateDatabase;
            return baseData;
        }
    }
    
    /// <summary>
    /// Represents an OSPF route in the routing table
    /// </summary>
    public class OspfRoute
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string NextHop { get; set; } = "";
        public string Interface { get; set; } = "";
        public int Cost { get; set; } = 0;
        public string RouteType { get; set; } = "Internal"; // Internal, External1, External2
        public int Area { get; set; } = 0;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Link State Advertisement for OSPF topology database
    /// </summary>
    public class LinkStateAdvertisement
    {
        public string LsId { get; set; } = "";
        public string AdvertisingRouter { get; set; } = "";
        public string LsType { get; set; } = "Router"; // Router, Network, Summary, ASBR, External
        public int SequenceNumber { get; set; } = 1;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Area { get; set; } = 0;
        public Dictionary<string, object> Data { get; set; } = new();
        
        public int Age => (int)(DateTime.Now - Timestamp).TotalSeconds;
        public bool IsMaxAge => Age >= 3600; // 1 hour max age
    }
    
    /// <summary>
    /// OSPF neighbor state enumeration
    /// </summary>
    public enum OspfNeighborState
    {
        Down,
        Attempt,
        Init,
        TwoWay,
        ExStart,
        Exchange,
        Loading,
        Full
    }
}
