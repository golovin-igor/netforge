using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// For RipConfig
// Added

// Added

namespace NetSim.Simulation.Protocols.Implementations
{
    public class RipProtocol : INetworkProtocol
    {
        private RipConfig _ripConfig;
        private NetworkDevice _device;
        private readonly RipState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _neighborLastSeen = new(); // Track neighbor contact times

        public ProtocolType Type => ProtocolType.RIP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _ripConfig = device.GetRipConfiguration();
            if (_ripConfig == null)
            {
                device.AddLogEntry("RipProtocol: RIP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry("RipProtocol: Successfully initialized with existing RIP configuration.");
                
                // Mark routes as changed to trigger initial update
                _state.MarkRoutesChanged();
            }
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            eventBus.Subscribe<InterfaceStateChangedEventArgs>(HandleInterfaceStateChangeAsync);
            eventBus.Subscribe<ProtocolConfigChangedEventArgs>(HandleProtocolConfigChangeAsync);
            // LinkChanged might also be relevant if RIP reacts to specific link failures beyond interface down
        }

        private async Task HandleInterfaceStateChangeAsync(InterfaceStateChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name)
            {
                _device.AddLogEntry($"RIPProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating RIP state.");
                
                // Mark routes as changed when interface states change
                _state.MarkRoutesChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"RIPProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating RIP configuration and state.");
                _ripConfig = _device.GetRipConfiguration();
                _state.MarkRoutesChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_ripConfig == null) _ripConfig = device.GetRipConfiguration();

            if (_ripConfig == null || !_ripConfig.IsEnabled)
            {
                device.AddLogEntry($"RIPProtocol on {device.Name}: RIP configuration missing or not enabled. Clearing RIP routes.");
                device.ClearRoutesByProtocol("RIP");
                _state.Routes.Clear();
                return;
            }

            device.AddLogEntry($"RipProtocol: Updating RIP state on device {device.Name}...");
            
            // Update neighbor adjacencies (RIP doesn't have formal adjacencies but tracks neighbors)
            await UpdateNeighbors(device);
            
            // Clean up stale neighbors
            await CleanupStaleNeighbors(device);
            
            // Process route timers (age out, flush routes)
            await ProcessRouteTimers(device);
            
            // Only process route updates if routes have changed or it's time to send periodic updates
            if (_state.RoutesChanged || ShouldSendPeriodicUpdates())
            {
                await ProcessRouteUpdates(device);
                _state.RoutesChanged = false;
                _state.LastAdvertisement = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("RIPProtocol: No route changes detected, skipping route updates.");
            }
            
            device.AddLogEntry("RipProtocol: RIP state update completed.");
        }
        
        private async Task UpdateNeighbors(NetworkDevice device)
        {
            device.AddLogEntry("RipProtocol: Updating RIP neighbors...");

            // Find potential RIP neighbors through physical connections
            var connectedDevice = device.GetConnectedDevice("eth0"); // Simplified - check primary interface
            if (connectedDevice.HasValue)
            {
                var neighborDevice = connectedDevice.Value.device;
                var neighborInterface = connectedDevice.Value.interfaceName;
                
                // Check if physical connection is suitable for routing
                if (!IsNeighborReachable(device, "eth0", neighborDevice))
                {
                    device.AddLogEntry($"RipProtocol: Physical connection to {neighborDevice.Name} on eth0 not suitable for routing");
                    return;
                }
                
                // Check if neighbor runs RIP
                var neighborRip = neighborDevice.GetRipConfiguration();
                if (neighborRip != null && neighborRip.IsEnabled)
                {
                    var neighborIp = neighborDevice.Name; // Use device name as identifier
                    
                    // Get or create RIP neighbor
                    var neighbor = _state.GetOrCreateNeighbor(neighborIp, "eth0");
                    neighbor.LastUpdate = DateTime.Now;
                    neighbor.UpdatesReceived++;
                    
                    // Track when we last saw this neighbor
                    _neighborLastSeen[neighborIp] = DateTime.Now;
                    
                    device.AddLogEntry($"RipProtocol: Neighbor {neighborIp} is active");
                }
            }
        }
        
        private async Task CleanupStaleNeighbors(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors();
            foreach (var staleNeighborIp in staleNeighbors)
            {
                device.AddLogEntry($"RipProtocol: Neighbor {staleNeighborIp} timed out, removing");
                _state.RemoveNeighbor(staleNeighborIp);
                _neighborLastSeen.Remove(staleNeighborIp);
            }
        }
        
        private async Task ProcessRouteTimers(NetworkDevice device)
        {
            // Check for timed out routes (180 seconds)
            var timedOutRoutes = _state.GetTimedOutRoutes();
            foreach (var routeKey in timedOutRoutes)
            {
                var route = _state.Routes[routeKey];
                if (route.State == RipRouteState.Valid)
                {
                    route.MarkInvalid();
                    _state.MarkRoutesChanged();
                    device.AddLogEntry($"RipProtocol: Route to {routeKey} timed out, marked invalid");
                }
            }
            
            // Check for routes to flush (240 seconds)
            var flushableRoutes = _state.GetFlushableRoutes();
            foreach (var routeKey in flushableRoutes)
            {
                device.AddLogEntry($"RipProtocol: Flushing route to {routeKey}");
                _state.Routes.Remove(routeKey);
                // Don't call ClearRoute as it may not exist - let normal route cleanup handle it
                _state.MarkRoutesChanged();
            }
        }
        
        private async Task ProcessRouteUpdates(NetworkDevice device)
        {
            device.AddLogEntry("RipProtocol: Processing RIP route updates...");
            
            // Clear existing RIP routes
            device.ClearRoutesByProtocol("RIP");
            
            // Add directly connected networks to RIP state
            await AddConnectedNetworks(device);
            
            // Learn routes from neighbors (simplified - in reality this would involve packet exchange)
            await LearnRoutesFromNeighbors(device);
            
                            // Install valid routes in device routing table
                var validRoutes = _state.Routes.Values.Where(r => r.State == RipRouteState.Valid && r.Metric < 16).ToList();
                await InstallRipRoutes(device, validRoutes);
                
                device.AddLogEntry($"RipProtocol: Route update completed, installed {validRoutes.Count} routes");
        }
        
        private async Task InstallRipRoutes(NetworkDevice device, List<RipRouteEntry> routes)
        {
            device.AddLogEntry("RipProtocol: Installing RIP routes...");
            
            device.ClearRoutesByProtocol("RIP");
            
            foreach (var route in routes)
            {
                var deviceRoute = new Route(route.Network, route.SubnetMask, route.NextHop, route.Interface, "RIP");
                deviceRoute.Metric = route.Metric;
                deviceRoute.AdminDistance = 120; // RIP administrative distance
                device.AddRoute(deviceRoute);
                
                device.AddLogEntry($"RipProtocol: Installed route to {route.Network} via {route.NextHop} (metric: {route.Metric}, AD: 120)");
            }
            
            device.AddLogEntry($"RipProtocol: Successfully installed {routes.Count} RIP routes");
        }
        
        private async Task AddConnectedNetworks(NetworkDevice device)
        {
            // Simplified - add a basic connected network for the device
            var routeKey = $"{device.Name}_network";
            
            // Add connected network to RIP state
            if (!_state.Routes.ContainsKey(routeKey))
            {
                var connectedRoute = new RipRouteEntry(device.Name, "255.255.255.0", "0.0.0.0", "eth0");
                connectedRoute.Metric = 0; // Connected networks have metric 0
                connectedRoute.Source = "connected";
                _state.Routes[routeKey] = connectedRoute;
                
                device.AddLogEntry($"RipProtocol: Added connected network {routeKey}");
            }
        }
        
        private async Task LearnRoutesFromNeighbors(NetworkDevice device)
        {
            // In a real implementation, this would process received RIP updates
            // For simulation, we'll discover routes through connected neighbors
            
            foreach (var neighbor in _state.Neighbors.Values)
            {
                if (!neighbor.IsActive)
                    continue;
                    
                // Simplified route learning from neighbor
                var routeKey = $"{neighbor.IpAddress}_network";
                
                if (!_state.Routes.ContainsKey(routeKey))
                {
                    var learnedRoute = new RipRouteEntry(neighbor.IpAddress, "255.255.255.0", neighbor.IpAddress, neighbor.InterfaceName);
                    learnedRoute.Metric = 1; // One hop away
                    learnedRoute.Source = "rip";
                    _state.Routes[routeKey] = learnedRoute;
                    
                    device.AddLogEntry($"RipProtocol: Learned route to {routeKey} from neighbor {neighbor.IpAddress}");
                }
            }
        }
        
        private bool ShouldSendPeriodicUpdates()
        {
            var timeSinceLastUpdate = DateTime.Now - _state.LastAdvertisement;
            return timeSinceLastUpdate.TotalSeconds >= 30; // RIP updates every 30 seconds
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
