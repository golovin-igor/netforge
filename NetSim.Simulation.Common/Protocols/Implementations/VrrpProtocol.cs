using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// VrrpConfig

namespace NetSim.Simulation.Protocols.Implementations
{
    public class VrrpProtocol : INetworkProtocol
    {
        private VrrpConfig _vrrpConfig;
        private NetworkDevice _device;
        private readonly VrrpState _state = new(); // Protocol-specific state
        private readonly Dictionary<int, DateTime> _advertisementTimers = new(); // Track advertisement timers per VRID

        public ProtocolType Type => ProtocolType.VRRP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _vrrpConfig = device.GetVrrpConfiguration();
            if (_vrrpConfig == null)
            {
                device.AddLogEntry("VrrpProtocol: VRRP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"VrrpProtocol: Successfully initialized with {_vrrpConfig.Groups.Count} VRRP groups.");
                
                // Initialize group states
                foreach (var group in _vrrpConfig.Groups.Values)
                {
                    _state.GetOrCreateGroupState(group.GroupId, group.Interface);
                    _advertisementTimers[group.GroupId] = DateTime.Now;
                }
                
                // Mark state as changed to trigger initial state processing
                _state.MarkStateChanged();
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
                _device.AddLogEntry($"VrrpProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating VRRP state.");
                
                // Mark state as changed when interface states change
                _state.MarkStateChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"VrrpProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating VRRP configuration and state.");
                _vrrpConfig = _device.GetVrrpConfiguration();
                _state.MarkStateChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_vrrpConfig == null) _vrrpConfig = device.GetVrrpConfiguration();

            if (_vrrpConfig == null || _vrrpConfig.Groups.Count == 0)
            {
                device.AddLogEntry($"VrrpProtocol on {device.Name}: VRRP configuration missing or no VRRP groups configured.");
                return;
            }

            device.AddLogEntry($"VrrpProtocol: Updating VRRP state for {_vrrpConfig.Groups.Count} VRRP groups on device {device.Name}...");
            
            // Update virtual router states
            await UpdateVirtualRouterStates(device);
            
            // Process advertisement timers
            await ProcessAdvertisementTimers(device);
            
            // Only process state changes if something changed
            if (_state.StateChanged)
            {
                await ProcessStateChanges(device);
                _state.StateChanged = false;
            }
            else
            {
                device.AddLogEntry("VrrpProtocol: No state changes detected, skipping state processing.");
            }
            
            device.AddLogEntry("VrrpProtocol: VRRP state update completed.");
        }
        
        private async Task UpdateVirtualRouterStates(NetworkDevice device)
        {
            device.AddLogEntry("VrrpProtocol: Updating VRRP group states...");

            foreach (var groupConfig in _vrrpConfig.Groups.Values)
            {
                var groupState = _state.GetOrCreateGroupState(groupConfig.GroupId, groupConfig.Interface);
                
                // Update group configuration
                groupState.VirtualIps.Clear();
                groupState.VirtualIps.Add(groupConfig.VirtualIp);
                groupState.Priority = groupConfig.Priority;
                groupState.AdvertisementInterval = groupConfig.HelloInterval;
                
                // Progress group state machine
                await ProgressGroupState(groupState, device);
                
                device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} in state {groupState.State} with priority {groupState.Priority}");
            }
        }
        
        private async Task ProgressGroupState(VrrpGroupState groupState, NetworkDevice device)
        {
            // Simplified VRRP state machine
            switch (groupState.State)
            {
                case VrrpGroupStateType.Initialize:
                    // Determine initial state based on priority
                    if (groupState.Priority == 255) // IP address owner
                    {
                        groupState.ChangeState(VrrpGroupStateType.Master);
                        _state.MarkStateChanged();
                        device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} transitioned to Master (IP owner)");
                    }
                    else
                    {
                        groupState.ChangeState(VrrpGroupStateType.Backup);
                        _state.MarkStateChanged();
                        device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} transitioned to Backup");
                    }
                    break;
                    
                case VrrpGroupStateType.Master:
                    // Master state - send advertisements
                    await SendAdvertisement(groupState, device);
                    break;
                    
                case VrrpGroupStateType.Backup:
                    // Backup state - monitor for master failures
                    await MonitorMaster(groupState, device);
                    break;
            }
        }
        
        private async Task SendAdvertisement(VrrpGroupState groupState, NetworkDevice device)
        {
            var timeSinceLastAdvertisement = DateTime.Now - _advertisementTimers[groupState.Vrid];
            
            if (timeSinceLastAdvertisement >= TimeSpan.FromSeconds(groupState.AdvertisementInterval))
            {
                device.AddLogEntry($"VrrpProtocol: Sending VRRP advertisement for Group {groupState.Vrid}");
                _advertisementTimers[groupState.Vrid] = DateTime.Now;
                groupState.LastAdvertisementSent = DateTime.Now;
                
                // In a real implementation, we would send VRRP advertisement packets
                // For simulation, we'll just log the activity
            }
        }
        
        private async Task MonitorMaster(VrrpGroupState groupState, NetworkDevice device)
        {
            var masterTimeout = TimeSpan.FromSeconds(3 * groupState.AdvertisementInterval + 0.5); // 3 * advertisement interval + skew time
            var timeSinceLastAdvertisement = DateTime.Now - groupState.LastAdvertisementReceived;
            
            if (timeSinceLastAdvertisement > masterTimeout)
            {
                device.AddLogEntry($"VrrpProtocol: Master timeout for Group {groupState.Vrid}, becoming Master");
                groupState.ChangeState(VrrpGroupStateType.Master);
                _state.MarkStateChanged();
                _advertisementTimers[groupState.Vrid] = DateTime.Now;
            }
        }
        
        private async Task ProcessAdvertisementTimers(NetworkDevice device)
        {
            // Process advertisement timers for all VRRP groups
            foreach (var groupState in _state.GroupStates.Values)
            {
                if (groupState.State == VrrpGroupStateType.Master)
                {
                    var timeSinceLastAdvertisement = DateTime.Now - _advertisementTimers[groupState.Vrid];
                    
                    if (timeSinceLastAdvertisement >= TimeSpan.FromSeconds(groupState.AdvertisementInterval))
                    {
                        device.AddLogEntry($"VrrpProtocol: Advertisement timer expired for Group {groupState.Vrid}");
                        _state.MarkStateChanged();
                    }
                }
            }
        }
        
        private async Task ProcessStateChanges(NetworkDevice device)
        {
            device.AddLogEntry("VrrpProtocol: Processing VRRP state changes...");
            
            // Process state changes for all VRRP groups
            foreach (var groupState in _state.GroupStates.Values)
            {
                if (groupState.State == VrrpGroupStateType.Master)
                {
                    // Master group - handle virtual IP address
                    await HandleMasterState(groupState, device);
                }
                else if (groupState.State == VrrpGroupStateType.Backup)
                {
                    // Backup group - release virtual IP address
                    await HandleBackupState(groupState, device);
                }
            }
            
            device.AddLogEntry("VrrpProtocol: State change processing completed.");
        }
        
        private async Task HandleMasterState(VrrpGroupState groupState, NetworkDevice device)
        {
            device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} is Master - handling virtual IPs {string.Join(", ", groupState.VirtualIps)}");
            
            // In a real implementation, we would:
            // 1. Add virtual IP address to the interface
            // 2. Send gratuitous ARP
            // 3. Process packets destined to virtual IP
            // For simulation, we'll just log the activity
            
            device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} activated virtual IPs {string.Join(", ", groupState.VirtualIps)}");
        }
        
        private async Task HandleBackupState(VrrpGroupState groupState, NetworkDevice device)
        {
            device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} is Backup - releasing virtual IPs {string.Join(", ", groupState.VirtualIps)}");
            
            // In a real implementation, we would:
            // 1. Remove virtual IP address from the interface
            // 2. Stop processing packets destined to virtual IP
            // For simulation, we'll just log the activity
            
            device.AddLogEntry($"VrrpProtocol: VRRP Group {groupState.Vrid} deactivated virtual IPs {string.Join(", ", groupState.VirtualIps)}");
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
