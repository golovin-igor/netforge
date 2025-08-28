using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

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
        public T GetProtocol<T>() where T : class
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

        // Enhanced interface methods with basic implementations

        /// <summary>
        /// Get protocols that support a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to filter by</param>
        /// <returns>Enumerable of protocols supporting the vendor</returns>
        public IEnumerable<IDeviceProtocol> GetProtocolsForVendor(string vendorName)
        {
            return GetAllProtocols().Where(p => p.SupportedVendors?.Contains(vendorName, StringComparer.OrdinalIgnoreCase) == true);
        }

        /// <summary>
        /// Get the state of a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol state or null if not available</returns>
        public IProtocolState GetProtocolState(ProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState();
        }

        /// <summary>
        /// Get states of all active protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to state</returns>
        public Dictionary<ProtocolType, object> GetAllProtocolStates()
        {
            return GetAllProtocols()
                .Where(p => p.GetState() != null)
                .ToDictionary(p => p.Type, p => (object)p.GetState());
        }

        /// <summary>
        /// Start a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to start</param>
        /// <returns>True if successfully started, false otherwise</returns>
        public Task<bool> StartProtocol(ProtocolType type)
        {
            // Basic implementation - not supported
            return Task.FromResult(false);
        }

        /// <summary>
        /// Stop a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to stop</param>
        /// <returns>True if successfully stopped, false otherwise</returns>
        public Task<bool> StopProtocol(ProtocolType type)
        {
            // Basic implementation - not supported
            return Task.FromResult(false);
        }

        /// <summary>
        /// Restart a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to restart</param>
        /// <returns>True if successfully restarted, false otherwise</returns>
        public Task<bool> RestartProtocol(ProtocolType type)
        {
            // Basic implementation - not supported
            return Task.FromResult(false);
        }

        /// <summary>
        /// Check if a protocol is registered
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if registered, false otherwise</returns>
        public bool IsProtocolRegistered(ProtocolType type)
        {
            return GetProtocol(type) != null;
        }

        /// <summary>
        /// Validate a protocol configuration
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateProtocolConfiguration(ProtocolType type, object configuration)
        {
            // Basic implementation - assume valid
            return configuration != null;
        }

        /// <summary>
        /// Apply configuration to a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>True if successfully applied, false otherwise</returns>
        public Task<bool> ApplyProtocolConfiguration(ProtocolType type, object configuration)
        {
            // Basic implementation - not supported
            return Task.FromResult(false);
        }

        /// <summary>
        /// Get protocols that the specified protocol depends on
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of dependency protocol types</returns>
        public IEnumerable<ProtocolType> GetProtocolDependencies(ProtocolType type)
        {
            // Basic implementation - no dependencies tracked
            return Enumerable.Empty<ProtocolType>();
        }

        /// <summary>
        /// Get protocols that conflict with the specified protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of conflicting protocol types</returns>
        public IEnumerable<ProtocolType> GetProtocolConflicts(ProtocolType type)
        {
            // Basic implementation - no conflicts tracked
            return Enumerable.Empty<ProtocolType>();
        }

        /// <summary>
        /// Validate that all dependencies for a protocol are satisfied
        /// </summary>
        /// <param name="type">Protocol type to validate</param>
        /// <returns>True if all dependencies are satisfied, false otherwise</returns>
        public bool ValidateProtocolDependencies(ProtocolType type)
        {
            // Basic implementation - assume valid
            return true;
        }

        /// <summary>
        /// Check if two protocols can coexist
        /// </summary>
        /// <param name="type1">First protocol type</param>
        /// <param name="type2">Second protocol type</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        public bool CanProtocolsCoexist(ProtocolType type1, ProtocolType type2)
        {
            // Basic implementation - assume compatible
            return true;
        }

        /// <summary>
        /// Get performance metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol metrics or null if not available</returns>
        public object GetProtocolMetrics(ProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetMetrics();
        }

        /// <summary>
        /// Get performance metrics for all protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to metrics</returns>
        public Dictionary<ProtocolType, object> GetAllProtocolMetrics()
        {
            return GetAllProtocols()
                .Where(p => p.GetMetrics() != null)
                .ToDictionary(p => p.Type, p => p.GetMetrics());
        }

        /// <summary>
        /// Reset metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        public void ResetProtocolMetrics(ProtocolType type)
        {
            // Basic implementation - no action
        }

        /// <summary>
        /// Reset metrics for all protocols
        /// </summary>
        public void ResetAllMetrics()
        {
            // Basic implementation - no action
        }

        /// <summary>
        /// Get service health status
        /// </summary>
        /// <returns>Dictionary containing service health information</returns>
        public Dictionary<string, object> GetServiceHealth()
        {
            var protocols = GetAllProtocols().ToList();
            return new Dictionary<string, object>
            {
                ["ServiceName"] = "BasicProtocolService",
                ["DeviceId"] = _device.DeviceId,
                ["DeviceName"] = _device.DeviceName,
                ["TotalProtocols"] = protocols.Count,
                ["ActiveProtocols"] = protocols.Count(p => p.GetState()?.IsActive == true),
                ["HealthStatus"] = "Basic",
                ["LastUpdate"] = DateTime.Now
            };
        }

        /// <summary>
        /// Get summary information about all managed protocols
        /// </summary>
        /// <returns>Dictionary containing protocol summary</returns>
        public Dictionary<string, object> GetProtocolSummary()
        {
            var protocols = GetAllProtocols().ToList();
            return new Dictionary<string, object>
            {
                ["DeviceId"] = _device.DeviceId,
                ["DeviceName"] = _device.DeviceName,
                ["TotalProtocols"] = protocols.Count,
                ["Protocols"] = protocols.Select(p => new
                {
                    Type = p.Type.ToString(),
                    Name = p.Name,
                    Version = p.Version,
                    IsActive = p.GetState()?.IsActive ?? false
                }),
                ["GeneratedAt"] = DateTime.Now
            };
        }
    }
}
