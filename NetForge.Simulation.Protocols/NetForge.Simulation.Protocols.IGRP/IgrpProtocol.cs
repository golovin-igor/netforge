using System.Net.Sockets;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.Protocols.Common.Events;


namespace NetForge.Simulation.Protocols.IGRP;

public class IgrpProtocol : BaseProtocol
{
    public override NetworkProtocolType Type => NetworkProtocolType.IGRP;
    public override string Name => "Interior Gateway Routing Protocol";

    private DateTime _lastUpdate = DateTime.MinValue;
    private DateTime _lastPeriodicUpdate = DateTime.MinValue;

    protected override BaseProtocolState CreateInitialState()
    {
        return new IgrpState();
    }

    protected override void OnInitialized()
    {
        var igrpConfig = GetIgrpConfig();
        if (igrpConfig.IsEnabled)
        {
            LogProtocolEvent("IGRP protocol initialized and enabled");
            _state.IsActive = true;
            _state.MarkStateChanged();
            _lastPeriodicUpdate = DateTime.Now;
        }
        else
        {
            LogProtocolEvent("IGRP protocol initialized but disabled");
            _state.IsActive = false;
        }
    }

    protected override async Task UpdateNeighbors(INetworkDevice device)
    {
        var igrpState = (IgrpState)_state;
        var igrpConfig = GetIgrpConfig();

        if (!igrpConfig.IsEnabled)
        {
            igrpState.IsActive = false;
            return;
        }

        // Check for periodic update time
        var now = DateTime.Now;
        var updateInterval = TimeSpan.FromSeconds(igrpConfig.UpdateTimer);

        if (now - _lastPeriodicUpdate >= updateInterval)
        {
            await SendPeriodicUpdates(device, igrpConfig, igrpState);
            _lastPeriodicUpdate = now;
        }

        // Discover IGRP neighbors on configured networks
        await DiscoverIgrpNeighbors(device, igrpConfig, igrpState);

        // Process route timers
        await ProcessRouteTimers(device, igrpConfig, igrpState);
    }

    protected override async Task RunProtocolCalculation(INetworkDevice device)
    {
        var igrpState = (IgrpState)_state;

        LogProtocolEvent("IGRP: Running route calculation due to topology change...");

        // Clear existing IGRP routes
        device.ClearRoutesByProtocol("IGRP");
        igrpState.CalculatedRoutes.Clear();

        // Calculate best routes using IGRP distance vector algorithm
        await CalculateIgrpRoutes(device, igrpState);

        // Install calculated routes
        await InstallRoutes(device, igrpState);

        igrpState.TopologyChanged = false;
        LogProtocolEvent("IGRP: Route calculation completed");
    }

    private async Task DiscoverIgrpNeighbors(INetworkDevice device, IgrpConfig config, IgrpState state)
    {
        foreach (var networkConfig in config.Networks)
        {
            var interfaces = GetInterfacesForNetwork(device, networkConfig);

            foreach (var interfaceName in interfaces)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;

                    // Check if neighbor has IGRP protocol active
                    var neighborIgrpProtocol = GetNeighborIgrpProtocol(neighborDevice);
                    if (neighborIgrpProtocol != null)
                    {
                        var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                        var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new IgrpNeighbor
                        {
                            RouterId = neighborDevice.Name,
                            InterfaceName = neighborInterface,
                            IpAddress = neighborDevice.GetInterface(neighborInterface)?.IpAddress ?? "",
                            State = IgrpNeighborState.Up,
                            AutonomousSystem = GetAutonomousSystem(neighborIgrpProtocol.GetConfiguration()),
                            HoldTime = GetHoldTimer(neighborIgrpProtocol.GetConfiguration())
                        });

                        neighbor.LastSeen = DateTime.Now;
                        state.UpdateNeighborActivity(neighborKey);

                        LogProtocolEvent($"IGRP: Neighbor {neighbor.RouterId} is reachable on AS {config.AutonomousSystem}");
                    }
                }
            }
        }
    }

    private async Task SendPeriodicUpdates(INetworkDevice device, IgrpConfig config, IgrpState state)
    {
        LogProtocolEvent($"IGRP: Sending periodic updates for AS {config.AutonomousSystem}");

        // In a real implementation, this would send IGRP update packets
        // For simulation, we mark topology as changed to trigger recalculation
        foreach (var neighbor in state.Neighbors.Values.Where(n => n.IsActive))
        {
            // Simulate sending routing table to neighbor
            LogProtocolEvent($"IGRP: Sent update to {neighbor.RouterId} with {state.Routes.Count} routes");
        }
    }

    private async Task ProcessRouteTimers(INetworkDevice device, IgrpConfig config, IgrpState state)
    {
        var now = DateTime.Now;
        var routesChanged = false;

        // Check for invalid routes
        var invalidRoutes = state.GetInvalidRoutes(config.InvalidTimer);
        foreach (var routeKey in invalidRoutes)
        {
            if (!state.InvalidTimers.ContainsKey(routeKey))
            {
                LogProtocolEvent($"IGRP: Route {routeKey} became invalid due to timeout");
                state.MarkRouteInvalid(routeKey);
                routesChanged = true;
            }
        }

        // Check for routes to flush
        var flushRoutes = state.GetFlushRoutes(config.FlushTimer - config.InvalidTimer);
        foreach (var routeKey in flushRoutes)
        {
            LogProtocolEvent($"IGRP: Flushing route {routeKey} due to timeout");
            state.RemoveRoute(routeKey);
            routesChanged = true;
        }

        if (routesChanged)
        {
            state.MarkStateChanged();
        }
    }

    private async Task CalculateIgrpRoutes(INetworkDevice device, IgrpState state)
    {
        var routes = new Dictionary<string, IgrpRoute>();

        // Add directly connected networks
        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                continue;

            var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
            var routeKey = $"{network}/{interfaceConfig.SubnetMask}";

            if (!routes.ContainsKey(routeKey))
            {
                routes[routeKey] = new IgrpRoute
                {
                    Network = network,
                    Mask = interfaceConfig.SubnetMask,
                    NextHop = "0.0.0.0", // Directly connected
                    Interface = interfaceName,
                    HopCount = 0,
                    Bandwidth = 1544, // Default T1 bandwidth
                    Delay = 20000, // Default delay
                    Source = "Connected"
                };
                routes[routeKey].Metric = routes[routeKey].CalculateMetric();
            }
        }

        // Add routes learned from neighbors
        foreach (var existingRoute in state.Routes.Values.Where(r => r.State == IgrpRouteState.Valid))
        {
            var routeKey = $"{existingRoute.Network}/{existingRoute.Mask}";

            if (!routes.ContainsKey(routeKey) || routes[routeKey].Metric > existingRoute.Metric)
            {
                routes[routeKey] = new IgrpRoute
                {
                    Network = existingRoute.Network,
                    Mask = existingRoute.Mask,
                    NextHop = existingRoute.NextHop,
                    Interface = existingRoute.Interface,
                    Metric = existingRoute.Metric,
                    Bandwidth = existingRoute.Bandwidth,
                    Delay = existingRoute.Delay,
                    HopCount = existingRoute.HopCount,
                    Source = "IGRP"
                };
            }
        }

        state.CalculatedRoutes = routes.Values.ToList();
        LogProtocolEvent($"IGRP: Calculated {state.CalculatedRoutes.Count} routes");
    }

    private async Task InstallRoutes(INetworkDevice device, IgrpState state)
    {
        foreach (var route in state.CalculatedRoutes.Where(r => r.Source == "IGRP"))
        {
            var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "IGRP")
            {
                Metric = route.Metric,
                AdminDistance = 100 // IGRP administrative distance
            };

            device.AddRoute(deviceRoute);
            LogProtocolEvent($"IGRP: Installed route to {route.Network}/{route.Mask} via {route.NextHop} metric {route.Metric}");
        }
    }

    protected override bool IsNeighborReachable(INetworkDevice device, string interfaceName, INetworkDevice neighbor)
    {
        var connection = device.GetPhysicalConnectionMetrics(interfaceName);
        return connection?.IsSuitableForRouting ?? false;
    }

    private IgrpConfig GetIgrpConfig()
    {
        // Use only the local configuration - no legacy routing config conversion
        return new IgrpConfig
        {
            IsEnabled = false, // Will be enabled via ApplyConfiguration
            AutonomousSystem = 1,
            Networks = new List<string>(),
            UpdateTimer = 90,
            InvalidTimer = 270,
            FlushTimer = 630,
            HoldTimer = 280
        };
    }

    protected override object GetProtocolConfiguration()
    {
        return GetIgrpConfig();
    }

    protected override void OnApplyConfiguration(object configuration)
    {
        if (configuration is IgrpConfig igrpConfig && igrpConfig.Validate())
        {
            // Store configuration in protocol state - no legacy integration needed
            _state.IsActive = igrpConfig.IsEnabled;
            _state.MarkStateChanged();

            if (_state is IgrpState igrpState)
            {
                igrpState.AutonomousSystem = igrpConfig.AutonomousSystem;
                igrpState.TopologyChanged = true;
            }
        }
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco" }; // IGRP is Cisco proprietary
    }

    private string GetNetworkAddress(string ipAddress, string subnetMask)
    {
        try
        {
            var ip = System.Net.IPAddress.Parse(ipAddress);
            var mask = System.Net.IPAddress.Parse(subnetMask);

            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var networkBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            return new System.Net.IPAddress(networkBytes).ToString();
        }
        catch
        {
            return ipAddress; // Fallback to original IP if parsing fails
        }
    }

    private IEnumerable<string> GetInterfacesForNetwork(INetworkDevice device, string networkConfig)
    {
        var interfaces = new List<string>();

        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                continue;

            // Simple check if interface IP is in the configured network
            // In real implementation, this would do proper subnet matching
            var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
            if (network == networkConfig || interfaceConfig.IpAddress.StartsWith(networkConfig.Split('/')[0]))
            {
                interfaces.Add(interfaceName);
            }
        }

        return interfaces;
    }

    private int GetAutonomousSystem(object config)
    {
        if (config is IgrpConfig localConfig)
            return localConfig.AutonomousSystem;
        return 1; // Default
    }

    private int GetHoldTimer(object config)
    {
        if (config is IgrpConfig localConfig)
            return localConfig.HoldTimer;
        return 280; // Default
    }

    private IgrpProtocol GetNeighborIgrpProtocol(INetworkDevice neighborDevice)
    {
        // In the new architecture, protocols are discovered through the device's protocol list
        // For now, return null - neighbor discovery will be handled differently
        return null;
    }

    protected override void OnSubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
    {
        // Subscribe to interface up/down events
        eventBus.Subscribe<InterfaceStateChangedEventArgs>(args =>
        {
            if (args.DeviceName == self.Name)
            {
                LogProtocolEvent($"IGRP: Interface {args.InterfaceName} state changed to {(args.IsUp ? "UP" : "DOWN")}");
                _state.MarkStateChanged();
            }
            return Task.CompletedTask;
        });

        // Subscribe to protocol configuration changes
        eventBus.Subscribe<ProtocolConfigChangedEventArgs>(args =>
        {
            if (args.DeviceName == self.Name)
            {
                LogProtocolEvent("IGRP: Protocol configuration changed, marking for update");
                _state.MarkStateChanged();
            }
            return Task.CompletedTask;
        });
    }

}
