using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.OSPF
{
    /// <summary>
    /// OSPF (Open Shortest Path First) protocol implementation
    /// Following the state management pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
    /// </summary>
    public class OspfProtocol : BaseProtocol
    {
        public override NetworkProtocolType Type => NetworkProtocolType.OSPF;
        public override string Name => "Open Shortest Path First";
        public override string Version => "2.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new OspfState();
        }

        protected override void OnInitialized()
        {
            var ospfConfig = GetOspfConfig();
            if (ospfConfig != null)
            {
                var ospfState = (OspfState)_state;
                ospfState.RouterId = ospfConfig.RouterId;
                ospfState.ProcessId = ospfConfig.ProcessId;
                ospfState.Areas = ospfConfig.Areas;
                ospfState.Interfaces = ospfConfig.Interfaces;
                ospfState.IsActive = ospfConfig.IsEnabled;

                LogProtocolEvent($"OSPF Process {ospfConfig.ProcessId} initialized with Router ID {ospfConfig.RouterId}");
            }
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var ospfState = (OspfState)_state;
            var ospfConfig = GetOspfConfig();

            if (ospfConfig == null || !ospfConfig.IsEnabled)
            {
                ospfState.IsActive = false;
                return;
            }

            // Discover OSPF neighbors using the state management pattern
            await DiscoverOspfNeighbors(device, ospfConfig, ospfState);
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var ospfState = (OspfState)_state;

            // Only run SPF if topology changed or periodic calculation is due
            if (!ospfState.ShouldRunSpfCalculation())
            {
                LogProtocolEvent("SPF calculation not needed - no topology changes");
                return;
            }

            LogProtocolEvent("Running SPF calculation due to topology change...");

            // Clear existing OSPF routes
            device.ClearRoutesByProtocol("OSPF");
            ospfState.RoutingTable.Clear();
            ospfState.CalculatedRoutes.Clear();

            try
            {
                // Build link-state database
                await BuildLinkStateDatabase(device, ospfState);

                // Run Dijkstra's SPF algorithm for each area
                foreach (var area in ospfState.Areas.Values)
                {
                    await RunSpfCalculationForArea(device, ospfState, area);
                }

                // Install calculated routes
                await InstallOspfRoutes(device, ospfState);

                // Record successful calculation
                ospfState.RecordSpfCalculation();

                LogProtocolEvent($"SPF calculation completed. {ospfState.CalculatedRoutes.Count} routes calculated.");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error during SPF calculation: {ex.Message}");
            }
        }

        private async Task DiscoverOspfNeighbors(INetworkDevice device, OspfConfig config, OspfState state)
        {
            // Implementation following the existing pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if interface is in an OSPF area
                if (!IsInterfaceInOspfArea(interfaceName, config))
                    continue;

                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;

                    // Check if neighbor has OSPF enabled
                    var neighborOspfConfig = GetNeighborOspfConfig(neighborDevice);
                    if (neighborOspfConfig?.IsEnabled == true)
                    {
                        await ProcessOspfNeighbor(device, state, config, interfaceName, neighborDevice, neighborInterface);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task ProcessOspfNeighbor(INetworkDevice device, OspfState state, OspfConfig config,
            string localInterface, INetworkDevice neighborDevice, string neighborInterface)
        {
            var neighborKey = $"{neighborDevice.Name}:{neighborInterface}";
            var neighborConfig = GetNeighborOspfConfig(neighborDevice);

            if (neighborConfig == null) return;

            // Check if both devices are in the same OSPF area
            var localArea = GetInterfaceArea(localInterface, config);
            var neighborArea = GetInterfaceArea(neighborInterface, neighborConfig);

            if (localArea != neighborArea)
            {
                LogProtocolEvent($"Area mismatch with neighbor {neighborDevice.Name}: local={localArea}, neighbor={neighborArea}");
                return;
            }

            var neighbor = state.GetOrCreateOspfNeighbor(neighborKey, () => new OspfNeighbor(
                neighborConfig.RouterId,
                neighborDevice.GetInterface(neighborInterface)?.IpAddress ?? "0.0.0.0",
                neighborInterface)
            {
                State = "Init"
            });

            // Advance neighbor state machine based on bidirectional communication
            await UpdateNeighborStateMachine(neighbor, device, neighborDevice, localInterface, neighborInterface);
            state.UpdateNeighborActivity(neighborKey);

            LogProtocolEvent($"OSPF Neighbor {neighbor.NeighborId} on {localInterface} - State: {neighbor.State}");

            // If neighbor reaches Full state, mark topology as changed
            if (neighbor.State == "Full" && state.Neighbors.ContainsKey(neighborKey))
            {
                var previousState = state.Neighbors[neighborKey].State;
                if (previousState != "Full")
                {
                    state.MarkTopologyChanged();
                    LogProtocolEvent($"Neighbor {neighbor.NeighborId} reached Full state - topology changed");
                }
            }

            state.Neighbors[neighborKey] = neighbor;
        }

        private async Task UpdateNeighborStateMachine(OspfNeighbor neighbor, INetworkDevice device,
            INetworkDevice neighborDevice, string localInterface, string neighborInterface)
        {
            // Simplified OSPF neighbor state machine
            switch (neighbor.State)
            {
                case "Init":
                    // Check if neighbor sees us (simplified - assume bidirectional)
                    if (CanEstablishAdjacency(device, neighborDevice, localInterface, neighborInterface))
                    {
                        neighbor.State = "TwoWay";
                        neighbor.StateTime = DateTime.Now;
                        LogProtocolEvent($"Neighbor {neighbor.NeighborId} moved to TwoWay state");
                    }
                    break;

                case "TwoWay":
                    // Check if we should form adjacency (simplified - always form on point-to-point)
                    if (ShouldFormAdjacency(device, neighborDevice, localInterface))
                    {
                        neighbor.State = "ExStart";
                        neighbor.StateTime = DateTime.Now;
                        LogProtocolEvent($"Neighbor {neighbor.NeighborId} moved to ExStart state");
                    }
                    break;

                case "ExStart":
                    // Database description exchange start (simplified)
                    neighbor.State = "Exchange";
                    neighbor.StateTime = DateTime.Now;
                    LogProtocolEvent($"Neighbor {neighbor.NeighborId} moved to Exchange state");
                    break;

                case "Exchange":
                    // Database description exchange (simplified)
                    neighbor.State = "Loading";
                    neighbor.StateTime = DateTime.Now;
                    LogProtocolEvent($"Neighbor {neighbor.NeighborId} moved to Loading state");
                    break;

                case "Loading":
                    // Link state request/update exchange (simplified)
                    neighbor.State = "Full";
                    neighbor.StateTime = DateTime.Now;
                    LogProtocolEvent($"Neighbor {neighbor.NeighborId} reached Full state - adjacency established");
                    break;
            }

            await Task.CompletedTask;
        }

        private bool CanEstablishAdjacency(INetworkDevice device, INetworkDevice neighbor, string localInterface, string neighborInterface)
        {
            // Simplified check - ensure both interfaces are up and in same subnet
            var localInterfaceConfig = device.GetInterface(localInterface);
            var neighborInterfaceConfig = neighbor.GetInterface(neighborInterface);

            return localInterfaceConfig?.IsUp == true &&
                   neighborInterfaceConfig?.IsUp == true &&
                   !localInterfaceConfig.IsShutdown &&
                   !neighborInterfaceConfig.IsShutdown;
        }

        private bool ShouldFormAdjacency(INetworkDevice device, INetworkDevice neighbor, string localInterface)
        {
            // For simulation purposes, always form adjacency on point-to-point links
            // In real OSPF, this depends on DR/BDR election for multi-access networks
            return true;
        }

        private async Task BuildLinkStateDatabase(INetworkDevice device, OspfState state)
        {
            // Build router LSA for this router
            var routerLsa = new LinkStateAdvertisement
            {
                LsId = state.RouterId,
                AdvertisingRouter = state.RouterId,
                LsType = "Router",
                SequenceNumber = state.SpfCalculationCount + 1,
                Timestamp = DateTime.Now,
                Area = 0 // Simplified - use area 0
            };

            // Add interface information to LSA
            var links = new List<Dictionary<string, object>>();
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsUp == true && !interfaceConfig.IsShutdown)
                {
                    var connectedDevice = device.GetConnectedDevice(interfaceName);
                    if (connectedDevice.HasValue)
                    {
                        var link = new Dictionary<string, object>
                        {
                            ["Type"] = "Point-to-Point",
                            ["LinkId"] = connectedDevice.Value.device.Name,
                            ["LinkData"] = interfaceConfig.IpAddress ?? "0.0.0.0",
                            ["Metric"] = GetInterfaceCost(interfaceName, state)
                        };
                        links.Add(link);
                    }
                }
            }

            routerLsa.Data["Links"] = links;
            state.LinkStateDatabase[routerLsa.LsId] = routerLsa;

            await Task.CompletedTask;
        }

        private async Task RunSpfCalculationForArea(INetworkDevice device, OspfState state, OspfArea area)
        {
            // Simplified Dijkstra's algorithm implementation
            var shortestPaths = new Dictionary<string, (int cost, string nextHop, string outInterface)>();
            var visited = new HashSet<string>();
            var candidates = new SortedDictionary<int, List<string>>();

            // Initialize with this router
            var routerId = state.RouterId;
            shortestPaths[routerId] = (0, "", "");
            candidates[0] = new List<string> { routerId };

            while (candidates.Count > 0)
            {
                // Get minimum cost candidate
                var minCost = candidates.Keys.First();
                var candidateList = candidates[minCost];
                var currentRouter = candidateList.First();

                candidateList.RemoveAt(0);
                if (candidateList.Count == 0)
                    candidates.Remove(minCost);

                if (visited.Contains(currentRouter))
                    continue;

                visited.Add(currentRouter);

                // Process all links from current router
                await ProcessRouterLinks(device, state, currentRouter, shortestPaths, candidates, visited);
            }

            // Convert shortest paths to routes
            ConvertShortestPathsToRoutes(device, state, shortestPaths);

            await Task.CompletedTask;
        }

        private async Task ProcessRouterLinks(INetworkDevice device, OspfState state, string currentRouter,
            Dictionary<string, (int cost, string nextHop, string outInterface)> shortestPaths,
            SortedDictionary<int, List<string>> candidates, HashSet<string> visited)
        {
            // Get LSA for current router
            if (!state.LinkStateDatabase.TryGetValue(currentRouter, out var lsa))
                return;

            var links = lsa.Data.GetValueOrDefault("Links") as List<Dictionary<string, object>> ?? new();
            var currentCost = shortestPaths[currentRouter].cost;

            foreach (var link in links)
            {
                var neighborId = link.GetValueOrDefault("LinkId")?.ToString() ?? "";
                var linkCost = Convert.ToInt32(link.GetValueOrDefault("Metric") ?? 1);
                var newCost = currentCost + linkCost;

                if (visited.Contains(neighborId))
                    continue;

                if (!shortestPaths.ContainsKey(neighborId) || newCost < shortestPaths[neighborId].cost)
                {
                    // Determine next hop and interface
                    string nextHop, outInterface;
                    if (currentRouter == state.RouterId)
                    {
                        // Direct neighbor
                        nextHop = neighborId;
                        outInterface = GetInterfaceToNeighbor(device, neighborId);
                    }
                    else
                    {
                        // Use next hop from current router's path
                        nextHop = shortestPaths[currentRouter].nextHop;
                        outInterface = shortestPaths[currentRouter].outInterface;
                    }

                    shortestPaths[neighborId] = (newCost, nextHop, outInterface);

                    // Add to candidates
                    if (!candidates.ContainsKey(newCost))
                        candidates[newCost] = new List<string>();
                    candidates[newCost].Add(neighborId);
                }
            }

            await Task.CompletedTask;
        }

        private void ConvertShortestPathsToRoutes(INetworkDevice device, OspfState state,
            Dictionary<string, (int cost, string nextHop, string outInterface)> shortestPaths)
        {
            foreach (var (destination, (cost, nextHop, outInterface)) in shortestPaths)
            {
                if (destination == state.RouterId || string.IsNullOrEmpty(nextHop))
                    continue;

                // Get destination network (simplified - use router ID as network)
                var network = destination;
                var mask = "255.255.255.255"; // Host route for router ID

                var route = new OspfRoute
                {
                    Network = network,
                    Mask = mask,
                    NextHop = GetNextHopIpAddress(device, nextHop, outInterface),
                    Interface = outInterface,
                    Cost = cost,
                    RouteType = "Internal",
                    Area = 0,
                    LastUpdate = DateTime.Now
                };

                state.CalculatedRoutes.Add(route);
                state.RoutingTable[network] = route;
            }
        }

        private async Task InstallOspfRoutes(INetworkDevice device, OspfState state)
        {
            foreach (var route in state.CalculatedRoutes)
            {
                try
                {
                    var deviceRoute = new Route(route.Network, route.Mask, route.NextHop, route.Interface, "OSPF")
                    {
                        Metric = route.Cost,
                        AdminDistance = 110 // OSPF administrative distance
                    };

                    device.AddRoute(deviceRoute);
                    LogProtocolEvent($"Installed route: {route.Network}/{route.Mask} via {route.NextHop} on {route.Interface} [110/{route.Cost}]");
                }
                catch (Exception ex)
                {
                    LogProtocolEvent($"Failed to install route {route.Network}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        // Helper methods

        private OspfConfig GetOspfConfig()
        {
            return _device?.GetOspfConfiguration() as OspfConfig;
        }

        private OspfConfig GetNeighborOspfConfig(INetworkDevice neighbor)
        {
            return neighbor?.GetOspfConfiguration() as OspfConfig;
        }

        private bool IsInterfaceInOspfArea(string interfaceName, OspfConfig config)
        {
            return config.Interfaces.ContainsKey(interfaceName) ||
                   config.Areas.Values.Any(area => area.Interfaces.Contains(interfaceName));
        }

        private int GetInterfaceArea(string interfaceName, OspfConfig config)
        {
            if (config.Interfaces.TryGetValue(interfaceName, out var ospfInterface))
                return ospfInterface.Area;

            foreach (var area in config.Areas.Values)
            {
                if (area.Interfaces.Contains(interfaceName))
                    return area.AreaId;
            }

            return 0; // Default area
        }

        private int GetInterfaceCost(string interfaceName, OspfState state)
        {
            if (state.Interfaces.TryGetValue(interfaceName, out var ospfInterface))
                return ospfInterface.Cost;

            return 10; // Default cost
        }

        private string GetInterfaceToNeighbor(INetworkDevice device, string neighborId)
        {
            // Find interface connected to neighbor
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue && connectedDevice.Value.device.Name == neighborId)
                {
                    return interfaceName;
                }
            }
            return "";
        }

        private string GetNextHopIpAddress(INetworkDevice device, string nextHopRouter, string outInterface)
        {
            // Get the IP address of the next hop router on the connected interface
            var connectedDevice = device.GetConnectedDevice(outInterface);
            if (connectedDevice.HasValue && connectedDevice.Value.device.Name == nextHopRouter)
            {
                return connectedDevice.Value.device.GetInterface(connectedDevice.Value.interfaceName)?.IpAddress ?? "0.0.0.0";
            }
            return nextHopRouter; // Fallback to router ID
        }

        protected override object GetProtocolConfiguration()
        {
            return GetOspfConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is OspfConfig ospfConfig)
            {
                _device.SetOspfConfiguration(ospfConfig);

                var ospfState = (OspfState)_state;
                ospfState.RouterId = ospfConfig.RouterId;
                ospfState.ProcessId = ospfConfig.ProcessId;
                ospfState.Areas = ospfConfig.Areas;
                ospfState.Interfaces = ospfConfig.Interfaces;
                ospfState.IsActive = ospfConfig.IsEnabled;
                ospfState.MarkTopologyChanged();

                LogProtocolEvent($"OSPF configuration updated - Router ID: {ospfConfig.RouterId}");
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" };
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            // OSPF neighbor timeout (Dead Interval) - typically 4x Hello Interval
            return 40; // 40 seconds for fast convergence in simulation
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            var ospfState = (OspfState)_state;
            if (ospfState.Neighbors.ContainsKey(neighborId))
            {
                var neighbor = ospfState.Neighbors[neighborId];
                LogProtocolEvent($"OSPF neighbor {neighbor.NeighborId} removed due to timeout");
                ospfState.Neighbors.Remove(neighborId);
                ospfState.MarkTopologyChanged();
            }
        }
    }
}
