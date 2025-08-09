using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
// For BgpConfig
// Added for events
// Added for Task

// Added for Any()

namespace NetSim.Simulation.Protocols.Implementations
{
    public class BgpProtocol : INetworkProtocol
    {
        private BgpConfig _bgpConfig;
        private NetworkDevice _device; // Store reference to the device
        private readonly BgpState _state = new(); // Protocol-specific state
        private readonly Dictionary<string, DateTime> _peerLastContact = new(); // Track peer contact times

        public ProtocolType Type => ProtocolType.BGP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
            _bgpConfig = device.GetBgpConfiguration();
            if (_bgpConfig == null)
            {
                device.AddLogEntry("BgpProtocol: BGP configuration not found on initialization.");
            }
            else
            {
                device.AddLogEntry("BgpProtocol: Successfully initialized with existing BGP configuration.");
                
                // Initialize peer sessions
                if (_bgpConfig.Neighbors != null)
                {
                    foreach (var neighbor in _bgpConfig.Neighbors)
                    {
                        var peerSession = _state.GetOrCreatePeerSession(neighbor.Key, neighbor.Value.RemoteAs);
                        peerSession.IsIbgp = neighbor.Value.RemoteAs == _bgpConfig.LocalAs;
                        peerSession.HoldTimer = neighbor.Value.HoldTime;
                    }
                }
                
                // Mark policy as changed to trigger initial route selection
                _state.MarkPolicyChanged();
            }
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            eventBus.Subscribe<InterfaceStateChangedEventArgs>(HandleInterfaceStateChangeAsync);
            eventBus.Subscribe<ProtocolConfigChangedEventArgs>(HandleProtocolConfigChangeAsync);
            // BGP might also be interested in LinkChangedEventArgs if specific interface IPs are used for peering
        }

        private async Task HandleInterfaceStateChangeAsync(InterfaceStateChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name)
            {
                // Check if the changed interface is relevant for BGP (e.g., source of a BGP session)
                _device.AddLogEntry($"BGPProtocol on {_device.Name}: Received InterfaceStateChange for {args.InterfaceName}. Re-evaluating BGP state.");
                
                // Mark policy as changed when interface states change
                _state.MarkPolicyChanged();
                await UpdateState(_device);
            }
        }

        private async Task HandleProtocolConfigChangeAsync(ProtocolConfigChangedEventArgs args)
        {
            if (args.DeviceName == _device.Name && args.ProtocolType == Type)
            {
                _device.AddLogEntry($"BGPProtocol on {_device.Name}: Received ProtocolConfigChange: {args.ChangeDetails}. Re-evaluating BGP configuration and state.");
                _bgpConfig = _device.GetBgpConfiguration(); // Re-fetch config
                _state.MarkPolicyChanged();
                await UpdateState(_device);
            }
        }

        public async Task UpdateState(NetworkDevice device)
        {
            if (_bgpConfig == null) _bgpConfig = device.GetBgpConfiguration();

            if (_bgpConfig == null || !_bgpConfig.IsEnabled)
            {
                device.AddLogEntry($"BGPProtocol on {device.Name}: BGP configuration missing or not enabled. Clearing BGP routes.");
                device.ClearRoutesByProtocol("BGP");
                _state.Rib.Clear();
                return;
            }

            device.AddLogEntry($"BgpProtocol: Updating BGP state for AS {_bgpConfig.LocalAs} on device {device.Name}...");
            
            // Maintain peer sessions
            await UpdatePeerSessions(device);
            
            // Clean up stale peers
            await CleanupStalePeers(device);
            
            // Only run route selection if policy changed
            if (_state.PolicyChanged)
            {
                await RunRouteSelection(device);
                _state.PolicyChanged = false;
                _state.LastRouteSelection = DateTime.Now;
            }
            else
            {
                device.AddLogEntry("BGPProtocol: No policy changes detected, skipping route selection.");
            }
        }

        private async Task UpdatePeerSessions(NetworkDevice device)
        {
            device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Updating peer sessions...");

            if (_bgpConfig.Neighbors == null) return;

            foreach (var neighborKvp in _bgpConfig.Neighbors)
            {
                var neighborIp = neighborKvp.Key;
                var bgpNeighbor = neighborKvp.Value;

                // Get or create peer session
                var peerSession = _state.GetOrCreatePeerSession(neighborIp, bgpNeighbor.RemoteAs);
                
                // Update peer contact time
                _peerLastContact[neighborIp] = DateTime.Now;

                device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Checking peer {neighborIp} in AS {bgpNeighbor.RemoteAs}, current state: {peerSession.State}");
                
                // Check if peer is reachable
                var neighborDevice = device.ParentNetwork?.FindDeviceByIp(neighborIp);
                if (neighborDevice != null)
                {
                    var neighborBgpConfig = neighborDevice.GetBgpConfiguration();
                    if (neighborBgpConfig != null && neighborBgpConfig.IsEnabled && 
                        neighborBgpConfig.Neighbors?.Any(n => n.Value.RemoteAs == _bgpConfig.LocalAs) == true)
                    {
                        // Progress peer session state
                        await ProgressPeerState(peerSession, device, neighborDevice);
                    }
                    else
                    {
                        // Peer not configured correctly, reset to Idle
                        if (peerSession.State != BgpSessionState.Idle)
                        {
                            peerSession.ChangeState(BgpSessionState.Idle);
                            _state.MarkPolicyChanged();
                            device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Peer {neighborIp} not configured correctly, resetting to Idle");
                        }
                    }
                }
                else
                {
                    // Peer not reachable, reset to Idle
                    if (peerSession.State != BgpSessionState.Idle)
                    {
                        peerSession.ChangeState(BgpSessionState.Idle);
                        _state.MarkPolicyChanged();
                        device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Peer {neighborIp} not reachable, resetting to Idle");
                    }
                }
            }
        }

        private async Task ProgressPeerState(BgpPeerSession peerSession, NetworkDevice device, NetworkDevice neighborDevice)
        {
            // Simplified BGP peer state machine
            switch (peerSession.State)
            {
                case BgpSessionState.Idle:
                    peerSession.ChangeState(BgpSessionState.Connect);
                    _state.MarkPolicyChanged();
                    break;
                    
                case BgpSessionState.Connect:
                    peerSession.ChangeState(BgpSessionState.Active);
                    _state.MarkPolicyChanged();
                    break;
                    
                case BgpSessionState.Active:
                    peerSession.ChangeState(BgpSessionState.OpenSent);
                    _state.MarkPolicyChanged();
                    break;
                    
                case BgpSessionState.OpenSent:
                    peerSession.ChangeState(BgpSessionState.OpenConfirm);
                    _state.MarkPolicyChanged();
                    break;
                    
                case BgpSessionState.OpenConfirm:
                    peerSession.ChangeState(BgpSessionState.Established);
                    _state.MarkPolicyChanged();
                    device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Peer {peerSession.PeerIp} established");
                    break;
                    
                case BgpSessionState.Established:
                    // Session is established - maintain state and exchange routes
                    await ExchangeRoutes(peerSession, device, neighborDevice);
                    break;
            }
        }

        private async Task ExchangeRoutes(BgpPeerSession peerSession, NetworkDevice device, NetworkDevice neighborDevice)
        {
            // Simplified route exchange - in reality this would be much more complex
            // For now, just advertise connected networks to established peers
            
            var deviceInterfaces = device.GetAllInterfaces().Values;
            foreach (var iface in deviceInterfaces)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress) && iface.IsUp)
                {
                    var network = device.GetNetworkAddress(iface.IpAddress, iface.SubnetMask);
                    var routeKey = $"{network}/{device.MaskToCidr(iface.SubnetMask)}";
                    
                    if (!_state.AdjRibOut[peerSession.PeerIp].ContainsKey(routeKey))
                    {
                        var bgpRoute = new BgpRoute(network, iface.SubnetMask, "0.0.0.0")
                        {
                            Origin = "IGP",
                            AsPath = new List<int> { _bgpConfig.LocalAs }
                        };
                        
                        _state.AdjRibOut[peerSession.PeerIp][routeKey] = bgpRoute;
                        device.AddLogEntry($"BGP ({_bgpConfig.LocalAs}) on {device.Name}: Advertising route {routeKey} to peer {peerSession.PeerIp}");
                    }
                }
            }
        }

        private async Task CleanupStalePeers(NetworkDevice device)
        {
            var stalePeers = _state.GetStalePeers();
            foreach (var stalePeerIp in stalePeers)
            {
                device.AddLogEntry($"BGPProtocol: Peer {stalePeerIp} became stale, removing session");
                _state.RemovePeerSession(stalePeerIp);
                _peerLastContact.Remove(stalePeerIp);
            }
        }

        private async Task RunRouteSelection(NetworkDevice device)
        {
            device.AddLogEntry("BGPProtocol: Running BGP route selection due to policy change...");
            
            // Clear existing BGP routes
            device.ClearRoutesByProtocol("BGP");
            _state.Rib.Clear();

            // Collect all valid routes from established peers
            var candidateRoutes = new List<BgpRoute>();
            
            foreach (var peerSession in _state.PeerSessions.Values.Where(p => p.State == BgpSessionState.Established))
            {
                if (_state.AdjRibIn.ContainsKey(peerSession.PeerIp))
                {
                    candidateRoutes.AddRange(_state.AdjRibIn[peerSession.PeerIp].Values.Where(r => r.IsValid));
                }
            }

            // Run BGP best path selection (simplified)
            var selectedRoutes = SelectBestPaths(candidateRoutes);
            
            // Install selected routes
            foreach (var route in selectedRoutes)
            {
                var deviceRoute = new Route(route.Network, route.SubnetMask, route.NextHop, "", "BGP");
                deviceRoute.Metric = route.CalculatePreference();
                device.AddRoute(deviceRoute);
                
                // Store in BGP RIB
                var routeKey = $"{route.Network}/{device.MaskToCidr(route.SubnetMask)}";
                _state.Rib[routeKey] = route;
                
                device.AddLogEntry($"BGPProtocol: Installed BGP route {routeKey} via {route.NextHop} (preference: {route.CalculatePreference()})");
            }
            
            device.AddLogEntry($"BGPProtocol: Route selection completed, installed {selectedRoutes.Count} routes");
        }

        private List<BgpRoute> SelectBestPaths(List<BgpRoute> candidateRoutes)
        {
            // Group routes by network prefix
            var routesByNetwork = candidateRoutes.GroupBy(r => $"{r.Network}/{r.SubnetMask}");
            var bestRoutes = new List<BgpRoute>();
            
            foreach (var networkGroup in routesByNetwork)
            {
                // Select best path for each network (simplified BGP decision process)
                var bestRoute = networkGroup.OrderByDescending(r => r.CalculatePreference()).First();
                bestRoutes.Add(bestRoute);
            }
            
            return bestRoutes;
        }
    }
} 
