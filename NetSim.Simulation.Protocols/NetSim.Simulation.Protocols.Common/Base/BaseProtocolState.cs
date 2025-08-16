namespace NetSim.Simulation.Protocols.Common
{
    /// <summary>
    /// Base implementation of protocol state management following the pattern 
    /// from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public abstract class BaseProtocolState : IProtocolState
    {
        // Core state tracking from PROTOCOL_STATE_MANAGEMENT.md
        public bool StateChanged { get; set; } = true;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool IsConfigured { get; set; } = false;
        
        // Neighbor management - generic storage for different neighbor types
        protected readonly Dictionary<string, object> _neighbors = new();
        protected readonly Dictionary<string, DateTime> _neighborLastSeen = new();
        
        /// <summary>
        /// Mark that the state has changed and needs recalculation
        /// </summary>
        public virtual void MarkStateChanged() => StateChanged = true;
        
        /// <summary>
        /// Get or create a neighbor of the specified type
        /// This provides type-safe neighbor management while maintaining flexibility
        /// </summary>
        /// <typeparam name="TNeighbor">Type of neighbor to get or create</typeparam>
        /// <param name="id">Unique identifier for the neighbor</param>
        /// <param name="factory">Factory function to create new neighbor if needed</param>
        /// <returns>The neighbor instance</returns>
        public virtual TNeighbor GetOrCreateNeighbor<TNeighbor>(string id, Func<TNeighbor> factory) 
            where TNeighbor : class
        {
            if (!_neighbors.ContainsKey(id))
            {
                _neighbors[id] = factory();
                _neighborLastSeen[id] = DateTime.Now;
                MarkStateChanged();
            }
            return (TNeighbor)_neighbors[id];
        }
        
        /// <summary>
        /// Get a neighbor by ID and type
        /// </summary>
        /// <typeparam name="TNeighbor">Type of neighbor to retrieve</typeparam>
        /// <param name="id">Neighbor identifier</param>
        /// <returns>The neighbor instance or null if not found</returns>
        public virtual TNeighbor GetNeighbor<TNeighbor>(string id) where TNeighbor : class
        {
            return _neighbors.TryGetValue(id, out var neighbor) ? neighbor as TNeighbor : null;
        }
        
        /// <summary>
        /// Get all neighbors of a specific type
        /// </summary>
        /// <typeparam name="TNeighbor">Type of neighbors to retrieve</typeparam>
        /// <returns>Dictionary of neighbors by ID</returns>
        public virtual Dictionary<string, TNeighbor> GetNeighbors<TNeighbor>() where TNeighbor : class
        {
            return _neighbors
                .Where(kvp => kvp.Value is TNeighbor)
                .ToDictionary(kvp => kvp.Key, kvp => (TNeighbor)kvp.Value);
        }
        
        /// <summary>
        /// Remove a neighbor from the protocol state
        /// </summary>
        /// <param name="id">Neighbor identifier to remove</param>
        public virtual void RemoveNeighbor(string id)
        {
            if (_neighbors.Remove(id))
            {
                _neighborLastSeen.Remove(id);
                MarkStateChanged();
            }
        }
        
        /// <summary>
        /// Get neighbors that have exceeded their timeout and should be removed
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds (default 180)</param>
        /// <returns>List of stale neighbor identifiers</returns>
        public virtual List<string> GetStaleNeighbors(int timeoutSeconds = 180)
        {
            var staleNeighbors = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in _neighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > timeoutSeconds)
                {
                    staleNeighbors.Add(kvp.Key);
                }
            }
            
            return staleNeighbors;
        }
        
        /// <summary>
        /// Update the last activity time for a neighbor
        /// </summary>
        /// <param name="id">Neighbor identifier</param>
        public virtual void UpdateNeighborActivity(string id)
        {
            if (_neighbors.ContainsKey(id))
            {
                _neighborLastSeen[id] = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Get all state data as key-value pairs for monitoring and debugging
        /// </summary>
        /// <returns>Dictionary of state data</returns>
        public virtual Dictionary<string, object> GetStateData()
        {
            return new Dictionary<string, object>
            {
                ["LastUpdate"] = LastUpdate,
                ["IsActive"] = IsActive,
                ["IsConfigured"] = IsConfigured,
                ["StateChanged"] = StateChanged,
                ["NeighborCount"] = _neighbors.Count,
                ["ActiveNeighbors"] = _neighbors.Keys.ToList()
            };
        }
        
        /// <summary>
        /// Get the typed state for specific protocol implementations
        /// </summary>
        /// <typeparam name="T">The specific state type</typeparam>
        /// <returns>Typed state or null if not available</returns>
        public virtual T GetTypedState<T>() where T : class => this as T;
        
        /// <summary>
        /// Get the count of neighbors
        /// </summary>
        public int NeighborCount => _neighbors.Count;
        
        /// <summary>
        /// Check if a neighbor exists
        /// </summary>
        /// <param name="id">Neighbor identifier</param>
        /// <returns>True if neighbor exists, false otherwise</returns>
        public bool HasNeighbor(string id) => _neighbors.ContainsKey(id);
        
        /// <summary>
        /// Clear all neighbors (useful for protocol resets)
        /// </summary>
        protected virtual void ClearNeighbors()
        {
            if (_neighbors.Count > 0)
            {
                _neighbors.Clear();
                _neighborLastSeen.Clear();
                MarkStateChanged();
            }
        }
    }
}