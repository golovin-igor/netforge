using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.HSRP
{
    /// <summary>
    /// HSRP (Hot Standby Router Protocol) implementation
    /// Cisco proprietary redundancy protocol for gateway failover
    /// RFC 2281 - provides high availability for default gateways
    /// </summary>
    public class HsrpProtocol : BaseProtocol, IDeviceProtocol
    {
        public override ProtocolType Type => ProtocolType.HSRP;
        public override string Name => "Hot Standby Router Protocol";

        protected override BaseProtocolState CreateInitialState()
        {
            return new HsrpState();
        }

        protected override void OnInitialized()
        {
            var hsrpConfig = GetHsrpConfig();
            var hsrpState = (HsrpState)_state;

            hsrpState.RouterId = _device.Name;

            // Initialize HSRP groups
            foreach (var kvp in hsrpConfig.Groups)
            {
                var groupConfig = kvp.Value;
                var groupState = hsrpState.GetOrCreateGroupState(groupConfig.GroupId);

                groupState.GroupId = groupConfig.GroupId;
                groupState.VirtualIpAddress = groupConfig.VirtualIp;
                groupState.VirtualMacAddress = HsrpMacAddressGenerator.GenerateVirtualMac(groupConfig.GroupId, groupConfig.Version);
                groupState.InterfaceName = groupConfig.Interface;
                groupState.Priority = groupConfig.Priority;
                groupState.Preempt = groupConfig.Preempt;
                groupState.PreemptDelay = groupConfig.PreemptDelay;
                groupState.HelloInterval = groupConfig.HelloInterval;
                groupState.HoldTime = groupConfig.HoldTime;
                groupState.Version = groupConfig.Version;
                groupState.AuthType = groupConfig.AuthType;
                groupState.AuthKey = groupConfig.AuthKey;
                groupState.Statistics.GroupId = groupConfig.GroupId;
                groupState.Statistics.InterfaceName = groupConfig.Interface;

                // Start in Initial state
                groupState.State = HsrpProtocolState.Initial;
            }

            _device.AddLogEntry($"HSRP: Initialized with {hsrpConfig.Groups.Count} groups on router {hsrpState.RouterId}");
        }

        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var hsrpConfig = GetHsrpConfig();
            var hsrpState = (HsrpState)_state;

            if (!hsrpConfig.IsEnabled)
            {
                hsrpState.IsActive = false;
                return;
            }

            hsrpState.IsActive = true;

            // Send Hello packets for each group
            await SendHelloPackets(device, hsrpConfig, hsrpState);

            // Process received Hello packets (simulated)
            await ProcessReceivedHellos(device, hsrpState);

            // Update timers for all groups
            await UpdateGroupTimers(device, hsrpState);
        }

        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var hsrpState = (HsrpState)_state;

            device.AddLogEntry("HSRP: Running state machine calculation due to policy change...");

            // Process state machine for each group
            foreach (var kvp in hsrpState.Groups)
            {
                var groupState = kvp.Value;
                await ProcessGroupStateMachine(device, groupState);
            }

            hsrpState.PolicyChanged = false;
            device.AddLogEntry("HSRP: State machine calculation completed");
        }

        private async Task SendHelloPackets(NetworkDevice device, HsrpConfig config, HsrpState state)
        {
            var now = DateTime.Now;

            foreach (var kvp in state.Groups)
            {
                var groupState = kvp.Value;
                var interfaceConfig = device.GetInterface(groupState.InterfaceName);

                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if it's time to send hello
                if (groupState.Timers.IsHelloTimerExpired() ||
                    (now - groupState.LastHelloSent).TotalSeconds >= groupState.HelloInterval)
                {
                    await SendHelloPacket(device, groupState, state);
                    groupState.Timers.StartHelloTimer(groupState.HelloInterval);
                    groupState.LastHelloSent = now;
                }
            }
        }

        private async Task SendHelloPacket(NetworkDevice device, HsrpGroupState groupState, HsrpState state)
        {
            var helloPacket = new HsrpHelloPacket
            {
                Version = groupState.Version,
                OpCode = HsrpOpCode.Hello,
                State = groupState.State,
                HelloTime = groupState.HelloInterval,
                HoldTime = groupState.HoldTime,
                Priority = groupState.Priority,
                GroupId = groupState.GroupId,
                AuthType = groupState.AuthType,
                VirtualIpAddress = groupState.VirtualIpAddress,
                SourceRouter = state.RouterId,
                SourceInterface = groupState.InterfaceName
            };

            // Set authentication data if needed
            if (groupState.AuthType == "text" && !string.IsNullOrEmpty(groupState.AuthKey))
            {
                var authBytes = System.Text.Encoding.ASCII.GetBytes(groupState.AuthKey);
                Array.Copy(authBytes, helloPacket.AuthData, Math.Min(authBytes.Length, 8));
            }

            device.AddLogEntry($"HSRP: Sending Hello for group {groupState.GroupId} on interface {groupState.InterfaceName} (state: {groupState.State})");

            // Simulate hello packet transmission
            await SimulatePacketTransmission(device, groupState.InterfaceName, helloPacket);

            groupState.Statistics.HellosSent++;
        }

        private async Task ProcessReceivedHellos(NetworkDevice device, HsrpState state)
        {
            // Simulate receiving HSRP Hello packets from connected routers
            foreach (var groupId in state.Groups.Keys)
            {
                var groupState = state.Groups[groupId];
                var interfaceConfig = device.GetInterface(groupState.InterfaceName);

                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                var connectedDevice = device.GetConnectedDevice(groupState.InterfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    // Check if neighbor is running HSRP
                    var neighborHsrpConfig = neighborDevice.GetHsrpConfiguration();
                    if (neighborHsrpConfig?.IsEnabled == true && neighborHsrpConfig.Groups.ContainsKey(groupId))
                    {
                        await ProcessNeighborHello(device, groupState, neighborDevice, neighborInterface, state);
                    }
                }
            }
        }

        private async Task ProcessNeighborHello(NetworkDevice device, HsrpGroupState groupState,
            NetworkDevice neighborDevice, string neighborInterface, HsrpState state)
        {
            var neighborHsrpConfig = neighborDevice.GetHsrpConfiguration();
            var neighborGroup = neighborHsrpConfig.Groups[groupState.GroupId];

            // Create simulated hello packet from neighbor
            var receivedHello = new HsrpHelloPacket
            {
                Version = neighborGroup.Version,
                OpCode = HsrpOpCode.Hello,
                State = ParseHsrpState(neighborGroup.State),
                HelloTime = neighborGroup.HelloInterval,
                HoldTime = neighborGroup.HoldTime,
                Priority = neighborGroup.Priority,
                GroupId = neighborGroup.GroupId,
                AuthType = neighborGroup.AuthType,
                VirtualIpAddress = neighborGroup.VirtualIp,
                SourceRouter = neighborDevice.Name,
                SourceInterface = neighborInterface
            };

            await ProcessHelloPacket(device, receivedHello, groupState, state);
        }

        private async Task ProcessHelloPacket(NetworkDevice device, HsrpHelloPacket packet, HsrpGroupState groupState, HsrpState state)
        {
            groupState.LastHelloReceived = DateTime.Now;
            groupState.Statistics.HellosReceived++;

            // Validate authentication
            var auth = new HsrpAuthentication { Type = groupState.AuthType, Key = groupState.AuthKey };
            if (!auth.ValidatePacket(packet))
            {
                groupState.Statistics.AuthFailures++;
                device.AddLogEntry($"HSRP: Authentication failed for group {groupState.GroupId} from {packet.SourceRouter}");
                return;
            }

            // Update neighbor information
            var neighborKey = $"{packet.SourceRouter}:{packet.GroupId}";
            var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new HsrpNeighbor
            {
                RouterId = packet.SourceRouter,
                IpAddress = packet.VirtualIpAddress,
                InterfaceName = packet.SourceInterface,
                Priority = packet.Priority,
                State = packet.State,
                Version = packet.Version
            });

            neighbor.LastSeen = DateTime.Now;
            neighbor.Priority = packet.Priority;
            neighbor.State = packet.State;

            // Process state machine based on received hello
            var stateMachine = new HsrpStateMachine(groupState);

            HsrpEvent eventType = DetermineEventType(packet, groupState);
            stateMachine.ProcessEvent(eventType, packet);

            // Update active/standby router information
            if (packet.State == HsrpProtocolState.Active && packet.Priority > GetCurrentActivePriority(groupState))
            {
                groupState.ActiveRouter = packet.SourceRouter;
            }
            else if (packet.State == HsrpProtocolState.Standby && packet.Priority > GetCurrentStandbyPriority(groupState))
            {
                groupState.StandbyRouter = packet.SourceRouter;
            }

            device.AddLogEntry($"HSRP: Processed Hello from {packet.SourceRouter} for group {packet.GroupId} (priority: {packet.Priority}, state: {packet.State})");
        }

        private async Task UpdateGroupTimers(NetworkDevice device, HsrpState state)
        {
            foreach (var kvp in state.Groups)
            {
                var groupState = kvp.Value;
                var stateMachine = new HsrpStateMachine(groupState);

                // Check active timer
                if (groupState.Timers.IsActiveTimerExpired())
                {
                    device.AddLogEntry($"HSRP: Active timer expired for group {groupState.GroupId}");
                    stateMachine.ProcessEvent(HsrpEvent.ActiveTimerExpired);
                }

                // Check standby timer
                if (groupState.Timers.IsStandbyTimerExpired())
                {
                    device.AddLogEntry($"HSRP: Standby timer expired for group {groupState.GroupId}");
                    stateMachine.ProcessEvent(HsrpEvent.StandbyTimerExpired);
                }

                // Check hello timer
                if (groupState.Timers.IsHelloTimerExpired())
                {
                    stateMachine.ProcessEvent(HsrpEvent.HelloTimerExpired);
                }
            }
        }

        private async Task ProcessGroupStateMachine(NetworkDevice device, HsrpGroupState groupState)
        {
            var stateMachine = new HsrpStateMachine(groupState);

            // Check interface status
            var interfaceConfig = device.GetInterface(groupState.InterfaceName);
            if (interfaceConfig?.IsShutdown == true || !interfaceConfig?.IsUp == true)
            {
                if (groupState.State != HsrpProtocolState.Initial)
                {
                    device.AddLogEntry($"HSRP: Interface {groupState.InterfaceName} down, transitioning group {groupState.GroupId} to Initial");
                    stateMachine.ProcessEvent(HsrpEvent.InterfaceDown);
                }
                return;
            }

            // If we're starting up and interface is up
            if (groupState.State == HsrpProtocolState.Initial)
            {
                device.AddLogEntry($"HSRP: Starting group {groupState.GroupId} on interface {groupState.InterfaceName}");
                stateMachine.ProcessEvent(HsrpEvent.Startup);
            }
        }

        private HsrpEvent DetermineEventType(HsrpHelloPacket packet, HsrpGroupState groupState)
        {
            if (packet.Priority > groupState.Priority)
            {
                return HsrpEvent.HigherPriorityHelloReceived;
            }
            else if (packet.Priority < groupState.Priority)
            {
                return HsrpEvent.LowerPriorityHelloReceived;
            }
            else
            {
                return HsrpEvent.EqualPriorityHelloReceived;
            }
        }

        private int GetCurrentActivePriority(HsrpGroupState groupState)
        {
            // In a real implementation, this would track the actual active router's priority
            return string.IsNullOrEmpty(groupState.ActiveRouter) ? 0 : 100;
        }

        private int GetCurrentStandbyPriority(HsrpGroupState groupState)
        {
            // In a real implementation, this would track the actual standby router's priority
            return string.IsNullOrEmpty(groupState.StandbyRouter) ? 0 : 100;
        }

        private HsrpProtocolState ParseHsrpState(string stateString)
        {
            return stateString.ToLowerInvariant() switch
            {
                "initial" => HsrpProtocolState.Initial,
                "learn" => HsrpProtocolState.Learn,
                "listen" => HsrpProtocolState.Listen,
                "speak" => HsrpProtocolState.Speak,
                "standby" => HsrpProtocolState.Standby,
                "active" => HsrpProtocolState.Active,
                _ => HsrpProtocolState.Initial
            };
        }

        private async Task SimulatePacketTransmission(NetworkDevice device, string interfaceName, HsrpHelloPacket packet)
        {
            // Simulate packet transmission - in real implementation this would send actual HSRP packets
            await Task.Delay(1); // Simulate network delay
        }

        private HsrpConfig GetHsrpConfig()
        {
            return _device?.GetHsrpConfiguration() ?? new HsrpConfig { IsEnabled = false };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetHsrpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is HsrpConfig hsrpConfig)
            {
                _device?.SetHsrpConfiguration(hsrpConfig);
                var hsrpState = (HsrpState)_state;
                hsrpState.RouterId = _device.Name;
                _state.IsActive = hsrpConfig.IsEnabled;
                _state.MarkStateChanged();
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Generic" }; // HSRP is Cisco proprietary but can be simulated generically
        }
    }
}
