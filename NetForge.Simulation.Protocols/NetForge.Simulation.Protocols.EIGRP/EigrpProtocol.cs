using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.EIGRP
{
    /// <summary>
    /// EIGRP (Enhanced Interior Gateway Routing Protocol) implementation
    /// Cisco proprietary distance vector protocol with DUAL algorithm
    /// Administrative Distance: 90 (internal), 170 (external)
    /// </summary>
    public class EigrpProtocol : BaseProtocol, IDeviceProtocol
    {
        private DualAlgorithm? _dualAlgorithm;

        public override NetworkProtocolType Type => NetworkProtocolType.EIGRP;
        public override string Name => "Enhanced Interior Gateway Routing Protocol";

        protected override BaseProtocolState CreateInitialState()
        {
            return new EigrpState();
        }

        protected override void OnInitialized()
        {
            var eigrpConfig = GetEigrpConfig();
            var eigrpState = (EigrpState)_state;

            eigrpState.RouterId = eigrpConfig.RouterId ?? _device.Name;
            eigrpState.AsNumber = eigrpConfig.AsNumber;
            eigrpState.SequenceNumber = 1;

            // Initialize DUAL algorithm
            _dualAlgorithm = new DualAlgorithm(_device, eigrpState);
            _dualAlgorithm.RouteComputationCompleted += OnRouteComputationCompleted;

            _device.AddLogEntry($"EIGRP: AS {eigrpState.AsNumber} initialized with Router ID {eigrpState.RouterId}");
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var eigrpConfig = GetEigrpConfig();
            var eigrpState = (EigrpState)_state;

            if (!eigrpConfig.IsEnabled)
            {
                eigrpState.IsActive = false;
                return;
            }

            eigrpState.IsActive = true;

            // Send Hello packets and discover neighbors
            await SendHelloPackets(device, eigrpConfig, eigrpState);

            // Discover EIGRP neighbors on all configured interfaces
            await DiscoverEigrpNeighbors(device, eigrpConfig, eigrpState);

            // Process received packets
            await ProcessReceivedPackets(device, eigrpState);

            // Handle neighbor timeouts
            await HandleNeighborTimeouts(device, eigrpState);
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var eigrpState = (EigrpState)_state;

            device.AddLogEntry("EIGRP: Running DUAL algorithm due to topology change...");

            // Clear existing EIGRP routes
            device.ClearRoutesByProtocol("EIGRP");
            eigrpState.RoutingTable.Clear();
            eigrpState.CalculatedRoutes.Clear();

            // Run DUAL algorithm to compute routes
            await RunDualAlgorithm(device, eigrpState);

            // Install calculated routes
            await InstallEigrpRoutes(device, eigrpState);

            eigrpState.TopologyChanged = false;
            device.AddLogEntry("EIGRP: DUAL algorithm completed");
        }

        private async Task SendHelloPackets(INetworkDevice device, EigrpConfig config, EigrpState state)
        {
            var now = DateTime.Now;

            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if interface is configured for EIGRP
                if (!IsInterfaceEnabledForEigrp(interfaceName, config))
                    continue;

                // Check if it's time to send hello
                var lastHello = state.InterfaceTimers.GetValueOrDefault(interfaceName + "_hello", DateTime.MinValue);
                if ((now - lastHello).TotalSeconds >= 5) // Default EIGRP hello interval
                {
                    await SendHelloPacket(device, interfaceName, config, state);
                    state.InterfaceTimers[interfaceName + "_hello"] = now;
                }
            }
        }

        private async Task SendHelloPacket(INetworkDevice device, string interfaceName, EigrpConfig config, EigrpState state)
        {
            var helloPacket = new EigrpPacket
            {
                PacketType = EigrpPacketType.Hello,
                SourceRouter = state.RouterId,
                AsNumber = state.AsNumber,
                SequenceNumber = 0, // Hello packets use sequence 0
                RequiresAck = false
            };

            // Add TLVs (Type-Length-Value)
            helloPacket.Tlvs["HoldTime"] = 15; // Default EIGRP hold time
            helloPacket.Tlvs["K-values"] = new int[] { 1, 0, 1, 0, 0 }; // Default K-values

            device.AddLogEntry($"EIGRP: Sending Hello on interface {interfaceName}");

            // Simulate hello packet transmission
            await SimulatePacketTransmission(device, interfaceName, helloPacket);
        }

        private async Task DiscoverEigrpNeighbors(INetworkDevice device, EigrpConfig config, EigrpState state)
        {
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                if (!IsInterfaceEnabledForEigrp(interfaceName, config))
                    continue;

                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;

                    var neighborEigrp = neighborDevice.GetEigrpConfiguration();
                    if (neighborEigrp?.IsEnabled == true && neighborEigrp.AsNumber == config.AsNumber)
                    {
                        var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                        var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new EigrpNeighbor
                        {
                            RouterId = neighborDevice.Name,
                            InterfaceName = neighborInterface,
                            IpAddress = neighborDevice.GetInterface(neighborInterface)?.IpAddress ?? "0.0.0.0",
                            AsNumber = neighborEigrp.AsNumber,
                            State = EigrpNeighborState.Pending,
                            HoldTime = 15 // Default EIGRP hold time
                        });

                        // Update neighbor state machine
                        await UpdateNeighborStateMachine(neighbor, device, neighborDevice, state);

                        // Update neighbor activity
                        state.UpdateNeighborActivity(neighborKey);

                        device.AddLogEntry($"EIGRP: Neighbor {neighbor.RouterId} state: {neighbor.State}");
                    }
                }
            }
        }

        private async Task UpdateNeighborStateMachine(EigrpNeighbor neighbor, INetworkDevice device, INetworkDevice neighborDevice, EigrpState state)
        {
            var previousState = neighbor.State;

            switch (neighbor.State)
            {
                case EigrpNeighborState.Down:
                    // Transition to Pending when we receive hello
                    neighbor.State = EigrpNeighborState.Pending;
                    break;

                case EigrpNeighborState.Pending:
                    // Transition to Up after successful parameter exchange
                    neighbor.State = EigrpNeighborState.Up;
                    break;

                case EigrpNeighborState.Up:
                    // Update timers
                    neighbor.LastHello = DateTime.Now;
                    break;
            }

            // If neighbor just came up, exchange topology information
            if (previousState != EigrpNeighborState.Up && neighbor.State == EigrpNeighborState.Up)
            {
                await ExchangeTopologyWithNeighbor(device, neighbor, state);
                state.TopologyChanged = true;
            }
        }

        private async Task ExchangeTopologyWithNeighbor(INetworkDevice device, EigrpNeighbor neighbor, EigrpState state)
        {
            device.AddLogEntry($"EIGRP: Exchanging topology with neighbor {neighbor.RouterId}");

            // Send update packet with our topology table
            var updatePacket = new EigrpPacket
            {
                PacketType = EigrpPacketType.Update,
                SourceRouter = state.RouterId,
                DestinationRouter = neighbor.RouterId,
                SequenceNumber = state.SequenceNumber++,
                AsNumber = state.AsNumber,
                RequiresAck = true
            };

            // Add topology entries as TLVs
            var topologyEntries = new List<object>();
            foreach (var entry in state.TopologyTable.Values)
            {
                topologyEntries.Add(new
                {
                    Network = entry.Network,
                    Mask = entry.Mask,
                    Metric = entry.Metric,
                    NextHop = entry.NextHop
                });
            }

            updatePacket.Tlvs["TopologyEntries"] = topologyEntries;

            await SimulatePacketTransmission(device, neighbor.InterfaceName, updatePacket);
        }

        private async Task ProcessReceivedPackets(INetworkDevice device, EigrpState state)
        {
            // Simulate processing received EIGRP packets
            // In a real implementation, this would process incoming network packets

            // For simulation, we'll update topology based on connected neighbors
            await ProcessTopologyUpdates(device, state);
        }

        private async Task ProcessTopologyUpdates(INetworkDevice device, EigrpState state)
        {
            var config = GetEigrpConfig();

            // Process directly connected networks
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                if (!IsInterfaceEnabledForEigrp(interfaceName, config))
                    continue;

                var networkAddress = CalculateNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
                var entryKey = $"{networkAddress}_{interfaceConfig.SubnetMask}";

                if (!state.TopologyTable.ContainsKey(entryKey))
                {
                    var metric = CalculateInterfaceMetric(interfaceConfig, config);

                    state.TopologyTable[entryKey] = new EigrpTopologyEntry
                    {
                        Network = networkAddress,
                        Mask = interfaceConfig.SubnetMask ?? "255.255.255.0",
                        ViaNeighbor = "Connected",
                        NextHop = "0.0.0.0",
                        Interface = interfaceName,
                        FeasibleDistance = metric,
                        ReportedDistance = 0,
                        Metric = new EigrpMetric
                        {
                            Bandwidth = GetInterfaceBandwidth(interfaceName),
                            Delay = GetInterfaceDelay(interfaceName),
                            Reliability = 255,
                            Load = 1,
                            Mtu = 1500,
                            HopCount = 0
                        },
                        IsSuccessor = true,
                        RouteState = EigrpRouteState.Passive
                    };

                    state.TopologyChanged = true;
                    device.AddLogEntry($"EIGRP: Added connected network {networkAddress}/{interfaceConfig.SubnetMask} via {interfaceName}");
                }
            }

            // Process learned routes from neighbors
            await ProcessLearnedRoutes(device, state);
        }

        private async Task ProcessLearnedRoutes(INetworkDevice device, EigrpState state)
        {
            // Simulate learning routes from EIGRP neighbors
            foreach (var neighbor in state.Neighbors.Values.Where(n => n.State == EigrpNeighborState.Up))
            {
                // Find connected device by checking all interfaces
                INetworkDevice connectedDevice = null;
                foreach (var interfaceName in device.GetAllInterfaces().Keys)
                {
                    var connected = device.GetConnectedDevice(interfaceName);
                    if (connected.HasValue && connected.Value.device.Name == neighbor.RouterId)
                    {
                        connectedDevice = connected.Value.device;
                        break;
                    }
                }
                if (connectedDevice != null)
                {
                    // Learn routes that the neighbor device knows about
                    var neighborRoutes = GetNeighborRoutes(connectedDevice);

                    foreach (var route in neighborRoutes)
                    {
                        var entryKey = $"{route.Network}_{route.Mask}_via_{neighbor.RouterId}";

                        if (!state.TopologyTable.ContainsKey(entryKey))
                        {
                            var metric = CalculateRouteMetric(route, neighbor);

                            state.TopologyTable[entryKey] = new EigrpTopologyEntry
                            {
                                Network = route.Network,
                                Mask = route.Mask,
                                ViaNeighbor = neighbor.RouterId,
                                NextHop = neighbor.IpAddress,
                                Interface = neighbor.InterfaceName,
                                FeasibleDistance = metric,
                                ReportedDistance = route.Metric,
                                Metric = route.CompositeMetric,
                                RouteState = EigrpRouteState.Passive
                            };

                            state.TopologyChanged = true;
                            device.AddLogEntry($"EIGRP: Learned route {route.Network}/{route.Mask} via {neighbor.RouterId}");
                        }
                    }
                }
            }
        }

        private async Task RunDualAlgorithm(INetworkDevice device, EigrpState state)
        {
            // Use the proper DUAL algorithm implementation
            device.AddLogEntry("EIGRP: Running DUAL algorithm...");

            if (_dualAlgorithm == null)
            {
                device.AddLogEntry("EIGRP: DUAL algorithm not initialized");
                return;
            }

            // Process all topology changes through DUAL
            var destinations = state.TopologyTable.Values
                .GroupBy(t => $"{t.Network}_{t.Mask}")
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in destinations)
            {
                var destinationKey = kvp.Key;
                var topologyEntries = kvp.Value;

                // For each topology entry, create an update and process through DUAL
                foreach (var entry in topologyEntries)
                {
                    var update = new EigrpUpdate
                    {
                        Network = entry.Network,
                        Mask = entry.Mask,
                        SourceRouter = entry.ViaNeighbor,
                        NextHopRouter = entry.NextHop,
                        IncomingInterface = entry.Interface,
                        ReportedDistance = entry.ReportedDistance,
                        CompositeMetric = entry.FeasibleDistance,
                        Metrics = entry.Metric,
                        UpdateTime = entry.LastUpdate
                    };

                    try
                    {
                        await _dualAlgorithm.ProcessUpdate(update);
                    }
                    catch (Exception ex)
                    {
                        device.AddLogEntry($"EIGRP: Error processing update for {destinationKey}: {ex.Message}");
                    }
                }
            }

            device.AddLogEntry("EIGRP: DUAL algorithm processing completed");
        }

        private async Task InstallEigrpRoutes(INetworkDevice device, EigrpState state)
        {
            foreach (var route in state.CalculatedRoutes)
            {
                var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "EIGRP")
                {
                    Metric = (int)Math.Min(route.Metric, int.MaxValue),
                    AdminDistance = route.AdminDistance
                };

                device.AddRoute(deviceRoute);
                state.RoutingTable[$"{route.Network}_{route.Mask}"] = route;
            }

            device.AddLogEntry($"EIGRP: Installed {state.CalculatedRoutes.Count} routes");
        }

        private async Task HandleNeighborTimeouts(INetworkDevice device, EigrpState state)
        {
            var staleNeighbors = state.GetStaleNeighbors(state.AsNumber == 1 ? 15 : 180); // Use hold time

            foreach (var neighborId in staleNeighbors)
            {
                device.AddLogEntry($"EIGRP: Neighbor {neighborId} timed out, removing");
                state.RemoveNeighbor(neighborId);
            }
        }

        private bool IsInterfaceEnabledForEigrp(string interfaceName, EigrpConfig config)
        {
            // Check if interface is in any configured network
            var interfaceConfig = _device.GetInterface(interfaceName);
            if (interfaceConfig?.IpAddress == null)
                return false;

            // Use the Networks list from the existing EigrpConfig
            foreach (var network in config.Networks)
            {
                // Simple network matching - assume network includes wildcard mask after space
                var parts = network.Split(' ');
                if (parts.Length >= 2)
                {
                    if (IsIpInNetwork(interfaceConfig.IpAddress, parts[0], parts[1]))
                    {
                        return true;
                    }
                }
                else
                {
                    // If no wildcard mask specified, assume default class mask
                    if (IsIpInNetwork(interfaceConfig.IpAddress, network, "0.0.0.255"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsIpInNetwork(string ipAddress, string networkAddress, string wildcardMask)
        {
            // Simple network matching - in production this would use proper IP parsing
            var ipParts = ipAddress.Split('.').Select(int.Parse).ToArray();
            var netParts = networkAddress.Split('.').Select(int.Parse).ToArray();
            var maskParts = wildcardMask.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < 4; i++)
            {
                if ((ipParts[i] & (255 - maskParts[i])) != netParts[i])
                    return false;
            }

            return true;
        }

        private new bool IsNeighborReachable(INetworkDevice device, string interfaceName, INetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }

        private string CalculateNetworkAddress(string ipAddress, string subnetMask)
        {
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(subnetMask))
                return "0.0.0.0";

            // Simple network address calculation
            var ipParts = ipAddress.Split('.').Select(int.Parse).ToArray();
            var maskParts = subnetMask.Split('.').Select(int.Parse).ToArray();

            var networkParts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                networkParts[i] = ipParts[i] & maskParts[i];
            }

            return string.Join(".", networkParts);
        }

        private long CalculateInterfaceMetric(IInterfaceConfig interfaceConfig, EigrpConfig config)
        {
            var metric = new EigrpMetric
            {
                Bandwidth = GetInterfaceBandwidth(interfaceConfig.Name),
                Delay = GetInterfaceDelay(interfaceConfig.Name),
                Reliability = 255,
                Load = 1,
                Mtu = 1500,
                HopCount = 0
            };

            return metric.CalculateCompositeMetric();
        }

        private long CalculateRouteMetric(EigrpRoute route, EigrpNeighbor neighbor)
        {
            // Add our interface metrics to the advertised metrics
            var interfaceBandwidth = GetInterfaceBandwidth(neighbor.InterfaceName);
            var interfaceDelay = GetInterfaceDelay(neighbor.InterfaceName);

            var metric = new EigrpMetric
            {
                Bandwidth = Math.Min(route.CompositeMetric.Bandwidth, interfaceBandwidth),
                Delay = route.CompositeMetric.Delay + interfaceDelay,
                Reliability = Math.Min(route.CompositeMetric.Reliability, 255),
                Load = Math.Max(route.CompositeMetric.Load, 1),
                Mtu = Math.Min(route.CompositeMetric.Mtu, 1500),
                HopCount = route.CompositeMetric.HopCount + 1
            };

            return metric.CalculateCompositeMetric();
        }

        private int GetInterfaceBandwidth(string interfaceName)
        {
            // Default bandwidth mapping by interface type
            if (interfaceName.StartsWith("Ethernet"))
                return 10000; // 10 Mbps
            else if (interfaceName.StartsWith("FastEthernet"))
                return 100000; // 100 Mbps
            else if (interfaceName.StartsWith("GigabitEthernet"))
                return 1000000; // 1 Gbps
            else
                return 1544; // Default T1 bandwidth
        }

        private int GetInterfaceDelay(string interfaceName)
        {
            // Default delay mapping by interface type (microseconds)
            if (interfaceName.StartsWith("Ethernet"))
                return 100000; // 100 ms
            else if (interfaceName.StartsWith("FastEthernet"))
                return 10000; // 10 ms
            else if (interfaceName.StartsWith("GigabitEthernet"))
                return 1000; // 1 ms
            else
                return 20000; // Default delay
        }

        private List<EigrpRoute> GetNeighborRoutes(INetworkDevice neighbor)
        {
            var routes = new List<EigrpRoute>();

            // Get directly connected networks from neighbor
            foreach (var kvp in neighbor.GetAllInterfaces())
            {
                var interfaceConfig = kvp.Value;
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                if (!string.IsNullOrEmpty(interfaceConfig.IpAddress))
                {
                    var networkAddress = CalculateNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask ?? "255.255.255.0");

                    routes.Add(new EigrpRoute
                    {
                        Network = networkAddress,
                        Mask = interfaceConfig.SubnetMask ?? "255.255.255.0",
                        NextHop = interfaceConfig.IpAddress,
                        Interface = interfaceConfig.Name,
                        Metric = 256000, // Base connected route metric
                        CompositeMetric = new EigrpMetric
                        {
                            Bandwidth = GetInterfaceBandwidth(interfaceConfig.Name),
                            Delay = GetInterfaceDelay(interfaceConfig.Name),
                            Reliability = 255,
                            Load = 1,
                            Mtu = 1500,
                            HopCount = 0
                        }
                    });
                }
            }

            return routes;
        }

        private async Task SimulatePacketTransmission(INetworkDevice device, string interfaceName, EigrpPacket packet)
        {
            // Simulate packet transmission - in real implementation this would send actual network packets
            await Task.Delay(1); // Simulate network delay
        }

        private EigrpConfig GetEigrpConfig()
        {
            return _device?.GetEigrpConfiguration() ?? new EigrpConfig(1) { IsEnabled = false };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetEigrpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is EigrpConfig eigrpConfig)
            {
                _device?.SetEigrpConfiguration(eigrpConfig);
                var eigrpState = (EigrpState)_state;
                eigrpState.RouterId = eigrpConfig.RouterId ?? _device.Name;
                eigrpState.AsNumber = eigrpConfig.AsNumber;
                _state.IsActive = eigrpConfig.IsEnabled;
                _state.MarkStateChanged();
            }
        }

        private void OnRouteComputationCompleted(object? sender, RouteComputationEventArgs e)
        {
            var eigrpState = (EigrpState)_state;

            if (e.SuccessorFound)
            {
                _device.AddLogEntry($"EIGRP: Route computation completed for {e.DestinationKey} in {e.ComputationTime.TotalMilliseconds:F1}ms");
            }
            else
            {
                _device.AddLogEntry($"EIGRP: No successor found for {e.DestinationKey}, route removed");
            }

            // Update metrics
            eigrpState.MarkStateChanged();
        }

        public override void Dispose()
        {
            if (_dualAlgorithm != null)
            {
                _dualAlgorithm.RouteComputationCompleted -= OnRouteComputationCompleted;
                _dualAlgorithm = null;
            }

            base.Dispose();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Generic" }; // EIGRP is Cisco proprietary but can be simulated generically
        }
    }
}
