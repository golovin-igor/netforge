using System.Collections.Concurrent;

namespace NetForge.Simulation.Common.CLI.AI
{
    /// <summary>
    /// Manages session context for AI CLI interactions
    /// </summary>
    public class SessionContextManager
    {
        private readonly ConcurrentDictionary<string, SessionContext> _sessions = new();
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2);

        /// <summary>
        /// Get or create session context
        /// </summary>
        public Task<SessionContext?> GetSessionContextAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return Task.FromResult<SessionContext?>(null);

            var context = _sessions.GetOrAdd(sessionId, _ => new SessionContext
            {
                LastCommandTimestamp = DateTime.UtcNow,
                CommandCount = 0
            });

            // Check if session is expired
            if (DateTime.UtcNow - context.LastCommandTimestamp > _sessionTimeout)
            {
                // Reset expired session
                context.PreviousCommands.Clear();
                context.SessionState.Clear();
                context.CommandCount = 0;
            }

            return Task.FromResult<SessionContext?>(context);
        }

        /// <summary>
        /// Update session context with new command
        /// </summary>
        public Task UpdateSessionContextAsync(string sessionId, string command, string response)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(command))
                return Task.CompletedTask;

            var context = _sessions.GetOrAdd(sessionId, _ => new SessionContext());

            context.PreviousCommands.Add(command);
            context.LastCommandTimestamp = DateTime.UtcNow;
            context.CommandCount++;

            // Keep only last 50 commands to prevent memory bloat
            if (context.PreviousCommands.Count > 50)
            {
                context.PreviousCommands.RemoveAt(0);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clear session context
        /// </summary>
        public Task ClearSessionAsync(string sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessions.TryRemove(sessionId, out _);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public Task CleanupExpiredSessionsAsync()
        {
            var expiredSessions = _sessions.Where(kvp =>
                DateTime.UtcNow - kvp.Value.LastCommandTimestamp > _sessionTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                _sessions.TryRemove(sessionId, out _);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Session context for maintaining state across commands (local version)
    /// </summary>
    public class SessionContext
    {
        public List<string> PreviousCommands { get; set; } = new();
        public Dictionary<string, object> SessionState { get; set; } = new();
        public DateTime LastCommandTimestamp { get; set; }
        public int CommandCount { get; set; }
    }
}
