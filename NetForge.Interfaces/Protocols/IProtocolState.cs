namespace NetForge.Interfaces
{
    /// <summary>
    /// Interface for protocol state management
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
        /// Get a dictionary representation of the state data
        /// </summary>
        /// <returns>Dictionary containing state information</returns>
        Dictionary<string, object> GetStateData();
    }
}