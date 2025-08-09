using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// For IsisConfig
// Added

// Added

namespace NetSim.Simulation.Protocols.Implementations
{
    public class IsisProtocol : INetworkProtocol
    {
        private IsIsConfig _isisConfig;
        private NetworkDevice _device;
        private readonly IsisState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _adjacencyLastSeen = new(); // Track adjacency timers

        public ProtocolType Type => ProtocolType.ISIS;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _isisConfig = device.GetIsisConfiguration();
            if (_isisConfig == null)
            {
                device.AddLogEntry("IsisProtocol: IS-IS configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"IsisProtocol: Successfully initialized with System ID {_isisConfig.SystemId}.");
                
                // Mark topology as changed to trigger initial SPF calculation
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
                _device.AddLogEntry($"IS-ISProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating IS-IS state.");
                
                // Mark topology as changed when interface states change
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"IS-ISProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating IS-IS configuration and state.");
                _isisConfig = _device.GetIsisConfiguration();
                _state.MarkTopologyChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_isisConfig == null) _isisConfig = device.GetIsisConfiguration();

            if (_isisConfig == null || !_isisConfig.IsEnabled)
            {
                device.AddLogEntry($"IS-ISProtocol on {device.Name}: IS-IS configuration missing or not enabled. Clearing IS-IS routes.");
                await ClearIsisRoutes(device);
                return;
            }

            device.AddLogEntry($"IsisProtocol: Updating IS-IS state for System ID {_isisConfig.SystemId} on device {device.Name}...");
            
            // Update adjacencies
            await UpdateAdjacencies(device);
            
            // Clean up stale adjacencies
            await CleanupStaleAdjacencies(device);
            
            // Process LSP aging
            await ProcessLspAging(device);
            
            // Only run SPF if topology changed
            if (_state.TopologyChanged)
            {
                await RunSpfCalculation(device);
                _state.TopologyChanged = false;
                _state.LastSpfCalculation = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("IS-ISProtocol: No topology changes detected, skipping SPF calculation.");
            }
            
            device.AddLogEntry("IsisProtocol: IS-IS state update completed.");
        }
        
        private async Task UpdateAdjacencies(NetworkDevice device)
        {
            device.AddLogEntry("IsisProtocol: Updating IS-IS adjacencies...");

            // Find potential IS-IS neighbors through physical connections
            var connectedDevice = device.GetConnectedDevice("eth0"); // Simplified - check primary interface
            if (connectedDevice.HasValue)
            {
                var neighborDevice = connectedDevice.Value.device;
                var neighborInterface = connectedDevice.Value.interfaceName;
                
                // Check if physical connection is suitable for routing
                if (!IsNeighborReachable(device, "eth0", neighborDevice))
                {
                    device.AddLogEntry($"IsisProtocol: Physical connection to {neighborDevice.Name} on eth0 not suitable for routing");
                    return;
                }
                
                // Check if neighbor runs IS-IS
                var neighborIsis = neighborDevice.GetIsisConfiguration();
                if (neighborIsis != null && neighborIsis.IsEnabled)
                {
                    var neighborSystemId = neighborIsis.SystemId;
                    
                    // Get or create IS-IS adjacency
                    var adjacency = _state.GetOrCreateAdjacency(neighborSystemId, "eth0");
                    
                    // Update adjacency last seen time
                    _adjacencyLastSeen[neighborSystemId] = DateTime.Now;
                    
                    // Progress adjacency state
                    await ProgressAdjacencyState(adjacency, device);
                    
                    device.AddLogEntry($"IsisProtocol: Adjacency with {neighborSystemId} on interface eth0 in state {adjacency.State}");
                }
            }
        }
        
        private async Task ProgressAdjacencyState(IsisAdjacency adjacency, NetworkDevice device)
        {
            // Simplified IS-IS adjacency state machine
            switch (adjacency.State)
            {
                case IsisAdjacencyState.Down:
                    adjacency.ChangeState(IsisAdjacencyState.Initializing);
                    _state.MarkTopologyChanged();
                    break;
                    
                case IsisAdjacencyState.Initializing:
                    // In real IS-IS, this would involve Hello packet exchange
                    adjacency.ChangeState(IsisAdjacencyState.Up);
                    _state.MarkTopologyChanged();
                    device.AddLogEntry($"IsisProtocol: Adjacency with {adjacency.SystemId} established");
                    break;
                    
                case IsisAdjacencyState.Up:
                    // Adjacency is up - maintain state
                    break;
            }
        }
        
        private async Task CleanupStaleAdjacencies(NetworkDevice device)
        {
            var staleAdjacencies = _state.GetStaleAdjacencies();
            foreach (var adjacencySystemId in staleAdjacencies)
            {
                device.AddLogEntry($"IsisProtocol: Adjacency with {adjacencySystemId} timed out, removing");
                _state.RemoveAdjacency(adjacencySystemId);
                _adjacencyLastSeen.Remove(adjacencySystemId);
            }
        }
        
        private async Task ProcessLspAging(NetworkDevice device)
        {
            var agedLsps = _state.GetAgedLsps();
            foreach (var lspId in agedLsps)
            {
                device.AddLogEntry($"IsisProtocol: LSP {lspId} aged out, removing from database");
                _state.LspDatabase.Remove(lspId);
                _state.MarkTopologyChanged();
            }
        }
        
        private async Task RunSpfCalculation(NetworkDevice device)
        {
            device.AddLogEntry("IsisProtocol: Running SPF calculation due to topology change...");
            
            // Clear existing IS-IS routes before recalculation
            await ClearIsisRoutes(device);
            
            // Build LSP database from adjacencies
            await BuildLspDatabase(device);
            
            // Run SPF algorithm (simplified Dijkstra)
            var routes = CalculateShortestPaths(device);
            
            // Install routes in device routing table
            await InstallIsisRoutes(device, routes);
            
            device.AddLogEntry($"IsisProtocol: SPF calculation completed, installed {routes.Count} routes");
        }
        
        private async Task BuildLspDatabase(NetworkDevice device)
        {
            device.AddLogEntry("IsisProtocol: Building LSP database...");

            // Generate our own LSP
            var ourLspId = $"{_isisConfig.SystemId}.00-00";
            if (!_state.LspDatabase.ContainsKey(ourLspId))
            {
                var ourLsp = new IsisLsp(ourLspId, _isisConfig.SystemId, _state.SequenceNumber++);
                _state.LspDatabase[ourLspId] = ourLsp;
                
                device.AddLogEntry($"IsisProtocol: Generated LSP {ourLspId}");
            }
            
            // In a real implementation, we would receive and process LSPs from neighbors
            // For simulation, we'll create simplified LSPs for connected neighbors
            foreach (var adjacency in _state.Adjacencies.Values.Where(a => a.State == IsisAdjacencyState.Up))
            {
                var neighborLspId = $"{adjacency.SystemId}.00-00";
                if (!_state.LspDatabase.ContainsKey(neighborLspId))
                {
                    var neighborLsp = new IsisLsp(neighborLspId, adjacency.SystemId, 1);
                    _state.LspDatabase[neighborLspId] = neighborLsp;
                    
                    device.AddLogEntry($"IsisProtocol: Added neighbor LSP {neighborLspId} to database");
                }
            }
        }
        
        private List<IsisRoute> CalculateShortestPaths(NetworkDevice device)
        {
            var routes = new List<IsisRoute>();
            
            // Simplified SPF calculation - add routes to directly connected networks
            // In a real implementation, this would be full Dijkstra's algorithm
            
            // Add connected networks
            var connectedDevice = device.GetConnectedDevice("eth0");
            if (connectedDevice.HasValue)
            {
                var neighborDevice = connectedDevice.Value.device;
                var routeKey = $"{neighborDevice.Name}_network";
                
                var route = new IsisRoute(neighborDevice.Name, "255.255.255.0", neighborDevice.Name, "eth0");
                route.Metric = 10; // Default IS-IS metric
                route.Type = IsisRouteType.Internal;
                
                routes.Add(route);
                
                // Store in IS-IS state
                _state.RoutingTable[routeKey] = route;
            }
            
            return routes;
        }
        
        private async Task InstallIsisRoutes(NetworkDevice device, List<IsisRoute> routes)
        {
            device.AddLogEntry("IsisProtocol: Installing IS-IS routes...");
            
            var installedRoutes = 0;
            foreach (var route in routes)
            {
                var deviceRoute = new Route(route.Network, route.SubnetMask, route.NextHop, route.Interface, "ISIS");
                deviceRoute.Metric = route.Metric;
                deviceRoute.AdminDistance = 115; // IS-IS administrative distance
                device.AddRoute(deviceRoute);
                installedRoutes++;
                
                device.AddLogEntry($"IsisProtocol: Installed route to {route.Network} via {route.NextHop} (metric: {route.Metric}, AD: 115)");
            }
            
            device.AddLogEntry($"IsisProtocol: Successfully installed {installedRoutes} IS-IS routes");
        }
        
        private async Task ClearIsisRoutes(NetworkDevice device)
        {
            device.ClearRoutesByProtocol("ISIS");
            _state.RoutingTable.Clear();
            device.AddLogEntry("IsisProtocol: Cleared all IS-IS routes");
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
