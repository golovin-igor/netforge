using System.Globalization;
using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// For OspfConfig
// Added for events
// Added for Task

namespace NetSim.Simulation.Protocols.Implementations
{
    public class OspfProtocol : INetworkProtocol
    {
        private OspfConfig _ospfConfig;
        private NetworkDevice _device; // Store reference to the device
        private readonly OspfState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _neighborLastSeen = new(); // Track neighbor timers

        public ProtocolType Type => ProtocolType.OSPF;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _ospfConfig = device.GetOspfConfiguration();
            if (_ospfConfig == null)
            {
                // Optionally, create a default OSPF config or log a warning
                device.AddLogEntry("OSPFProtocol: OSPF configuration not found on initialization.");
                // _ospfConfig = new OspfConfig(); // Example: if you want to create a default
                // device.SetOspfConfiguration(_ospfConfig); // And assign it back
            }
            else
            {
                device.AddLogEntry("OSPFProtocol: Successfully initialized with existing OSPF configuration.");
                
                // Initialize interface states
                foreach (var ospfInterface in _ospfConfig.Interfaces.Values)
                {
                    _state.InterfaceStates[ospfInterface.Name] = new OspfInterfaceState(ospfInterface.Name);
                }
                
                // Mark topology as changed to trigger initial SPF calculation
                _state.MarkTopologyChanged();
            }
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            // Subscribe to relevant events
            eventBus.Subscribe<InterfaceStateChangedEventArgs>(HandleInterfaceStateChangeAsync);
            eventBus.Subscribe<ProtocolConfigChangedEventArgs>(HandleProtocolConfigChangeAsync);
            eventBus.Subscribe<PhysicalConnectionStateChangedEventArgs>(HandlePhysicalConnectionStateChangeAsync);
            // Could also subscribe to LinkChangedEventArgs if direct link info is more useful than inferred from interface states
        }

        private async Task HandleInterfaceStateChangeAsync(InterfaceStateChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name)
            {
                _device.AddLogEntry($"OSPFProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating OSPF state.");
                
                // Update interface state and mark topology as changed
                if (_state.InterfaceStates.ContainsKey(args.InterfaceName))
                {
                    _state.MarkTopologyChanged();
                }
                
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == ProtocolType.OSPF)
            {
                _device.AddLogEntry($"OSPFProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating OSPF configuration and state.");
                // Refresh our configuration reference
                _ospfConfig = _device.GetOspfConfiguration();
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandlePhysicalConnectionStateChangeAsync(PhysicalConnectionStateChangedEventArgs args)
        {
            // Check if this physical connection change affects this device
            if (args.Connection.Device1Name == _device.Name || args.Connection.Device2Name == _device.Name)
            {
                string affectedInterface = args.Connection.Device1Name == _device.Name 
                    ? args.Connection.Interface1Name 
                    : args.Connection.Interface2Name;

                _device.AddLogEntry($"OSPFProtocol on {_device.Name}: Physical connection state changed for interface {affectedInterface}: {args.PreviousState} -> {args.NewState}");
                
                // Mark topology as changed when physical connections change
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            device.AddLogEntry($"OSPFProtocol: Updating OSPF state on device {device.Name}...");

            if (_ospfConfig == null)
            {
                device.AddLogEntry("OSPFProtocol: No OSPF configuration found, skipping state update.");
                return;
            }

            if (!_ospfConfig.IsEnabled)
            {
                device.AddLogEntry($"OSPFProtocol ({_ospfConfig.ProcessId}): Not enabled, clearing OSPF routes and skipping update.");
                device.ClearRoutesByProtocol("OSPF");
                _state.RoutingTable.Clear();
                return;
            }

            // Maintain neighbor adjacencies
            await UpdateNeighborAdjacencies(device);
            
            // Clean up dead neighbors
            await CleanupDeadNeighbors(device);
            
            // Only run SPF if topology changed
            if (_state.TopologyChanged)
            {
                await RunSpf(device);
                _state.TopologyChanged = false;
                _state.LastSpfCalculation = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("OSPFProtocol: No topology changes detected, skipping SPF calculation.");
            }

            device.AddLogEntry("OSPFProtocol: OSPF state update completed.");
        }

        private async Task UpdateNeighborAdjacencies(NetworkDevice device)
        {
            device.AddLogEntry("OSPFProtocol: Updating OSPF neighbor adjacencies...");

            var ospfInterfaces = _ospfConfig.Interfaces.Values;
            foreach (var ospfInterface in ospfInterfaces)
            {
                // Only consider interfaces that should participate in protocols (physically connected)
                if (!device.ShouldInterfaceParticipateInProtocols(ospfInterface.Name))
                {
                    // Update interface state to Down
                    if (_state.InterfaceStates.ContainsKey(ospfInterface.Name))
                    {
                        _state.InterfaceStates[ospfInterface.Name].ChangeState(OspfInterfaceStateType.Down);
                    }
                    
                    device.AddLogEntry($"OSPFProtocol: Interface {ospfInterface.Name} not physically connected - setting to Down state");
                    continue;
                }

                // Update interface state to appropriate state (simplified to PointToPoint or DR/BDR)
                if (_state.InterfaceStates.ContainsKey(ospfInterface.Name))
                {
                    var interfaceState = _state.InterfaceStates[ospfInterface.Name];
                    if (interfaceState.State == OspfInterfaceStateType.Down)
                    {
                        // Transition to appropriate state based on network type
                        interfaceState.ChangeState(ospfInterface.NetworkType == "point-to-point" 
                            ? OspfInterfaceStateType.PointToPoint 
                            : OspfInterfaceStateType.Waiting);
                    }
                }

                // Get physical connection metrics to determine OSPF cost
                var metrics = device.GetPhysicalConnectionMetrics(ospfInterface.Name);
                if (metrics != null)
                {
                    // Adjust OSPF cost based on physical connection quality
                    int adjustedCost = CalculateOspfCost(ospfInterface.Cost, metrics);
                    if (adjustedCost != ospfInterface.Cost)
                    {
                        device.AddLogEntry($"OSPFProtocol: Adjusted OSPF cost for {ospfInterface.Name} from {ospfInterface.Cost} to {adjustedCost} based on physical connection quality");
                        ospfInterface.Cost = adjustedCost;
                        _state.MarkTopologyChanged();
                    }

                    // Check if connection quality is suitable for OSPF
                    if (!metrics.IsSuitableForRouting)
                    {
                        device.AddLogEntry(string.Format(CultureInfo.InvariantCulture, "OSPFProtocol: Connection quality too poor for OSPF on interface {0} (Quality: {1:F1}%)", ospfInterface.Name, metrics.QualityScore));
                        continue;
                    }
                }

                // Find OSPF neighbors through physical connections
                var connectedDevice = device.GetConnectedDevice(ospfInterface.Name);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    
                    // Check if neighbor also runs OSPF in the same area
                    var neighborOspf = neighborDevice.GetOspfConfiguration();
                    if (neighborOspf != null && neighborOspf.IsEnabled && 
                        IsInSameOspfArea(ospfInterface, neighborOspf, neighborInterface))
                    {
                        // Get or create neighbor adjacency
                        var neighborId = neighborOspf.RouterId;
                        var neighborAdjacency = _state.GetOrCreateNeighbor(neighborId, neighborDevice.Name, ospfInterface.Name);
                        
                        // Update neighbor last seen time
                        _neighborLastSeen[neighborId] = DateTime.Now;
                        
                        // Progress neighbor state (simplified state machine)
                        await ProgressNeighborState(neighborAdjacency, device);
                        
                        device.AddLogEntry($"OSPFProtocol: Neighbor {neighborDevice.Name} ({neighborId}) on interface {ospfInterface.Name} in state {neighborAdjacency.State}");
                    }
                }
            }
        }

        private async Task ProgressNeighborState(OspfNeighborAdjacency neighbor, NetworkDevice device)
        {
            // Simplified OSPF neighbor state machine
            switch (neighbor.State)
            {
                case OspfNeighborState.Down:
                    neighbor.ChangeState(OspfNeighborState.Init);
                    _state.MarkTopologyChanged();
                    break;
                    
                case OspfNeighborState.Init:
                    // In real OSPF, this would involve Hello packet exchange
                    neighbor.ChangeState(OspfNeighborState.TwoWay);
                    _state.MarkTopologyChanged();
                    break;
                    
                case OspfNeighborState.TwoWay:
                    // Progress to database exchange
                    neighbor.ChangeState(OspfNeighborState.ExStart);
                    _state.MarkTopologyChanged();
                    break;
                    
                case OspfNeighborState.ExStart:
                    // Skip Exchange and Loading for simplicity
                    neighbor.ChangeState(OspfNeighborState.Full);
                    _state.MarkTopologyChanged();
                    device.AddLogEntry($"OSPFProtocol: Neighbor {neighbor.NeighborId} reached Full adjacency");
                    break;
                    
                case OspfNeighborState.Full:
                    // Neighbor is fully adjacent - maintain state
                    break;
            }
        }

        private async Task CleanupDeadNeighbors(NetworkDevice device)
        {
            var deadNeighbors = _state.GetDeadNeighbors();
            foreach (var deadNeighborId in deadNeighbors)
            {
                device.AddLogEntry($"OSPFProtocol: Neighbor {deadNeighborId} timed out, removing adjacency");
                _state.RemoveNeighbor(deadNeighborId);
                _neighborLastSeen.Remove(deadNeighborId);
            }
        }

        private async Task RunSpf(NetworkDevice device)
        {
            device.AddLogEntry("OSPFProtocol: Running SPF calculation due to topology change...");
            
            // Clear existing OSPF routes
            device.ClearRoutesByProtocol("OSPF");
            _state.RoutingTable.Clear();

            // Build topology graph from neighbor adjacencies
            var topology = BuildTopologyGraph(device);
            
            // Run Dijkstra's algorithm (simplified)
            var routes = CalculateShortestPaths(device, topology);
            
            // Install routes in device routing table
            foreach (var route in routes)
            {
                var deviceRoute = new Route(route.Network, route.SubnetMask, route.NextHop, route.Interface, "OSPF");
                deviceRoute.Metric = route.Cost;
                device.AddRoute(deviceRoute);
                
                // Store in OSPF state
                _state.RoutingTable[route.Network] = route;
                
                device.AddLogEntry($"OSPFProtocol: Installed route to {route.Network}/{device.MaskToCidr(route.SubnetMask)} via {route.NextHop} (cost: {route.Cost})");
            }
            
            device.AddLogEntry($"OSPFProtocol: SPF calculation completed, installed {routes.Count} routes");
        }

        private Dictionary<string, List<(string neighbor, int cost)>> BuildTopologyGraph(NetworkDevice device)
        {
            var topology = new Dictionary<string, List<(string neighbor, int cost)>>();
            
            // Add this device to topology
            topology[device.Name] = new List<(string neighbor, int cost)>();
            
            // Add neighbors from Full adjacencies
            foreach (var neighbor in _state.Neighbors.Values.Where(n => n.State == OspfNeighborState.Full))
            {
                var ospfInterface = _ospfConfig.Interfaces.Values.FirstOrDefault(i => i.Name == neighbor.InterfaceName);
                if (ospfInterface != null)
                {
                    topology[device.Name].Add((neighbor.NeighborId, ospfInterface.Cost));
                }
            }
            
            return topology;
        }

        private List<OspfRoute> CalculateShortestPaths(NetworkDevice device, Dictionary<string, List<(string neighbor, int cost)>> topology)
        {
            var routes = new List<OspfRoute>();
            
            // For now, just add directly connected networks
            var ospfInterfaces = _ospfConfig.Interfaces.Values
                .Where(iface => device.ShouldInterfaceParticipateInProtocols(iface.Name));

            foreach (var ospfInterface in ospfInterfaces)
            {
                var interfaceConfig = device.GetInterface(ospfInterface.Name);
                if (interfaceConfig == null || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                    continue;

                // Add routes to connected networks (only if physically connected)
                var network = device.GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
                var route = new OspfRoute(network, interfaceConfig.SubnetMask, "0.0.0.0", ospfInterface.Name, ospfInterface.Cost, OspfRouteType.IntraArea);
                routes.Add(route);
            }
            
            return routes;
        }

        private int CalculateOspfCost(int baseCost, PhysicalConnectionMetrics metrics)
        {
            // Adjust OSPF cost based on physical connection characteristics
            double adjustedCost = baseCost;

            // Increase cost for higher latency
            if (metrics.Latency > 10)
            {
                adjustedCost *= (1.0 + (metrics.Latency - 10) * 0.1);
            }

            // Increase cost for packet loss
            if (metrics.PacketLoss > 0)
            {
                adjustedCost *= (1.0 + metrics.PacketLoss * 0.2);
            }

            // Increase cost for degraded connections
            if (metrics.State == PhysicalConnectionState.Degraded)
            {
                adjustedCost *= 2.0; // Double the cost for degraded connections
            }

            // Apply bandwidth-based cost (similar to Cisco's calculation)
            int bandwidthCost = 100000000 / Math.Max(metrics.Bandwidth * 1000000, 1); // 100Mbps reference bandwidth
            adjustedCost = Math.Max(adjustedCost, bandwidthCost);

            return Math.Max((int)adjustedCost, 1); // OSPF cost must be at least 1
        }

        private bool IsInSameOspfArea(OspfInterface localInterface, OspfConfig neighborOspf, string neighborInterfaceName)
        {
            // Check if neighbor interface is in the same OSPF area
            if (neighborOspf.Interfaces.TryGetValue(neighborInterfaceName, out var neighborInterface))
            {
                return localInterface.Area == neighborInterface.Area;
            }
            return false;
        }
    }
} 
