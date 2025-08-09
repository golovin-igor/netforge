using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// IgrpConfig

namespace NetSim.Simulation.Protocols.Implementations
{
    public class IgrpProtocol : INetworkProtocol
    {
        private IgrpConfig _igrpConfig;
        private NetworkDevice _device;
        private readonly IgrpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _neighborLastSeen = new(); // Track neighbor timers

        public ProtocolType Type => ProtocolType.IGRP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _igrpConfig = device.GetIgrpConfiguration();
            if (_igrpConfig == null)
            {
                device.AddLogEntry("IgrpProtocol: IGRP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry($"IgrpProtocol: Successfully initialized with AS {_igrpConfig.AutonomousSystemNumber}.");
                
                // Mark routes as changed to trigger initial route calculation
                _state.MarkRoutesChanged();
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
                _device.AddLogEntry($"IgrpProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating IGRP state.");
                
                // Mark routes as changed when interface states change
                _state.MarkRoutesChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"IgrpProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating IGRP configuration and state.");
                _igrpConfig = _device.GetIgrpConfiguration();
                _state.MarkRoutesChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_igrpConfig == null) _igrpConfig = device.GetIgrpConfiguration();

            if (_igrpConfig == null || !_igrpConfig.IsEnabled)
            {
                device.AddLogEntry($"IgrpProtocol on {device.Name}: IGRP configuration missing or not enabled. Clearing IGRP routes.");
                await ClearIgrpRoutes(device);
                return;
            }

            device.AddLogEntry($"IgrpProtocol: Updating IGRP state for AS {_igrpConfig.AutonomousSystemNumber} on device {device.Name}...");
            
            // Update neighbor adjacencies
            await UpdateNeighborAdjacencies(device);
            
            // Clean up stale neighbors
            await CleanupStaleNeighbors(device);
            
            // Process route aging
            await ProcessRouteAging(device);
            
            // Only recalculate and install routes if something changed
            if (_state.RoutesChanged)
            {
                await RecalculateAndInstallRoutes(device);
                _state.RoutesChanged = false;
                _state.LastRouteCalculation = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("IgrpProtocol: No route changes detected, skipping route recalculation.");
            }
            
            device.AddLogEntry("IgrpProtocol: IGRP state update completed.");
        }
        
        private async Task UpdateNeighborAdjacencies(NetworkDevice device)
        {
            device.AddLogEntry("IgrpProtocol: Updating IGRP neighbor adjacencies...");

            // Find potential IGRP neighbors through physical connections
            var connectedDevice = device.GetConnectedDevice("eth0"); // Simplified - check primary interface
            if (connectedDevice.HasValue)
            {
                var neighborDevice = connectedDevice.Value.device;
                var neighborInterface = connectedDevice.Value.interfaceName;
                
                // Check if physical connection is suitable for routing
                if (!IsNeighborReachable(device, "eth0", neighborDevice))
                {
                    device.AddLogEntry($"IgrpProtocol: Physical connection to {neighborDevice.Name} on eth0 not suitable for routing");
                    return;
                }
                
                // Check if neighbor runs IGRP in the same AS
                var neighborIgrp = neighborDevice.GetIgrpConfiguration();
                if (neighborIgrp != null && neighborIgrp.IsEnabled && neighborIgrp.AutonomousSystemNumber == _igrpConfig.AutonomousSystemNumber)
                {
                    var neighborId = neighborDevice.Name;
                    
                    // Get or create IGRP neighbor adjacency
                    var adjacency = _state.GetOrCreateAdjacency(neighborId, "eth0");
                    
                    // Update neighbor last seen time
                    _neighborLastSeen[neighborId] = DateTime.Now;
                    
                    // Progress adjacency state
                    await ProgressAdjacencyState(adjacency, device);
                    
                    device.AddLogEntry($"IgrpProtocol: Adjacency with {neighborId} on interface eth0 in state {adjacency.State}");
                }
            }
        }
        
        private async Task ProgressAdjacencyState(IgrpNeighborAdjacency adjacency, NetworkDevice device)
        {
            // Simplified IGRP adjacency state machine
            switch (adjacency.State)
            {
                case IgrpNeighborState.Down:
                    adjacency.ChangeState(IgrpNeighborState.Initializing);
                    _state.MarkRoutesChanged();
                    break;
                    
                case IgrpNeighborState.Initializing:
                    // In real IGRP, this would involve Hello packet exchange
                    adjacency.ChangeState(IgrpNeighborState.Up);
                    _state.MarkRoutesChanged();
                    device.AddLogEntry($"IgrpProtocol: Adjacency with {adjacency.NeighborId} established");
                    break;
                    
                case IgrpNeighborState.Up:
                    // Adjacency is up - maintain state
                    break;
            }
        }
        
        private async Task CleanupStaleNeighbors(NetworkDevice device)
        {
            var staleNeighbors = _state.GetStaleNeighbors();
            foreach (var neighborId in staleNeighbors)
            {
                device.AddLogEntry($"IgrpProtocol: Neighbor {neighborId} timed out, removing");
                _state.RemoveAdjacency(neighborId);
                _neighborLastSeen.Remove(neighborId);
            }
        }
        
        private async Task ProcessRouteAging(NetworkDevice device)
        {
            var currentTime = DateTime.Now;
            var routesToRemove = new List<string>();
            
            foreach (var routeEntry in _state.Routes)
            {
                var timeSinceLastUpdate = currentTime - routeEntry.Value.LastUpdateTime;
                
                if (timeSinceLastUpdate > TimeSpan.FromSeconds(630)) // IGRP flush timer
                {
                    device.AddLogEntry($"IgrpProtocol: Route to {routeEntry.Key} flushed due to timeout");
                    routesToRemove.Add(routeEntry.Key);
                    _state.MarkRoutesChanged();
                }
                else if (timeSinceLastUpdate > TimeSpan.FromSeconds(270)) // IGRP invalid timer
                {
                    if (routeEntry.Value.State != IgrpRouteState.Invalid)
                    {
                        device.AddLogEntry($"IgrpProtocol: Route to {routeEntry.Key} marked invalid due to timeout");
                        routeEntry.Value.State = IgrpRouteState.Invalid;
                        _state.MarkRoutesChanged();
                    }
                }
            }
            
            // Remove flushed routes
            foreach (var routeKey in routesToRemove)
            {
                _state.Routes.Remove(routeKey);
            }
        }
        
        private async Task RecalculateAndInstallRoutes(NetworkDevice device)
        {
            device.AddLogEntry("IgrpProtocol: Recalculating and installing IGRP routes...");
            
            // Clear existing IGRP routes before recalculation
            await ClearIgrpRoutes(device);
            
            // Build routing table from adjacencies
            await BuildRoutingTable(device);
            
            // Install valid routes in device routing table
            await InstallIgrpRoutes(device);
            
            device.AddLogEntry($"IgrpProtocol: Route recalculation completed, installed {_state.Routes.Count(r => r.Value.State == IgrpRouteState.Valid)} routes");
        }
        
        private async Task BuildRoutingTable(NetworkDevice device)
        {
            device.AddLogEntry("IgrpProtocol: Building IGRP routing table...");

            // In a real implementation, we would receive route updates from neighbors
            // For simulation, we'll create simplified routes for connected neighbors
            foreach (var adjacency in _state.Adjacencies.Values.Where(a => a.State == IgrpNeighborState.Up))
            {
                var routeKey = $"{adjacency.NeighborId}_network";
                
                // Create or update route entry
                if (!_state.Routes.ContainsKey(routeKey))
                {
                    var route = new IgrpRoute(adjacency.NeighborId, "255.255.255.0", adjacency.NeighborId, adjacency.InterfaceName, (int)CalculateIgrpMetric(1000, 100, 255, 1, 1500));
                    route.State = IgrpRouteState.Valid;
                    route.LastUpdateTime = DateTime.Now;
                    
                    _state.Routes[routeKey] = route.ToRouteEntry();
                    device.AddLogEntry($"IgrpProtocol: Added route to {routeKey} with metric {route.Metric}");
                }
                else
                {
                    // Update existing route
                    _state.Routes[routeKey].LastUpdateTime = DateTime.Now;
                    _state.Routes[routeKey].State = IgrpRouteState.Valid;
                }
            }
        }
        
        private uint CalculateIgrpMetric(uint bandwidth, uint delay, byte reliability, byte load, uint mtu)
        {
            // IGRP metric calculation: metric = k1*bandwidth + k2*bandwidth/(256-load) + k3*delay + k4*delay*reliability/255
            // Using default k values: k1=1, k2=0, k3=1, k4=0, k5=0
            var k1 = 1;
            var k3 = 1;
            
            var metric = (k1 * (10000000 / bandwidth)) + (k3 * delay);
            return (uint)metric;
        }
        
        private async Task InstallIgrpRoutes(NetworkDevice device)
        {
            device.AddLogEntry("IgrpProtocol: Installing IGRP routes...");
            
            var installedRoutes = 0;
            foreach (var routeEntry in _state.Routes.Values)
            {
                if (routeEntry.State == IgrpRouteState.Valid)
                {
                    var deviceRoute = new Route(routeEntry.Network, routeEntry.SubnetMask, routeEntry.NextHop, routeEntry.Interface, "IGRP");
                    deviceRoute.Metric = routeEntry.Metric;
                    deviceRoute.AdminDistance = 100; // IGRP administrative distance
                    device.AddRoute(deviceRoute);
                    installedRoutes++;
                    
                    device.AddLogEntry($"IgrpProtocol: Installed route to {routeEntry.Network} via {routeEntry.NextHop} (metric: {routeEntry.Metric}, AD: 100)");
                }
            }
            
            device.AddLogEntry($"IgrpProtocol: Successfully installed {installedRoutes} IGRP routes");
        }
        
        private async Task ClearIgrpRoutes(NetworkDevice device)
        {
            device.ClearRoutesByProtocol("IGRP");
            device.AddLogEntry("IgrpProtocol: Cleared all IGRP routes");
        }
        
        private bool IsNeighborReachable(NetworkDevice device, string interfaceName, NetworkDevice neighbor)
        {
            var connection = device.GetPhysicalConnectionMetrics(interfaceName);
            return connection?.IsSuitableForRouting ?? false;
        }
    }
} 
