namespace NetForge.Simulation.Protocols.Common.State
{
    /// <summary>
    /// Protocol status enumeration for standardized state reporting
    /// </summary>
    public enum ProtocolStatus
    {
        Stopped,
        Starting,
        Active,
        Stopping,
        Error,
        Disabled
    }

    /// <summary>
    /// Base interface for all protocol states in NetForge
    /// Provides standardized state properties for monitoring and debugging
    /// </summary>
    public interface IProtocolState
    {
        /// <summary>
        /// Timestamp of the last state update
        /// </summary>
        DateTime LastUpdate { get; }

        /// <summary>
        /// Whether the protocol is currently active and processing
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Whether the protocol has been properly configured
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Whether the state has changed since last processing
        /// </summary>
        bool StateChanged { get; }

        /// <summary>
        /// Current operational status of the protocol
        /// </summary>
        ProtocolStatus Status { get; }

        /// <summary>
        /// Mark that the state has changed and requires processing
        /// </summary>
        void MarkStateChanged();

        /// <summary>
        /// Get a dictionary representation of the current state for monitoring
        /// </summary>
        /// <returns>Dictionary containing state data</returns>
        Dictionary<string, object> GetStateData();

        /// <summary>
        /// Get the typed state for protocol-specific access
        /// </summary>
        /// <typeparam name="T">Specific state type</typeparam>
        /// <returns>Typed state or null if not available</returns>
        T GetTypedState<T>() where T : class;

        /// <summary>
        /// Get neighbor IDs for protocols that support neighbor relationships
        /// </summary>
        /// <returns>Enumerable of neighbor identifiers</returns>
        IEnumerable<string> GetNeighborIds();

        /// <summary>
        /// Get neighbors that have not been seen within the timeout period
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds (default 180)</param>
        /// <returns>List of stale neighbor IDs</returns>
        List<string> GetStaleNeighbors(int timeoutSeconds = 180);

        /// <summary>
        /// Remove a neighbor from the protocol state
        /// </summary>
        /// <param name="neighborId">ID of neighbor to remove</param>
        void RemoveNeighbor(string neighborId);

        /// <summary>
        /// Update the last seen time for a neighbor
        /// </summary>
        /// <param name="neighborId">ID of neighbor to update</param>
        void UpdateNeighborActivity(string neighborId);
    }
}