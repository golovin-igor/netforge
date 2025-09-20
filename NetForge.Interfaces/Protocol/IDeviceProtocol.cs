using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Events;

namespace NetForge.Interfaces.Devices
{
    /// <summary>
    /// Unified interface for all device protocols in NetForge
    /// Defines the complete contract for protocol lifecycle, state management, and vendor support
    /// Merged from IDeviceProtocol and IEnhancedDeviceProtocol for unified protocol management
    /// </summary>
    public interface IDeviceProtocol
    {
        /// <summary>
        /// Human-readable name of the protocol
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the protocol implementation
        /// </summary>
        string Version { get; }

        // Core lifecycle management
        /// <summary>
        /// Initialize the protocol with device context
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        void Initialize(INetworkDevice device);

        /// <summary>
        /// Update the protocol state (called periodically by simulation engine)
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        Task UpdateState(INetworkDevice device);

        /// <summary>
        /// Subscribe to network events for protocol operation
        /// </summary>
        /// <param name="eventBus">The network event bus</param>
        /// <param name="self">The device this protocol is running on</param>
        void SubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self);

        // State access for CLI handlers and monitoring
        /// <summary>
        /// Get the current state of the protocol for CLI handlers and monitoring
        /// </summary>
        /// <returns>Protocol state interface</returns>
        Simulation.Common.Interfaces.IProtocolState GetState();

        /// <summary>
        /// Get the typed state of the protocol
        /// </summary>
        /// <typeparam name="T">The specific state type</typeparam>
        /// <returns>Typed protocol state or null if not available</returns>
        T GetTypedState<T>() where T : class;

        // Configuration management
        /// <summary>
        /// Get the current configuration of the protocol
        /// </summary>
        /// <returns>Protocol configuration object</returns>
        object GetConfiguration();

        /// <summary>
        /// Apply new configuration to the protocol
        /// </summary>
        /// <param name="configuration">New configuration to apply</param>
        void ApplyConfiguration(object configuration);

        // Protocol lifecycle management
        /// <summary>
        /// Start the protocol
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task<bool> Start();

        /// <summary>
        /// Stop the protocol
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task<bool> Stop();

        /// <summary>
        /// Configure the protocol with new settings
        /// </summary>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> Configure(object configuration);

        // Vendor support information
        /// <summary>
        /// Get all vendors supported by this protocol implementation
        /// </summary>
        IEnumerable<string> SupportedVendors { get; }

        NetworkProtocolType Type { get; set; }

        /// <summary>
        /// Get all vendors supported by this protocol implementation
        /// </summary>
        /// <returns>Enumerable of vendor names</returns>
        IEnumerable<string> GetSupportedVendors();

        /// <summary>
        /// Check if this protocol supports a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if vendor is supported, false otherwise</returns>
        bool SupportsVendor(string vendorName);

        // Protocol dependencies and compatibility
        /// <summary>
        /// Get protocols that this protocol depends on
        /// </summary>
        /// <returns>Enumerable of required protocol types</returns>
        IEnumerable<NetworkProtocolType> GetDependencies();

        /// <summary>
        /// Get protocols that conflict with this protocol
        /// </summary>
        /// <returns>Enumerable of conflicting protocol types</returns>
        IEnumerable<NetworkProtocolType> GetConflicts();

        /// <summary>
        /// Check if this protocol can coexist with another protocol
        /// </summary>
        /// <param name="otherNetworkProtocol">Other protocol type to check</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        bool CanCoexistWith(NetworkProtocolType otherNetworkProtocol);

        // Performance and monitoring (optional implementation - can return null for basic protocols)
        /// <summary>
        /// Get performance metrics for this protocol
        /// </summary>
        /// <returns>Protocol metrics interface (optional - can return null)</returns>
        object GetMetrics(); // Using object instead of IProtocolMetrics to avoid dependency issues
    }
}
