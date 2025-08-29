using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.Protocols.Common.State;

namespace NetForge.Simulation.Protocols.Common.Base
{
    /// <summary>
    /// Base class for Layer 2 discovery protocols (CDP, LLDP)
    /// Provides common discovery functionality and standardized neighbor discovery behavior
    /// </summary>
    public abstract class BaseDiscoveryProtocol : BaseProtocol
    {
        protected readonly Dictionary<string, object> _discoveredDevices = new();
        protected readonly Dictionary<string, DateTime> _lastAdvertisement = new();
        protected DateTime _lastDiscoveryRun = DateTime.MinValue;

        /// <summary>
        /// Interval between discovery advertisements in seconds
        /// Override to set protocol-specific advertisement interval
        /// </summary>
        public abstract int AdvertisementInterval { get; }

        /// <summary>
        /// Hold time for discovered neighbors in seconds
        /// Override to set protocol-specific hold time
        /// </summary>
        public abstract int HoldTime { get; }

        /// <summary>
        /// Whether this discovery protocol can discover devices across VLANs
        /// </summary>
        public virtual bool SupportsVLANDiscovery => false;

        /// <summary>
        /// Maximum number of devices this protocol can discover
        /// </summary>
        public virtual int MaxDiscoveredDevices => 100;

        /// <summary>
        /// Create the initial discovery protocol state
        /// </summary>
        protected override BaseProtocolState CreateInitialState()
        {
            return new DiscoveryProtocolState();
        }

        /// <summary>
        /// Get the discovery protocol specific state
        /// </summary>
        /// <returns>Discovery protocol state</returns>
        public IDiscoveryProtocolState GetDiscoveryState()
        {
            return _state as IDiscoveryProtocolState;
        }

        /// <summary>
        /// Core discovery protocol operation - called when state changes
        /// Implements common discovery logic while allowing protocol-specific customization
        /// </summary>
        /// <param name="device">Network device</param>
        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            try
            {
                // Step 1: Collect device information for advertisement
                var deviceInfo = await CollectDeviceInformation(device);

                // Step 2: Send discovery advertisements on active interfaces
                await SendDiscoveryAdvertisements(device, deviceInfo);

                // Step 3: Process received discovery information
                await ProcessDiscoveryInformation(device);

                // Step 4: Update discovered device database
                await UpdateDiscoveredDevices(device);

                // Step 5: Clean up stale discoveries
                await CleanupStaleDiscoveries();

                // Update discovery state
                if (_state is DiscoveryProtocolState discoveryState)
                {
                    discoveryState.DiscoveredDevices = _discoveredDevices.Count;
                    discoveryState.LastDiscoveryAdvertisement = DateTime.Now;
                }

                _lastDiscoveryRun = DateTime.Now;
                LogProtocolEvent($"Discovery cycle completed: {_discoveredDevices.Count} devices discovered");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error in discovery operation: {ex.Message}");
                _metrics.RecordError($"Discovery operation failed: {ex.Message}");
            }
        }

        // Abstract methods for protocol-specific implementation

        /// <summary>
        /// Collect device information for advertisement
        /// Override to implement protocol-specific device information collection
        /// </summary>
        /// <param name="device">Network device</param>
        /// <returns>Device information for advertisement</returns>
        protected abstract Task<Dictionary<string, object>> CollectDeviceInformation(INetworkDevice device);

        /// <summary>
        /// Send discovery advertisements on active interfaces
        /// Override to implement protocol-specific advertisement format and transmission
        /// </summary>
        /// <param name="device">Network device</param>
        /// <param name="deviceInfo">Device information to advertise</param>
        protected abstract Task SendDiscoveryAdvertisements(INetworkDevice device, Dictionary<string, object> deviceInfo);

        /// <summary>
        /// Process received discovery information from neighbors
        /// Override to implement protocol-specific discovery packet processing
        /// </summary>
        /// <param name="device">Network device</param>
        protected abstract Task ProcessDiscoveryInformation(INetworkDevice device);

        /// <summary>
        /// Update the discovered devices database
        /// Implements common device database management
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task UpdateDiscoveredDevices(INetworkDevice device)
        {
            // This would be called by ProcessDiscoveryInformation to update the database
            // Base implementation does nothing - derived classes handle actual updates
            await Task.CompletedTask;
        }

        /// <summary>
        /// Add or update a discovered device
        /// </summary>
        /// <param name="deviceId">Unique device identifier</param>
        /// <param name="deviceInfo">Device information</param>
        /// <param name="interfaceName">Interface where device was discovered</param>
        protected virtual void AddDiscoveredDevice(string deviceId, Dictionary<string, object> deviceInfo, string interfaceName)
        {
            if (_discoveredDevices.Count >= MaxDiscoveredDevices)
            {
                LogProtocolEvent($"Maximum discovered devices limit ({MaxDiscoveredDevices}) reached");
                return;
            }

            var discoveryEntry = new Dictionary<string, object>(deviceInfo)
            {
                ["DiscoveredAt"] = DateTime.Now,
                ["Interface"] = interfaceName,
                ["LastSeen"] = DateTime.Now
            };

            var wasNew = !_discoveredDevices.ContainsKey(deviceId);
            _discoveredDevices[deviceId] = discoveryEntry;
            _lastAdvertisement[deviceId] = DateTime.Now;

            LogProtocolEvent($"{(wasNew ? "Discovered new" : "Updated")} device: {deviceId} on {interfaceName}");
            _state.MarkStateChanged();
        }

        /// <summary>
        /// Remove stale discoveries based on hold time
        /// </summary>
        protected virtual async Task CleanupStaleDiscoveries()
        {
            var now = DateTime.Now;
            var staleDevices = _lastAdvertisement
                .Where(kvp => (now - kvp.Value).TotalSeconds > HoldTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var deviceId in staleDevices)
            {
                _discoveredDevices.Remove(deviceId);
                _lastAdvertisement.Remove(deviceId);
                LogProtocolEvent($"Removed stale discovery: {deviceId}");
            }

            if (staleDevices.Any())
            {
                _state.MarkStateChanged();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get the current discovered devices
        /// </summary>
        /// <returns>Dictionary of discovered devices</returns>
        public Dictionary<string, object> GetDiscoveredDevices()
        {
            return new Dictionary<string, object>(_discoveredDevices);
        }

        /// <summary>
        /// Get discovered devices on a specific interface
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <returns>Devices discovered on the interface</returns>
        public Dictionary<string, object> GetDiscoveredDevicesOnInterface(string interfaceName)
        {
            return _discoveredDevices
                .Where(kvp => kvp.Value is Dictionary<string, object> info &&
                             info.ContainsKey("Interface") &&
                             info["Interface"].ToString() == interfaceName)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Check if discovery is enabled on a specific interface
        /// Override to implement protocol-specific interface filtering
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <returns>True if discovery is enabled, false otherwise</returns>
        protected virtual bool IsDiscoveryEnabledOnInterface(string interfaceName)
        {
            // Default implementation enables discovery on all active interfaces
            return true;
        }

        /// <summary>
        /// Get interfaces where discovery is currently active
        /// </summary>
        /// <returns>List of active interface names</returns>
        public List<string> GetActiveInterfaces()
        {
            // This would be populated based on device interface configuration
            // For now, return empty list - derived classes should override
            return new List<string>();
        }

        /// <summary>
        /// Process timer events for discovery protocols
        /// Implements common timer functionality for advertisements and cleanup
        /// </summary>
        /// <param name="device">Network device</param>
        protected override async Task ProcessTimers(INetworkDevice device)
        {
            var now = DateTime.Now;

            // Check if it's time for next advertisement
            if ((now - _lastDiscoveryRun).TotalSeconds >= AdvertisementInterval)
            {
                _state.MarkStateChanged(); // Trigger discovery cycle
            }

            // Clean up stale discoveries periodically
            await CleanupStaleDiscoveries();

            // Call protocol-specific timer processing
            await ProcessDiscoveryTimers(device);
        }

        /// <summary>
        /// Process protocol-specific discovery timers
        /// Override to implement additional timer-based functionality
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task ProcessDiscoveryTimers(INetworkDevice device)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup discovery-specific resources on disposal
        /// </summary>
        protected override void OnDispose()
        {
            _discoveredDevices.Clear();
            _lastAdvertisement.Clear();
            base.OnDispose();
        }
    }

    /// <summary>
    /// Concrete implementation of discovery protocol state
    /// </summary>
    public class DiscoveryProtocolState : BaseProtocolState, IDiscoveryProtocolState
    {
        public int DiscoveredDevices { get; set; }
        public TimeSpan DiscoveryInterval { get; set; }
        public DateTime LastDiscoveryAdvertisement { get; set; } = DateTime.MinValue;
        public TimeSpan HoldTime { get; set; }
        public bool DiscoveryEnabled { get; set; } = true;

        public Dictionary<string, object> GetDiscoveredDevices()
        {
            // This would be populated by the discovery protocol
            return new Dictionary<string, object>();
        }

        public IEnumerable<string> GetActiveInterfaces()
        {
            // This would be populated based on interface configuration
            return new List<string>();
        }

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["DiscoveredDevices"] = DiscoveredDevices;
            baseData["DiscoveryInterval"] = DiscoveryInterval.TotalSeconds;
            baseData["LastDiscoveryAdvertisement"] = LastDiscoveryAdvertisement;
            baseData["HoldTime"] = HoldTime.TotalSeconds;
            baseData["DiscoveryEnabled"] = DiscoveryEnabled;
            return baseData;
        }
    }
}
