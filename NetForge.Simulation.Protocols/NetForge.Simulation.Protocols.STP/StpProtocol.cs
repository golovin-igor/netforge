using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.STP
{
    /// <summary>
    /// STP (Spanning Tree Protocol) implementation
    /// IEEE 802.1D standard for loop prevention in Layer 2 networks
    /// Administrative Distance: N/A (Layer 2 protocol)
    /// </summary>
    public class StpProtocol : BaseProtocol, IDeviceProtocol
    {
        public override NetworkProtocolType Type => NetworkProtocolType.STP;
        public override string Name => "Spanning Tree Protocol";

        protected override BaseProtocolState CreateInitialState()
        {
            return new StpState();
        }

        protected override void OnInitialized()
        {
            var stpConfig = GetStpConfig();
            var stpState = (StpState)_state;

            // Initialize bridge ID using device MAC address and priority
            stpState.BridgeId = GenerateBridgeId(stpConfig);
            stpState.RootBridgeId = stpState.BridgeId; // Initially assume we are root
            stpState.RootPathCost = 0;
            stpState.RootPort = "";

            // Initialize port states for all interfaces
            foreach (var interfaceName in _device.GetAllInterfaces().Keys)
            {
                var portInfo = stpState.GetOrCreatePortInfo(interfaceName);
                portInfo.PathCost = StpPortCost.GetDefaultCost(interfaceName);
                portInfo.Priority = 128; // Default port priority
                portInfo.DesignatedBridge = stpState.BridgeId;
                portInfo.DesignatedPort = GeneratePortId(interfaceName, portInfo.Priority);
                portInfo.DesignatedCost = 0;
            }

            _device.AddLogEntry($"STP: Initialized with Bridge ID {stpState.BridgeId}");
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var stpConfig = GetStpConfig();
            var stpState = (StpState)_state;

            if (!stpConfig.IsEnabled)
            {
                stpState.IsActive = false;
                return;
            }

            stpState.IsActive = true;

            // Send Hello BPDUs
            await SendHelloBpdus(device, stpConfig, stpState);

            // Process received BPDUs (simulated)
            await ProcessReceivedBpdus(device, stpState);

            // Update port timers
            await UpdatePortTimers(device, stpState);
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var stpState = (StpState)_state;
            var stpConfig = GetStpConfig();

            device.AddLogEntry("STP: Running spanning tree calculation due to topology change...");

            // Step 1: Root bridge election
            await ElectRootBridge(device, stpState);

            // Step 2: Root port selection
            await SelectRootPort(device, stpState);

            // Step 3: Designated port selection
            await SelectDesignatedPorts(device, stpState);

            // Step 4: Update port states
            await UpdatePortStates(device, stpState, stpConfig);

            stpState.TopologyChanged = false;
            device.AddLogEntry("STP: Spanning tree calculation completed");
        }

        private async Task SendHelloBpdus(INetworkDevice device, StpConfig config, StpState state)
        {
            var now = DateTime.Now;

            foreach (var portName in state.PortInfo.Keys)
            {
                var portInfo = state.PortInfo[portName];
                var interfaceConfig = device.GetInterface(portName);

                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if it's time to send hello
                if (portInfo.Timers.IsHelloTimerExpired() ||
                    (now - state.LastBpduSent).TotalSeconds >= config.HelloTime)
                {
                    await SendConfigurationBpdu(device, portName, state, config);
                    portInfo.Timers.StartHelloTimer(config.HelloTime);
                    state.LastBpduSent = now;
                }
            }
        }

        private async Task SendConfigurationBpdu(INetworkDevice device, string portName, StpState state, StpConfig config)
        {
            var portInfo = state.PortInfo[portName];

            var bpdu = new StpBpdu
            {
                Type = StpBpduType.Configuration,
                RootBridgeId = state.RootBridgeId,
                RootPathCost = state.RootPathCost,
                BridgeId = state.BridgeId,
                PortId = GeneratePortId(portName, portInfo.Priority),
                MessageAge = 0,
                MaxAge = config.MaxAge,
                HelloTime = config.HelloTime,
                ForwardDelay = config.ForwardDelay,
                SourceInterface = portName
            };

            device.AddLogEntry($"STP: Sending Configuration BPDU on port {portName}");

            // Simulate BPDU transmission
            await SimulateBpduTransmission(device, portName, bpdu);

            portInfo.LastBpduSent = DateTime.Now;
            if (portInfo.Statistics != null)
            {
                portInfo.Statistics.BpdusSent++;
                portInfo.Statistics.ConfigBpdusSent++;
                portInfo.Statistics.LastBpduSent = DateTime.Now;
            }
        }

        private async Task ProcessReceivedBpdus(INetworkDevice device, StpState state)
        {
            // Simulate receiving BPDUs from connected bridges
            foreach (var portName in state.PortInfo.Keys)
            {
                var interfaceConfig = device.GetInterface(portName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                var connectedDevice = device.GetConnectedDevice(portName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    // Check if neighbor is running STP
                    var neighborStpConfig = neighborDevice.GetStpConfiguration();
                    if (neighborStpConfig?.IsEnabled == true)
                    {
                        await ProcessNeighborBpdu(device, portName, neighborDevice, neighborInterface, state);
                    }
                }
            }
        }

        private async Task ProcessNeighborBpdu(INetworkDevice device, string portName,
            INetworkDevice neighborDevice, string neighborInterface, StpState state)
        {
            var neighborStpConfig = neighborDevice.GetStpConfiguration();
            var neighborBridgeId = GenerateBridgeId(neighborStpConfig);

            // Create simulated BPDU from neighbor
            var receivedBpdu = new StpBpdu
            {
                Type = StpBpduType.Configuration,
                RootBridgeId = neighborBridgeId, // Simplified: assume neighbor thinks it's root
                RootPathCost = 0,
                BridgeId = neighborBridgeId,
                PortId = GeneratePortId(neighborInterface, 128),
                MessageAge = 0,
                MaxAge = neighborStpConfig.MaxAge,
                HelloTime = neighborStpConfig.HelloTime,
                ForwardDelay = neighborStpConfig.ForwardDelay,
                ReceivedTime = DateTime.Now,
                SourceInterface = portName
            };

            await ProcessBpdu(device, receivedBpdu, state);
        }

        private async Task ProcessBpdu(INetworkDevice device, StpBpdu receivedBpdu, StpState state)
        {
            var portInfo = state.PortInfo[receivedBpdu.SourceInterface];
            portInfo.LastBpduReceived = DateTime.Now;
            portInfo.LastReceivedBpdu = receivedBpdu;

            // Update statistics
            if (portInfo.Statistics == null)
                portInfo.Statistics = new StpStatistics { InterfaceName = receivedBpdu.SourceInterface };

            portInfo.Statistics.BpdusReceived++;
            portInfo.Statistics.ConfigBpdusReceived++;
            portInfo.Statistics.LastBpduReceived = DateTime.Now;

            // Compare received BPDU with current information
            var currentBpdu = CreateCurrentBpdu(state, receivedBpdu.SourceInterface);
            var comparison = StpBpduComparator.Compare(receivedBpdu, currentBpdu);

            switch (comparison)
            {
                case BpduComparison.Superior:
                    await ProcessSuperiorBpdu(device, receivedBpdu, state);
                    break;
                case BpduComparison.Inferior:
                    // Send our better BPDU
                    await SendConfigurationBpdu(device, receivedBpdu.SourceInterface, state, GetStpConfig());
                    break;
                case BpduComparison.Same:
                    // Reset message age timer
                    portInfo.Timers.StartMessageAgeTimer(receivedBpdu.MaxAge);
                    break;
            }

            state.LastConfigurationBpduReceived = DateTime.Now;
            device.AddLogEntry($"STP: Processed BPDU from {receivedBpdu.BridgeId} on port {receivedBpdu.SourceInterface}");
        }

        private async Task ProcessSuperiorBpdu(INetworkDevice device, StpBpdu receivedBpdu, StpState state)
        {
            bool topologyChanged = false;

            // Update root bridge information if we received superior information
            if (StpBridgePriority.CompareBridgeIds(receivedBpdu.RootBridgeId, state.RootBridgeId) < 0)
            {
                state.RootBridgeId = receivedBpdu.RootBridgeId;
                topologyChanged = true;
                device.AddLogEntry($"STP: New root bridge elected: {state.RootBridgeId}");
            }

            // Update our root path cost
            var newRootPathCost = receivedBpdu.RootPathCost + state.PortInfo[receivedBpdu.SourceInterface].PathCost;
            if (newRootPathCost < state.RootPathCost || state.RootPathCost == 0)
            {
                state.RootPathCost = newRootPathCost;
                state.RootPort = receivedBpdu.SourceInterface;
                topologyChanged = true;
            }

            if (topologyChanged)
            {
                state.TopologyChanged = true;
                state.LastTopologyChange = DateTime.Now;
                state.TopologyChangeCount++;
                state.IsRootBridge = (state.BridgeId == state.RootBridgeId);
            }
        }

        private async Task UpdatePortTimers(INetworkDevice device, StpState state)
        {
            var config = GetStpConfig();

            foreach (var kvp in state.PortInfo)
            {
                var portName = kvp.Key;
                var portInfo = kvp.Value;

                // Check message age timer
                if (portInfo.Timers.IsMessageAgeTimerExpired())
                {
                    device.AddLogEntry($"STP: Message age timer expired on port {portName}");
                    await HandleMessageAgeExpiry(device, portName, state);
                }

                // Check forward delay timer
                if (portInfo.Timers.IsForwardDelayTimerExpired())
                {
                    await HandleForwardDelayExpiry(device, portName, state, config);
                }
            }
        }

        private async Task HandleMessageAgeExpiry(INetworkDevice device, string portName, StpState state)
        {
            var portInfo = state.PortInfo[portName];

            // If this was our root port, we need to recalculate
            if (portName == state.RootPort)
            {
                device.AddLogEntry($"STP: Root port {portName} lost, recalculating spanning tree");
                state.TopologyChanged = true;
            }

            // Reset port to designated role
            portInfo.Role = StpPortRole.Designated;
            portInfo.DesignatedBridge = state.BridgeId;
            portInfo.DesignatedCost = state.RootPathCost;
        }

        private async Task HandleForwardDelayExpiry(INetworkDevice device, string portName, StpState state, StpConfig config)
        {
            var portInfo = state.PortInfo[portName];
            var stateMachine = new StpStateMachine(portInfo);

            switch (portInfo.State)
            {
                case StpPortState.Listening:
                    stateMachine.ProcessStateTransition(StpPortState.Learning, "Forward delay timer expired");
                    device.AddLogEntry($"STP: Port {portName} transitioned from Listening to Learning");
                    break;
                case StpPortState.Learning:
                    stateMachine.ProcessStateTransition(StpPortState.Forwarding, "Forward delay timer expired");
                    device.AddLogEntry($"STP: Port {portName} transitioned from Learning to Forwarding");
                    break;
            }

            state.UpdatePortState(portName, portInfo.State);
        }

        private async Task ElectRootBridge(INetworkDevice device, StpState state)
        {
            // In a full implementation, this would consider all received BPDUs
            // For simulation, we'll use a simplified approach

            string currentRoot = state.RootBridgeId;

            // Check all neighbors to see if any has a better bridge ID
            foreach (var portName in state.PortInfo.Keys)
            {
                var connectedDevice = device.GetConnectedDevice(portName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborStpConfig = neighborDevice.GetStpConfiguration();

                    if (neighborStpConfig?.IsEnabled == true)
                    {
                        var neighborBridgeId = GenerateBridgeId(neighborStpConfig);
                        if (StpBridgePriority.CompareBridgeIds(neighborBridgeId, currentRoot) < 0)
                        {
                            currentRoot = neighborBridgeId;
                        }
                    }
                }
            }

            if (currentRoot != state.RootBridgeId)
            {
                state.RootBridgeId = currentRoot;
                state.IsRootBridge = (currentRoot == state.BridgeId);
                state.TopologyChanged = true;
                device.AddLogEntry($"STP: Root bridge changed to {currentRoot}");
            }
        }

        private async Task SelectRootPort(INetworkDevice device, StpState state)
        {
            if (state.IsRootBridge)
            {
                state.RootPort = "";
                state.RootPathCost = 0;
                return;
            }

            string bestRootPort = "";
            int bestRootPathCost = int.MaxValue;

            foreach (var portName in state.PortInfo.Keys)
            {
                var portInfo = state.PortInfo[portName];
                var connectedDevice = device.GetConnectedDevice(portName);

                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborStpConfig = neighborDevice.GetStpConfiguration();

                    if (neighborStpConfig?.IsEnabled == true)
                    {
                        var neighborBridgeId = GenerateBridgeId(neighborStpConfig);
                        int pathCostViaThisPort = portInfo.PathCost;

                        // If this neighbor can reach the root bridge
                        if (neighborBridgeId == state.RootBridgeId || pathCostViaThisPort < bestRootPathCost)
                        {
                            bestRootPort = portName;
                            bestRootPathCost = pathCostViaThisPort;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(bestRootPort))
            {
                state.RootPort = bestRootPort;
                state.RootPathCost = bestRootPathCost;
                state.PortInfo[bestRootPort].Role = StpPortRole.Root;
            }
        }

        private async Task SelectDesignatedPorts(INetworkDevice device, StpState state)
        {
            foreach (var portName in state.PortInfo.Keys)
            {
                var portInfo = state.PortInfo[portName];

                // Root port is already assigned
                if (portName == state.RootPort)
                    continue;

                // Check if we should be the designated bridge for this segment
                var connectedDevice = device.GetConnectedDevice(portName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborStpConfig = neighborDevice.GetStpConfiguration();

                    if (neighborStpConfig?.IsEnabled == true)
                    {
                        var neighborBridgeId = GenerateBridgeId(neighborStpConfig);

                        // We become designated if we have better bridge ID or better cost to root
                        bool shouldBeDesignated = StpBridgePriority.CompareBridgeIds(state.BridgeId, neighborBridgeId) < 0 ||
                                                 state.RootPathCost < 0; // Simplified logic

                        if (shouldBeDesignated)
                        {
                            portInfo.Role = StpPortRole.Designated;
                            portInfo.DesignatedBridge = state.BridgeId;
                            portInfo.DesignatedCost = state.RootPathCost;
                        }
                        else
                        {
                            portInfo.Role = StpPortRole.Alternate;
                        }
                    }
                    else
                    {
                        // No STP neighbor, we are designated
                        portInfo.Role = StpPortRole.Designated;
                        portInfo.DesignatedBridge = state.BridgeId;
                        portInfo.DesignatedCost = state.RootPathCost;
                    }
                }
            }
        }

        private async Task UpdatePortStates(INetworkDevice device, StpState state, StpConfig config)
        {
            foreach (var kvp in state.PortInfo)
            {
                var portName = kvp.Key;
                var portInfo = kvp.Value;
                var stateMachine = new StpStateMachine(portInfo);

                StpPortState newState = portInfo.Role switch
                {
                    StpPortRole.Root => GetRootPortState(portInfo, config),
                    StpPortRole.Designated => GetDesignatedPortState(portInfo, config),
                    StpPortRole.Alternate => StpPortState.Blocking,
                    StpPortRole.Backup => StpPortState.Blocking,
                    _ => StpPortState.Blocking
                };

                if (newState != portInfo.State)
                {
                    stateMachine.ProcessStateTransition(newState, $"Role: {portInfo.Role}");
                    state.UpdatePortState(portName, newState);
                    device.AddLogEntry($"STP: Port {portName} ({portInfo.Role}) transitioned to {newState}");
                }
            }
        }

        private StpPortState GetRootPortState(StpPortInfo portInfo, StpConfig config)
        {
            // Root port state progression: Blocking -> Listening -> Learning -> Forwarding
            return portInfo.State switch
            {
                StpPortState.Blocking => StpPortState.Listening,
                StpPortState.Listening => portInfo.Timers.IsForwardDelayTimerExpired() ? StpPortState.Learning : StpPortState.Listening,
                StpPortState.Learning => portInfo.Timers.IsForwardDelayTimerExpired() ? StpPortState.Forwarding : StpPortState.Learning,
                _ => StpPortState.Forwarding
            };
        }

        private StpPortState GetDesignatedPortState(StpPortInfo portInfo, StpConfig config)
        {
            // Designated port state progression: Blocking -> Listening -> Learning -> Forwarding
            if (portInfo.PortFast || portInfo.EdgePort)
            {
                return StpPortState.Forwarding; // PortFast skips listening and learning
            }

            return portInfo.State switch
            {
                StpPortState.Blocking => StpPortState.Listening,
                StpPortState.Listening => portInfo.Timers.IsForwardDelayTimerExpired() ? StpPortState.Learning : StpPortState.Listening,
                StpPortState.Learning => portInfo.Timers.IsForwardDelayTimerExpired() ? StpPortState.Forwarding : StpPortState.Learning,
                _ => StpPortState.Forwarding
            };
        }

        private string GenerateBridgeId(StpConfig config)
        {
            var priority = config?.DefaultPriority ?? 32768;
            var macAddress = _device.DeviceId ?? "00:00:00:00:00:00";
            return $"{priority:X4}:{macAddress}";
        }

        private string GeneratePortId(string portName, int priority)
        {
            // Simple port ID generation: priority + port number
            var portNumber = GetPortNumber(portName);
            return $"{priority:X2}:{portNumber:X2}";
        }

        private int GetPortNumber(string portName)
        {
            // Extract number from interface name or use hash
            var numbers = portName.Where(char.IsDigit).ToArray();
            if (numbers.Length > 0 && int.TryParse(new string(numbers), out int number))
            {
                return number;
            }
            return Math.Abs(portName.GetHashCode()) % 256;
        }

        private StpBpdu CreateCurrentBpdu(StpState state, string portName)
        {
            var portInfo = state.PortInfo[portName];
            return new StpBpdu
            {
                Type = StpBpduType.Configuration,
                RootBridgeId = state.RootBridgeId,
                RootPathCost = state.RootPathCost,
                BridgeId = state.BridgeId,
                PortId = GeneratePortId(portName, portInfo.Priority),
                MessageAge = 0,
                MaxAge = 20,
                HelloTime = 2,
                ForwardDelay = 15
            };
        }

        private async Task SimulateBpduTransmission(INetworkDevice device, string portName, StpBpdu bpdu)
        {
            // Simulate BPDU transmission - in real implementation this would send actual Layer 2 frames
            await Task.Delay(1); // Simulate network delay
        }

        private StpConfig GetStpConfig()
        {
            return _device?.GetStpConfiguration() ?? new StpConfig { IsEnabled = false };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetStpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is StpConfig stpConfig)
            {
                _device?.SetStpConfiguration(stpConfig);
                var stpState = (StpState)_state;
                stpState.BridgeId = GenerateBridgeId(stpConfig);
                _state.IsActive = stpConfig.IsEnabled;
                _state.MarkStateChanged();
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Generic" }; // STP is a standard protocol
        }
    }
}
