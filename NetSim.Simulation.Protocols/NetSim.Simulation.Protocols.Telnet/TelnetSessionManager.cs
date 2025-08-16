using System.Collections.Concurrent;

namespace NetSim.Simulation.Protocols.Telnet
{
    /// <summary>
    /// Manages multiple Telnet sessions for a device
    /// </summary>
    public class TelnetSessionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, TelnetSession> _sessions = new();
        private readonly object _statsLock = new();
        private long _totalConnections = 0;
        private DateTime _lastActivity = DateTime.MinValue;
        private bool _disposed = false;
        
        /// <summary>
        /// Maximum number of concurrent sessions
        /// </summary>
        public int MaxSessions { get; set; } = 5;
        
        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;
        
        /// <summary>
        /// Event fired when a new session is created
        /// </summary>
        public event EventHandler<TelnetSessionEventArgs>? SessionCreated;
        
        /// <summary>
        /// Event fired when a session is disconnected
        /// </summary>
        public event EventHandler<TelnetSessionEventArgs>? SessionDisconnected;
        
        /// <summary>
        /// Add a new session to the manager
        /// </summary>
        /// <param name="session">Session to add</param>
        /// <returns>True if added successfully, false if max sessions reached</returns>
        public bool AddSession(TelnetSession session)
        {
            if (_disposed || session == null)
                return false;
                
            if (_sessions.Count >= MaxSessions)
            {
                return false; // Max sessions reached
            }
            
            session.TimeoutMinutes = SessionTimeoutMinutes;
            session.SessionDisconnected += OnSessionDisconnected;
            
            if (_sessions.TryAdd(session.SessionId, session))
            {
                lock (_statsLock)
                {
                    _totalConnections++;
                    _lastActivity = DateTime.Now;
                }
                
                OnSessionCreated(session);
                
                // Start processing the session in the background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await session.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in Telnet session {session.SessionId}: {ex.Message}");
                    }
                });
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Remove a session from the manager
        /// </summary>
        /// <param name="sessionId">Session ID to remove</param>
        /// <returns>True if removed successfully</returns>
        public bool RemoveSession(string sessionId)
        {
            if (_sessions.TryRemove(sessionId, out var session))
            {
                session.Disconnect();
                session.Dispose();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get a session by ID
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Session or null if not found</returns>
        public TelnetSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }
        
        /// <summary>
        /// Get all active sessions
        /// </summary>
        /// <returns>Collection of active sessions</returns>
        public ICollection<TelnetSession> GetActiveSessions()
        {
            return _sessions.Values.Where(s => s.IsActive).ToList();
        }
        
        /// <summary>
        /// Get session statistics
        /// </summary>
        /// <returns>Session statistics</returns>
        public Dictionary<string, object> GetSessionStatistics()
        {
            lock (_statsLock)
            {
                var activeSessions = GetActiveSessions();
                
                return new Dictionary<string, object>
                {
                    ["ActiveSessions"] = activeSessions.Count,
                    ["TotalConnections"] = _totalConnections,
                    ["LastActivity"] = _lastActivity,
                    ["MaxSessions"] = MaxSessions,
                    ["SessionTimeoutMinutes"] = SessionTimeoutMinutes,
                    ["AuthenticatedSessions"] = activeSessions.Count(s => s.IsAuthenticated),
                    ["SessionDetails"] = activeSessions.Select(s => new
                    {
                        SessionId = s.SessionId,
                        ClientEndpoint = s.ClientEndpoint?.ToString() ?? "Unknown",
                        CreatedAt = s.CreatedAt,
                        LastActivity = s.LastActivity,
                        IsAuthenticated = s.IsAuthenticated,
                        Username = s.Username,
                        CurrentMode = s.CurrentMode.ToString()
                    }).ToList()
                };
            }
        }
        
        /// <summary>
        /// Update sessions (cleanup timed out sessions)
        /// </summary>
        /// <returns>Task representing the update operation</returns>
        public async Task UpdateSessions()
        {
            var sessionsToRemove = new List<string>();
            
            foreach (var session in _sessions.Values)
            {
                if (!session.IsActive || session.IsTimedOut())
                {
                    sessionsToRemove.Add(session.SessionId);
                }
            }
            
            foreach (var sessionId in sessionsToRemove)
            {
                RemoveSession(sessionId);
            }
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Get total connection count
        /// </summary>
        /// <returns>Total connections since startup</returns>
        public long GetTotalConnectionCount()
        {
            lock (_statsLock)
            {
                return _totalConnections;
            }
        }
        
        /// <summary>
        /// Get last activity timestamp
        /// </summary>
        /// <returns>Last activity time</returns>
        public DateTime GetLastActivity()
        {
            lock (_statsLock)
            {
                return _lastActivity;
            }
        }
        
        /// <summary>
        /// Disconnect all sessions
        /// </summary>
        public void DisconnectAllSessions()
        {
            foreach (var session in _sessions.Values)
            {
                session.Disconnect();
            }
            
            _sessions.Clear();
        }
        
        /// <summary>
        /// Check if we can accept more sessions
        /// </summary>
        /// <returns>True if more sessions can be accepted</returns>
        public bool CanAcceptNewSession()
        {
            return !_disposed && _sessions.Count < MaxSessions;
        }
        
        private void OnSessionCreated(TelnetSession session)
        {
            SessionCreated?.Invoke(this, new TelnetSessionEventArgs(session));
        }
        
        private void OnSessionDisconnected(object? sender, TelnetSessionEventArgs e)
        {
            // Remove the session from our collection
            _sessions.TryRemove(e.Session.SessionId, out _);
            
            // Fire the event for listeners
            SessionDisconnected?.Invoke(this, e);
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                DisconnectAllSessions();
            }
        }
    }
}