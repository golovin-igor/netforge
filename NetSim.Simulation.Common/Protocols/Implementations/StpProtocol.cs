using NetSim.Simulation.Common;
using NetSim.Simulation.Common.Configuration;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// StpConfig

namespace NetSim.Simulation.Protocols.Implementations
{
    public class StpProtocol : INetworkProtocol
    {
        private StpConfig _stpConfig;
        private NetworkDevice _device;
        private readonly StpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _bpduTimers = new(); // Track BPDU timers per interface

        public ProtocolType Type => ProtocolType.STP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _stpConfig = device.GetStpConfiguration();
            if (_stpConfig == null)
            {
                device.AddLogEntry("StpProtocol: STP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"StpProtocol: Successfully initialized with bridge priority {_stpConfig.DefaultPriority}.");
                
                // Initialize bridge ID
                _state.BridgeId = _stpConfig.DefaultPriority.ToString();
                _state.BridgePriority = _stpConfig.DefaultPriority;
                
                // Initialize port states for all interfaces
                foreach (var interfaceName in device.GetAllInterfaces().Keys)
                {
                    var port = _state.GetOrCreatePortState(interfaceName);
                    // Remove port.PortId assignment as it's not a property of StpPortState
                    _bpduTimers[interfaceName] = DateTime.Now;
                }
                
                // Mark topology as changed to trigger initial STP calculation
                _state.MarkTopologyChanged();
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
                _device.AddLogEntry($"StpProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating STP state.");
                
                // Mark topology as changed when interface states change
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"StpProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating STP configuration and state.");
                _stpConfig = _device.GetStpConfiguration();
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_stpConfig == null) _stpConfig = device.GetStpConfiguration();

            if (_stpConfig == null || !_stpConfig.IsEnabled)
            {
                device.AddLogEntry($"StpProtocol on {device.Name}: STP configuration missing or not enabled.");
                return;
            }

            device.AddLogEntry($"StpProtocol: Updating STP state for bridge {_state.BridgeId} on device {device.Name}...");
            
            // Update port states
            await UpdatePortStates(device);
            
            // Process BPDU timers
            await ProcessBpduTimers(device);
            
            // Only run STP algorithm if topology changed
            if (_state.TopologyChanged)
            {
                await RunStpAlgorithm(device);
                _state.TopologyChanged = false;
                _state.LastTopologyChange = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("StpProtocol: No topology changes detected, skipping STP algorithm.");
            }
            
            device.AddLogEntry("StpProtocol: STP state update completed.");
        }
        
        private async Task UpdatePortStates(NetworkDevice device)
        {
            device.AddLogEntry("StpProtocol: Updating STP port states...");

            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig == null) continue;
                
                var port = _state.GetOrCreatePortState(interfaceName);
                
                // Update port configuration
                // Remove port.PortId assignment as it's not a property of StpPortState
                port.PathCost = CalculatePathCost(interfaceConfig);
                
                // Check if interface is operational
                if (interfaceConfig.IsShutdown || !interfaceConfig.IsUp)
                {
                    if (port.State != StpPortStateType.Disabled)
                    {
                        port.ChangeState(StpPortStateType.Disabled);
                        _state.MarkTopologyChanged();
                        device.AddLogEntry($"StpProtocol: Port {interfaceName} disabled due to interface down");
                    }
                }
                else
                {
                    // Initialize port state if it was disabled
                    if (port.State == StpPortStateType.Disabled)
                    {
                        port.ChangeState(StpPortStateType.Blocking);
                        _state.MarkTopologyChanged();
                        device.AddLogEntry($"StpProtocol: Port {interfaceName} initialized to Blocking state");
                    }
                    
                    // Progress port state machine
                    await ProgressPortState(port, device);
                }
                
                device.AddLogEntry($"StpProtocol: Port {port.PortName} in state {port.State}, role {port.Role}");
            }
        }
        
        private int CalculatePathCost(InterfaceConfig interfaceConfig)
        {
            // Standard STP path cost calculation based on interface speed
            // Default to 100 for simulation
            return 100;
        }
        
        private async Task ProgressPortState(StpPortState port, NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            var timeSinceLastTransition = currentTime - port.StateChangeTime;
            
            // Simplified STP port state machine
            switch (port.State)
            {
                case StpPortStateType.Blocking:
                    // Check if we should transition to Listening
                    if (port.Role == StpPortRole.Root || port.Role == StpPortRole.Designated)
                    {
                        if (timeSinceLastTransition >= TimeSpan.FromSeconds(_stpConfig.MaxAge))
                        {
                            port.ChangeState(StpPortStateType.Listening);
                            _state.MarkTopologyChanged();
                            device.AddLogEntry($"StpProtocol: Port {port.PortName} transitioned to Listening");
                        }
                    }
                    break;
                    
                case StpPortStateType.Listening:
                    // Transition to Learning after Forward Delay
                    if (timeSinceLastTransition >= TimeSpan.FromSeconds(_stpConfig.ForwardDelay))
                    {
                        port.ChangeState(StpPortStateType.Learning);
                        _state.MarkTopologyChanged();
                        device.AddLogEntry($"StpProtocol: Port {port.PortName} transitioned to Learning");
                    }
                    break;
                    
                case StpPortStateType.Learning:
                    // Transition to Forwarding after Forward Delay
                    if (timeSinceLastTransition >= TimeSpan.FromSeconds(_stpConfig.ForwardDelay))
                    {
                        port.ChangeState(StpPortStateType.Forwarding);
                        _state.MarkTopologyChanged();
                        device.AddLogEntry($"StpProtocol: Port {port.PortName} transitioned to Forwarding");
                    }
                    break;
                    
                case StpPortStateType.Forwarding:
                    // Port is forwarding - maintain state
                    break;
                    
                case StpPortStateType.Disabled:
                    // Port is disabled - no state transitions
                    break;
            }
        }
        
        private async Task ProcessBpduTimers(NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            
            foreach (var port in _state.PortStates.Values)
            {
                if (port.State == StpPortStateType.Disabled) continue;
                
                var timeSinceLastBpdu = currentTime - _bpduTimers[port.PortName];
                
                // Send BPDU if we are designated root or designated bridge
                if (port.Role == StpPortRole.Designated && timeSinceLastBpdu >= TimeSpan.FromSeconds(_stpConfig.HelloTime))
                {
                    await SendBpdu(port, device);
                    _bpduTimers[port.PortName] = currentTime;
                }
                
                // Check for BPDU timeout
                if (timeSinceLastBpdu > TimeSpan.FromSeconds(_stpConfig.MaxAge))
                {
                    device.AddLogEntry($"StpProtocol: BPDU timeout on port {port.PortName}");
                    _state.MarkTopologyChanged();
                }
            }
        }
        
        private async Task SendBpdu(StpPortState port, NetworkDevice device)
        {
            device.AddLogEntry($"StpProtocol: Sending BPDU on port {port.PortName}");
            
            // In a real implementation, we would construct and send actual BPDU packets
            // For simulation, we'll just log the activity
            
            device.AddLogEntry($"StpProtocol: BPDU sent on port {port.PortName} with root bridge {_state.RootBridgeId}");
        }
        
        private async Task RunStpAlgorithm(NetworkDevice device)
        {
            device.AddLogEntry("StpProtocol: Running STP algorithm due to topology change...");
            
            // Step 1: Determine root bridge
            await DetermineRootBridge(device);
            
            // Step 2: Calculate root path cost
            await CalculateRootPathCost(device);
            
            // Step 3: Determine port roles
            await DeterminePortRoles(device);
            
            // Step 4: Set port states based on roles
            await SetPortStates(device);
            
            device.AddLogEntry($"StpProtocol: STP algorithm completed, root bridge: {_state.RootBridgeId}");
        }
        
        private async Task DetermineRootBridge(NetworkDevice device)
        {
            // Simplified root bridge selection - lowest bridge ID wins
            // In a real implementation, we would compare with received BPDUs
            
            _state.RootBridgeId = _state.BridgeId; // Assume we are root initially
            _state.RootPathCost = 0;
            
            device.AddLogEntry($"StpProtocol: Determined root bridge: {_state.RootBridgeId}");
        }
        
        private async Task CalculateRootPathCost(NetworkDevice device)
        {
            // Simplified path cost calculation
            // In a real implementation, we would calculate based on received BPDUs
            
            if (_state.RootBridgeId == _state.BridgeId)
            {
                _state.RootPathCost = 0; // We are the root
            }
            else
            {
                _state.RootPathCost = 100; // Default cost to root
            }
            
            device.AddLogEntry($"StpProtocol: Root path cost: {_state.RootPathCost}");
        }
        
        private async Task DeterminePortRoles(NetworkDevice device)
        {
            device.AddLogEntry("StpProtocol: Determining port roles...");
            
            foreach (var port in _state.PortStates.Values)
            {
                if (port.State == StpPortStateType.Disabled) continue;
                
                // Simplified port role assignment
                if (_state.RootBridgeId == _state.BridgeId)
                {
                    // We are the root bridge - all ports are designated
                    port.Role = StpPortRole.Designated;
                    device.AddLogEntry($"StpProtocol: Port {port.PortName} assigned Designated role (root bridge)");
                }
                else
                {
                    // We are not the root - simplified role assignment
                    // In a real implementation, we would compare path costs and bridge IDs
                    port.Role = StpPortRole.Root; // Simplified - assume first port is root port
                    device.AddLogEntry($"StpProtocol: Port {port.PortName} assigned Root role");
                }
            }
        }
        
        private async Task SetPortStates(NetworkDevice device)
        {
            device.AddLogEntry("StpProtocol: Setting port states based on roles...");
            
            foreach (var port in _state.PortStates.Values)
            {
                if (port.State == StpPortStateType.Disabled) continue;
                
                // Set port state based on role
                switch (port.Role)
                {
                    case StpPortRole.Root:
                    case StpPortRole.Designated:
                        if (port.State == StpPortStateType.Blocking)
                        {
                            port.ChangeState(StpPortStateType.Listening);
                            device.AddLogEntry($"StpProtocol: Port {port.PortName} transitioned to Listening (role: {port.Role})");
                        }
                        break;
                        
                    case StpPortRole.Backup:
                    case StpPortRole.Alternate:
                        if (port.State != StpPortStateType.Blocking)
                        {
                            port.ChangeState(StpPortStateType.Blocking);
                            device.AddLogEntry($"StpProtocol: Port {port.PortName} transitioned to Blocking (role: {port.Role})");
                        }
                        break;
                                 }
             }
         }
         
         private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
         {
             var connection = device.GetPhysicalConnectionMetrics(interfaceName);
             return connection?.IsSuitableForRouting ?? false;
         }
    }
} 
