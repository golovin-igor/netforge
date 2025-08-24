using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Protocols.CDP
{
    /// <summary>
    /// CDP protocol state following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class CdpState : BaseProtocolState
    {
        public string DeviceId { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Version { get; set; } = "";
        public List<string> Capabilities { get; set; } = new();
        public Dictionary<string, CdpNeighbor> Neighbors { get; set; } = new();
        public DateTime LastAdvertisement { get; set; } = DateTime.MinValue;
        public int AdvertisementCount { get; set; } = 0;
        public Dictionary<string, CdpInterfaceConfig> InterfaceSettings { get; set; } = new();

        /// <summary>
        /// Get or create CDP neighbor with type safety
        /// </summary>
        public CdpNeighbor GetOrCreateCdpNeighbor(string neighborKey, Func<CdpNeighbor> factory)
        {
            return GetOrCreateNeighbor<CdpNeighbor>(neighborKey, factory);
        }

        /// <summary>
        /// Record successful advertisement
        /// </summary>
        public void RecordAdvertisement()
        {
            LastAdvertisement = DateTime.Now;
            AdvertisementCount++;
        }

        /// <summary>
        /// Check if it's time to send advertisements
        /// </summary>
        public bool ShouldSendAdvertisement(int timerSeconds)
        {
            return (DateTime.Now - LastAdvertisement).TotalSeconds >= timerSeconds;
        }

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["DeviceId"] = DeviceId;
            baseData["Platform"] = Platform;
            baseData["Version"] = Version;
            baseData["Capabilities"] = Capabilities;
            baseData["Neighbors"] = Neighbors;
            baseData["LastAdvertisement"] = LastAdvertisement;
            baseData["AdvertisementCount"] = AdvertisementCount;
            baseData["InterfaceSettings"] = InterfaceSettings;
            return baseData;
        }
    }

    /// <summary>
    /// Represents a CDP neighbor
    /// </summary>
    public class CdpNeighbor
    {
        public string DeviceId { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Version { get; set; } = "";
        public List<string> Capabilities { get; set; } = new();
        public string LocalInterface { get; set; } = "";
        public string RemoteInterface { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public int HoldTime { get; set; } = 180;

        public CdpNeighbor(string deviceId, string localInterface, string remoteInterface)
        {
            DeviceId = deviceId;
            LocalInterface = localInterface;
            RemoteInterface = remoteInterface;
        }

        public bool IsExpired => (DateTime.Now - LastSeen).TotalSeconds > HoldTime;

        public void UpdateLastSeen()
        {
            LastSeen = DateTime.Now;
        }
    }
}
