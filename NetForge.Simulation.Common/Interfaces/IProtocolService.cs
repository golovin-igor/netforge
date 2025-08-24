namespace NetForge.Simulation.Common.Interfaces
{
    /// <summary>
    /// Service interface for CLI handlers to access protocol state and information
    /// This provides the bridge between CLI handlers and protocols via IoC/DI
    /// </summary>
    public interface IProtocolService
    {
        /// <summary>
        /// Get a protocol instance by its type
        /// </summary>
        /// <typeparam name="T">The specific protocol type</typeparam>
        /// <returns>Protocol instance or null if not available</returns>
        T GetProtocol<T>() where T : class, IDeviceProtocol;

        /// <summary>
        /// Get a protocol instance by protocol type enum
        /// </summary>
        /// <param name="type">Protocol type to retrieve</param>
        /// <returns>Protocol instance or null if not available</returns>
        IDeviceProtocol GetProtocol(ProtocolType type);

        /// <summary>
        /// Get the typed state of a specific protocol
        /// </summary>
        /// <typeparam name="TState">The specific state type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Typed protocol state or null if not available</returns>
        TState GetProtocolState<TState>(ProtocolType type) where TState : class;

        /// <summary>
        /// Get all registered protocol instances
        /// </summary>
        /// <returns>Enumerable of all protocols</returns>
        IEnumerable<IDeviceProtocol> GetAllProtocols();

        /// <summary>
        /// Check if a specific protocol type is active on the device
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if active, false otherwise</returns>
        bool IsProtocolActive(ProtocolType type);

        /// <summary>
        /// Get protocol configuration for a specific protocol type
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol configuration or null if not available</returns>
        object GetProtocolConfiguration(ProtocolType type);

        /// <summary>
        /// Get all active protocol types on this device
        /// </summary>
        /// <returns>Enumerable of active protocol types</returns>
        IEnumerable<ProtocolType> GetActiveProtocolTypes();
    }
}
