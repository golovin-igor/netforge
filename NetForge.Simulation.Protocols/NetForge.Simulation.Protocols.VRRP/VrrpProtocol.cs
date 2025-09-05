using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.VRRP
{
    /// <summary>
    /// VRRP (Virtual Router Redundancy Protocol) implementation
    /// RFC 3768 standard for providing high availability for default gateways
    /// Provides redundancy through Master/Backup election
    /// </summary>
    public class VrrpProtocol : BaseProtocol, IDeviceProtocol
    {
        public override NetworkProtocolType Type => NetworkProtocolType.VRRP;
        public override string Name => "Virtual Router Redundancy Protocol";

        protected override BaseProtocolState CreateInitialState()
        {
            return new VrrpState();
        }

        protected override void OnInitialized()
        {
            var vrrpConfig = GetVrrpConfig();
            var vrrpState = (VrrpState)_state;

            vrrpState.RouterId = _device.Name;

            // Initialize VRRP groups
            foreach (var kvp in vrrpConfig.Groups)
            {
                var groupConfig = kvp.Value;
                var groupState = vrrpState.GetOrCreateGroup(groupConfig.GroupId);

                groupState.GroupId = groupConfig.GroupId;
                groupState.VirtualIpAddress = groupConfig.VirtualIp;
                groupState.Interface = groupConfig.Interface;
                groupState.Priority = groupConfig.Priority;
                groupState.Preempt = groupConfig.Preempt;
                groupState.PreemptDelay = groupConfig.PreemptDelay;
                groupState.AdvertisementInterval = groupConfig.HelloInterval;
                groupState.MasterDownInterval = groupConfig.MasterDownInterval;
                groupState.Version = (VrrpVersion)groupConfig.Version;
                groupState.VirtualMacAddress = GenerateVirtualMacAddress(groupConfig.GroupId);

                // Determine if this router owns the virtual IP
                var interfaceConfig = _device.GetInterface(groupConfig.Interface);
                groupState.IsOwner = interfaceConfig?.IpAddress == groupConfig.VirtualIp;

                // Start in Initialize state
                groupState.State = VrrpProtocolState.Initialize;
            }

            _device.AddLogEntry($"VRRP: Initialized with {vrrpConfig.Groups.Count} groups on router {vrrpState.RouterId}");
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var vrrpConfig = GetVrrpConfig();
            var vrrpState = (VrrpState)_state;

            if (!vrrpConfig.IsEnabled)
            {
                vrrpState.IsActive = false;
                return;
            }

            vrrpState.IsActive = true;

            // Send Advertisement packets for each group
            await SendAdvertisements(device, vrrpConfig, vrrpState);

            // Process received Advertisement packets (simulated)
            await ProcessReceivedAdvertisements(device, vrrpState);

            // Update timers for all groups
            await UpdateGroupTimers(device, vrrpState);
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var vrrpState = (VrrpState)_state;

            device.AddLogEntry("VRRP: Running state machine calculation due to policy change...");

            // Process state machine for each group
            foreach (var kvp in vrrpState.Groups)
            {
                var groupState = kvp.Value;
                await ProcessGroupStateMachine(device, groupState);
            }

            vrrpState.PolicyChanged = false;
            device.AddLogEntry("VRRP: State machine calculation completed");
        }

        private async Task SendAdvertisements(INetworkDevice device, VrrpConfig config, VrrpState state)
        {
            var now = DateTime.Now;

            foreach (var kvp in state.Groups)
            {
                var groupState = kvp.Value;
                var interfaceConfig = device.GetInterface(groupState.Interface);

                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Only Master sends advertisements
                if (groupState.State != VrrpProtocolState.Master)
                    continue;

                // Check if it's time to send advertisement
                if ((now - groupState.LastAdvertisement).TotalSeconds >= groupState.AdvertisementInterval)
                {
                    await SendAdvertisementPacket(device, groupState, state);
                    groupState.LastAdvertisement = now;
                }
            }
        }

        private async Task SendAdvertisementPacket(INetworkDevice device, VrrpGroupState groupState, VrrpState state)
        {
            var advertisement = new VrrpAdvertisement
            {
                Version = groupState.Version,
                Type = VrrpPacketType.Advertisement,
                VirtualRouterId = groupState.GroupId,
                Priority = groupState.Priority,
                CountIpAddrs = 1,
                AuthType = 0, // No authentication for VRRPv3
                AdvertisementInterval = groupState.AdvertisementInterval,
                IpAddresses = new List<string> { groupState.VirtualIpAddress },
                SourceRouter = state.RouterId
            };

            device.AddLogEntry($"VRRP: Sending Advertisement for group {groupState.GroupId} on interface {groupState.Interface} (priority: {groupState.Priority})");

            // Simulate advertisement packet transmission
            await SimulatePacketTransmission(device, groupState.Interface, advertisement);
        }

        private async Task ProcessReceivedAdvertisements(INetworkDevice device, VrrpState state)
        {
            // Simulate receiving VRRP Advertisement packets from connected routers
            foreach (var groupId in state.Groups.Keys)
            {
                var groupState = state.Groups[groupId];
                var interfaceConfig = device.GetInterface(groupState.Interface);

                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                var connectedDevice = device.GetConnectedDevice(groupState.Interface);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    // Check if neighbor is running VRRP
                    var neighborVrrpConfig = neighborDevice.GetVrrpConfiguration();
                    if (neighborVrrpConfig?.IsEnabled == true && neighborVrrpConfig.Groups.ContainsKey(groupId))
                    {
                        await ProcessNeighborAdvertisement(device, groupState, neighborDevice, neighborInterface, state);
                    }
                }
            }
        }

        private async Task ProcessNeighborAdvertisement(INetworkDevice device, VrrpGroupState groupState,
            INetworkDevice neighborDevice, string neighborInterface, VrrpState state)
        {
            var neighborVrrpConfig = neighborDevice.GetVrrpConfiguration();
            var neighborGroup = neighborVrrpConfig.Groups[groupState.GroupId];

            // Create simulated advertisement packet from neighbor
            var receivedAdvertisement = new VrrpAdvertisement
            {
                Version = (VrrpVersion)neighborGroup.Version,
                Type = VrrpPacketType.Advertisement,
                VirtualRouterId = neighborGroup.GroupId,
                Priority = neighborGroup.Priority,
                CountIpAddrs = 1,
                AuthType = 0,
                AdvertisementInterval = neighborGroup.HelloInterval,
                IpAddresses = new List<string> { neighborGroup.VirtualIp },
                SourceRouter = neighborDevice.Name
            };

            await ProcessAdvertisementPacket(device, receivedAdvertisement, groupState, state);
        }

        private async Task ProcessAdvertisementPacket(INetworkDevice device, VrrpAdvertisement packet, VrrpGroupState groupState, VrrpState state)
        {
            // Update neighbor information
            var neighborKey = $"{packet.SourceRouter}:{packet.VirtualRouterId}";
            var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new VrrpNeighbor
            {
                RouterId = packet.SourceRouter,
                IpAddress = packet.IpAddresses.FirstOrDefault() ?? "",
                InterfaceName = groupState.Interface,
                Priority = packet.Priority,
                State = VrrpProtocolState.Master, // Assume sender is master
                Groups = new Dictionary<int, VrrpGroupInfo>
                {
                    [packet.VirtualRouterId] = new VrrpGroupInfo
                    {
                        GroupId = packet.VirtualRouterId,
                        VirtualIpAddress = packet.IpAddresses.FirstOrDefault() ?? "",
                        Priority = packet.Priority,
                        State = VrrpProtocolState.Master
                    }
                }
            });

            neighbor.LastSeen = DateTime.Now;
            neighbor.Priority = packet.Priority;

            // Process state machine based on received advertisement
            var stateMachine = new VrrpStateMachine(groupState);

            VrrpEvent eventType = DetermineEventType(packet, groupState);
            stateMachine.ProcessEvent(eventType, packet);

            // Update master router information
            if (packet.Priority > groupState.Priority ||
                (packet.Priority == groupState.Priority &&
                 string.Compare(packet.SourceRouter, state.RouterId, StringComparison.Ordinal) > 0))
            {
                groupState.MasterIpAddress = packet.SourceRouter;
            }

            device.AddLogEntry($"VRRP: Processed Advertisement from {packet.SourceRouter} for group {packet.VirtualRouterId} (priority: {packet.Priority})");
        }

        private async Task UpdateGroupTimers(INetworkDevice device, VrrpState state)
        {
            foreach (var kvp in state.Groups)
            {
                var groupState = kvp.Value;
                var stateMachine = new VrrpStateMachine(groupState);

                // Check master down timer
                if (groupState.Timers.IsMasterDownExpired())
                {
                    device.AddLogEntry($"VRRP: Master down timer expired for group {groupState.GroupId}");
                    stateMachine.ProcessEvent(VrrpEvent.MasterDownTimer);
                }

                // Check preempt delay timer
                if (groupState.Timers.IsPreemptDelayExpired())
                {
                    device.AddLogEntry($"VRRP: Preempt delay timer expired for group {groupState.GroupId}");
                    stateMachine.ProcessEvent(VrrpEvent.PreemptDelayTimer);
                }
            }
        }

        private async Task ProcessGroupStateMachine(INetworkDevice device, VrrpGroupState groupState)
        {
            var stateMachine = new VrrpStateMachine(groupState);

            // Check interface status
            var interfaceConfig = device.GetInterface(groupState.Interface);
            if (interfaceConfig?.IsShutdown == true || !interfaceConfig?.IsUp == true)
            {
                if (groupState.State != VrrpProtocolState.Initialize)
                {
                    device.AddLogEntry($"VRRP: Interface {groupState.Interface} down, transitioning group {groupState.GroupId} to Initialize");
                    stateMachine.ProcessEvent(VrrpEvent.InterfaceDown);
                }
                return;
            }

            // If we're starting up and interface is up
            if (groupState.State == VrrpProtocolState.Initialize)
            {
                device.AddLogEntry($"VRRP: Starting group {groupState.GroupId} on interface {groupState.Interface}");
                stateMachine.ProcessEvent(VrrpEvent.Startup);
            }
        }

        private VrrpEvent DetermineEventType(VrrpAdvertisement packet, VrrpGroupState groupState)
        {
            if (packet.Priority > groupState.Priority)
            {
                return VrrpEvent.HigherPriorityReceived;
            }
            else if (packet.Priority < groupState.Priority)
            {
                return VrrpEvent.LowerPriorityReceived;
            }
            else
            {
                return VrrpEvent.EqualPriorityReceived;
            }
        }

        private string GenerateVirtualMacAddress(int groupId)
        {
            // RFC 3768 - VRRP Virtual MAC Address format: 00-00-5E-00-01-XX
            return $"00:00:5e:00:01:{groupId:x2}";
        }

        private async Task SimulatePacketTransmission(INetworkDevice device, string interfaceName, VrrpAdvertisement packet)
        {
            // Simulate packet transmission - in real implementation this would send actual VRRP packets
            await Task.Delay(1); // Simulate network delay
        }

        private VrrpConfig GetVrrpConfig()
        {
            return _device?.GetVrrpConfiguration() ?? new VrrpConfig { IsEnabled = false };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetVrrpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is VrrpConfig vrrpConfig)
            {
                _device?.SetVrrpConfiguration(vrrpConfig);
                var vrrpState = (VrrpState)_state;
                vrrpState.RouterId = _device.Name;
                _state.IsActive = vrrpConfig.IsEnabled;
                _state.MarkStateChanged();
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Generic" }; // VRRP is an industry standard
        }
    }
}
