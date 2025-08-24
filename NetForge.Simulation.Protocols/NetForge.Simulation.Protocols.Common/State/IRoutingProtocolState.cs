namespace NetForge.Simulation.Protocols.Common.State
{
    /// <summary>
    /// Extended state interface for routing protocols (OSPF, BGP, RIP, EIGRP, etc.)
    /// Provides routing-specific state information and metrics
    /// </summary>
    public interface IRoutingProtocolState : IProtocolState
    {
        /// <summary>
        /// Number of routes currently in the routing table
        /// </summary>
        int RouteCount { get; }

        /// <summary>
        /// Number of active neighbors
        /// </summary>
        int NeighborCount { get; }

        /// <summary>
        /// Timestamp of the last routing convergence event
        /// </summary>
        DateTime LastConvergence { get; }

        /// <summary>
        /// Time taken for the last convergence event
        /// </summary>
        TimeSpan LastConvergenceTime { get; }

        /// <summary>
        /// Administrative distance used by this routing protocol
        /// </summary>
        int AdministrativeDistance { get; }

        /// <summary>
        /// Whether the routing table has been modified since last update
        /// </summary>
        bool RoutingTableChanged { get; }

        /// <summary>
        /// Get detailed neighbor information
        /// </summary>
        /// <returns>Dictionary mapping neighbor ID to neighbor details</returns>
        Dictionary<string, object> GetNeighborDetails();

        /// <summary>
        /// Get routing table entries managed by this protocol
        /// </summary>
        /// <returns>Enumerable of route entries</returns>
        IEnumerable<object> GetRoutes();

        /// <summary>
        /// Mark that the routing table has changed
        /// </summary>
        void MarkRoutingTableChanged();
    }
}