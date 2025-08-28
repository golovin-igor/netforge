using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Common.Interfaces
{
    /// <summary>
    /// Unified protocol service interface for comprehensive protocol management
    /// Provides unified access to protocol instances, state, and configuration
    /// Consolidates basic and enhanced protocol service capabilities
    /// </summary>
    public interface IProtocolService
    {
        // Basic protocol access
        /// <summary>
        /// Get a protocol instance by type
        /// </summary>
        /// <typeparam name="T">Protocol type to retrieve</typeparam>
        /// <returns>Protocol instance or null if not available</returns>
        T GetProtocol<T>() where T : class;

        /// <summary>
        /// Get a protocol instance by protocol type enumeration
        /// </summary>
        /// <param name="type">Protocol type to retrieve</param>
        /// <returns>Protocol instance or null if not available</returns>
        IDeviceProtocol GetProtocol(ProtocolType type);

        /// <summary>
        /// Get all registered protocols
        /// </summary>
        /// <returns>Enumerable of all protocol instances</returns>
        IEnumerable<IDeviceProtocol> GetAllProtocols();

        /// <summary>
        /// Get protocols that support a specific vendor
        /// </summary>
        /// <param name="vendorName">Vendor name to filter by</param>
        /// <returns>Enumerable of protocols supporting the vendor</returns>
        IEnumerable<IDeviceProtocol> GetProtocolsForVendor(string vendorName);

        // State access
        /// <summary>
        /// Get the state of a specific protocol
        /// </summary>
        /// <typeparam name="TState">State type to retrieve</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Typed protocol state or null if not available</returns>
        TState GetProtocolState<TState>(ProtocolType type) where TState : class;

        /// <summary>
        /// Get the state of a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol state or null if not available</returns>
        IProtocolState GetProtocolState(ProtocolType type);

        /// <summary>
        /// Get states of all active protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to state</returns>
        Dictionary<ProtocolType, object> GetAllProtocolStates();

        // Protocol lifecycle management
        /// <summary>
        /// Start a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to start</param>
        /// <returns>True if successfully started, false otherwise</returns>
        Task<bool> StartProtocol(ProtocolType type);

        /// <summary>
        /// Stop a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to stop</param>
        /// <returns>True if successfully stopped, false otherwise</returns>
        Task<bool> StopProtocol(ProtocolType type);

        /// <summary>
        /// Restart a specific protocol
        /// </summary>
        /// <param name="type">Protocol type to restart</param>
        /// <returns>True if successfully restarted, false otherwise</returns>
        Task<bool> RestartProtocol(ProtocolType type);

        /// <summary>
        /// Check if a protocol is currently active
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if active, false otherwise</returns>
        bool IsProtocolActive(ProtocolType type);

        /// <summary>
        /// Check if a protocol is registered
        /// </summary>
        /// <param name="type">Protocol type to check</param>
        /// <returns>True if registered, false otherwise</returns>
        bool IsProtocolRegistered(ProtocolType type);

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

        // Configuration management
        /// <summary>
        /// Validate a protocol configuration
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateProtocolConfiguration(ProtocolType type, object configuration);

        /// <summary>
        /// Apply configuration to a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>True if successfully applied, false otherwise</returns>
        Task<bool> ApplyProtocolConfiguration(ProtocolType type, object configuration);

        // Dependency management
        /// <summary>
        /// Get protocols that the specified protocol depends on
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of dependency protocol types</returns>
        IEnumerable<ProtocolType> GetProtocolDependencies(ProtocolType type);

        /// <summary>
        /// Get protocols that conflict with the specified protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of conflicting protocol types</returns>
        IEnumerable<ProtocolType> GetProtocolConflicts(ProtocolType type);

        /// <summary>
        /// Validate that all dependencies for a protocol are satisfied
        /// </summary>
        /// <param name="type">Protocol type to validate</param>
        /// <returns>True if all dependencies are satisfied, false otherwise</returns>
        bool ValidateProtocolDependencies(ProtocolType type);

        /// <summary>
        /// Check if two protocols can coexist
        /// </summary>
        /// <param name="type1">First protocol type</param>
        /// <param name="type2">Second protocol type</param>
        /// <returns>True if protocols can coexist, false otherwise</returns>
        bool CanProtocolsCoexist(ProtocolType type1, ProtocolType type2);

        // Metrics and monitoring
        /// <summary>
        /// Get performance metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Protocol metrics or null if not available</returns>
        object GetProtocolMetrics(ProtocolType type);

        /// <summary>
        /// Get performance metrics for all protocols
        /// </summary>
        /// <returns>Dictionary mapping protocol type to metrics</returns>
        Dictionary<ProtocolType, object> GetAllProtocolMetrics();

        /// <summary>
        /// Reset metrics for a specific protocol
        /// </summary>
        /// <param name="type">Protocol type</param>
        void ResetProtocolMetrics(ProtocolType type);

        /// <summary>
        /// Reset metrics for all protocols
        /// </summary>
        void ResetAllMetrics();

        // Service management
        /// <summary>
        /// Get service health status
        /// </summary>
        /// <returns>Dictionary containing service health information</returns>
        Dictionary<string, object> GetServiceHealth();

        /// <summary>
        /// Get summary information about all managed protocols
        /// </summary>
        /// <returns>Dictionary containing protocol summary</returns>
        Dictionary<string, object> GetProtocolSummary();
    }
}