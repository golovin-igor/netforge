using NetForge.Interfaces.Devices;

namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// HTTP session management
    /// </summary>
    public class HttpSessionManager : IDisposable
    {
        private readonly Dictionary<string, HttpSession> _sessions = new();
        private readonly int _sessionTimeoutSeconds;
        private readonly Timer _cleanupTimer;
        private readonly object _lock = new();

        public HttpSessionManager(int sessionTimeoutMinutes)
        {
            _sessionTimeoutSeconds = sessionTimeoutMinutes * 60;
            _cleanupTimer = new Timer(CleanupExpiredSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Create new session
        /// </summary>
        public HttpSession CreateSession(HttpUser user)
        {
            lock (_lock)
            {
                var session = new HttpSession
                {
                    Id = Guid.NewGuid().ToString(),
                    User = user,
                    CreatedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IsActive = true
                };

                _sessions[session.Id] = session;
                return session;
            }
        }

        /// <summary>
        /// Get session by ID
        /// </summary>
        public HttpSession? GetSession(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    if (session.IsActive && !IsSessionExpired(session))
                    {
                        session.LastActivity = DateTime.UtcNow;
                        return session;
                    }
                    else
                    {
                        // Remove expired session
                        _sessions.Remove(sessionId);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Update session activity
        /// </summary>
        public void UpdateSessionActivity(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    session.LastActivity = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// End session
        /// </summary>
        public void EndSession(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    session.IsActive = false;
                    _sessions.Remove(sessionId);
                }
            }
        }

        /// <summary>
        /// Get active session count
        /// </summary>
        public int GetActiveSessionCount()
        {
            lock (_lock)
            {
                return _sessions.Count(s => s.Value.IsActive && !IsSessionExpired(s.Value));
            }
        }

        /// <summary>
        /// Check if session is expired
        /// </summary>
        private bool IsSessionExpired(HttpSession session)
        {
            return (DateTime.UtcNow - session.LastActivity).TotalSeconds > _sessionTimeoutSeconds;
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        private void CleanupExpiredSessions(object? state)
        {
            lock (_lock)
            {
                var expiredSessions = _sessions
                    .Where(s => !s.Value.IsActive || IsSessionExpired(s.Value))
                    .Select(s => s.Key)
                    .ToList();

                foreach (var sessionId in expiredSessions)
                {
                    _sessions.Remove(sessionId);
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}