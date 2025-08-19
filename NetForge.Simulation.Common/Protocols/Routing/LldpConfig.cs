namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents an LLDP configuration
    /// </summary>
    public class LldpConfig
    {
        /// <summary>
        /// Whether LLDP is enabled globally
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// LLDP transmit interval in seconds
        /// </summary>
        public int TransmitInterval { get; set; } = 30;
        
        /// <summary>
        /// LLDP time-to-live multiplier
        /// </summary>
        public int TimeToLiveMultiplier { get; set; } = 4;
        
        /// <summary>
        /// LLDP hold time in seconds
        /// </summary>
        public int HoldTime { get; set; } = 120;
        
        /// <summary>
        /// Local chassis ID
        /// </summary>
        public string ChassisId { get; set; } = string.Empty;
        
        /// <summary>
        /// Local system name
        /// </summary>
        public string SystemName { get; set; } = string.Empty;
        
        /// <summary>
        /// Local system description
        /// </summary>
        public string SystemDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// System capabilities
        /// </summary>
        public List<string> SystemCapabilities { get; set; } = new();
        
        /// <summary>
        /// Interface-specific LLDP settings
        /// </summary>
        public Dictionary<string, LldpInterfaceConfig> InterfaceSettings { get; set; } = new();
        
        public LldpConfig()
        {
            // Default capabilities
            SystemCapabilities.Add("Bridge");
            SystemCapabilities.Add("Router");
        }
        
        /// <summary>
        /// Calculate TTL value based on transmit interval and multiplier
        /// </summary>
        public int GetTimeToLive()
        {
            return TransmitInterval * TimeToLiveMultiplier;
        }
    }
    
    /// <summary>
    /// Interface-specific LLDP configuration
    /// </summary>
    public class LldpInterfaceConfig
    {
        /// <summary>
        /// Interface name
        /// </summary>
        public string InterfaceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether LLDP is enabled on this interface
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Port ID for LLDP advertisements
        /// </summary>
        public string PortId { get; set; } = string.Empty;
        
        /// <summary>
        /// Port description
        /// </summary>
        public string PortDescription { get; set; } = string.Empty;
        
        public LldpInterfaceConfig(string interfaceName)
        {
            InterfaceName = interfaceName;
            PortId = interfaceName; // Default port ID is interface name
        }
    }
} 
