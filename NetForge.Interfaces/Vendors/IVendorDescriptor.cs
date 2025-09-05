using NetForge.Simulation.DataTypes;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Declarative descriptor for a network device vendor, defining all supported protocols and handlers
    /// </summary>
    public interface IVendorDescriptor
    {
        /// <summary>
        /// Vendor name (e.g., "Cisco", "Juniper", "Arista")
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Vendor display name for UI purposes
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Vendor description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Supported device models/types for this vendor
        /// </summary>
        IEnumerable<DeviceModelDescriptor> SupportedModels { get; }

        /// <summary>
        /// Supported protocols for this vendor
        /// </summary>
        IEnumerable<ProtocolDescriptor> SupportedProtocols { get; }

        /// <summary>
        /// CLI handler configurations for this vendor
        /// </summary>
        IEnumerable<HandlerDescriptor> CliHandlers { get; }

        /// <summary>
        /// Vendor-specific configuration
        /// </summary>
        VendorConfiguration Configuration { get; }

        /// <summary>
        /// Priority for this vendor (higher values have priority in conflicts)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Check if this vendor supports a specific device model
        /// </summary>
        bool SupportsModel(string modelName);

        /// <summary>
        /// Check if this vendor supports a specific protocol
        /// </summary>
        bool SupportsProtocol(NetworkProtocolType protocolType);

        /// <summary>
        /// Get protocol descriptor for a specific protocol type
        /// </summary>
        ProtocolDescriptor? GetProtocolDescriptor(NetworkProtocolType protocolType);

        /// <summary>
        /// Get device model descriptor by name
        /// </summary>
        DeviceModelDescriptor? GetModelDescriptor(string modelName);
    }

    /// <summary>
    /// Describes a device model supported by a vendor
    /// </summary>
    public class DeviceModelDescriptor
    {
        public string ModelName { get; set; } = "";
        public string ModelFamily { get; set; } = "";
        public string Description { get; set; } = "";
        public DeviceType DeviceType { get; set; }
        public IList<string> Features { get; set; } = new List<string>();
        public IDictionary<string, object> Capabilities { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Describes a protocol supported by a vendor
    /// </summary>
    public class ProtocolDescriptor
    {
        public NetworkProtocolType ProtocolType { get; set; }
        public string ImplementationClass { get; set; } = "";
        public string AssemblyName { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public IDictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        public IList<string> RequiredFeatures { get; set; } = new List<string>();
    }

    /// <summary>
    /// Describes a CLI handler for a vendor
    /// </summary>
    public class HandlerDescriptor
    {
        public string HandlerName { get; set; } = "";
        public string CommandPattern { get; set; } = "";
        public string ImplementationClass { get; set; } = "";
        public string AssemblyName { get; set; } = "";
        public HandlerType Type { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public IList<string> RequiredModes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Types of CLI handlers
    /// </summary>
    public enum HandlerType
    {
        Basic,
        Configuration,
        Show,
        Interface,
        Routing,
        Security,
        System,
        Diagnostic
    }

    /// <summary>
    /// Vendor-specific configuration
    /// </summary>
    public class VendorConfiguration
    {
        public string DefaultPrompt { get; set; } = ">";
        public string EnabledPrompt { get; set; } = "#";
        public string ConfigPrompt { get; set; } = "(config)#";
        public IDictionary<string, string> PromptModes { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Device types
    /// </summary>
    public enum DeviceType
    {
        Router,
        Switch,
        Firewall,
        LoadBalancer,
        AccessPoint,
        Server,
        Workstation,
        Other
    }
}