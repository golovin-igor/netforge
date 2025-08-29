using System.Collections.Concurrent;

namespace NetForge.Simulation.Protocols.SSH
{
    /// <summary>
    /// Manages SSH sessions for the SSH server
    /// </summary>
    public class SshSessionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, SshSession> _sessions = new();
        private readonly object _lockObject = new();
        private long _totalConnections;
        private bool _isDisposed;

        /// <summary>
        /// Add a new session
        /// </summary>
        /// <param name="session">Session to add</param>
        public void AddSession(SshSession session)
        {
            if (_isDisposed || session == null)
                return;
            
            lock (_lockObject)
            {
                _sessions.TryAdd(session.SessionId, session);
                Interlocked.Increment(ref _totalConnections);
            }
        }
        
        /// <summary>
        /// Remove a session
        /// </summary>
        /// <param name="session">Session to remove</param>
        public void RemoveSession(SshSession session)
        {
            if (_isDisposed || session == null)
                return;
            
            lock (_lockObject)
            {
                _sessions.TryRemove(session.SessionId, out _);
            }
        }
        
        /// <summary>
        /// Get all active sessions
        /// </summary>
        /// <returns>List of active sessions</returns>
        public List<SshSession> GetActiveSessions()
        {
            if (_isDisposed)
                return new List<SshSession>();
            
            lock (_lockObject)
            {
                return _sessions.Values.Where(s => s.IsConnected).ToList();
            }
        }
        
        /// <summary>
        /// Get a session by ID
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Session or null if not found</returns>
        public SshSession? GetSession(string sessionId)
        {
            if (_isDisposed)
                return null;
            
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }
        
        /// <summary>
        /// Update all sessions and clean up disconnected ones
        /// </summary>
        public async Task UpdateSessions()
        {
            if (_isDisposed)
                return;
            
            var sessionsToRemove = new List<SshSession>();
            
            lock (_lockObject)
            {
                foreach (var session in _sessions.Values)
                {
                    if (!session.IsConnected || session.IsTimedOut())
                    {
                        sessionsToRemove.Add(session);
                    }
                }
            }
            
            // Remove disconnected or timed out sessions
            foreach (var session in sessionsToRemove)
            {
                try
                {
                    if (session.IsTimedOut())
                    {
                        await session.SendMessage("Session timed out. Connection closed.\r\n");
                    }
                    
                    session.Disconnect();
                    RemoveSession(session);
                    session.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        
        /// <summary>
        /// Close all active sessions
        /// </summary>
        public async Task CloseAllSessionsAsync()
        {
            var activeSessions = GetActiveSessions();
            
            foreach (var session in activeSessions)
            {
                try
                {
                    await session.SendMessage("Server shutting down. Connection closed.\r\n");
                    session.Disconnect();
                    session.Dispose();
                }
                catch
                {
                    // Ignore errors when closing sessions
                }
            }
            
            lock (_lockObject)
            {
                _sessions.Clear();
            }
        }
        
        /// <summary>
        /// Get total connection count since server start
        /// </summary>
        /// <returns>Total connection count</returns>
        public long GetTotalConnectionCount()
        {
            return Interlocked.Read(ref _totalConnections);
        }
        
        /// <summary>
        /// Get session statistics
        /// </summary>
        /// <returns>Dictionary of session statistics</returns>
        public Dictionary<string, object> GetSessionStatistics()
        {
            var activeSessions = GetActiveSessions();
            
            var statistics = new Dictionary<string, object>
            {
                ["ActiveSessionCount"] = activeSessions.Count,
                ["TotalConnections"] = GetTotalConnectionCount(),
                ["Sessions"] = activeSessions.Select(s => s.GetStatistics()).ToList()
            };
            
            // Add authentication statistics
            var authenticatedSessions = activeSessions.Where(s => s.IsAuthenticated).Count();
            var unauthenticatedSessions = activeSessions.Count - authenticatedSessions;
            
            statistics["AuthenticatedSessions"] = authenticatedSessions;
            statistics["UnauthenticatedSessions"] = unauthenticatedSessions;
            
            // Add connection time statistics
            if (activeSessions.Any())
            {
                var connectionTimes = activeSessions.Select(s => (DateTime.Now - s.ConnectionTime).TotalMinutes);
                statistics["AverageConnectionTimeMinutes"] = connectionTimes.Average();
                statistics["MaxConnectionTimeMinutes"] = connectionTimes.Max();
                statistics["MinConnectionTimeMinutes"] = connectionTimes.Min();
            }
            else
            {
                statistics["AverageConnectionTimeMinutes"] = 0;
                statistics["MaxConnectionTimeMinutes"] = 0;
                statistics["MinConnectionTimeMinutes"] = 0;
            }
            
            // Add encryption statistics
            var encryptionAlgorithms = activeSessions
                .GroupBy(s => s.EncryptionAlgorithm)
                .ToDictionary(g => g.Key, g => g.Count());
            statistics["EncryptionAlgorithms"] = encryptionAlgorithms;
            
            return statistics;
        }
        
        /// <summary>
        /// Get session by client endpoint
        /// </summary>
        /// <param name="clientEndpoint">Client endpoint</param>
        /// <returns>Session or null if not found</returns>
        public SshSession? GetSessionByEndpoint(string clientEndpoint)
        {
            if (_isDisposed)
                return null;
            
            lock (_lockObject)
            {
                return _sessions.Values.FirstOrDefault(s => 
                    s.ClientEndpoint.Equals(clientEndpoint, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        /// <summary>
        /// Get sessions by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of sessions for the user</returns>
        public List<SshSession> GetSessionsByUsername(string username)
        {
            if (_isDisposed)
                return new List<SshSession>();
            
            lock (_lockObject)
            {
                return _sessions.Values
                    .Where(s => s.IsAuthenticated && 
                               string.Equals(s.Username, username, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
        
        /// <summary>
        /// Disconnect sessions for a specific user
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Number of sessions disconnected</returns>
        public async Task<int> DisconnectUserSessionsAsync(string username)
        {
            var userSessions = GetSessionsByUsername(username);
            
            foreach (var session in userSessions)
            {
                try
                {
                    await session.SendMessage("Session terminated by administrator.\r\n");
                    session.Disconnect();
                }
                catch
                {
                    // Ignore errors when disconnecting
                }
            }
            
            return userSessions.Count;
        }
        
        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _isDisposed = true;
            
            _ = CloseAllSessionsAsync();
        }
    }
}
