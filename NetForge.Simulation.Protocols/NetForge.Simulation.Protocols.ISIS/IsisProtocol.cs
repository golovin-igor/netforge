using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.Protocols.Common.Events;

// TODO: Replace with local Route class in next migration phase

namespace NetForge.Simulation.Protocols.ISIS;

public class IsisProtocol : BaseProtocol
{
    public override NetworkProtocolType Type => NetworkProtocolType.ISIS;
    public override string Name => "Intermediate System to Intermediate System";

    private DateTime _lastHelloSent = DateTime.MinValue;
    private DateTime _lastLspRefresh = DateTime.MinValue;
    private DateTime _lastSpfCalculation = DateTime.MinValue;

    private SpfAlgorithm? _spfAlgorithm;
    private LinkStateDatabase? _lsdb;

    protected override BaseProtocolState CreateInitialState()
    {
        return new IsisState();
    }

    protected override void OnInitialized()
    {
        var isisConfig = GetIsisConfig();
        if (isisConfig.IsEnabled && isisConfig.Validate())
        {
            var isisState = (IsisState)_state;
            isisState.SystemId = isisConfig.SystemId;
            isisState.AreaId = isisConfig.AreaId;
            isisState.Level = isisConfig.Level;

            // Initialize SPF algorithm and LSDB
            _spfAlgorithm = new SpfAlgorithm(_device, isisState);
            _lsdb = new LinkStateDatabase(_device, isisState);

            // Subscribe to LSDB events
            _lsdb.LspAdded += OnLspChanged;
            _lsdb.LspUpdated += OnLspChanged;
            _lsdb.LspRemoved += OnLspChanged;

            LogProtocolEvent("IS-IS protocol initialized and enabled");
            _state.IsActive = true;
            _state.MarkStateChanged();
            _lastLspRefresh = DateTime.Now;
        }
        else
        {
            LogProtocolEvent("IS-IS protocol initialized but disabled or invalid configuration");
            _state.IsActive = false;
        }
    }

    protected override async Task UpdateNeighbors(INetworkDevice device)
    {
        var isisState = (IsisState)_state;
        var isisConfig = GetIsisConfig();

        if (!isisConfig.IsEnabled || !isisConfig.Validate())
        {
            isisState.IsActive = false;
            return;
        }

        // Send Hello PDUs
        var now = DateTime.Now;
        var helloInterval = TimeSpan.FromSeconds(isisConfig.HelloInterval);

        if (now - _lastHelloSent >= helloInterval)
        {
            await SendHelloPdus(device, isisConfig, isisState);
            _lastHelloSent = now;
        }

        // Discover IS-IS neighbors
        await DiscoverIsisNeighbors(device, isisConfig, isisState);

        // Refresh LSPs periodically
        var refreshInterval = TimeSpan.FromSeconds(isisConfig.LspRefreshInterval);
        if (now - _lastLspRefresh >= refreshInterval)
        {
            await RefreshLsps(device, isisConfig, isisState);
            _lastLspRefresh = now;
        }

        // Clean up expired LSPs
        await CleanupExpiredLsps(device, isisState);
    }

    protected override async Task RunProtocolCalculation(INetworkDevice device)
    {
        var isisState = (IsisState)_state;

        if (_spfAlgorithm == null)
        {
            LogProtocolEvent("IS-IS: SPF algorithm not initialized");
            return;
        }

        LogProtocolEvent("IS-IS: Running SPF calculation due to topology change...");

        // Clear existing IS-IS routes
        device.ClearRoutesByProtocol("ISIS");
        isisState.CalculatedRoutes.Clear();

        // Run enhanced SPF algorithm
        var spfResult = await _spfAlgorithm.RunSpf();

        if (spfResult.IsSuccessful)
        {
            isisState.CalculatedRoutes = spfResult.Routes;

            // Install calculated routes
            await InstallRoutes(device, isisState);

            LogProtocolEvent($"IS-IS: SPF calculation completed in {spfResult.CalculationTime.TotalMilliseconds:F1}ms, {spfResult.Routes.Count} routes calculated");
        }
        else
        {
            LogProtocolEvent($"IS-IS: SPF calculation failed - {spfResult.ErrorMessage}");
        }

        isisState.TopologyChanged = false;
        isisState.LspChanged = false;
        _lastSpfCalculation = DateTime.Now;
    }

    private async Task SendHelloPdus(INetworkDevice device, IsisConfig config, IsisState state)
    {
        foreach (var interfaceName in config.Interfaces.Keys.Where(i => config.Interfaces[i]))
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                continue;

            // In a real implementation, this would construct and send IIH PDUs
            LogProtocolEvent($"IS-IS: Sending Hello on interface {interfaceName}");
        }
    }

    private async Task DiscoverIsisNeighbors(INetworkDevice device, IsisConfig config, IsisState state)
    {
        foreach (var interfaceName in config.Interfaces.Keys.Where(i => config.Interfaces[i]))
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

                // Check if neighbor has ISIS protocol active
                var neighborIsisProtocol = GetNeighborIsisProtocol(neighborDevice);
                if (neighborIsisProtocol != null)
                {
                    // Check if area IDs match for Level-1 adjacency
                    bool canFormAdjacency = false;

                    var neighborConfig = neighborIsisProtocol.GetConfiguration() as IsisConfig;
                    if (neighborConfig != null)
                    {
                        if (config.Level == IsisLevel.Level1 || config.Level == IsisLevel.Level1Level2)
                        {
                            canFormAdjacency = config.AreaId == neighborConfig.AreaId;
                        }
                        else if (config.Level == IsisLevel.Level2)
                        {
                            canFormAdjacency = true; // Level-2 can form adjacency across areas
                        }

                        if (canFormAdjacency)
                        {
                            var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
                            var neighbor = state.GetOrCreateNeighbor(neighborKey, () => new IsisNeighbor
                            {
                                SystemId = neighborConfig.SystemId ?? neighborDevice.Name,
                                InterfaceName = neighborInterface,
                                CircuitId = $"{neighborDevice.Name}.{neighborInterface}",
                                State = IsisNeighborState.Up,
                                Level = neighborConfig.Level,
                                HoldTime = neighborConfig.HoldTime,
                                Priority = 64, // Default priority
                                AreaAddresses = new List<string> { neighborConfig.AreaId }
                            });

                            neighbor.LastSeen = DateTime.Now;
                            state.UpdateNeighborActivity(neighborKey);

                            LogProtocolEvent($"IS-IS: Neighbor {neighbor.SystemId} is reachable (Level-{(int)neighbor.Level})");
                        }
                    }
                }
            }
        }
    }

    private async Task RefreshLsps(INetworkDevice device, IsisConfig config, IsisState state)
    {
        if (_lsdb == null) return;

        // Generate LSP for this system using enhanced LSDB
        var myLsp = _lsdb.GenerateMyLsp(config);
        var result = _lsdb.AddOrUpdateLsp(myLsp, false);

        LogProtocolEvent($"IS-IS: Refreshed LSP {myLsp.LspId} (seq: {myLsp.SequenceNumber}) - {result.Result}");
    }

    private IsisLsp GenerateSystemLsp(INetworkDevice device, IsisConfig config, IsisState state)
    {
        var lsp = new IsisLsp
        {
            LspId = $"{config.SystemId}.00-00",
            SequenceNumber = (uint)(DateTime.Now.Ticks % uint.MaxValue),
            RemainingLifetime = (ushort)config.LspMaxLifetime,
            Level = config.Level,
            OriginatingSystem = config.SystemId,
            IsOverloaded = config.IsOverloaded,
            LastUpdate = DateTime.Now
        };

        // Add TLVs (Type-Length-Value fields)
        var tlvs = new List<IsisTlv>();

        // Area addresses TLV
        tlvs.Add(new IsisTlv
        {
            Type = 1, // Area addresses
            Description = "Area Addresses",
            Value = System.Text.Encoding.ASCII.GetBytes(config.AreaId)
        });

        // IS neighbors TLV (simplified)
        var neighborData = new List<byte>();
        foreach (var neighbor in state.Neighbors.Values.Where(n => n.IsActive))
        {
            neighborData.AddRange(System.Text.Encoding.ASCII.GetBytes(neighbor.SystemId));
        }

        if (neighborData.Any())
        {
            tlvs.Add(new IsisTlv
            {
                Type = 2, // IS neighbors
                Description = "IS Neighbors",
                Value = neighborData.ToArray()
            });
        }

        // IP Internal reachability TLV
        var ipReachData = new List<byte>();
        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                continue;

            var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
            ipReachData.AddRange(System.Text.Encoding.ASCII.GetBytes($"{network}/{interfaceConfig.SubnetMask}"));
        }

        if (ipReachData.Any())
        {
            tlvs.Add(new IsisTlv
            {
                Type = 128, // IP Internal Reachability
                Description = "IP Internal Reachability",
                Value = ipReachData.ToArray()
            });
        }

        lsp.Tlvs = tlvs;
        return lsp;
    }

    private async Task CleanupExpiredLsps(INetworkDevice device, IsisState state)
    {
        var expiredLsps = state.GetExpiredLsps();
        foreach (var lspId in expiredLsps)
        {
            LogProtocolEvent($"IS-IS: LSP {lspId} expired, removing from database");
            state.RemoveLsp(lspId);
        }

        if (expiredLsps.Any())
        {
            state.MarkStateChanged();
        }
    }

    private async Task RunSpfCalculation(INetworkDevice device, IsisState state)
    {
        var routes = new Dictionary<string, IsisRoute>();

        // Add directly connected networks
        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                continue;

            var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask);
            var routeKey = $"{network}/{interfaceConfig.SubnetMask}";

            routes[routeKey] = new IsisRoute
            {
                Destination = network,
                Mask = interfaceConfig.SubnetMask,
                NextHop = "0.0.0.0", // Directly connected
                Interface = interfaceName,
                Metric = state.IsDis ? 0 : 10, // DIS has metric 0 for directly connected
                Level = state.Level,
                RouteType = IsisRouteType.Internal,
                LastUpdate = DateTime.Now
            };
        }

        // Run simplified Dijkstra's algorithm on LSP database
        await RunDijkstraSpf(device, state, routes);

        state.CalculatedRoutes = routes.Values.ToList();
        LogProtocolEvent($"IS-IS: SPF calculated {state.CalculatedRoutes.Count} routes");
    }

    private async Task RunDijkstraSpf(INetworkDevice device, IsisState state, Dictionary<string, IsisRoute> routes)
    {
        var visited = new HashSet<string>();
        var distances = new Dictionary<string, int>();
        var previous = new Dictionary<string, string>();

        // Initialize distances
        distances[state.SystemId] = 0;

        // Simple SPF implementation based on LSP database
        foreach (var lsp in state.LspDatabase.Values.Where(l => !l.IsExpired))
        {
            if (lsp.OriginatingSystem == state.SystemId)
                continue;

            // Extract reachability information from LSP
            foreach (var tlv in lsp.Tlvs.Where(t => t.Type == 128)) // IP Internal Reachability
            {
                var reachabilityInfo = System.Text.Encoding.ASCII.GetString(tlv.Value);
                var parts = reachabilityInfo.Split('/');
                if (parts.Length == 2)
                {
                    var network = parts[0];
                    var mask = parts[1];
                    var routeKey = $"{network}/{mask}";

                    if (!routes.ContainsKey(routeKey))
                    {
                        // Find best path to this network
                        var metric = CalculateMetricToSystem(state, lsp.OriginatingSystem);
                        var nextHop = FindNextHopToSystem(state, lsp.OriginatingSystem);
                        var outInterface = FindOutgoingInterface(device, nextHop);

                        if (!string.IsNullOrEmpty(nextHop) && !string.IsNullOrEmpty(outInterface))
                        {
                            routes[routeKey] = new IsisRoute
                            {
                                Destination = network,
                                Mask = mask,
                                NextHop = nextHop,
                                Interface = outInterface,
                                Metric = metric,
                                Level = lsp.Level,
                                RouteType = IsisRouteType.Internal,
                                OriginatingLsp = lsp.LspId,
                                LastUpdate = DateTime.Now
                            };
                        }
                    }
                }
            }
        }
    }

    private int CalculateMetricToSystem(IsisState state, string systemId)
    {
        // Simplified metric calculation - in real implementation would use Dijkstra
        // For now, return a base metric plus hop count
        var baseMetric = 10;
        var hops = 1; // Assume direct neighbor for simplicity

        return baseMetric + (hops * 10);
    }

    private string FindNextHopToSystem(IsisState state, string systemId)
    {
        // Find the next hop to reach the given system
        // In a real implementation, this would use the SPT
        foreach (var neighbor in state.Neighbors.Values.Where(n => n.IsActive))
        {
            if (neighbor.SystemId == systemId)
            {
                // Direct neighbor - find its IP address
                var device = _device;
                var connectedDevice = device?.GetConnectedDevice(neighbor.InterfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    var neighborConfig = connectedDevice.Value.device.GetInterface(neighborInterface);
                    return neighborConfig?.IpAddress ?? "";
                }
            }
        }

        return ""; // No path found
    }

    private string FindOutgoingInterface(INetworkDevice device, string nextHop)
    {
        // Find the interface to reach the next hop
        if (string.IsNullOrEmpty(nextHop))
            return "";

        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var connectedDevice = device.GetConnectedDevice(interfaceName);
            if (connectedDevice.HasValue)
            {
                var neighborInterface = connectedDevice.Value.interfaceName;
                var neighborConfig = connectedDevice.Value.device.GetInterface(neighborInterface);
                if (neighborConfig?.IpAddress == nextHop)
                {
                    return interfaceName;
                }
            }
        }

        return "";
    }

    private async Task InstallRoutes(INetworkDevice device, IsisState state)
    {
        foreach (var route in state.CalculatedRoutes.Where(r => r.NextHop != "0.0.0.0"))
        {
            var deviceRoute = new Route(route.Destination, route.Mask, route.NextHop, route.Interface, "ISIS")
            {
                Metric = route.Metric,
                AdminDistance = 115 // IS-IS administrative distance
            };

            device.AddRoute(deviceRoute);
            LogProtocolEvent($"IS-IS: Installed route to {route.Destination}/{route.Mask} via {route.NextHop} metric {route.Metric}");
        }
    }

    protected override bool IsNeighborReachable(INetworkDevice device, string interfaceName, INetworkDevice neighbor)
    {
        var connection = device.GetPhysicalConnectionMetrics(interfaceName);
        return connection?.IsSuitableForRouting ?? false;
    }

    private IsisConfig GetIsisConfig()
    {
        // Use only the local configuration - no legacy routing config conversion
        var config = new IsisConfig
        {
            IsEnabled = false, // Will be enabled via ApplyConfiguration
            SystemId = "",
            AreaId = "49.0001",
            Level = IsisLevel.Level2,
            HelloInterval = 10,
            HoldTime = 30,
            LspRefreshInterval = 900,
            LspMaxLifetime = 1200,
            IsOverloaded = false
        };

        // Auto-generate system ID if not set
        if (string.IsNullOrEmpty(config.SystemId) && _device != null)
        {
            config.SystemId = config.GenerateSystemId(_device.Name);
        }

        return config;
    }


    protected override object GetProtocolConfiguration()
    {
        return GetIsisConfig();
    }

    protected override void OnApplyConfiguration(object configuration)
    {
        if (configuration is IsisConfig isisConfig && isisConfig.Validate())
        {
            // Store configuration in protocol state - no legacy integration needed
            _state.IsActive = isisConfig.IsEnabled;
            _state.MarkStateChanged();

            if (_state is IsisState isisState)
            {
                isisState.SystemId = isisConfig.SystemId;
                isisState.AreaId = isisConfig.AreaId;
                isisState.Level = isisConfig.Level;
                isisState.TopologyChanged = true;
            }
        }
    }

    private IsisProtocol GetNeighborIsisProtocol(INetworkDevice neighborDevice)
    {
        // In the new architecture, protocols are discovered through the device's protocol list
        // For now, return null - neighbor discovery will be handled differently
        return null;
    }

    private void OnLspChanged(object? sender, LspEventArgs e)
    {
        LogProtocolEvent($"IS-IS: LSP {e.Lsp.LspId} changed, triggering SPF recalculation");
        _state.MarkStateChanged();
    }

    public override void Dispose()
    {
        if (_lsdb != null)
        {
            _lsdb.LspAdded -= OnLspChanged;
            _lsdb.LspUpdated -= OnLspChanged;
            _lsdb.LspRemoved -= OnLspChanged;
            _lsdb = null;
        }

        _spfAlgorithm = null;
        base.Dispose();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco", "Juniper", "Nokia", "Generic" }; // IS-IS is multi-vendor
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

    protected override void OnSubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
    {
        // Subscribe to interface up/down events
        eventBus.Subscribe<InterfaceStateChangedEventArgs>(args =>
        {
            if (args.DeviceName == self.Name)
            {
                LogProtocolEvent($"IS-IS: Interface {args.InterfaceName} state changed to {(args.IsUp ? "UP" : "DOWN")}");
                _state.MarkStateChanged();
            }
            return Task.CompletedTask;
        });

        // Subscribe to protocol configuration changes
        eventBus.Subscribe<ProtocolConfigChangedEventArgs>(args =>
        {
            if (args.DeviceName == self.Name)
            {
                LogProtocolEvent("IS-IS: Protocol configuration changed, marking for update");
                _state.MarkStateChanged();
            }
            return Task.CompletedTask;
        });
    }

}
