using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Common
{
    /// <summary>
    /// Base implementation of network protocols following the state management pattern
    /// from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public abstract class BaseProtocol : IDeviceProtocol, IDisposable
    {
        protected NetworkDevice _device;
        protected readonly BaseProtocolState _state;

        // Abstract properties that derived classes must implement
        public abstract ProtocolType Type { get; }
        public abstract string Name { get; }
        public virtual string Version => "1.0.0";

        protected BaseProtocol()
        {
            _state = CreateInitialState();
        }

        /// <summary>
        /// Create the initial state for this protocol
        /// Derived classes must implement this to provide their specific state type
        /// </summary>
        /// <returns>Initial protocol state</returns>
        protected abstract BaseProtocolState CreateInitialState();

        /// <summary>
        /// Initialize the protocol with device context
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        public virtual void Initialize(NetworkDevice device)
        {
            _device = device;
            _state.IsConfigured = true;
            _state.MarkStateChanged();

            device.AddLogEntry($"{Name} protocol initialized");
            OnInitialized();
        }

        /// <summary>
        /// Called after basic initialization is complete
        /// Override to add protocol-specific initialization logic
        /// </summary>
        protected virtual void OnInitialized() { }

        /// <summary>
        /// Core state management pattern from PROTOCOL_STATE_MANAGEMENT.md
        /// Update the protocol state (called periodically by the simulation engine)
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        public virtual async Task UpdateState(NetworkDevice device)
        {
            if (!_state.IsActive || !_state.IsConfigured)
                return;

            try
            {
                // Always update neighbors and timers - these are lightweight operations
                await UpdateNeighbors(device);
                await CleanupStaleNeighbors(device);
                await ProcessTimers(device);

                // Only run expensive operations if state changed
                if (_state.StateChanged)
                {
                    device.AddLogEntry($"{Name}: State changed, running protocol calculations...");
                    await RunProtocolCalculation(device);
                    _state.StateChanged = false;
                    _state.LastUpdate = DateTime.Now;
                }
                else
                {
                    device.AddLogEntry($"{Name}: No state changes detected, skipping expensive calculations.");
                }
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"{Name}: Error during state update: {ex.Message}");
                // Continue execution to prevent one protocol from breaking others
            }
        }

        // Template methods for protocol-specific implementation
        // These provide hooks for derived classes while maintaining the base pattern

        /// <summary>
        /// Update neighbor relationships and discovery
        /// Override to implement protocol-specific neighbor discovery
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task UpdateNeighbors(NetworkDevice device)
        {
            // Default implementation does nothing
            await Task.CompletedTask;
        }

        /// <summary>
        /// Clean up stale neighbors based on timeouts
        /// Uses the base state management for automatic cleanup
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task CleanupStaleNeighbors(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors(GetNeighborTimeoutSeconds());
            foreach (var neighborId in staleNeighbors)
            {
                device.AddLogEntry($"{Name}: Neighbor {neighborId} timed out, removing");
                _state.RemoveNeighbor(neighborId);
                OnNeighborRemoved(neighborId);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Process protocol-specific timers
        /// Override to implement advertisement timers, hello timers, etc.
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task ProcessTimers(NetworkDevice device)
        {
            // Default implementation does nothing
            await Task.CompletedTask;
        }

        /// <summary>
        /// Run expensive protocol calculations (SPF, route selection, etc.)
        /// This is only called when state has changed
        /// </summary>
        /// <param name="device">Network device</param>
        protected abstract Task RunProtocolCalculation(NetworkDevice device);

        /// <summary>
        /// Called when a neighbor is removed due to timeout
        /// Override to add protocol-specific cleanup logic
        /// </summary>
        /// <param name="neighborId">ID of the removed neighbor</param>
        protected virtual void OnNeighborRemoved(string neighborId) { }

        /// <summary>
        /// Get the neighbor timeout in seconds for this protocol
        /// Override to provide protocol-specific timeout values
        /// </summary>
        /// <returns>Timeout in seconds (default 180)</returns>
        protected virtual int GetNeighborTimeoutSeconds() => 180;

        // State access for CLI handlers and monitoring

        /// <summary>
        /// Get the current state of the protocol for CLI handlers and monitoring
        /// </summary>
        /// <returns>Protocol state interface</returns>
        public IProtocolState GetState() => _state;

        /// <summary>
        /// Get the typed state of the protocol
        /// </summary>
        /// <typeparam name="T">The specific state type</typeparam>
        /// <returns>Typed protocol state or null if not available</returns>
        public T GetTypedState<T>() where T : class => _state as T;

        // Configuration management

        /// <summary>
        /// Get the current configuration of the protocol
        /// </summary>
        /// <returns>Protocol configuration object</returns>
        public virtual object GetConfiguration() => GetProtocolConfiguration();

        /// <summary>
        /// Get protocol-specific configuration from the device
        /// Override to access device-specific configuration methods
        /// </summary>
        /// <returns>Protocol configuration</returns>
        protected abstract object GetProtocolConfiguration();

        /// <summary>
        /// Apply new configuration to the protocol
        /// </summary>
        /// <param name="configuration">New configuration to apply</param>
        public virtual void ApplyConfiguration(object configuration)
        {
            OnApplyConfiguration(configuration);
            _state.MarkStateChanged();
        }

        /// <summary>
        /// Apply protocol-specific configuration
        /// Override to handle configuration changes
        /// </summary>
        /// <param name="configuration">Configuration to apply</param>
        protected abstract void OnApplyConfiguration(object configuration);

        // Vendor support

        /// <summary>
        /// Get the list of vendor names this protocol supports
        /// Override for vendor-specific protocols (e.g., EIGRP for Cisco only)
        /// </summary>
        /// <returns>Enumerable of supported vendor names</returns>
        public virtual IEnumerable<string> GetSupportedVendors() => new[] { "Generic" };

        /// <summary>
        /// Check if this protocol supports a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to check</param>
        /// <returns>True if supported, false otherwise</returns>
        public virtual bool SupportsVendor(string vendorName)
        {
            return GetSupportedVendors().Contains(vendorName, StringComparer.OrdinalIgnoreCase);
        }

        // Event subscription from original interface

        /// <summary>
        /// Subscribe to network events for this protocol
        /// </summary>
        /// <param name="eventBus">The network event bus</param>
        /// <param name="self">Reference to the device running this protocol</param>
        public virtual void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            OnSubscribeToEvents(eventBus, self);
        }

        /// <summary>
        /// Protocol-specific event subscription
        /// Override to subscribe to relevant network events
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        /// <param name="self">Device reference</param>
        protected virtual void OnSubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self) { }

        // Utility methods for derived classes

        /// <summary>
        /// Check if a neighbor connection is suitable for this protocol
        /// Uses physical connection metrics for validation
        /// </summary>
        /// <param name="device">Local device</param>
        /// <param name="interfaceName">Local interface name</param>
        /// <param name="neighbor">Remote neighbor device</param>
        /// <returns>True if connection is suitable, false otherwise</returns>
        protected virtual bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }

        /// <summary>
        /// Add a log entry with protocol name prefix
        /// </summary>
        /// <param name="message">Log message</param>
        protected void LogProtocolEvent(string message)
        {
            _device?.AddLogEntry($"{Name}: {message}");
        }

        /// <summary>
        /// Dispose pattern implementation
        /// Override this in derived classes to clean up protocol-specific resources
        /// </summary>
        public virtual void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called when the protocol is being disposed
        /// Override this in derived classes to clean up protocol-specific resources
        /// </summary>
        protected virtual void OnDispose()
        {
            // Default implementation does nothing
        }
    }
}
