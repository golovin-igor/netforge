using System.Globalization;
using NetForge.Simulation.Common;
using NetForge.Simulation.Events;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Routing;
// CdpConfig

namespace NetForge.Simulation.Protocols.Implementations
{
    /// <summary>
    /// Legacy CDP Protocol implementation - DEPRECATED
    /// Use NetForge.Simulation.Protocols.CDP.CdpProtocol instead
    /// </summary>
    [Obsolete("This legacy CDP implementation is deprecated. Use NetForge.Simulation.Protocols.CDP.CdpProtocol from the new plugin-based architecture instead.", false)]
    public class CdpProtocol : INetworkProtocol
    {
        private CdpConfig _cdpConfig;
        private NetworkDevice _device;
        private readonly CdpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _advertisementTimers = new(); // Track advertisement timers per interface

        public ProtocolType Type => ProtocolType.CDP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _cdpConfig = device.GetCdpConfiguration();
            if (_cdpConfig == null)
            {
                device.AddLogEntry("CdpProtocol: CDP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"CdpProtocol: Successfully initialized with timer {_cdpConfig.Timer} seconds.");
                
                // Initialize local device information
                _state.LocalDeviceId = device.Name;
                _state.LocalPlatform = device.Vendor; // Use vendor as platform
                
                // Initialize advertisement timers for all interfaces
                foreach (var interfaceName in device.GetAllInterfaces().Keys)
                {
                    _advertisementTimers[interfaceName] = DateTime.Now;
                }
                
                // Mark neighbors as changed to trigger initial neighbor discovery
                _state.MarkNeighborsChanged();
            }
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            eventBus.Subscribe<InterfaceStateChangedEventArgs>(HandleInterfaceStateChangeAsync);
            eventBus.Subscribe<ProtocolConfigChangedEventArgs>(HandleProtocolConfigChangeAsync);
        }

        private async Task HandleInterfaceStateChangeAsync(InterfaceStateChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name)
            {
                _device.AddLogEntry($"CdpProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating CDP state.");
                
                // Mark neighbors as changed when interface states change
                _state.MarkNeighborsChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"CdpProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating CDP configuration and state.");
                _cdpConfig = _device.GetCdpConfiguration();
                _state.MarkNeighborsChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_cdpConfig == null) _cdpConfig = device.GetCdpConfiguration();

            if (_cdpConfig == null || !_cdpConfig.IsEnabled)
            {
                device.AddLogEntry($"CdpProtocol on {device.Name}: CDP configuration missing or not enabled.");
                return;
            }

            device.AddLogEntry($"CdpProtocol: Updating CDP state for device {device.Name}...");
            
            // Update neighbor discovery
            await UpdateNeighborDiscovery(device);
            
            // Process neighbor aging
            await ProcessNeighborAging(device);
            
            // Process advertisement timers
            await ProcessAdvertisementTimers(device);
            
            // Only process neighbor changes if something changed
            if (_state.NeighborsChanged)
            {
                await ProcessNeighborChanges(device);
                _state.NeighborsChanged = false;
            }
            else
            {
                device.AddLogEntry("CdpProtocol: No neighbor changes detected, skipping neighbor processing.");
            }
            
            device.AddLogEntry("CdpProtocol: CDP state update completed.");
        }
        
        private async Task UpdateNeighborDiscovery(NetworkDevice device)
        {
            device.AddLogEntry("CdpProtocol: Updating CDP neighbor discovery...");

            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig == null || interfaceConfig.IsShutdown || !interfaceConfig.IsUp)
                {
                    continue;
                }
                
                // Find potential CDP neighbors through physical connections
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    
                    // Check if physical connection is suitable for neighbor discovery
                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                    {
                        device.AddLogEntry($"CdpProtocol: Physical connection to {neighborDevice.Name} on {interfaceName} not suitable for CDP");
                        continue;
                    }
                    
                    // Check if neighbor runs CDP (CDP is typically Cisco-specific)
                    var neighborCdp = neighborDevice.GetCdpConfiguration();
                    if (neighborCdp != null && neighborCdp.IsEnabled)
                    {
                        var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                        
                        // Get or create CDP neighbor
                        var neighbor = _state.GetOrCreateNeighbor(neighborKey, neighborDevice.Name, neighborInterface);
                        
                        // Update neighbor information
                        neighbor.DeviceId = neighborDevice.Name;
                        neighbor.Platform = neighborDevice.Vendor;
                        neighbor.RemoteInterface = neighborInterface;
                        neighbor.LocalInterface = interfaceName;
                        neighbor.LastUpdateTime = DateTime.Now;
                        
                        // Update neighbor capabilities (simplified)
                        neighbor.Capabilities = new List<string> { "Router", "Switch" }; // Default CDP capabilities
                        
                        device.AddLogEntry($"CdpProtocol: Discovered neighbor {neighbor.DeviceId} on interface {interfaceName}");
                    }
                }
            }
        }
        
        private async Task ProcessNeighborAging(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors();
            
            foreach (var neighborKey in staleNeighbors)
            {
                var neighbor = _state.Neighbors[neighborKey];
                device.AddLogEntry($"CdpProtocol: Neighbor {neighbor.DeviceId} on interface {neighbor.LocalInterface} aged out");
                _state.RemoveNeighbor(neighborKey);
                _state.MarkNeighborsChanged();
            }
        }
        
        private async Task ProcessAdvertisementTimers(NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig == null || interfaceConfig.IsShutdown || !interfaceConfig.IsUp)
                {
                    continue;
                }
                
                var timeSinceLastAdvertisement = currentTime - _advertisementTimers[interfaceName];
                
                if (timeSinceLastAdvertisement >= TimeSpan.FromSeconds(_cdpConfig.Timer))
                {
                    await SendCdpAdvertisement(interfaceName, device);
                    _advertisementTimers[interfaceName] = currentTime;
                }
            }
        }
        
        private async Task SendCdpAdvertisement(string interfaceName, NetworkDevice device)
        {
            device.AddLogEntry($"CdpProtocol: Sending CDP advertisement on interface {interfaceName}");
            
            // In a real implementation, we would construct and send actual CDP packets
            // For simulation, we'll just log the activity
            
            // Create CDP advertisement content (simplified)
            var cdpAdvertisement = new
            {
                DeviceId = _state.LocalDeviceId,
                Platform = _state.LocalPlatform,
                Interface = interfaceName,
                Capabilities = "Router Switch",
                Version = "Simulation Version 1.0"
            };
            
            device.AddLogEntry($"CdpProtocol: CDP advertisement sent on {interfaceName} with device ID {cdpAdvertisement.DeviceId}");
        }
        
        private async Task ProcessNeighborChanges(NetworkDevice device)
        {
            device.AddLogEntry("CdpProtocol: Processing CDP neighbor changes...");
            
            // Update neighbor table and log changes
            foreach (var neighbor in _state.Neighbors.Values)
            {
                device.AddLogEntry($"CdpProtocol: Neighbor {neighbor.DeviceId} (platform: {neighbor.Platform}) on interface {neighbor.LocalInterface}");
                
                // In a real implementation, we would:
                // 1. Update CDP neighbor database
                // 2. Trigger network topology discovery
                // 3. Update network management systems
                // For simulation, we'll just log the neighbor information
                
                await LogNeighborDetails(neighbor, device);
            }
            
            device.AddLogEntry($"CdpProtocol: Total CDP neighbors discovered: {_state.Neighbors.Count}");
        }
        
        private async Task LogNeighborDetails(CdpNeighbor neighbor, NetworkDevice device)
        {
            device.AddLogEntry($"CdpProtocol: Neighbor details - Device: {neighbor.DeviceId}, Platform: {neighbor.Platform}, Remote Interface: {neighbor.RemoteInterface}, Capabilities: {neighbor.Capabilities}");
            
            // Calculate neighbor age
            var neighborAge = DateTime.Now - neighbor.LastUpdateTime;
            device.AddLogEntry(string.Format(CultureInfo.InvariantCulture, "CdpProtocol: Neighbor {0} last seen {1:F0} seconds ago", neighbor.DeviceId, neighborAge.TotalSeconds));
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
