using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.ARP
{
    /// <summary>
    /// ARP protocol state following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class ArpState : BaseProtocolState
    {
        public Dictionary<string, ArpEntry> ArpTable { get; set; } = new();
        public DateTime LastArpRequest { get; set; } = DateTime.MinValue;
        public int ArpRequestCount { get; set; } = 0;
        public int ArpResponseCount { get; set; } = 0;
        public int ArpCacheHits { get; set; } = 0;
        public int ArpCacheMisses { get; set; } = 0;
        
        /// <summary>
        /// Get or create ARP entry with type safety
        /// </summary>
        public ArpEntry GetOrCreateArpEntry(string ipAddress, Func<ArpEntry> factory)
        {
            return GetOrCreateNeighbor<ArpEntry>(ipAddress, factory);
        }
        
        /// <summary>
        /// Record ARP request sent
        /// </summary>
        public void RecordArpRequest()
        {
            LastArpRequest = DateTime.Now;
            ArpRequestCount++;
        }
        
        /// <summary>
        /// Record ARP response received
        /// </summary>
        public void RecordArpResponse()
        {
            ArpResponseCount++;
        }
        
        /// <summary>
        /// Record ARP cache hit
        /// </summary>
        public void RecordCacheHit()
        {
            ArpCacheHits++;
        }
        
        /// <summary>
        /// Record ARP cache miss
        /// </summary>
        public void RecordCacheMiss()
        {
            ArpCacheMisses++;
        }
        
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["ArpTable"] = ArpTable;
            baseData["LastArpRequest"] = LastArpRequest;
            baseData["ArpRequestCount"] = ArpRequestCount;
            baseData["ArpResponseCount"] = ArpResponseCount;
            baseData["ArpCacheHits"] = ArpCacheHits;
            baseData["ArpCacheMisses"] = ArpCacheMisses;
            return baseData;
        }
    }
    
    /// <summary>
    /// Represents an ARP table entry
    /// </summary>
    public class ArpEntry
    {
        public string IpAddress { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string Interface { get; set; } = "";
        public ArpEntryType Type { get; set; } = ArpEntryType.Dynamic;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Age => (int)(DateTime.Now - Timestamp).TotalMinutes;
        
        public ArpEntry(string ipAddress, string macAddress, string interfaceName)
        {
            IpAddress = ipAddress;
            MacAddress = macAddress;
            Interface = interfaceName;
        }
        
        public bool IsExpired(int maxAgeMinutes = 20)
        {
            return Age > maxAgeMinutes;
        }
        
        public void UpdateTimestamp()
        {
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// ARP entry types
    /// </summary>
    public enum ArpEntryType
    {
        Dynamic,
        Static
    }
}
