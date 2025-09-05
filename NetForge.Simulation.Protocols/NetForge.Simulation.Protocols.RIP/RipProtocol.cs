using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.RIP;

public class RipProtocol : BaseProtocol, IDeviceProtocol
{
    public override NetworkProtocolType Type => NetworkProtocolType.RIP;
    public override string Name => "Routing Information Protocol";

    private DateTime _lastUpdate = DateTime.MinValue;
    private DateTime _lastPeriodicUpdate = DateTime.MinValue;
    private const int UPDATE_INTERVAL = 30; // RIP updates every 30 seconds
    private const int INVALID_TIMER = 180; // Route invalid after 180 seconds
    private const int FLUSH_TIMER = 240; // Route flushed after 240 seconds

    protected override BaseProtocolState CreateInitialState()
    {
        return new RipState();
    }

    protected override void OnInitialized()
    {
        var ripConfig = GetRipConfig();
        if (ripConfig.IsEnabled)
        {
            LogProtocolEvent("RIP protocol initialized and enabled");
            _state.IsActive = true;
            _state.MarkStateChanged();
            _lastPeriodicUpdate = DateTime.Now;
        }
        else
        {
            LogProtocolEvent("RIP protocol initialized but disabled");
            _state.IsActive = false;
        }
    }

    protected override async Task UpdateNeighbors(INetworkDevice device)
    {
        var ripState = (RipState)_state;
        var ripConfig = GetRipConfig();

        if (!ripConfig.IsEnabled)
        {
            ripState.IsActive = false;
            return;
        }

        // Process any received RIP updates
        await ProcessReceivedUpdates(device, ripState);

        // Send periodic updates if it's time
        if (ShouldSendPeriodicUpdate())
        {
            await SendPeriodicUpdates(device, ripConfig, ripState);
            _lastPeriodicUpdate = DateTime.Now;
        }
    }

    protected override async Task RunProtocolCalculation(INetworkDevice device)
    {
        var ripState = (RipState)_state;
        var ripConfig = GetRipConfig();

        LogProtocolEvent("Running RIP route calculation due to state change");

        // Age out invalid routes and remove flushed routes
        await AgeRoutes(device, ripState);

        // Clear existing RIP routes from device routing table
        device.ClearRoutesByProtocol("RIP");

        // Install valid RIP routes
        await InstallRipRoutes(device, ripState);

        LogProtocolEvent($"RIP calculation completed - {ripState.Routes.Count} routes in table");
    }

    protected override async Task ProcessTimers(INetworkDevice device)
    {
        var ripState = (RipState)_state;

        // Check if any route timers have expired
        var now = DateTime.Now;
        bool stateChanged = false;

        foreach (var route in ripState.Routes.Values.ToList())
        {
            if (route.State == RipRouteState.Valid)
            {
                if ((now - route.LastUpdated).TotalSeconds > INVALID_TIMER)
                {
                    LogProtocolEvent($"Route to {route.Network} marked as invalid (timeout)");
                    route.State = RipRouteState.Invalid;
                    route.Metric = 16; // Infinity in RIP
                    route.InvalidTime = now;
                    stateChanged = true;
                }
            }
            else if (route.State == RipRouteState.Invalid)
            {
                if ((now - route.InvalidTime).TotalSeconds > (FLUSH_TIMER - INVALID_TIMER))
                {
                    LogProtocolEvent($"Route to {route.Network} flushed from table");
                    ripState.Routes.Remove(route.Network);
                    stateChanged = true;
                }
            }
        }

        if (stateChanged)
        {
            ripState.MarkStateChanged();
        }

        await Task.CompletedTask;
    }

    private bool ShouldSendPeriodicUpdate()
    {
        return (DateTime.Now - _lastPeriodicUpdate).TotalSeconds >= UPDATE_INTERVAL;
    }

    private async Task ProcessReceivedUpdates(INetworkDevice device, RipState ripState)
    {
        // In a real implementation, this would process received RIP packets
        // For simulation purposes, we discover routes from connected neighbors

        var interfaces = device.GetAllInterfaces();
        foreach (var (interfaceName, interfaceConfig) in interfaces)
        {
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                continue;

            var connectedDevice = device.GetConnectedDevice(interfaceName);
            if (connectedDevice.HasValue)
            {
                var neighbor = connectedDevice.Value.device;
                var neighborInterface = connectedDevice.Value.interfaceName;

                if (!IsNeighborReachable(device, interfaceName, neighbor))
                    continue;

                // Get neighbor's RIP configuration
                var neighborRipConfig = neighbor.GetRipConfiguration();
                if (neighborRipConfig?.IsEnabled == true)
                {
                    await ProcessNeighborRoutes(device, neighbor, interfaceName, ripState);
                }
            }
        }
    }

    private async Task ProcessNeighborRoutes(INetworkDevice device, INetworkDevice neighbor, string localInterface, RipState ripState)
    {
        // Get neighbor's routing table and process as RIP advertisements
        var neighborRoutes = neighbor.GetRoutingTable();
        var localInterfaceConfig = device.GetInterface(localInterface);

        if (localInterfaceConfig?.IpAddress == null)
            return;

        foreach (var neighborRoute in neighborRoutes)
        {
            // Don't learn routes back to ourselves or invalid routes
            if (neighborRoute.Interface == localInterface || neighborRoute.Metric >= 16)
                continue;

            var network = neighborRoute.Network;
            var advertisedMetric = Math.Min(neighborRoute.Metric + 1, 16);
            var nextHop = GetNextHopForNeighbor(device, neighbor, localInterface);

            if (nextHop == null) continue;

            var routeKey = $"{network}/{neighborRoute.Mask}";

            if (ripState.Routes.TryGetValue(routeKey, out var existingRoute))
            {
                // Update existing route if this is a better path or from same next hop
                if (advertisedMetric < existingRoute.Metric || existingRoute.NextHop == nextHop)
                {
                    if (advertisedMetric != existingRoute.Metric || existingRoute.NextHop != nextHop)
                    {
                        LogProtocolEvent($"Updated route to {network}: metric {existingRoute.Metric} -> {advertisedMetric}");
                        existingRoute.Metric = advertisedMetric;
                        existingRoute.NextHop = nextHop;
                        existingRoute.Interface = localInterface;
                        existingRoute.LastUpdated = DateTime.Now;
                        existingRoute.State = advertisedMetric < 16 ? RipRouteState.Valid : RipRouteState.Invalid;
                        ripState.MarkStateChanged();
                    }
                    else
                    {
                        // Just refresh the timer
                        existingRoute.LastUpdated = DateTime.Now;
                    }
                }
            }
            else if (advertisedMetric < 16)
            {
                // Add new route
                LogProtocolEvent($"Learned new route to {network} via {nextHop}, metric {advertisedMetric}");
                ripState.Routes[routeKey] = new RipRoute
                {
                    Network = network,
                    Mask = neighborRoute.Mask,
                    NextHop = nextHop,
                    Interface = localInterface,
                    Metric = advertisedMetric,
                    State = RipRouteState.Valid,
                    LastUpdated = DateTime.Now
                };
                ripState.MarkStateChanged();
            }
        }

        await Task.CompletedTask;
    }

    private string? GetNextHopForNeighbor(INetworkDevice device, INetworkDevice neighbor, string localInterface)
    {
        var localInterfaceConfig = device.GetInterface(localInterface);
        var connection = device.GetConnectedDevice(localInterface);

        if (connection.HasValue)
        {
            var neighborInterfaceConfig = neighbor.GetInterface(connection.Value.interfaceName);
            return neighborInterfaceConfig?.IpAddress;
        }

        return null;
    }

    private async Task SendPeriodicUpdates(INetworkDevice device, RipConfig ripConfig, RipState ripState)
    {
        LogProtocolEvent($"Sending periodic RIP updates on {ripConfig.Networks.Count} networks");

        // In a real implementation, this would send UDP packets on port 520
        // For simulation purposes, we just log the update

        foreach (var network in ripConfig.Networks)
        {
            // In the existing config, Networks contains network addresses, not interface names
            // For now, we'll use all enabled interfaces
            var interfaces = device.GetAllInterfaces();
            foreach (var (interfaceName, interfaceConfig) in interfaces)
            {
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                LogProtocolEvent($"Sending RIP update on interface {interfaceName} ({ripState.Routes.Count} routes)");
            }
        }

        await Task.CompletedTask;
    }

    private async Task AgeRoutes(INetworkDevice device, RipState ripState)
    {
        var now = DateTime.Now;
        var routesToRemove = new List<string>();

        foreach (var (routeKey, route) in ripState.Routes)
        {
            switch (route.State)
            {
                case RipRouteState.Valid:
                    if ((now - route.LastUpdated).TotalSeconds > INVALID_TIMER)
                    {
                        LogProtocolEvent($"Route to {route.Network} marked as invalid (aged out)");
                        route.State = RipRouteState.Invalid;
                        route.Metric = 16;
                        route.InvalidTime = now;
                    }
                    break;

                case RipRouteState.Invalid:
                    if ((now - route.InvalidTime).TotalSeconds > (FLUSH_TIMER - INVALID_TIMER))
                    {
                        LogProtocolEvent($"Route to {route.Network} flushed from table");
                        routesToRemove.Add(routeKey);
                    }
                    break;
            }
        }

        foreach (var routeKey in routesToRemove)
        {
            ripState.Routes.Remove(routeKey);
        }

        await Task.CompletedTask;
    }

    private async Task InstallRipRoutes(INetworkDevice device, RipState ripState)
    {
        foreach (var route in ripState.Routes.Values)
        {
            if (route.State == RipRouteState.Valid && route.Metric < 16)
            {
                var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "RIP")
                {
                    Metric = route.Metric,
                    AdminDistance = 120 // RIP administrative distance
                };
                device.AddRoute(deviceRoute);
            }
        }

        await Task.CompletedTask;
    }

    protected override int GetNeighborTimeoutSeconds() => INVALID_TIMER;

    private RipConfig GetRipConfig()
    {
        return _device?.GetRipConfiguration() as RipConfig ?? new RipConfig { IsEnabled = false };
    }

    protected override object GetProtocolConfiguration()
    {
        return GetRipConfig();
    }

    protected override void OnApplyConfiguration(object configuration)
    {
        if (configuration is RipConfig ripConfig)
        {
            _device?.SetRipConfiguration(ripConfig);
            _state.IsActive = ripConfig.IsEnabled;

            if (ripConfig.IsEnabled)
            {
                LogProtocolEvent("RIP configuration updated and enabled");
            }
            else
            {
                LogProtocolEvent("RIP configuration updated and disabled");
            }
        }
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco", "Juniper", "Generic" };
    }

    protected override void OnSubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
    {
        // Subscribe to interface state change events for triggered updates
        // In a complete implementation, would trigger immediate updates when interfaces change
    }
}
