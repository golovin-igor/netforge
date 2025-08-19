namespace NetForge.Simulation.Protocols.Common
{
    /// <summary>
    /// Interface for protocol state management following the state management pattern
    /// from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public interface IProtocolState
    {
        /// <summary>
        /// When the protocol state was last updated
        /// </summary>
        DateTime LastUpdate { get; }
        
        /// <summary>
        /// Whether the protocol is currently active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Whether the protocol is properly configured
        /// </summary>
        bool IsConfigured { get; }
        
        /// <summary>
        /// Whether the state has changed since last calculation
        /// </summary>
        bool StateChanged { get; }
        
        /// <summary>
        /// Mark that the state has changed and needs recalculation
        /// </summary>
        void MarkStateChanged();
        
        /// <summary>
        /// Get all state data as key-value pairs for monitoring and debugging
        /// </summary>
        /// <returns>Dictionary of state data</returns>
        Dictionary<string, object> GetStateData();
        
        /// <summary>
        /// Get the typed state for specific protocol implementations
        /// </summary>
        /// <typeparam name="T">The specific state type</typeparam>
        /// <returns>Typed state or null if not available</returns>
        T GetTypedState<T>() where T : class;
        
        /// <summary>
        /// Get neighbors that have exceeded their timeout and should be removed
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds (default 180)</param>
        /// <returns>List of stale neighbor identifiers</returns>
        List<string> GetStaleNeighbors(int timeoutSeconds = 180);
        
        /// <summary>
        /// Remove a neighbor from the protocol state
        /// </summary>
        /// <param name="id">Neighbor identifier to remove</param>
        void RemoveNeighbor(string id);
        
        /// <summary>
        /// Update the last activity time for a neighbor
        /// </summary>
        /// <param name="id">Neighbor identifier</param>
        void UpdateNeighborActivity(string id);
    }
}
