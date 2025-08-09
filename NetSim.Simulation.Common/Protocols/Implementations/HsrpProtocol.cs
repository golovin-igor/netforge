using NetSim.Simulation.Common;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;

namespace NetSim.Simulation.Protocols.Implementations
{
    public class HsrpProtocol : INetworkProtocol
    {
        private HsrpConfig _hsrpConfig;
        private NetworkDevice _device;
        private readonly HsrpState _state = new(); // Protocol-specific state

        public ProtocolType Type => ProtocolType.HSRP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _hsrpConfig = device.GetHsrpConfiguration();
            if (_hsrpConfig == null)
            {
                device.AddLogEntry("HsrpProtocol: HSRP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"HsrpProtocol: Successfully initialized with {_hsrpConfig.Groups.Count} HSRP groups.");
                
                // Initialize group states for all configured groups
                foreach (var group in _hsrpConfig.Groups.Values)
                {
                    var groupState = _state.GetOrCreateGroupState(group.GroupId, group.Interface);
                    groupState.VirtualIp = group.VirtualIp;
                    groupState.Priority = group.Priority;
                    groupState.Preempt = group.Preempt;
                    groupState.HelloInterval = group.HelloInterval;
                    groupState.HoldTime = group.HoldTime;
                    
                    device.AddLogEntry($"HsrpProtocol: Initialized group {group.GroupId} on interface {group.Interface}");
                }
                
                // Mark state as changed to trigger initial state evaluation
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
                _device.AddLogEntry($"HSRPProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating HSRP state.");
                
                // Mark state as changed when interface states change
                _state.MarkStateChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"HSRPProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating HSRP configuration and state.");
                _hsrpConfig = _device.GetHsrpConfiguration();
                _state.MarkStateChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_hsrpConfig == null) _hsrpConfig = device.GetHsrpConfiguration();

            if (_hsrpConfig == null || !_hsrpConfig.IsEnabled)
            {
                device.AddLogEntry($"HSRPProtocol on {device.Name}: HSRP configuration missing or not enabled.");
                return;
            }

            device.AddLogEntry($"HsrpProtocol: Updating HSRP state with {_hsrpConfig.Groups.Count} groups on device {device.Name}...");
            
            // Process hello timers - check if we need to send hellos
            await ProcessHelloTimers(device);
            
            // Check for groups that have timed out
            await ProcessTimeouts(device);
            
            // Only process state changes if state has changed
            if (_state.StateChanged)
            {
                // Process each HSRP group
                foreach (var groupKvp in _hsrpConfig.Groups)
                {
                    await UpdateHsrpGroup(device, groupKvp.Value);
                }
                
                _state.StateChanged = false;
                _state.LastStateUpdate = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("HSRPProtocol: No state changes detected, maintaining current state.");
            }

            device.AddLogEntry("HsrpProtocol: HSRP state update completed.");
        }
        
        private async Task ProcessHelloTimers(NetworkDevice device)
        {
            var groupsNeedingHello = _state.GetGroupsNeedingHello();
            foreach (var groupId in groupsNeedingHello)
            {
                if (_state.GroupStates.TryGetValue(groupId, out var groupState))
                {
                    groupState.LastHelloSent = DateTime.Now;
                    device.AddLogEntry($"HsrpProtocol: Sent hello for group {groupId} in state {groupState.State}");
                }
            }
        }
        
        private async Task ProcessTimeouts(NetworkDevice device)
        {
            var timedOutGroups = _state.GetTimedOutGroups();
            foreach (var groupId in timedOutGroups)
            {
                if (_state.GroupStates.TryGetValue(groupId, out var groupState))
                {
                    device.AddLogEntry($"HsrpProtocol: Group {groupId} master timeout detected");
                    _state.MarkStateChanged();
                }
            }
        }

        private async Task UpdateHsrpGroup(NetworkDevice device, HsrpGroup group)
        {
            if (!group.IsEnabled)
            {
                device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} is disabled, skipping.");
                return;
            }

            // Check if the interface exists and is operational
            var interfaceConfig = device.GetInterface(group.Interface);
            if (interfaceConfig == null)
            {
                device.AddLogEntry($"HsrpProtocol: Interface {group.Interface} for HSRP group {group.GroupId} not found.");
                return;
            }

            if (interfaceConfig.IsShutdown)
            {
                device.AddLogEntry($"HsrpProtocol: Interface {group.Interface} for HSRP group {group.GroupId} is shutdown, setting state to Initial.");
                var groupState = _state.GetOrCreateGroupState(group.GroupId, group.Interface);
                groupState.ChangeState(HsrpGroupStateType.Initial);
                return;
            }

            // Get or create group state
            var currentGroupState = _state.GetOrCreateGroupState(group.GroupId, group.Interface);
            
            // Simulate HSRP state machine with optimized state changes
            await UpdateHsrpGroupState(device, group, interfaceConfig, currentGroupState);
        }

        private async Task UpdateHsrpGroupState(NetworkDevice device, HsrpGroup group, InterfaceConfig interfaceConfig, HsrpGroupState groupState)
        {
            // Simplified HSRP state machine simulation with state management optimization
            var currentTime = DateTime.Now;

            switch (groupState.State)
            {
                case HsrpGroupStateType.Initial:
                    // Move to Learn state to discover other routers
                    groupState.ChangeState(HsrpGroupStateType.Learn);
                    device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} transitioned to Learn state");
                    break;

                case HsrpGroupStateType.Learn:
                    // Learn about other HSRP routers, then move to Listen
                    await Task.Delay(100); // Simulate learning period
                    groupState.ChangeState(HsrpGroupStateType.Listen);
                    device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} transitioned to Listen state");
                    break;

                case HsrpGroupStateType.Listen:
                    // Listen to hellos from active/standby routers
                    if (ShouldBecomeActive(device, group, groupState))
                    {
                        groupState.ChangeState(HsrpGroupStateType.Speak);
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} transitioned to Speak state");
                    }
                    break;

                case HsrpGroupStateType.Speak:
                    // Send hellos and try to become active or standby
                    if (groupState.ShouldSendHello())
                    {
                        groupState.LastHelloSent = currentTime;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} sent hello (Speak state)");
                    }

                    // Simulate becoming standby or active
                    if (ShouldBecomeActive(device, group, groupState))
                    {
                        groupState.ChangeState(HsrpGroupStateType.Active);
                        groupState.ActiveRouter = device.Name;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} became Active router (priority: {group.Priority})");
                    }
                    else if (ShouldBecomeStandby(device, group, groupState))
                    {
                        groupState.ChangeState(HsrpGroupStateType.Standby);
                        groupState.StandbyRouter = device.Name;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} became Standby router (priority: {group.Priority})");
                    }
                    break;

                case HsrpGroupStateType.Standby:
                    // Monitor active router and be ready to take over
                    if (groupState.ShouldSendHello())
                    {
                        groupState.LastHelloSent = currentTime;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} sent hello (Standby state)");
                    }

                    // Check if we should become active (preempt or active router failed)
                    if ((group.Preempt && ShouldPreempt(device, group, groupState)) || ShouldTakeOverAsActive(device, group, groupState))
                    {
                        groupState.ChangeState(HsrpGroupStateType.Active);
                        groupState.ActiveRouter = device.Name;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} took over as Active router");
                    }
                    break;

                case HsrpGroupStateType.Active:
                    // Send periodic hellos and maintain active state
                    if (groupState.ShouldSendHello())
                    {
                        groupState.LastHelloSent = currentTime;
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} sent hello (Active state)");
                    }

                    // Check if we should step down due to higher priority router or interface issues
                    if (!interfaceConfig.IsUp)
                    {
                        groupState.ChangeState(HsrpGroupStateType.Initial);
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} stepped down due to interface issues");
                    }
                    else if (ShouldStepDown(device, group, groupState))
                    {
                        groupState.ChangeState(HsrpGroupStateType.Listen);
                        device.AddLogEntry($"HsrpProtocol: HSRP group {group.GroupId} stepped down due to higher priority router");
                    }
                    break;
            }
        }

        private bool ShouldBecomeActive(NetworkDevice device, HsrpGroup group, HsrpGroupState groupState)
        {
            // Simplified logic - in reality, this would involve comparing with other HSRP routers
            // For simulation: become active if we're the virtual IP owner or have high priority
            return groupState.IsVirtualIpOwner || group.Priority > 100;
        }

        private bool ShouldBecomeStandby(NetworkDevice device, HsrpGroup group, HsrpGroupState groupState)
        {
            // Simplified logic for becoming standby
            return !groupState.IsVirtualIpOwner && group.Priority >= 100;
        }

        private bool ShouldPreempt(NetworkDevice device, HsrpGroup group, HsrpGroupState groupState)
        {
            // Check if we should preempt based on priority
            return group.Preempt && group.Priority > 100;
        }

        private bool ShouldTakeOverAsActive(NetworkDevice device, HsrpGroup group, HsrpGroupState groupState)
        {
            // Check if active router has failed (simplified)
            return groupState.HasTimedOut();
        }

        private bool ShouldStepDown(NetworkDevice device, HsrpGroup group, HsrpGroupState groupState)
        {
            // Check if a higher priority router is available
            return false; // Simplified - no stepping down in this simulation
        }

        private bool IsVirtualIpOwner(NetworkDevice device, HsrpGroup group)
        {
            // Check if this device owns the virtual IP (simplified)
            var interfaceConfig = device.GetInterface(group.Interface);
            return interfaceConfig?.IpAddress == group.VirtualIp;
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
