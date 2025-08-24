using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// State tracking for the Telnet protocol
    /// </summary>
    public class TelnetState : BaseProtocolState
    {
        /// <summary>
        /// Number of active Telnet sessions
        /// </summary>
        public int ActiveSessions { get; set; } = 0;
        
        /// <summary>
        /// Total number of connections since startup
        /// </summary>
        public long TotalConnections { get; set; } = 0;
        
        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Port the Telnet server is listening on
        /// </summary>
        public int ListeningPort { get; set; } = 23;
        
        /// <summary>
        /// Whether the Telnet server is currently running
        /// </summary>
        public bool IsServerRunning { get; set; } = false;
        
        /// <summary>
        /// Session statistics
        /// </summary>
        public Dictionary<string, object> SessionStatistics { get; set; } = new();
        
        /// <summary>
        /// Get all state data including Telnet-specific information
        /// </summary>
        /// <returns>Dictionary of state data</returns>
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            
            baseData["ActiveSessions"] = ActiveSessions;
            baseData["TotalConnections"] = TotalConnections;
            baseData["LastActivity"] = LastActivity;
            baseData["ListeningPort"] = ListeningPort;
            baseData["IsServerRunning"] = IsServerRunning;
            baseData["SessionStatistics"] = SessionStatistics;
            
            return baseData;
        }
        
        /// <summary>
        /// Update session count and activity
        /// </summary>
        /// <param name="sessionCount">Current active session count</param>
        public void UpdateSessionActivity(int sessionCount)
        {
            ActiveSessions = sessionCount;
            LastActivity = DateTime.Now;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Record a new connection
        /// </summary>
        public void RecordNewConnection()
        {
            TotalConnections++;
            LastActivity = DateTime.Now;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Update server status
        /// </summary>
        /// <param name="isRunning">Whether server is running</param>
        /// <param name="port">Port server is listening on</param>
        public void UpdateServerStatus(bool isRunning, int port = 23)
        {
            IsServerRunning = isRunning;
            ListeningPort = port;
            MarkStateChanged();
        }
    }
}
