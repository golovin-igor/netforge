using System.Globalization;
using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// LldpConfig

namespace NetSim.Simulation.Protocols.Implementations
{
    public class LldpProtocol : INetworkProtocol
    {
        private LldpConfig _lldpConfig;
        private NetworkDevice _device;
        private readonly LldpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _transmissionTimers = new(); // Track transmission timers per interface

        public ProtocolType Type => ProtocolType.LLDP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _lldpConfig = device.GetLldpConfiguration();
            if (_lldpConfig == null)
            {
                device.AddLogEntry("LldpProtocol: LLDP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"LldpProtocol: Successfully initialized with transmit interval {_lldpConfig.TransmitInterval} seconds.");
                
                // Initialize local chassis information
                _state.LocalChassisId = device.Name; // Simplified - use device name as chassis ID
                _state.LocalSystemName = device.Name;
                
                // Initialize transmission timers for all interfaces
                foreach (var interfaceName in device.GetAllInterfaces().Keys)
                {
                    _transmissionTimers[interfaceName] = DateTime.Now;
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
                _device.AddLogEntry($"LldpProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating LLDP state.");
                
                // Mark neighbors as changed when interface states change
                _state.MarkNeighborsChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"LldpProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating LLDP configuration and state.");
                _lldpConfig = _device.GetLldpConfiguration();
                _state.MarkNeighborsChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_lldpConfig == null) _lldpConfig = device.GetLldpConfiguration();

            if (_lldpConfig == null || !_lldpConfig.IsEnabled)
            {
                device.AddLogEntry($"LldpProtocol on {device.Name}: LLDP configuration missing or not enabled.");
                return;
            }

            device.AddLogEntry($"LldpProtocol: Updating LLDP state for device {device.Name}...");
            
            // Update neighbor discovery
            await UpdateNeighborDiscovery(device);
            
            // Process neighbor aging
            await ProcessNeighborAging(device);
            
            // Process transmission timers
            await ProcessTransmissionTimers(device);
            
            // Only process neighbor changes if something changed
            if (_state.NeighborsChanged)
            {
                await ProcessNeighborChanges(device);
                _state.NeighborsChanged = false;
            }
            else
            {
                device.AddLogEntry("LldpProtocol: No neighbor changes detected, skipping neighbor processing.");
            }
            
            device.AddLogEntry("LldpProtocol: LLDP state update completed.");
        }
        
        private async Task UpdateNeighborDiscovery(NetworkDevice device)
        {
            device.AddLogEntry("LldpProtocol: Updating LLDP neighbor discovery...");

            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig == null || interfaceConfig.IsShutdown || !interfaceConfig.IsUp)
                {
                    continue;
                }
                
                // Find potential LLDP neighbors through physical connections
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    
                    // Check if physical connection is suitable for neighbor discovery
                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                    {
                        device.AddLogEntry($"LldpProtocol: Physical connection to {neighborDevice.Name} on {interfaceName} not suitable for LLDP");
                        continue;
                    }
                    
                    // Check if neighbor runs LLDP
                    var neighborLldp = neighborDevice.GetLldpConfiguration();
                    if (neighborLldp != null && neighborLldp.IsEnabled)
                    {
                        // Create neighbor key based on chassis ID and port ID
                        var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                        
                        // Get or create LLDP neighbor
                        var neighbor = _state.GetOrCreateNeighbor(neighborKey, neighborDevice.Name, neighborInterface);
                        
                        // Update neighbor information
                        neighbor.ChassisId = neighborDevice.Name;
                        neighbor.PortId = neighborInterface;
                        neighbor.SystemName = neighborDevice.Name;
                        neighbor.LocalInterface = interfaceName;
                        neighbor.LastUpdateTime = DateTime.Now;
                        
                        // Update neighbor capabilities (simplified)
                        neighbor.SystemCapabilities = new List<string> { "Bridge", "Router" }; // Default capabilities
                        
                        device.AddLogEntry($"LldpProtocol: Discovered neighbor {neighbor.SystemName} on interface {interfaceName}");
                    }
                }
            }
        }
        
        private async Task ProcessNeighborAging(NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            var staleNeighbors = _state.GetStaleNeighbors();
            
            foreach (var neighborKey in staleNeighbors)
            {
                var neighbor = _state.Neighbors[neighborKey];
                device.AddLogEntry($"LldpProtocol: Neighbor {neighbor.SystemName} on interface {neighbor.LocalInterface} aged out");
                _state.RemoveNeighbor(neighbor.ChassisId, neighbor.PortId);
                _state.MarkNeighborsChanged();
            }
        }
        
        private async Task ProcessTransmissionTimers(NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig == null || interfaceConfig.IsShutdown || !interfaceConfig.IsUp)
                {
                    continue;
                }
                
                var timeSinceLastTransmission = currentTime - _transmissionTimers[interfaceName];
                
                if (timeSinceLastTransmission >= TimeSpan.FromSeconds(_lldpConfig.TransmitInterval))
                {
                    await TransmitLldpFrame(interfaceName, device);
                    _transmissionTimers[interfaceName] = currentTime;
                }
            }
        }
        
        private async Task TransmitLldpFrame(string interfaceName, NetworkDevice device)
        {
            device.AddLogEntry($"LldpProtocol: Transmitting LLDP frame on interface {interfaceName}");
            
            // In a real implementation, we would construct and send actual LLDP frames
            // For simulation, we'll just log the activity
            
            // Create LLDP frame content (simplified)
            var lldpFrame = new
            {
                ChassisId = _state.LocalChassisId,
                PortId = interfaceName,
                SystemName = _state.LocalSystemName,
                TimeToLive = _lldpConfig.HoldTime,
                SystemCapabilities = "Bridge,Router" // Default capabilities
            };
            
            device.AddLogEntry($"LldpProtocol: LLDP frame sent on {interfaceName} with chassis ID {lldpFrame.ChassisId}");
        }
        
        private async Task ProcessNeighborChanges(NetworkDevice device)
        {
            device.AddLogEntry("LldpProtocol: Processing LLDP neighbor changes...");
            
            // Update neighbor table and log changes
            foreach (var neighbor in _state.Neighbors.Values)
            {
                device.AddLogEntry($"LldpProtocol: Neighbor {neighbor.SystemName} (chassis: {neighbor.ChassisId}) on interface {neighbor.LocalInterface}");
                
                // In a real implementation, we would:
                // 1. Update management information bases
                // 2. Trigger network topology discovery
                // 3. Update network management systems
                // For simulation, we'll just log the neighbor information
                
                await LogNeighborDetails(neighbor, device);
            }
            
            device.AddLogEntry($"LldpProtocol: Total neighbors discovered: {_state.Neighbors.Count}");
        }
        
        private async Task LogNeighborDetails(LldpNeighbor neighbor, NetworkDevice device)
        {
            device.AddLogEntry($"LldpProtocol: Neighbor details - System: {neighbor.SystemName}, Port: {neighbor.PortId}, Capabilities: {neighbor.SystemCapabilities}");
            
            // Calculate neighbor age
            var neighborAge = DateTime.Now - neighbor.LastUpdateTime;
            device.AddLogEntry(string.Format(CultureInfo.InvariantCulture, "LldpProtocol: Neighbor {0} last seen {1:F0} seconds ago", neighbor.SystemName, neighborAge.TotalSeconds));
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
