using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.Common
{
    /// <summary>
    /// Enhanced interface for network protocols with state management and vendor support
    /// </summary>
    public interface INetworkProtocol
    {
        /// <summary>
        /// The type of protocol (OSPF, BGP, etc.)
        /// </summary>
        ProtocolType Type { get; }
        
        /// <summary>
        /// Human-readable name of the protocol
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Version of the protocol implementation
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Initialize the protocol with device context
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        void Initialize(NetworkDevice device);
        
        /// <summary>
        /// Update the protocol state (called periodically by the simulation engine)
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateState(NetworkDevice device);
        
        /// <summary>
        /// Subscribe to network events for this protocol
        /// </summary>
        /// <param name="eventBus">The network event bus</param>
        /// <param name="self">Reference to the device running this protocol</param>
        void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self);
        
        /// <summary>
        /// Get the current state of the protocol for CLI handlers and monitoring
        /// </summary>
        /// <returns>Protocol state interface</returns>
        IProtocolState GetState();
        
        /// <summary>
        /// Get the typed state of the protocol
        /// </summary>
        /// <typeparam name="T">The specific state type</typeparam>
        /// <returns>Typed protocol state or null if not available</returns>
        T GetTypedState<T>() where T : class;
        
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
        
        /// <summary>
        /// Get the list of vendor names this protocol supports
        /// </summary>
        /// <returns>Enumerable of supported vendor names</returns>
        IEnumerable<string> GetSupportedVendors();
        
        /// <summary>
        /// Check if this protocol supports a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if supported, false otherwise</returns>
        bool SupportsVendor(string vendorName);
    }
}