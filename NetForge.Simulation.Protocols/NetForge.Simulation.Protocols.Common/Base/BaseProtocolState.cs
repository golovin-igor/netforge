using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Common.Base
{
    /// <summary>
    /// Base implementation of protocol state following the standardized interface
    /// Provides common state management functionality for all protocols
    /// </summary>
    public abstract class BaseProtocolState : IProtocolState
    {
        // Core state tracking from PROTOCOL_STATE_MANAGEMENT.md
        public bool StateChanged { get; set; } = true;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool IsConfigured { get; set; } = false;

        // Neighbor management
        protected readonly Dictionary<string, object> _neighbors = new();
        protected readonly Dictionary<string, DateTime> _neighborLastSeen = new();

        /// <summary>
        /// Mark that the state has changed and requires processing
        /// </summary>
        public virtual void MarkStateChanged() 
        {
            StateChanged = true;
        }

        /// <summary>
        /// Get or create a neighbor of the specified type
        /// </summary>
        /// <typeparam name="TNeighbor">Type of neighbor to create</typeparam>
        /// <param name="id">Neighbor identifier</param>
        /// <param name="factory">Factory function to create new neighbor</param>
        /// <returns>Existing or new neighbor instance</returns>
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
        /// Get an existing neighbor by ID
        /// </summary>
        /// <typeparam name="TNeighbor">Type of neighbor to retrieve</typeparam>
        /// <param name="id">Neighbor identifier</param>
        /// <returns>Neighbor instance or null if not found</returns>
        public virtual TNeighbor GetNeighbor<TNeighbor>(string id) where TNeighbor : class
        {
            return _neighbors.TryGetValue(id, out var neighbor) ? neighbor as TNeighbor : null;
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
        /// Get neighbors that have not been seen within the timeout period
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds (default 180)</param>
        /// <returns>List of stale neighbor IDs</returns>
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
        /// Update the last seen time for a neighbor
        /// </summary>
        /// <param name="id">ID of neighbor to update</param>
        public virtual void UpdateNeighborActivity(string id)
        {
            _neighborLastSeen[id] = DateTime.Now;
        }

        /// <summary>
        /// Get neighbor IDs for protocols that support neighbor relationships
        /// </summary>
        /// <returns>Enumerable of neighbor identifiers</returns>
        public virtual IEnumerable<string> GetNeighborIds()
        {
            return _neighbors.Keys;
        }

        /// <summary>
        /// Get a dictionary representation of the current state for monitoring
        /// </summary>
        /// <returns>Dictionary containing state data</returns>
        public virtual Dictionary<string, object> GetStateData()
        {
            return new Dictionary<string, object>
            {
                ["LastUpdate"] = LastUpdate,
                ["IsActive"] = IsActive,
                ["IsConfigured"] = IsConfigured,
                ["StateChanged"] = StateChanged,
                ["NeighborCount"] = _neighbors.Count,
                ["Neighbors"] = _neighbors.Keys.ToList()
            };
        }

        /// <summary>
        /// Get the typed state for protocol-specific access
        /// </summary>
        /// <typeparam name="T">Specific state type</typeparam>
        /// <returns>Typed state or null if not available</returns>
        public virtual T GetTypedState<T>() where T : class => this as T;

        /// <summary>
        /// Set the protocol as active and configured
        /// </summary>
        public virtual void Activate()
        {
            IsActive = true;
            IsConfigured = true;
            MarkStateChanged();
        }

        /// <summary>
        /// Set the protocol as inactive
        /// </summary>
        public virtual void Deactivate()
        {
            IsActive = false;
            MarkStateChanged();
        }
    }
}