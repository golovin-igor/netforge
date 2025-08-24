using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.LLDP
{
    /// <summary>
    /// LLDP protocol state following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class LldpState : BaseProtocolState
    {
        public string ChassisId { get; set; } = "";
        public string ChassisIdType { get; set; } = "mac";
        public string SystemName { get; set; } = "";
        public string SystemDescription { get; set; } = "";
        public List<string> SystemCapabilities { get; set; } = new();
        public string ManagementAddress { get; set; } = "";
        public Dictionary<string, LldpNeighbor> Neighbors { get; set; } = new();
        public DateTime LastAdvertisement { get; set; } = DateTime.MinValue;
        public int AdvertisementCount { get; set; } = 0;
        public Dictionary<string, LldpInterfaceConfig> InterfaceSettings { get; set; } = new();

        /// <summary>
        /// Get or create LLDP neighbor with type safety
        /// </summary>
        public LldpNeighbor GetOrCreateLldpNeighbor(string neighborKey, Func<LldpNeighbor> factory)
        {
            return GetOrCreateNeighbor<LldpNeighbor>(neighborKey, factory);
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
        public bool ShouldSendAdvertisement(int intervalSeconds)
        {
            return (DateTime.Now - LastAdvertisement).TotalSeconds >= intervalSeconds;
        }

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["ChassisId"] = ChassisId;
            baseData["ChassisIdType"] = ChassisIdType;
            baseData["SystemName"] = SystemName;
            baseData["SystemDescription"] = SystemDescription;
            baseData["SystemCapabilities"] = SystemCapabilities;
            baseData["ManagementAddress"] = ManagementAddress;
            baseData["Neighbors"] = Neighbors;
            baseData["LastAdvertisement"] = LastAdvertisement;
            baseData["AdvertisementCount"] = AdvertisementCount;
            baseData["InterfaceSettings"] = InterfaceSettings;
            return baseData;
        }
    }

    /// <summary>
    /// Represents an LLDP neighbor (IEEE 802.1AB)
    /// </summary>
    public class LldpNeighbor
    {
        public string ChassisId { get; set; } = "";
        public string ChassisIdType { get; set; } = "mac";
        public string PortId { get; set; } = "";
        public string PortIdType { get; set; } = "ifName";
        public string LocalPortId { get; set; } = "";
        public string SystemName { get; set; } = "";
        public string SystemDescription { get; set; } = "";
        public List<string> SystemCapabilities { get; set; } = new();
        public string ManagementAddress { get; set; } = "";
        public int TimeToLive { get; set; } = 120;
        public DateTime LastSeen { get; set; } = DateTime.Now;

        // LLDP-specific TLVs (Type-Length-Value)
        public string PortDescription { get; set; } = "";
        public List<string> ManagementVids { get; set; } = new();
        public Dictionary<string, string> OrganizationalTlvs { get; set; } = new();

        public LldpNeighbor(string chassisId, string chassisIdType, string portId, string portIdType, string localPortId)
        {
            ChassisId = chassisId;
            ChassisIdType = chassisIdType;
            PortId = portId;
            PortIdType = portIdType;
            LocalPortId = localPortId;
        }

        public bool IsExpired => (DateTime.Now - LastSeen).TotalSeconds > TimeToLive;

        public void UpdateLastSeen()
        {
            LastSeen = DateTime.Now;
        }

        /// <summary>
        /// Get neighbor identification string
        /// </summary>
        public string GetNeighborIdentity()
        {
            return $"{ChassisId}:{PortId}";
        }

        /// <summary>
        /// Check if neighbor has specific capability
        /// </summary>
        public bool HasCapability(string capability)
        {
            return SystemCapabilities.Contains(capability, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get time since last update in seconds
        /// </summary>
        public int GetAgeSinceLastSeen()
        {
            return (int)(DateTime.Now - LastSeen).TotalSeconds;
        }
    }

    /// <summary>
    /// LLDP system capabilities
    /// </summary>
    public static class LldpSystemCapabilities
    {
        public const string Other = "Other";
        public const string Repeater = "Repeater";
        public const string Bridge = "Bridge";
        public const string WlanAccessPoint = "WLAN Access Point";
        public const string Router = "Router";
        public const string Telephone = "Telephone";
        public const string DocsisCableDevice = "DOCSIS Cable Device";
        public const string StationOnly = "Station Only";
    }

    /// <summary>
    /// LLDP chassis ID subtypes
    /// </summary>
    public enum LldpChassisIdType
    {
        ChassisComponent = 1,
        InterfaceAlias = 2,
        PortComponent = 3,
        MacAddress = 4,
        NetworkAddress = 5,
        InterfaceName = 6,
        Local = 7
    }

    /// <summary>
    /// LLDP port ID subtypes
    /// </summary>
    public enum LldpPortIdType
    {
        InterfaceAlias = 1,
        PortComponent = 2,
        MacAddress = 3,
        NetworkAddress = 4,
        InterfaceName = 5,
        AgentCircuitId = 6,
        Local = 7
    }

    /// <summary>
    /// LLDP interface-specific configuration
    /// </summary>
    public class LldpInterfaceConfig
    {
        /// <summary>
        /// Interface name
        /// </summary>
        public string InterfaceName { get; set; } = "";

        /// <summary>
        /// Whether LLDP is enabled on this interface
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Transmit LLDP frames on this interface
        /// </summary>
        public bool TransmitEnabled { get; set; } = true;

        /// <summary>
        /// Receive LLDP frames on this interface
        /// </summary>
        public bool ReceiveEnabled { get; set; } = true;

        /// <summary>
        /// Port description for this interface
        /// </summary>
        public string PortDescription { get; set; } = "";

        /// <summary>
        /// Management VID for this interface
        /// </summary>
        public int ManagementVid { get; set; } = 1;

        public LldpInterfaceConfig(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
    }
}
