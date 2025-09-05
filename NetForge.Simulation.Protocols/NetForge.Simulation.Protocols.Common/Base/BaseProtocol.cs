using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common.Metrics;
using NetForge.Simulation.Protocols.Common.Base;
using System.Diagnostics;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common
{
    /// <summary>
    /// Base implementation of network protocols following the state management pattern
    /// from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md and implementing the unified IDeviceProtocol interface
    /// Also implements INetworkProtocol for backward compatibility
    /// </summary>
    public abstract class BaseProtocol : IDeviceProtocol, IDisposable
    {
        protected INetworkDevice _device;
        protected readonly BaseProtocolState _state;
        protected readonly ProtocolMetrics _metrics;

        // Abstract properties that derived classes must implement

        public abstract string Name { get; }
        public virtual string Version => "1.0.0";

        protected BaseProtocol()
        {
            _state = CreateInitialState();
            _metrics = new ProtocolMetrics();
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
        public virtual void Initialize(INetworkDevice device)
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
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        /// Core state management pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
        /// Update the protocol state (called periodically by the simulation engine)
        /// Enhanced with performance tracking and metrics collection
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        public virtual async Task UpdateState(INetworkDevice device)
        {
            if (!_state.IsActive || !_state.IsConfigured)
                return;

            var stopwatch = Stopwatch.StartNew();
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

                _metrics.RecordProcessingTime(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                _metrics.RecordError($"Error during state update: {ex.Message}");
                device.AddLogEntry($"{Name}: Error during state update: {ex.Message}");
                // Continue execution to prevent one protocol from breaking others
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        // Template methods for protocol-specific implementation
        // These provide hooks for derived classes while maintaining the base pattern

        /// <summary>
        /// Update neighbor relationships and discovery
        /// Override to implement protocol-specific neighbor discovery
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task UpdateNeighbors(INetworkDevice device)
        {
            // Default implementation does nothing
            await Task.CompletedTask;
        }

        /// <summary>
        /// Clean up stale neighbors based on timeouts
        /// Uses the base state management for automatic cleanup
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task CleanupStaleNeighbors(INetworkDevice device)
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
        protected virtual async Task ProcessTimers(INetworkDevice device)
        {
            // Default implementation does nothing
            await Task.CompletedTask;
        }

        /// <summary>
        /// Run expensive protocol calculations (SPF, route selection, etc.)
        /// This is only called when state has changed
        /// </summary>
        /// <param name="device">Network device</param>
        protected abstract Task RunProtocolCalculation(INetworkDevice device);

        /// <summary>
        /// Called when a neighbor is removed due to timeout
        /// Override to add protocol-specific cleanup logic
        /// </summary>
        /// <param name="neighborId">ID of the removed neighbor</param>
        protected virtual void OnNeighborRemoved(string neighborId)
        {
        }

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
        public IProtocolState GetState() => _state as IProtocolState;

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

        // Protocol lifecycle management
        /// <summary>
        /// Start the protocol
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        public virtual async Task<bool> Start()
        {
            try
            {
                _state.IsActive = true;
                _state.MarkStateChanged();
                LogProtocolEvent("Protocol started");
                await OnStart();
                return true;
            }
            catch (Exception ex)
            {
                _metrics.RecordError($"Failed to start protocol: {ex.Message}");
                LogProtocolEvent($"Failed to start protocol: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stop the protocol
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        public virtual async Task<bool> Stop()
        {
            try
            {
                _state.IsActive = false;
                _state.MarkStateChanged();
                LogProtocolEvent("Protocol stopped");
                await OnStop();
                return true;
            }
            catch (Exception ex)
            {
                _metrics.RecordError($"Failed to stop protocol: {ex.Message}");
                LogProtocolEvent($"Failed to stop protocol: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Configure the protocol with new settings
        /// </summary>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>Task representing the async operation</returns>
        public virtual async Task<bool> Configure(object configuration)
        {
            try
            {
                ApplyConfiguration(configuration);
                LogProtocolEvent("Protocol configured");
                await OnConfigure(configuration);
                return true;
            }
            catch (Exception ex)
            {
                _metrics.RecordError($"Failed to configure protocol: {ex.Message}");
                LogProtocolEvent($"Failed to configure protocol: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Called when the protocol is started
        /// Override for protocol-specific startup logic
        /// </summary>
        protected virtual async Task OnStart()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when the protocol is stopped
        /// Override for protocol-specific shutdown logic
        /// </summary>
        protected virtual async Task OnStop()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when the protocol is configured
        /// Override for protocol-specific configuration logic
        /// </summary>
        /// <param name="configuration">Configuration applied</param>
        protected virtual async Task OnConfigure(object configuration)
        {
            await Task.CompletedTask;
        }

        // Vendor support

        /// <summary>
        /// Get the list of vendor names this protocol supports as a property
        /// </summary>
        public virtual IEnumerable<string> SupportedVendors => GetSupportedVendors();

        public virtual NetworkProtocolType Type { get; set; }

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
        public virtual void SubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
        {
            OnSubscribeToEvents(eventBus, self);
        }

        /// <summary>
        /// Protocol-specific event subscription
        /// Override to subscribe to relevant network events
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        /// <param name="self">Device reference</param>
        protected virtual void OnSubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
        {
        }

        // Protocol dependencies and compatibility - New IDeviceProtocol methods
        /// <summary>
        /// Get protocols that this protocol depends on
        /// Override to specify protocol dependencies
        /// </summary>
        /// <returns>Enumerable of required protocol types</returns>
        public virtual IEnumerable<NetworkProtocolType> GetDependencies()
        {
            return Enumerable.Empty<NetworkProtocolType>();
        }

        /// <summary>
        /// Get protocols that conflict with this protocol
        /// Override to specify protocol conflicts
        /// </summary>
        /// <returns>Enumerable of conflicting protocol types</returns>
        public virtual IEnumerable<NetworkProtocolType> GetConflicts()
        {
            return Enumerable.Empty<NetworkProtocolType>();
        }

        /// <summary>
        /// Check if this protocol can coexist with another protocol
        /// Default implementation checks conflict list
        /// </summary>
        /// <param name="otherNetworkProtocol">Other protocol type to check</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        public virtual bool CanCoexistWith(NetworkProtocolType otherNetworkProtocol)
        {
            return !GetConflicts().Contains(otherNetworkProtocol);
        }

        // Performance monitoring
        /// <summary>
        /// Get performance metrics for this protocol
        /// </summary>
        /// <returns>Protocol metrics interface</returns>
        public virtual IProtocolMetrics GetMetrics()
        {
            return _metrics;
        }

        /// <summary>
        /// Get performance metrics for this protocol (unified interface compatibility)
        /// </summary>
        /// <returns>Protocol metrics as object</returns>
        object IDeviceProtocol.GetMetrics()
        {
            return _metrics;
        }

        // Utility methods for derived classes

        /// <summary>
        /// Check if a neighbor connection is suitable for this protocol
        /// Uses physical connection metrics for validation
        /// </summary>
        /// <param name="device">Local device</param>
        /// <param name="interfaceName">Local interface name</param>
        /// <param name="neighbor">Remote neighbor device</param>
        /// <returns>True if connection is suitable, false otherwise</returns>
        protected virtual bool IsNeighborReachable(INetworkDevice device, string interfaceName, INetworkDevice neighbor)
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
