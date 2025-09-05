using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Protocols.Common.State;

namespace NetForge.Simulation.Protocols.Common.Base
{
    /// <summary>
    /// Base class for Layer 3 routing protocols (OSPF, BGP, RIP, EIGRP, etc.)
    /// Provides common routing functionality and standardized routing protocol behavior
    /// </summary>
    public abstract class BaseRoutingProtocol : BaseProtocol
    {
        protected readonly Dictionary<string, object> _routingTable = new();
        protected readonly Dictionary<string, DateTime> _routeLastUpdate = new();

        /// <summary>
        /// Administrative distance for this routing protocol
        /// Override to set protocol-specific administrative distance
        /// </summary>
        public abstract int AdministrativeDistance { get; }

        /// <summary>
        /// Whether this routing protocol supports ECMP (Equal Cost Multi-Path)
        /// </summary>
        public virtual bool SupportsECMP => false;

        /// <summary>
        /// Maximum number of ECMP paths supported
        /// </summary>
        public virtual int MaxECMPPaths => 1;

        /// <summary>
        /// Create the initial routing protocol state
        /// </summary>
        protected override BaseProtocolState CreateInitialState()
        {
            return new RoutingProtocolState();
        }

        /// <summary>
        /// Get the routing protocol specific state
        /// </summary>
        /// <returns>Routing protocol state</returns>
        public IRoutingProtocolState GetRoutingState()
        {
            return _state as IRoutingProtocolState;
        }

        /// <summary>
        /// Core routing protocol calculation - called when state changes
        /// Implements common routing logic while allowing protocol-specific customization
        /// </summary>
        /// <param name="device">Network device</param>
        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            try
            {
                // Step 1: Collect routing information from neighbors
                var routingInformation = await CollectRoutingInformation(device);

                // Step 2: Run protocol-specific routing algorithm
                var computedRoutes = await ComputeRoutes(device, routingInformation);

                // Step 3: Apply routing policy and filters
                var filteredRoutes = await ApplyRoutingPolicy(device, computedRoutes);

                // Step 4: Install routes in device routing table
                await InstallRoutes(device, filteredRoutes);

                // Step 5: Advertise routes to neighbors
                await AdvertiseRoutes(device, filteredRoutes);

                // Update routing state
                if (_state is RoutingProtocolState routingState)
                {
                    routingState.RouteCount = _routingTable.Count;
                    routingState.LastConvergence = DateTime.Now;
                    routingState.RoutingTableChanged = true;
                }

                LogProtocolEvent($"Routing calculation completed: {_routingTable.Count} routes");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error in routing calculation: {ex.Message}");
                _metrics.RecordError($"Routing calculation failed: {ex.Message}");
            }
        }

        // Abstract methods for protocol-specific implementation

        /// <summary>
        /// Collect routing information from neighbors
        /// Override to implement protocol-specific neighbor communication
        /// </summary>
        /// <param name="device">Network device</param>
        /// <returns>Routing information from neighbors</returns>
        protected abstract Task<Dictionary<string, object>> CollectRoutingInformation(INetworkDevice device);

        /// <summary>
        /// Compute optimal routes using protocol-specific algorithm
        /// Override to implement protocol-specific routing algorithm (SPF, Distance Vector, etc.)
        /// </summary>
        /// <param name="device">Network device</param>
        /// <param name="routingInformation">Information from neighbors</param>
        /// <returns>Computed routes</returns>
        protected abstract Task<List<object>> ComputeRoutes(INetworkDevice device, Dictionary<string, object> routingInformation);

        /// <summary>
        /// Apply routing policy and filters to computed routes
        /// Override to implement protocol-specific policy application
        /// </summary>
        /// <param name="device">Network device</param>
        /// <param name="routes">Computed routes</param>
        /// <returns>Filtered routes</returns>
        protected virtual async Task<List<object>> ApplyRoutingPolicy(INetworkDevice device, List<object> routes)
        {
            // Default implementation applies no filtering
            await Task.CompletedTask;
            return routes;
        }

        /// <summary>
        /// Install routes in the device routing table
        /// Implements common route installation logic
        /// </summary>
        /// <param name="device">Network device</param>
        /// <param name="routes">Routes to install</param>
        protected virtual async Task InstallRoutes(INetworkDevice device, List<object> routes)
        {
            var installedCount = 0;
            var updatedCount = 0;

            foreach (var route in routes)
            {
                var routeKey = GetRouteKey(route);
                if (_routingTable.ContainsKey(routeKey))
                {
                    _routingTable[routeKey] = route;
                    updatedCount++;
                }
                else
                {
                    _routingTable[routeKey] = route;
                    installedCount++;
                }
                _routeLastUpdate[routeKey] = DateTime.Now;
            }

            LogProtocolEvent($"Route installation: {installedCount} new, {updatedCount} updated");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Advertise routes to neighbors
        /// Override to implement protocol-specific route advertisement
        /// </summary>
        /// <param name="device">Network device</param>
        /// <param name="routes">Routes to advertise</param>
        protected abstract Task AdvertiseRoutes(INetworkDevice device, List<object> routes);

        /// <summary>
        /// Get a unique key for a route for routing table management
        /// Override to implement protocol-specific route identification
        /// </summary>
        /// <param name="route">Route object</param>
        /// <returns>Unique route key</returns>
        protected abstract string GetRouteKey(object route);

        /// <summary>
        /// Remove stale routes from the routing table
        /// </summary>
        /// <param name="maxAge">Maximum age for routes in seconds</param>
        protected virtual void RemoveStaleRoutes(int maxAge = 300)
        {
            var now = DateTime.Now;
            var staleRoutes = _routeLastUpdate
                .Where(kvp => (now - kvp.Value).TotalSeconds > maxAge)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var routeKey in staleRoutes)
            {
                _routingTable.Remove(routeKey);
                _routeLastUpdate.Remove(routeKey);
                LogProtocolEvent($"Removed stale route: {routeKey}");
            }

            if (staleRoutes.Any())
            {
                _state.MarkStateChanged();
            }
        }

        /// <summary>
        /// Get the current routing table for this protocol
        /// </summary>
        /// <returns>Dictionary of routes</returns>
        public Dictionary<string, object> GetRoutingTable()
        {
            return new Dictionary<string, object>(_routingTable);
        }

        /// <summary>
        /// Get the number of routes in the routing table
        /// </summary>
        /// <returns>Route count</returns>
        public int GetRouteCount()
        {
            return _routingTable.Count;
        }

        /// <summary>
        /// Check if a specific route exists
        /// </summary>
        /// <param name="destination">Destination network</param>
        /// <returns>True if route exists, false otherwise</returns>
        public bool HasRoute(string destination)
        {
            return _routingTable.Keys.Any(key => key.Contains(destination));
        }

        /// <summary>
        /// Process timer events for routing protocols
        /// Implements common timer functionality for route aging and advertisements
        /// </summary>
        /// <param name="device">Network device</param>
        protected override async Task ProcessTimers(INetworkDevice device)
        {
            // Remove stale routes
            RemoveStaleRoutes();

            // Call protocol-specific timer processing
            await ProcessRoutingTimers(device);
        }

        /// <summary>
        /// Process protocol-specific routing timers
        /// Override to implement hello timers, update timers, etc.
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task ProcessRoutingTimers(INetworkDevice device)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup routing-specific resources on disposal
        /// </summary>
        protected override void OnDispose()
        {
            _routingTable.Clear();
            _routeLastUpdate.Clear();
            base.OnDispose();
        }
    }

    /// <summary>
    /// Concrete implementation of routing protocol state
    /// </summary>
    public class RoutingProtocolState : BaseProtocolState, IRoutingProtocolState
    {
        public int RouteCount { get; set; }
        public int NeighborCount { get; set; }
        public DateTime LastConvergence { get; set; } = DateTime.MinValue;
        public TimeSpan LastConvergenceTime { get; set; } = TimeSpan.Zero;
        public int AdministrativeDistance { get; set; }
        public bool RoutingTableChanged { get; set; }

        public Dictionary<string, object> GetNeighborDetails()
        {
            var details = new Dictionary<string, object>();
            foreach (var neighborId in _neighbors.Keys)
            {
                details[neighborId] = _neighbors[neighborId];
            }
            return details;
        }

        public IEnumerable<object> GetRoutes()
        {
            // This would be populated by the routing protocol
            return new List<object>();
        }

        public void MarkRoutingTableChanged()
        {
            RoutingTableChanged = true;
            MarkStateChanged();
        }

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["RouteCount"] = RouteCount;
            baseData["NeighborCount"] = NeighborCount;
            baseData["LastConvergence"] = LastConvergence;
            baseData["LastConvergenceTime"] = LastConvergenceTime.TotalSeconds;
            baseData["AdministrativeDistance"] = AdministrativeDistance;
            baseData["RoutingTableChanged"] = RoutingTableChanged;
            return baseData;
        }
    }
}
