using NetSim.Simulation.Protocols.Common;

namespace NetSim.Simulation.Protocols.SSH
{
    /// <summary>
    /// State tracking for the SSH protocol
    /// </summary>
    public class SshState : BaseProtocolState
    {
        /// <summary>
        /// Number of active SSH sessions
        /// </summary>
        public int ActiveSessions { get; set; } = 0;
        
        /// <summary>
        /// Total number of connections since startup
        /// </summary>
        public long TotalConnections { get; set; } = 0;
        
        /// <summary>
        /// Total number of successful authentications
        /// </summary>
        public long SuccessfulAuthentications { get; set; } = 0;
        
        /// <summary>
        /// Total number of failed authentication attempts
        /// </summary>
        public long FailedAuthentications { get; set; } = 0;
        
        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Port the SSH server is listening on
        /// </summary>
        public int ListeningPort { get; set; } = 22;
        
        /// <summary>
        /// SSH protocol version in use
        /// </summary>
        public int ProtocolVersion { get; set; } = 2;
        
        /// <summary>
        /// Whether the SSH server is currently running
        /// </summary>
        public bool IsServerRunning { get; set; } = false;
        
        /// <summary>
        /// Host key fingerprint
        /// </summary>
        public string HostKeyFingerprint { get; set; } = "";
        
        /// <summary>
        /// Session statistics
        /// </summary>
        public Dictionary<string, object> SessionStatistics { get; set; } = new();
        
        /// <summary>
        /// Security events (failed logins, etc.)
        /// </summary>
        public List<SshSecurityEvent> SecurityEvents { get; set; } = new();
        
        /// <summary>
        /// Current encryption algorithms in use by active sessions
        /// </summary>
        public Dictionary<string, string> ActiveEncryption { get; set; } = new();
        
        /// <summary>
        /// Get all state data including SSH-specific information
        /// </summary>
        /// <returns>Dictionary of state data</returns>
        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            
            baseData["ActiveSessions"] = ActiveSessions;
            baseData["TotalConnections"] = TotalConnections;
            baseData["SuccessfulAuthentications"] = SuccessfulAuthentications;
            baseData["FailedAuthentications"] = FailedAuthentications;
            baseData["LastActivity"] = LastActivity;
            baseData["ListeningPort"] = ListeningPort;
            baseData["ProtocolVersion"] = ProtocolVersion;
            baseData["IsServerRunning"] = IsServerRunning;
            baseData["HostKeyFingerprint"] = HostKeyFingerprint;
            baseData["SessionStatistics"] = SessionStatistics;
            baseData["SecurityEvents"] = SecurityEvents;
            baseData["ActiveEncryption"] = ActiveEncryption;
            
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
        /// Record successful authentication
        /// </summary>
        /// <param name="username">Username that authenticated</param>
        /// <param name="clientEndpoint">Client endpoint</param>
        public void RecordSuccessfulAuthentication(string username, string clientEndpoint)
        {
            SuccessfulAuthentications++;
            LastActivity = DateTime.Now;
            
            SecurityEvents.Add(new SshSecurityEvent
            {
                Timestamp = DateTime.Now,
                EventType = "Successful Authentication",
                Username = username,
                ClientEndpoint = clientEndpoint,
                Details = "User successfully authenticated"
            });
            
            // Keep only last 100 security events
            if (SecurityEvents.Count > 100)
            {
                SecurityEvents.RemoveAt(0);
            }
            
            MarkStateChanged();
        }
        
        /// <summary>
        /// Record failed authentication
        /// </summary>
        /// <param name="username">Username that failed</param>
        /// <param name="clientEndpoint">Client endpoint</param>
        /// <param name="reason">Failure reason</param>
        public void RecordFailedAuthentication(string username, string clientEndpoint, string reason)
        {
            FailedAuthentications++;
            LastActivity = DateTime.Now;
            
            SecurityEvents.Add(new SshSecurityEvent
            {
                Timestamp = DateTime.Now,
                EventType = "Failed Authentication",
                Username = username,
                ClientEndpoint = clientEndpoint,
                Details = reason
            });
            
            // Keep only last 100 security events
            if (SecurityEvents.Count > 100)
            {
                SecurityEvents.RemoveAt(0);
            }
            
            MarkStateChanged();
        }
        
        /// <summary>
        /// Update server status
        /// </summary>
        /// <param name="isRunning">Whether server is running</param>
        /// <param name="port">Port server is listening on</param>
        /// <param name="version">SSH protocol version</param>
        public void UpdateServerStatus(bool isRunning, int port = 22, int version = 2)
        {
            IsServerRunning = isRunning;
            ListeningPort = port;
            ProtocolVersion = version;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Set host key fingerprint
        /// </summary>
        /// <param name="fingerprint">Host key fingerprint</param>
        public void SetHostKeyFingerprint(string fingerprint)
        {
            HostKeyFingerprint = fingerprint;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Update encryption information for a session
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="encryption">Encryption algorithm</param>
        public void UpdateSessionEncryption(string sessionId, string encryption)
        {
            ActiveEncryption[sessionId] = encryption;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Remove session encryption information
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        public void RemoveSessionEncryption(string sessionId)
        {
            ActiveEncryption.Remove(sessionId);
            MarkStateChanged();
        }
    }
    
    /// <summary>
    /// SSH security event for tracking authentication and security issues
    /// </summary>
    public class SshSecurityEvent
    {
        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Type of security event
        /// </summary>
        public string EventType { get; set; } = "";
        
        /// <summary>
        /// Username involved in the event
        /// </summary>
        public string Username { get; set; } = "";
        
        /// <summary>
        /// Client endpoint (IP:Port)
        /// </summary>
        public string ClientEndpoint { get; set; } = "";
        
        /// <summary>
        /// Event details
        /// </summary>
        public string Details { get; set; } = "";
        
        /// <summary>
        /// String representation of the event
        /// </summary>
        /// <returns>Formatted event string</returns>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {EventType}: {Username}@{ClientEndpoint} - {Details}";
        }
    }
}