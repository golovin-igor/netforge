using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// For EigrpConfig
// Added

// Added

namespace NetSim.Simulation.Protocols.Implementations
{
    public class EigrpProtocol : INetworkProtocol
    {
        private EigrpConfig _eigrpConfig;
        private NetworkDevice _device;
        private readonly EigrpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _neighborLastSeen = new(); // Track neighbor timers

        public ProtocolType Type => ProtocolType.EIGRP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _eigrpConfig = device.GetEigrpConfiguration();
            if (_eigrpConfig == null)
            {
                device.AddLogEntry("EigrpProtocol: EIGRP configuration not found on initialization.");
                // Optionally create a default EigrpConfig
                // _eigrpConfig = new EigrpConfig(autonomousSystem: 1); // Example ASN
                // device.SetEigrpConfiguration(_eigrpConfig);
            }
            else
            {
                device.AddLogEntry($"EigrpProtocol: Successfully initialized with EIGRP AS {_eigrpConfig.AsNumber}.");
                
                // Initialize interface states
                var deviceInterfaces = device.GetAllInterfaces();
                foreach (var iface in deviceInterfaces.Values)
                {
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        _state.InterfaceStates[iface.Name] = new EigrpInterfaceState(iface.Name);
                    }
                }
                
                // Mark topology as changed to trigger initial DUAL calculation
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
                _device.AddLogEntry($"EIGRPProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating EIGRP state.");
                
                // Mark topology as changed when interface states change
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"EIGRPProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating EIGRP configuration and state.");
                _eigrpConfig = _device.GetEigrpConfiguration();
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_eigrpConfig == null) _eigrpConfig = device.GetEigrpConfiguration();

            if (_eigrpConfig == null || !_eigrpConfig.IsEnabled)
            {
                device.AddLogEntry($"EIGRPProtocol on {device.Name}: EIGRP configuration missing or not enabled. Clearing EIGRP routes.");
                device.ClearRoutesByProtocol("EIGRP");
                _state.RoutingTable.Clear();
                return;
            }

            device.AddLogEntry($"EigrpProtocol: Updating EIGRP state for AS {_eigrpConfig.AsNumber} on device {device.Name}...");
            
            // Update neighbor adjacencies
            await UpdateNeighborAdjacencies(device);
            
            // Clean up stale neighbors
            await CleanupStaleNeighbors(device);
            
            // Only run DUAL if topology changed
            if (_state.TopologyChanged)
            {
                await RunDualAlgorithm(device);
                _state.TopologyChanged = false;
                _state.LastDualCalculation = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("EIGRPProtocol: No topology changes detected, skipping DUAL calculation.");
            }
            
            device.AddLogEntry("EigrpProtocol: EIGRP state update completed.");
        }

        private async Task UpdateNeighborAdjacencies(NetworkDevice device)
        {
            device.AddLogEntry("EigrpProtocol: Updating EIGRP neighbor adjacencies...");

            var deviceInterfaces = device.GetAllInterfaces();
            foreach (var iface in deviceInterfaces.Values)
            {
                if (string.IsNullOrEmpty(iface.IpAddress) || !iface.IsUp)
                    continue;

                // Check if interface should participate in EIGRP
                if (!device.ShouldInterfaceParticipateInProtocols(iface.Name))
                    continue;

                // Find potential EIGRP neighbors
                var connectedDevice = device.GetConnectedDevice(iface.Name);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    
                    // Check if physical connection is suitable for routing
                    if (!IsNeighborReachable(device, iface.Name, neighborDevice))
                    {
                        device.AddLogEntry($"EigrpProtocol: Physical connection to {neighborDevice.Name} on {iface.Name} not suitable for routing");
                        continue;
                    }
                    
                    // Check if neighbor runs EIGRP in the same AS
                    var neighborEigrp = neighborDevice.GetEigrpConfiguration();
                    if (neighborEigrp != null && neighborEigrp.IsEnabled && 
                        neighborEigrp.AsNumber == _eigrpConfig.AsNumber)
                    {
                        var neighborIp = neighborDevice.GetInterface(neighborInterface)?.IpAddress;
                        if (!string.IsNullOrEmpty(neighborIp))
                        {
                            // Get or create neighbor adjacency
                            var neighborAdjacency = _state.GetOrCreateNeighbor(neighborIp, iface.Name);
                            
                            // Update neighbor last seen time
                            _neighborLastSeen[neighborIp] = DateTime.Now;
                            
                            // Progress neighbor state
                            await ProgressNeighborState(neighborAdjacency, device);
                            
                            device.AddLogEntry($"EigrpProtocol: Neighbor {neighborIp} on interface {iface.Name} in state {neighborAdjacency.State}");
                        }
                    }
                }
            }
        }

        private async Task ProgressNeighborState(EigrpNeighborAdjacency neighbor, NetworkDevice device)
        {
            // Simplified EIGRP neighbor state machine
            switch (neighbor.State)
            {
                case EigrpNeighborState.Down:
                    neighbor.ChangeState(EigrpNeighborState.Pending);
                    _state.MarkTopologyChanged();
                    break;
                    
                case EigrpNeighborState.Pending:
                    // In real EIGRP, this would involve Hello packet exchange and parameter negotiation
                    neighbor.ChangeState(EigrpNeighborState.Up);
                    _state.MarkTopologyChanged();
                    device.AddLogEntry($"EigrpProtocol: Neighbor {neighbor.IpAddress} established adjacency");
                    break;
                    
                case EigrpNeighborState.Up:
                    // Neighbor is up - maintain state
                    break;
            }
        }

        private async Task CleanupStaleNeighbors(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors();
            foreach (var staleNeighborIp in staleNeighbors)
            {
                device.AddLogEntry($"EigrpProtocol: Neighbor {staleNeighborIp} timed out, removing adjacency");
                _state.RemoveNeighbor(staleNeighborIp);
                _neighborLastSeen.Remove(staleNeighborIp);
            }
        }

        private async Task RunDualAlgorithm(NetworkDevice device)
        {
            device.AddLogEntry("EigrpProtocol: Running DUAL algorithm due to topology change...");
            
            // Clear existing EIGRP routes
            device.ClearRoutesByProtocol("EIGRP");
            _state.RoutingTable.Clear();
            _state.TopologyTable.Clear();

            // Build topology table from neighbor adjacencies and connected networks
            await BuildTopologyTable(device);
            
            // Run DUAL algorithm to select best paths
            var routes = CalculateBestPaths(device);
            
            // Install routes in device routing table
            await InstallEigrpRoutes(device, routes);
            
            device.AddLogEntry($"EigrpProtocol: DUAL algorithm completed, installed {routes.Count} routes");
        }

        private async Task InstallEigrpRoutes(NetworkDevice device, List<EigrpRoute> routes)
        {
            device.AddLogEntry("EigrpProtocol: Installing EIGRP routes...");
            
            device.ClearRoutesByProtocol("EIGRP");
            
            foreach (var route in routes)
            {
                var deviceRoute = new Route(route.Network, route.SubnetMask, route.NextHop, route.Interface, "EIGRP");
                deviceRoute.Metric = route.Metric;
                deviceRoute.AdminDistance = 90; // EIGRP administrative distance
                device.AddRoute(deviceRoute);
                
                // Store in EIGRP state
                var routeKey = $"{route.Network}/{device.MaskToCidr(route.SubnetMask)}";
                _state.RoutingTable[routeKey] = route;
                
                device.AddLogEntry($"EigrpProtocol: Installed route to {routeKey} via {route.NextHop} (metric: {route.Metric}, AD: 90)");
            }
            
            device.AddLogEntry($"EigrpProtocol: Successfully installed {routes.Count} EIGRP routes");
        }

        private async Task BuildTopologyTable(NetworkDevice device)
        {
            device.AddLogEntry("EigrpProtocol: Building topology table...");

            // Add directly connected networks
            var deviceInterfaces = device.GetAllInterfaces();
            foreach (var iface in deviceInterfaces.Values)
            {
                if (string.IsNullOrEmpty(iface.IpAddress) || !iface.IsUp)
                    continue;

                if (!device.ShouldInterfaceParticipateInProtocols(iface.Name))
                    continue;

                var network = device.GetNetworkAddress(iface.IpAddress, iface.SubnetMask);
                var routeKey = $"{network}/{device.MaskToCidr(iface.SubnetMask)}";
                
                if (!_state.TopologyTable.ContainsKey(routeKey))
                {
                    _state.TopologyTable[routeKey] = new EigrpTopologyEntry(network, iface.SubnetMask);
                }
                
                var topologyEntry = _state.TopologyTable[routeKey];
                var connectedRoute = new EigrpRoute(network, iface.SubnetMask, "0.0.0.0", iface.Name);
                connectedRoute.Metric = connectedRoute.CalculateMetric();
                topologyEntry.Routes.Add(connectedRoute);
                
                device.AddLogEntry($"EigrpProtocol: Added connected network {routeKey} to topology table");
            }
        }

        private List<EigrpRoute> CalculateBestPaths(NetworkDevice device)
        {
            var bestRoutes = new List<EigrpRoute>();
            
            // For each network in topology table, select successor (best path)
            foreach (var topologyEntry in _state.TopologyTable.Values)
            {
                if (topologyEntry.Routes.Count > 0)
                {
                    // Select route with lowest metric as successor
                    var successor = topologyEntry.Routes.OrderBy(r => r.Metric).First();
                    successor.IsSuccessor = true;
                    topologyEntry.Successor = successor;
                    bestRoutes.Add(successor);
                    
                    // Select feasible successors (simplified feasibility condition)
                    var feasibleSuccessors = topologyEntry.Routes
                        .Where(r => r != successor && r.Metric < successor.Metric * 2)
                        .ToList();
                    
                    foreach (var fs in feasibleSuccessors)
                    {
                        fs.IsFeasibleSuccessor = true;
                    }
                    
                    topologyEntry.FeasibleSuccessors = feasibleSuccessors;
                }
            }
            
            return bestRoutes;
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
