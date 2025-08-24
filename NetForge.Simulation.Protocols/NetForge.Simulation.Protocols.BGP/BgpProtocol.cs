using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.BGP
{
    /// <summary>
    /// BGP (Border Gateway Protocol) protocol implementation
    /// Following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class BgpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.BGP;
        public override string Name => "Border Gateway Protocol";
        public override string Version => "4.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new BgpState();
        }

        protected override void OnInitialized()
        {
            var bgpConfig = GetBgpConfig();
            if (bgpConfig != null)
            {
                var bgpState = (BgpState)_state;
                bgpState.LocalAs = bgpConfig.LocalAs;
                bgpState.RouterId = bgpConfig.RouterId;
                bgpState.IsActive = bgpConfig.IsEnabled;

                // Initialize peer sessions
                foreach (var neighbor in bgpConfig.Neighbors.Values)
                {
                    var peerSession = bgpState.GetOrCreateBgpPeer(neighbor.IpAddress, () => new BgpPeer(
                        neighbor.IpAddress,
                        neighbor.RemoteAs,
                        neighbor.RemoteAs == bgpConfig.LocalAs // IBGP if same AS
                    ));

                    peerSession.HoldTime = neighbor.HoldTime;
                    peerSession.IsEnabled = neighbor.IsEnabled;
                    peerSession.Description = neighbor.Description;
                }

                LogProtocolEvent($"BGP AS {bgpConfig.LocalAs} initialized with Router ID {bgpConfig.RouterId}");
            }
        }

        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var bgpState = (BgpState)_state;
            var bgpConfig = GetBgpConfig();

            if (bgpConfig == null || !bgpConfig.IsEnabled)
            {
                bgpState.IsActive = false;
                return;
            }

            // Process configured BGP peers
            await ProcessBgpPeers(device, bgpConfig, bgpState);
        }

        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var bgpState = (BgpState)_state;
            var bgpConfig = GetBgpConfig();

            if (bgpConfig == null || !bgpConfig.IsEnabled)
            {
                return;
            }

            // Only run route selection if policy changed or periodic update is due
            if (!bgpState.ShouldRunRouteSelection())
            {
                LogProtocolEvent("BGP route selection not needed");
                return;
            }

            LogProtocolEvent("Running BGP route selection process...");

            try
            {
                // Clear existing BGP routes
                device.ClearRoutesByProtocol("BGP");
                bgpState.BestRoutes.Clear();

                // Run BGP best path selection algorithm
                await RunBestPathSelection(device, bgpState, bgpConfig);

                // Install selected routes
                await InstallBgpRoutes(device, bgpState);

                // Record successful route selection
                bgpState.RecordRouteSelection();

                LogProtocolEvent($"BGP route selection completed. {bgpState.BestRoutes.Count} routes selected.");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error during BGP route selection: {ex.Message}");
            }
        }

        private async Task ProcessBgpPeers(NetworkDevice device, BgpConfig config, BgpState state)
        {
            foreach (var neighborConfig in config.Neighbors.Values)
            {
                if (!neighborConfig.IsEnabled)
                    continue;

                var peerKey = neighborConfig.IpAddress;

                // Check if peer is reachable
                if (!IsPeerReachable(device, neighborConfig.IpAddress))
                {
                    var peer = state.Neighbors.GetValueOrDefault(peerKey) as BgpPeer;
                    if (peer != null && peer.State != "Idle")
                    {
                        peer.State = "Idle";
                        peer.StateTime = DateTime.Now;
                        LogProtocolEvent($"BGP peer {peerKey} not reachable - moved to Idle state");
                        state.MarkPolicyChanged();
                    }
                    continue;
                }

                var bgpPeer = state.GetOrCreateBgpPeer(peerKey, () => new BgpPeer(
                    neighborConfig.IpAddress,
                    neighborConfig.RemoteAs,
                    neighborConfig.RemoteAs == config.LocalAs
                )
                {
                    HoldTime = neighborConfig.HoldTime,
                    IsEnabled = neighborConfig.IsEnabled,
                    Description = neighborConfig.Description
                });

                // Update peer state machine
                await UpdatePeerStateMachine(bgpPeer, device, neighborConfig, state);
                state.UpdateNeighborActivity(peerKey);

                LogProtocolEvent($"BGP Peer {bgpPeer.PeerIp} (AS{bgpPeer.PeerAs}) - State: {bgpPeer.State}");

                // If peer reaches Established state, mark policy as changed
                if (bgpPeer.State == "Established" && state.Neighbors.ContainsKey(peerKey))
                {
                    var previousPeer = state.Neighbors[peerKey] as BgpPeer;
                    if (previousPeer?.State != "Established")
                    {
                        state.MarkPolicyChanged();
                        LogProtocolEvent($"BGP peer {bgpPeer.PeerIp} established - policy changed");
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task UpdatePeerStateMachine(BgpPeer peer, NetworkDevice device,
            BgpNeighbor config, BgpState state)
        {
            // Simplified BGP FSM
            switch (peer.State)
            {
                case "Idle":
                    if (config.IsEnabled && IsPeerReachable(device, peer.PeerIp))
                    {
                        peer.State = "Connect";
                        peer.StateTime = DateTime.Now;
                        LogProtocolEvent($"BGP peer {peer.PeerIp} moved to Connect state");
                    }
                    break;

                case "Connect":
                    // Simulate TCP connection establishment
                    peer.State = "Active";
                    peer.StateTime = DateTime.Now;
                    LogProtocolEvent($"BGP peer {peer.PeerIp} moved to Active state");
                    break;

                case "Active":
                    // Simulate BGP OPEN message exchange
                    peer.State = "OpenSent";
                    peer.StateTime = DateTime.Now;
                    LogProtocolEvent($"BGP peer {peer.PeerIp} moved to OpenSent state");
                    break;

                case "OpenSent":
                    // Simulate OPEN message received
                    peer.State = "OpenConfirm";
                    peer.StateTime = DateTime.Now;
                    LogProtocolEvent($"BGP peer {peer.PeerIp} moved to OpenConfirm state");
                    break;

                case "OpenConfirm":
                    // Simulate KEEPALIVE exchange
                    peer.State = "Established";
                    peer.StateTime = DateTime.Now;
                    peer.AdvertisedRouteCount = 0;
                    peer.ReceivedRouteCount = 0;
                    LogProtocolEvent($"BGP peer {peer.PeerIp} established - session active");
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task RunBestPathSelection(NetworkDevice device, BgpState state, BgpConfig config)
        {
            // Collect all received routes from all peers
            var candidateRoutes = new List<(BgpRouteEntry route, BgpPeer peer)>();

            foreach (var peerKvp in state.Neighbors.Cast<KeyValuePair<string, object>>())
            {
                if (peerKvp.Value is BgpPeer peer && peer.State == "Established")
                {
                    // Simulate receiving routes from peer (in real BGP, these come from UPDATE messages)
                    var receivedRoutes = await SimulateReceivedRoutes(device, peer, config);

                    foreach (var route in receivedRoutes)
                    {
                        candidateRoutes.Add((route, peer));
                    }

                    peer.ReceivedRouteCount = receivedRoutes.Count;
                }
            }

            // Group routes by prefix and run best path selection
            var routeGroups = candidateRoutes.GroupBy(r => $"{r.route.Network}/{r.route.PrefixLength}");

            foreach (var group in routeGroups)
            {
                var bestRoute = SelectBestPath(group.ToList());
                if (bestRoute.HasValue)
                {
                    var (route, peer) = bestRoute.Value;
                    state.BestRoutes[group.Key] = route;

                    LogProtocolEvent($"Best path for {group.Key}: via {route.NextHop} from AS{peer.PeerAs} (peer {peer.PeerIp})");
                }
            }

            await Task.CompletedTask;
        }

        private async Task<List<BgpRouteEntry>> SimulateReceivedRoutes(NetworkDevice device, BgpPeer peer, BgpConfig config)
        {
            var routes = new List<BgpRouteEntry>();

            // In a real implementation, these would come from UPDATE messages
            // For simulation, we generate some sample routes based on peer characteristics

            if (peer.IsIbgp)
            {
                // IBGP peer - might receive external routes and internal networks
                var externalRoute = new BgpRouteEntry
                {
                    Network = "203.0.113.0",
                    PrefixLength = 24,
                    NextHop = peer.PeerIp,
                    AsPath = new List<int> { config.LocalAs, peer.PeerAs, 65001 },
                    LocalPreference = 100,
                    Med = 0,
                    Origin = "IGP"
                };
                routes.Add(externalRoute);
            }
            else
            {
                // EBGP peer - advertise routes from this AS and learned routes
                var externalRoute = new BgpRouteEntry
                {
                    Network = $"198.51.100.{peer.PeerAs % 256}",
                    PrefixLength = 24,
                    NextHop = peer.PeerIp,
                    AsPath = new List<int> { peer.PeerAs },
                    LocalPreference = 100,
                    Med = 10,
                    Origin = "IGP"
                };
                routes.Add(externalRoute);
            }

            return await Task.FromResult(routes);
        }

        private (BgpRouteEntry route, BgpPeer peer)? SelectBestPath(List<(BgpRouteEntry route, BgpPeer peer)> candidates)
        {
            if (candidates.Count == 0)
                return null;

            // BGP Best Path Selection Algorithm (simplified)
            // 1. Highest LOCAL_PREF
            // 2. Shortest AS_PATH
            // 3. Lowest ORIGIN (IGP < EGP < INCOMPLETE)
            // 4. Lowest MED (for routes from same AS)
            // 5. EBGP over IBGP
            // 6. Lowest IGP metric to BGP next hop
            // 7. Router ID tiebreaker

            var best = candidates.First();

            foreach (var candidate in candidates.Skip(1))
            {
                // Compare LOCAL_PREF (higher is better)
                if (candidate.route.LocalPreference > best.route.LocalPreference)
                {
                    best = candidate;
                    continue;
                }
                if (candidate.route.LocalPreference < best.route.LocalPreference)
                    continue;

                // Compare AS_PATH length (shorter is better)
                if (candidate.route.AsPath.Count < best.route.AsPath.Count)
                {
                    best = candidate;
                    continue;
                }
                if (candidate.route.AsPath.Count > best.route.AsPath.Count)
                    continue;

                // Compare ORIGIN (IGP < EGP < INCOMPLETE)
                var candidateOriginValue = GetOriginValue(candidate.route.Origin);
                var bestOriginValue = GetOriginValue(best.route.Origin);

                if (candidateOriginValue < bestOriginValue)
                {
                    best = candidate;
                    continue;
                }
                if (candidateOriginValue > bestOriginValue)
                    continue;

                // Compare MED (lower is better, only for routes from same neighboring AS)
                if (candidate.route.AsPath.FirstOrDefault() == best.route.AsPath.FirstOrDefault())
                {
                    if (candidate.route.Med < best.route.Med)
                    {
                        best = candidate;
                        continue;
                    }
                    if (candidate.route.Med > best.route.Med)
                        continue;
                }

                // Prefer EBGP over IBGP
                if (!candidate.peer.IsIbgp && best.peer.IsIbgp)
                {
                    best = candidate;
                    continue;
                }
                if (candidate.peer.IsIbgp && !best.peer.IsIbgp)
                    continue;

                // Router ID tiebreaker (lower is better)
                if (string.Compare(candidate.peer.PeerIp, best.peer.PeerIp, StringComparison.Ordinal) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private static int GetOriginValue(string origin)
        {
            return origin switch
            {
                "IGP" => 0,
                "EGP" => 1,
                "INCOMPLETE" => 2,
                _ => 3
            };
        }

        private async Task InstallBgpRoutes(NetworkDevice device, BgpState state)
        {
            foreach (var routeEntry in state.BestRoutes.Values)
            {
                try
                {
                    var deviceRoute = new Route(
                        routeEntry.Network,
                        GetSubnetMask(routeEntry.PrefixLength),
                        routeEntry.NextHop,
                        GetOutgoingInterface(device, routeEntry.NextHop),
                        "BGP"
                    )
                    {
                        Metric = routeEntry.Med,
                        AdminDistance = 200 // EBGP: 20, IBGP: 200
                    };

                    device.AddRoute(deviceRoute);
                    LogProtocolEvent($"Installed BGP route: {routeEntry.Network}/{routeEntry.PrefixLength} via {routeEntry.NextHop} [{deviceRoute.AdminDistance}/{routeEntry.Med}]");
                }
                catch (Exception ex)
                {
                    LogProtocolEvent($"Failed to install BGP route {routeEntry.Network}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        // Helper methods

        private BgpConfig? GetBgpConfig()
        {
            return _device?.GetBgpConfiguration() as BgpConfig;
        }

        private bool IsPeerReachable(NetworkDevice device, string peerIp)
        {
            // Check if we have a route to the peer
            var routes = device.GetRoutingTable();
            return routes.Any(r => IsIpInNetwork(peerIp, r.Network, r.Mask));
        }

        private string GetSubnetMask(int prefixLength)
        {
            uint mask = 0xFFFFFFFF << (32 - prefixLength);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        private string GetOutgoingInterface(NetworkDevice device, string nextHop)
        {
            var routes = device.GetRoutingTable();
            var route = routes.FirstOrDefault(r => IsIpInNetwork(nextHop, r.Network, r.Mask));
            return route?.Interface ?? "";
        }

        /// <summary>
        /// Helper method to check if IP is in network (copied from NetworkDevice functionality)
        /// </summary>
        private static bool IsIpInNetwork(string ip, string network, string mask)
        {
            try
            {
                var ipBytes = ip.Split('.').Select(byte.Parse).ToArray();
                var networkBytes = network.Split('.').Select(byte.Parse).ToArray();
                var maskBytes = mask.Split('.').Select(byte.Parse).ToArray();

                for (int i = 0; i < 4; i++)
                {
                    if ((ipBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override object GetProtocolConfiguration()
        {
            return GetBgpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is BgpConfig bgpConfig)
            {
                _device.SetBgpConfiguration(bgpConfig);

                var bgpState = (BgpState)_state;
                bgpState.LocalAs = bgpConfig.LocalAs;
                bgpState.RouterId = bgpConfig.RouterId;
                bgpState.IsActive = bgpConfig.IsEnabled;
                bgpState.MarkPolicyChanged();

                LogProtocolEvent($"BGP configuration updated - AS{bgpConfig.LocalAs} Router ID: {bgpConfig.RouterId}");
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" };
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            // BGP hold time - typically 180 seconds
            return 180;
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            var bgpState = (BgpState)_state;
            if (bgpState.Neighbors.ContainsKey(neighborId))
            {
                var peer = bgpState.Neighbors[neighborId] as BgpPeer;
                LogProtocolEvent($"BGP peer {peer?.PeerIp} (AS{peer?.PeerAs}) removed due to timeout");
                bgpState.Neighbors.Remove(neighborId);
                bgpState.MarkPolicyChanged();
            }
        }
    }
}
