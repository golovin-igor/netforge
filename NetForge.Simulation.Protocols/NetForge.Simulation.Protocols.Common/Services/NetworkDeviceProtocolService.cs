using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common.Metrics;
using NetForge.Simulation.Protocols.Common.Dependencies;
using NetForge.Simulation.Protocols.Common.Configuration;
using CommonProtocolService = NetForge.Simulation.Common.Interfaces.IProtocolService;

namespace NetForge.Simulation.Protocols.Common.Services
{
    /// <summary>
    /// Implementation of IProtocolService for NetworkDevice
    /// This provides the bridge between CLI handlers and protocols via IoC/DI
    /// </summary>
    public class NetworkDeviceProtocolService(INetworkDevice device) : CommonProtocolService
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));
        private readonly ProtocolDependencyManager _dependencyManager = new();
        private readonly ProtocolConfigurationManager _configurationManager = new();
        private readonly Dictionary<NetworkProtocolType, IProtocolMetrics> _metricsCache = new();

        /// <summary>
        /// Get a protocol instance by its type
        /// </summary>
        /// <typeparam name="T">The specific protocol type</typeparam>
        /// <returns>Protocol instance or null if not available</returns>
        public T GetProtocol<T>() where T : class
        {
            return GetAllProtocols().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Get a protocol instance by protocol type enum
        /// </summary>
        /// <param name="type">Protocol type to retrieve</param>
        /// <returns>Protocol instance or null if not available</returns>
        public IDeviceProtocol GetProtocol(NetworkProtocolType type)
        {
            return GetAllProtocols().FirstOrDefault(p => p.Type == type);
        }

        /// <summary>
        /// Get the typed state of a specific protocol
        /// </summary>
        /// <typeparam name="TState">The specific state type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Typed protocol state or null if not available</returns>
        public TState GetProtocolState<TState>(NetworkProtocolType type) where TState : class
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState() as TState;
        }

        /// <summary>
        /// Get all registered protocol instances
        /// </summary>
        /// <returns>Enumerable of all protocols</returns>
        public IEnumerable<IDeviceProtocol> GetAllProtocols()
        {
            // Convert from old interface to new interface
            return _device.GetRegisteredProtocols()
                .OfType<IDeviceProtocol>()
                .ToList();
        }

        /// <summary>
        /// Check if a specific protocol type is active on the device
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if active, false otherwise</returns>
        public bool IsProtocolActive(NetworkProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState()?.IsActive ?? false;
        }

        /// <summary>
        /// Get protocol configuration for a specific protocol type
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol configuration or null if not available</returns>
        public object GetProtocolConfiguration(NetworkProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetConfiguration();
        }

        /// <summary>
        /// Get all active protocol types on this device
        /// </summary>
        /// <returns>Enumerable of active protocol types</returns>
        public IEnumerable<NetworkProtocolType> GetActiveProtocolTypes()
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
        public bool IsProtocolRegistered(NetworkProtocolType type)
        {
            return GetProtocol(type) != null;
        }

        /// <summary>
        /// Get protocols that support a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to filter by</param>
        /// <returns>Enumerable of protocols supporting the vendor</returns>
        public IEnumerable<IDeviceProtocol> GetProtocolsForVendor(string vendorName)
        {
            return GetAllProtocols().Where(p => p.SupportedVendors.Contains(vendorName, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get the state of a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol state or null if not available</returns>
        public IProtocolState GetProtocolState(NetworkProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetState();
        }

        /// <summary>
        /// Get states of all active protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to state</returns>
        public Dictionary<NetworkProtocolType, object> GetAllProtocolStates()
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
        public async Task<bool> StartProtocol(NetworkProtocolType type)
        {
            try
            {
                var protocol = GetProtocol(type);
                if (protocol == null)
                {
                    return false;
                }

                // Validate dependencies before starting
                if (!ValidateProtocolDependencies(type))
                {
                    return false;
                }

                await protocol.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stop a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to stop</param>
        /// <returns>True if successfully stopped, false otherwise</returns>
        public async Task<bool> StopProtocol(NetworkProtocolType type)
        {
            try
            {
                var protocol = GetProtocol(type);
                if (protocol == null)
                {
                    return false;
                }

                // Check if other protocols depend on this one
                var dependents = _dependencyManager.GetDependents(type);
                var activeProtocols = GetActiveProtocolTypes();
                var conflictingDependents = dependents
                    .Where(dep => dep.Type == DependencyType.Required && activeProtocols.Contains(dep.RequiredNetworkProtocol))
                    .ToList();

                if (conflictingDependents.Any())
                {
                    // Cannot stop - other protocols depend on it
                    return false;
                }

                await protocol.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restart a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to restart</param>
        /// <returns>True if successfully restarted, false otherwise</returns>
        public async Task<bool> RestartProtocol(NetworkProtocolType type)
        {
            var stopResult = await StopProtocol(type);
            if (!stopResult)
            {
                return false;
            }

            // Small delay to ensure clean shutdown
            await Task.Delay(100);

            return await StartProtocol(type);
        }

        /// <summary>
        /// Get the configuration of a specific protocol
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Typed configuration or null if not available</returns>
        public T GetProtocolConfiguration<T>(NetworkProtocolType type) where T : class
        {
            return _configurationManager.GetConfiguration<T>(type);
        }

        /// <summary>
        /// Apply configuration to a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>True if successfully applied, false otherwise</returns>
        public async Task<bool> ApplyProtocolConfiguration(NetworkProtocolType type, object configuration)
        {
            try
            {
                var protocol = GetProtocol(type);
                if (protocol == null)
                {
                    return false;
                }

                // Apply through configuration manager for validation
                var result = await _configurationManager.ApplyConfiguration(type, configuration);
                if (!result)
                {
                    return false;
                }

                // Apply to actual protocol instance
                return await protocol.Configure(configuration);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate a protocol configuration
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateProtocolConfiguration(NetworkProtocolType type, object configuration)
        {
            return _configurationManager.ValidateConfiguration(configuration);
        }

        /// <summary>
        /// Get protocols that the specified protocol depends on
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of dependency protocol types</returns>
        public IEnumerable<NetworkProtocolType> GetProtocolDependencies(NetworkProtocolType type)
        {
            return _dependencyManager.GetRequiredProtocols(type)
                .Concat(_dependencyManager.GetOptionalProtocols(type));
        }

        /// <summary>
        /// Get protocols that conflict with the specified protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of conflicting protocol types</returns>
        public IEnumerable<NetworkProtocolType> GetProtocolConflicts(NetworkProtocolType type)
        {
            return _dependencyManager.GetConflictingProtocols(type);
        }

        /// <summary>
        /// Validate that all dependencies for a protocol are satisfied
        /// </summary>
        /// <param name="type">Protocol type to validate</param>
        /// <returns>True if all dependencies are satisfied, false otherwise</returns>
        public bool ValidateProtocolDependencies(NetworkProtocolType type)
        {
            var activeProtocols = GetActiveProtocolTypes();
            var result = _dependencyManager.ValidateProtocolAddition(activeProtocols, type);
            return result.IsValid;
        }

        /// <summary>
        /// Check if two protocols can coexist
        /// </summary>
        /// <param name="type1">First protocol type</param>
        /// <param name="type2">Second protocol type</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        public bool CanProtocolsCoexist(NetworkProtocolType type1, NetworkProtocolType type2)
        {
            var conflicts1 = GetProtocolConflicts(type1);
            var conflicts2 = GetProtocolConflicts(type2);

            return !conflicts1.Contains(type2) && !conflicts2.Contains(type1);
        }

        /// <summary>
        /// Get performance metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol metrics or null if not available</returns>
        public object GetProtocolMetrics(NetworkProtocolType type)
        {
            var protocol = GetProtocol(type);
            return protocol?.GetMetrics();
        }

        /// <summary>
        /// Get performance metrics for all protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to metrics</returns>
        public Dictionary<NetworkProtocolType, object> GetAllProtocolMetrics()
        {
            return GetAllProtocols()
                .Where(p => p.GetMetrics() != null)
                .ToDictionary(p => p.Type, p => p.GetMetrics());
        }

        /// <summary>
        /// Reset metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        public void ResetProtocolMetrics(NetworkProtocolType type)
        {
            var protocol = GetProtocol(type);
            if (protocol?.GetMetrics() is IProtocolMetrics metrics)
            {
                metrics.ResetMetrics();
            }
        }

        /// <summary>
        /// Reset metrics for all protocols
        /// </summary>
        public void ResetAllMetrics()
        {
            foreach (var protocol in GetAllProtocols())
            {
                if (protocol.GetMetrics() is IProtocolMetrics metrics)
                {
                    metrics.ResetMetrics();
                }
            }
        }

        /// <summary>
        /// Get service health status
        /// </summary>
        /// <returns>Dictionary containing service health information</returns>
        public Dictionary<string, object> GetServiceHealth()
        {
            var protocols = GetAllProtocols().ToList();
            var activeCount = protocols.Count(p => p.GetState()?.IsActive ?? false);
            var configuredCount = protocols.Count(p => p.GetState()?.IsConfigured ?? false);

            var health = new Dictionary<string, object>
            {
                ["ServiceName"] = "NetworkDeviceProtocolService",
                ["DeviceId"] = _device.DeviceId,
                ["DeviceName"] = _device.DeviceName,
                ["TotalProtocols"] = protocols.Count,
                ["ActiveProtocols"] = activeCount,
                ["ConfiguredProtocols"] = configuredCount,
                ["HealthStatus"] = activeCount > 0 ? "Healthy" : "Inactive",
                ["LastUpdate"] = DateTime.Now,
                ["Dependencies"] = _dependencyManager.GetDependencyStatistics(),
                ["Configurations"] = _configurationManager.GetType().Name // Placeholder for config stats
            };

            return health;
        }

        /// <summary>
        /// Get summary information about all managed protocols
        /// </summary>
        /// <returns>Dictionary containing protocol summary</returns>
        public Dictionary<string, object> GetProtocolSummary()
        {
            var protocols = GetAllProtocols().ToList();
            var protocolSummary = protocols.Select(p => new
            {
                Type = p.Type.ToString(),
                Name = p.Name,
                Version = p.Version,
                IsActive = p.GetState()?.IsActive ?? false,
                IsConfigured = p.GetState()?.IsConfigured ?? false,
                Status = "Active", // Simplified status since unified interface doesn't have Status property
                SupportedVendors = p.SupportedVendors.ToList(),
                Dependencies = GetProtocolDependencies(p.Type).Select(d => d.ToString()).ToList(),
                Conflicts = GetProtocolConflicts(p.Type).Select(c => c.ToString()).ToList()
            });

            return new Dictionary<string, object>
            {
                ["DeviceId"] = _device.DeviceId,
                ["DeviceName"] = _device.DeviceName,
                ["DeviceType"] = _device.DeviceType,
                ["TotalProtocols"] = protocols.Count,
                ["Protocols"] = protocolSummary,
                ["GeneratedAt"] = DateTime.Now
            };
        }

        /// <summary>
        /// Get the device this service is associated with
        /// </summary>
        /// <returns>Network device</returns>
        public INetworkDevice GetDevice()
        {
            return _device;
        }

    }
}
