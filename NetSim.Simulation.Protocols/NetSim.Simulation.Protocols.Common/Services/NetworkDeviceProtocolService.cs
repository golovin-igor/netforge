using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.Common.Services
{
    /// <summary>
    /// Implementation of IProtocolService for NetworkDevice
    /// This provides the bridge between CLI handlers and protocols via IoC/DI
    /// </summary>
    public class NetworkDeviceProtocolService : IProtocolService
    {
        private readonly NetworkDevice _device;
        
        public NetworkDeviceProtocolService(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }
        
        /// <summary>
        /// Get a protocol instance by its type
        /// </summary>
        /// <typeparam name="T">The specific protocol type</typeparam>
        /// <returns>Protocol instance or null if not available</returns>
        public T GetProtocol<T>() where T : class, INetworkProtocol
        {
            return GetAllProtocols().OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Get a protocol instance by protocol type enum
        /// </summary>
        /// <param name="type">Protocol type to retrieve</param>
        /// <returns>Protocol instance or null if not available</returns>
        public INetworkProtocol GetProtocol(ProtocolType type)
        {
            return GetAllProtocols().FirstOrDefault(p => p.Type == type);
        }
        
        /// <summary>
        /// Get the typed state of a specific protocol
        /// </summary>
        /// <typeparam name="TState">The specific state type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Typed protocol state or null if not available</returns>
        public TState GetProtocolState<TState>(ProtocolType type) where TState : class
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState()?.GetTypedState<TState>();
        }
        
        /// <summary>
        /// Get all registered protocol instances
        /// </summary>
        /// <returns>Enumerable of all protocols</returns>
        public IEnumerable<INetworkProtocol> GetAllProtocols()
        {
            // TODO: This requires access to the private _protocols field from NetworkDevice
            // We'll need to add a public method to NetworkDevice to expose this
            // For now, return empty collection to allow compilation
            return Enumerable.Empty<INetworkProtocol>();
        }
        
        /// <summary>
        /// Check if a specific protocol type is active on the device
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if active, false otherwise</returns>
        public bool IsProtocolActive(ProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState()?.IsActive ?? false;
        }
        
        /// <summary>
        /// Get protocol configuration for a specific protocol type
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol configuration or null if not available</returns>
        public object GetProtocolConfiguration(ProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetConfiguration();
        }
        
        /// <summary>
        /// Get all active protocol types on this device
        /// </summary>
        /// <returns>Enumerable of active protocol types</returns>
        public IEnumerable<ProtocolType> GetActiveProtocolTypes()
        {
            return GetAllProtocols()
                .Where(p => p.GetState()?.IsActive ?? false)
                .Select(p => p.Type);
        }
        
        /// <summary>
        /// Get protocol statistics for monitoring and debugging
        /// </summary>
        /// <returns>Protocol statistics</returns>
        public Dictionary<string, object> GetProtocolStatistics()
        {
            var protocols = GetAllProtocols().ToList();
            
            return new Dictionary<string, object>
            {
                ["TotalProtocols"] = protocols.Count,
                ["ActiveProtocols"] = protocols.Count(p => p.GetState()?.IsActive ?? false),
                ["ConfiguredProtocols"] = protocols.Count(p => p.GetState()?.IsConfigured ?? false),
                ["ProtocolTypes"] = protocols.Select(p => p.Type.ToString()).ToList(),
                ["ProtocolStates"] = protocols.ToDictionary(
                    p => p.Type.ToString(), 
                    p => p.GetState()?.GetStateData() ?? new Dictionary<string, object>()
                )
            };
        }
        
        /// <summary>
        /// Check if a protocol type is registered (regardless of active state)
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if registered, false otherwise</returns>
        public bool IsProtocolRegistered(ProtocolType type)
        {
            return GetProtocol(type) != null;
        }
        
        /// <summary>
        /// Get the device this service is associated with
        /// </summary>
        /// <returns>Network device</returns>
        public NetworkDevice GetDevice()
        {
            return _device;
        }
    }
}