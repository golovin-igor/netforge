using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using OldNetworkProtocol = NetForge.Simulation.Common.Interfaces.INetworkProtocol;

namespace NetForge.Simulation.Common.Services
{
    /// <summary>
    /// Basic implementation of IProtocolService for backward compatibility
    /// This provides the minimal interface when the enhanced protocol service is not available
    /// </summary>
    public class BasicProtocolService : IProtocolService
    {
        private readonly NetworkDevice _device;

        public BasicProtocolService(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Get a protocol instance by its type
        /// </summary>
        /// <typeparam name="T">The specific protocol type</typeparam>
        /// <returns>Protocol instance or null if not available</returns>
        public T GetProtocol<T>() where T : class, IDeviceProtocol
        {
            // Convert from old interface if possible
            return GetAllProtocols().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get a protocol instance by protocol type enum
        /// </summary>
        /// <param name="type">Protocol type to retrieve</param>
        /// <returns>Protocol instance or null if not available</returns>
        public IDeviceProtocol GetProtocol(ProtocolType type)
        {
            // Convert from old interface if possible
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
        public IEnumerable<IDeviceProtocol> GetAllProtocols()
        {
            // Try to convert from old interface to new interface
            return _device.GetRegisteredProtocols()
                .OfType<IDeviceProtocol>()
                .ToList();
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
    }
}