namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents a CDP configuration
    /// </summary>
    public class CdpConfig
    {
        /// <summary>
        /// Whether CDP is enabled globally
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// CDP advertisement timer in seconds
        /// </summary>
        public int Timer { get; set; } = 60;
        
        /// <summary>
        /// CDP hold time in seconds
        /// </summary>
        public int HoldTime { get; set; } = 180;
        
        /// <summary>
        /// Device ID for CDP advertisements
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Platform information for CDP advertisements
        /// </summary>
        public string Platform { get; set; } = string.Empty;
        
        /// <summary>
        /// Version information for CDP advertisements
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Capabilities for CDP advertisements
        /// </summary>
        public List<string> Capabilities { get; set; } = new();
        
        /// <summary>
        /// Interface-specific CDP settings
        /// </summary>
        public Dictionary<string, CdpInterfaceConfig> InterfaceSettings { get; set; } = new();
        
        public CdpConfig()
        {
            // Default capabilities
            Capabilities.Add("Router");
            Capabilities.Add("Switch");
        }
    }
    
    /// <summary>
    /// Interface-specific CDP configuration
    /// </summary>
    public class CdpInterfaceConfig
    {
        /// <summary>
        /// Interface name
        /// </summary>
        public string InterfaceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether CDP is enabled on this interface
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        public CdpInterfaceConfig(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
    }
} 
