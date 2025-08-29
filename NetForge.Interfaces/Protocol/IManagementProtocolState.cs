using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Common.State
{
    /// <summary>
    /// Extended state interface for management protocols (SSH, Telnet, SNMP)
    /// Provides management-specific state information and session metrics
    /// </summary>
    public interface IManagementProtocolState : IProtocolState
    {
        /// <summary>
        /// Number of active management sessions
        /// </summary>
        int ActiveSessions { get; }

        /// <summary>
        /// Maximum number of concurrent sessions allowed
        /// </summary>
        int MaxSessions { get; }

        /// <summary>
        /// Port number the management service is listening on
        /// </summary>
        int ListeningPort { get; }

        /// <summary>
        /// Whether authentication is required for access
        /// </summary>
        bool RequiresAuthentication { get; }

        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        int SessionTimeoutMinutes { get; }

        /// <summary>
        /// Whether the management service is currently listening
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Get information about active sessions
        /// </summary>
        /// <returns>Dictionary mapping session ID to session details</returns>
        Dictionary<string, object> GetActiveSessions();

        /// <summary>
        /// Get connection statistics
        /// </summary>
        /// <returns>Dictionary containing connection metrics</returns>
        Dictionary<string, object> GetConnectionStatistics();
    }
}