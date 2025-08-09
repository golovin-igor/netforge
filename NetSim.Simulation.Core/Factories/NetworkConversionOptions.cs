using NetSim.Simulation.Common;

namespace NetSim.Simulation.Factories
{
    /// <summary>
    /// Configuration options for network topology conversion
    /// </summary>
    public class NetworkConversionOptions
    {
        /// <summary>
        /// Whether to apply NVRAM configuration from the source devices
        /// Default: true
        /// </summary>
        public bool ApplyNvramConfiguration { get; set; } = true;

        /// <summary>
        /// Whether to configure default interface settings based on interface type
        /// Default: true
        /// </summary>
        public bool ConfigureDefaultInterfaceSettings { get; set; } = true;

        /// <summary>
        /// Whether to enable protocol initialization after conversion
        /// Default: false (protocols should be configured manually)
        /// </summary>
        public bool EnableProtocolInitialization { get; set; } = true;

        /// <summary>
        /// Whether to enable OSPF protocol initialization
        /// Only applies if EnableProtocolInitialization is true
        /// Default: false
        /// </summary>
        public bool EnableOspf { get; set; } = true;

        /// <summary>
        /// Whether to enable BGP protocol initialization
        /// Only applies if EnableProtocolInitialization is true
        /// Default: false
        /// </summary>
        public bool EnableBgp { get; set; } = true;

        /// <summary>
        /// Whether to enable RIP protocol initialization
        /// Only applies if EnableProtocolInitialization is true
        /// Default: false
        /// </summary>
        public bool EnableRip { get; set; } = true;

        /// <summary>
        /// Whether to update connected routes after conversion
        /// Default: true
        /// </summary>
        public bool UpdateConnectedRoutes { get; set; } = true;

        /// <summary>
        /// Whether to validate device configurations during conversion
        /// Default: true
        /// </summary>
        public bool ValidateConfigurations { get; set; } = true;

        /// <summary>
        /// Whether to preserve original device IDs as system settings
        /// Default: true
        /// </summary>
        public bool PreserveOriginalIds { get; set; } = true;

        /// <summary>
        /// Whether to automatically generate missing MAC addresses
        /// Default: true
        /// </summary>
        public bool GenerateMissingMacAddresses { get; set; } = true;

        /// <summary>
        /// Default connection type to use when not specified
        /// Default: Ethernet
        /// </summary>
        public PhysicalConnectionType DefaultConnectionType { get; set; } = PhysicalConnectionType.Ethernet;

        /// <summary>
        /// Whether to establish all connections as operational initially
        /// Default: true
        /// </summary>
        public bool EstablishConnectionsAsOperational { get; set; } = true;

        /// <summary>
        /// Maximum number of errors to tolerate before aborting conversion
        /// Default: -1 (unlimited)
        /// </summary>
        public int MaxErrorsBeforeAbort { get; set; } = -1;

        /// <summary>
        /// Whether to log detailed conversion progress
        /// Default: false
        /// </summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Custom device factory mappings for unsupported vendor/type combinations
        /// Key format: "vendor:devicetype" (case insensitive)
        /// </summary>
        public Dictionary<string, Func<string, NetworkDevice>> CustomDeviceFactories { get; set; } = new();

        /// <summary>
        /// Custom connection type mappings for unsupported link types
        /// Key format: link type string (case insensitive)
        /// </summary>
        public Dictionary<string, PhysicalConnectionType> CustomConnectionTypes { get; set; } = new();

        /// <summary>
        /// Interface name mappings for renaming interfaces during conversion
        /// Key: original name, Value: new name
        /// </summary>
        public Dictionary<string, string> InterfaceNameMappings { get; set; } = new();

        /// <summary>
        /// Device name mappings for renaming devices during conversion
        /// Key: original hostname, Value: new hostname
        /// </summary>
        public Dictionary<string, string> DeviceNameMappings { get; set; } = new();

        /// <summary>
        /// Additional system settings to apply to all devices
        /// </summary>
        public Dictionary<string, string> GlobalSystemSettings { get; set; } = new();

        /// <summary>
        /// Create default options for basic topology conversion
        /// </summary>
        public static NetworkConversionOptions CreateDefault()
        {
            return new NetworkConversionOptions();
        }

        /// <summary>
        /// Create options optimized for testing scenarios
        /// </summary>
        public static NetworkConversionOptions CreateForTesting()
        {
            return new NetworkConversionOptions
            {
                ApplyNvramConfiguration = false,
                EnableProtocolInitialization = true,
                EnableOspf = true,
                VerboseLogging = true,
                ValidateConfigurations = false // Skip validation for faster testing
            };
        }

        /// <summary>
        /// Create options optimized for importing complete topology data with high fidelity
        /// Prioritizes preserving all data from the source topology including MAC addresses
        /// </summary>
        public static NetworkConversionOptions CreateForTopologyImport()
        {
            return new NetworkConversionOptions
            {
                ApplyNvramConfiguration = true,
                ConfigureDefaultInterfaceSettings = true,
                EnableProtocolInitialization = false, // Let NVRAM config handle protocols
                UpdateConnectedRoutes = true,
                ValidateConfigurations = true,
                PreserveOriginalIds = true,
                GenerateMissingMacAddresses = true, // Generate MAC addresses for interfaces without them
                EstablishConnectionsAsOperational = true,
                VerboseLogging = true // Enable detailed logging for import process
            };
        }

        /// <summary>
        /// Create options for production deployment
        /// </summary>
        public static NetworkConversionOptions CreateForProduction()
        {
            return new NetworkConversionOptions
            {
                ApplyNvramConfiguration = true,
                EnableProtocolInitialization = true, 
                ValidateConfigurations = true,
                VerboseLogging = false,
                MaxErrorsBeforeAbort = 10 // Limit errors in production
            };
        }
    }
} 
