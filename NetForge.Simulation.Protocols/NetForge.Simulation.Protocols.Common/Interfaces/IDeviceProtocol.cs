using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common.State;

namespace NetForge.Simulation.Protocols.Common.Interfaces
{
    /// <summary>
    /// Primary interface for all device protocols in NetForge
    /// Defines the complete contract for protocol lifecycle, state management, and vendor support
    /// </summary>
    public interface IDeviceProtocol : IProtocol
    {
        // Core lifecycle management
        /// <summary>
        /// Initialize the protocol with device context
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        void Initialize(NetworkDevice device);

        /// <summary>
        /// Update the protocol state (called periodically by simulation engine)
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        Task UpdateState(NetworkDevice device);

        /// <summary>
        /// Subscribe to network events for protocol operation
        /// </summary>
        /// <param name="eventBus">The network event bus</param>
        /// <param name="self">The device this protocol is running on</param>
        void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self);

        // State access for CLI handlers and monitoring
        /// <summary>
        /// Get the current state of the protocol
        /// </summary>
        /// <returns>Protocol state interface</returns>
        IProtocolState GetState();

        /// <summary>
        /// Get the typed state of the protocol for specific protocol access
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

        // Vendor support information
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
        IEnumerable<ProtocolType> GetDependencies();

        /// <summary>
        /// Get protocols that conflict with this protocol
        /// </summary>
        /// <returns>Enumerable of conflicting protocol types</returns>
        IEnumerable<ProtocolType> GetConflicts();

        /// <summary>
        /// Check if this protocol can coexist with another protocol
        /// </summary>
        /// <param name="otherProtocol">Other protocol type to check</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        bool CanCoexistWith(ProtocolType otherProtocol);

        // Performance and monitoring
        /// <summary>
        /// Get performance metrics for this protocol
        /// </summary>
        /// <returns>Protocol metrics interface</returns>
        IProtocolMetrics GetMetrics();
    }
}