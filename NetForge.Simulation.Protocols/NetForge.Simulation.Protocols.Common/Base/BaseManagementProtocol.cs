using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.Protocols.Common.State;
using System.Collections.Concurrent;

namespace NetForge.Simulation.Protocols.Common.Base
{
    /// <summary>
    /// Base class for management protocols (SSH, Telnet, SNMP, HTTP)
    /// Provides common session management, authentication, and security functionality
    /// </summary>
    public abstract class BaseManagementProtocol : BaseProtocol
    {
        protected readonly ConcurrentDictionary<string, object> _activeSessions = new();
        protected readonly Dictionary<string, DateTime> _sessionStartTimes = new();
        protected readonly Dictionary<string, int> _connectionAttempts = new();
        protected readonly Dictionary<string, DateTime> _lastConnectionAttempt = new();

        /// <summary>
        /// Port number this management protocol listens on
        /// Override to set protocol-specific default port
        /// </summary>
        public abstract int DefaultPort { get; }

        /// <summary>
        /// Maximum number of concurrent sessions allowed
        /// Override to set protocol-specific session limits
        /// </summary>
        public virtual int MaxSessions => 10;

        /// <summary>
        /// Session timeout in minutes
        /// Override to set protocol-specific timeout
        /// </summary>
        public virtual int SessionTimeoutMinutes => 30;

        /// <summary>
        /// Whether authentication is required for this protocol
        /// Override to set protocol-specific authentication requirements
        /// </summary>
        public virtual bool RequiresAuthentication => true;

        /// <summary>
        /// Maximum failed authentication attempts before blocking
        /// </summary>
        public virtual int MaxFailedAttempts => 3;

        /// <summary>
        /// Block duration in minutes after max failed attempts
        /// </summary>
        public virtual int BlockDurationMinutes => 15;

        /// <summary>
        /// Whether the management service is currently listening
        /// </summary>
        public bool IsListening { get; protected set; }

        /// <summary>
        /// Current listening port (may differ from default)
        /// </summary>
        public int ListeningPort { get; protected set; }

        /// <summary>
        /// Create the initial management protocol state
        /// </summary>
        protected override BaseProtocolState CreateInitialState()
        {
            return new ManagementProtocolState();
        }

        /// <summary>
        /// Get the management protocol specific state
        /// </summary>
        /// <returns>Management protocol state</returns>
        public IManagementProtocolState GetManagementState()
        {
            return _state as IManagementProtocolState;
        }

        /// <summary>
        /// Initialize the management protocol
        /// </summary>
        /// <param name="device">Network device</param>
        public override void Initialize(INetworkDevice device)
        {
            base.Initialize(device);

            // Set default listening port
            ListeningPort = DefaultPort;

            // Start listening if enabled
            if (_state.IsConfigured)
            {
                StartListening();
            }
        }

        /// <summary>
        /// Core management protocol operation - called when state changes
        /// Implements common session management while allowing protocol-specific customization
        /// </summary>
        /// <param name="device">Network device</param>
        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            try
            {
                // Step 1: Process pending connection requests
                await ProcessConnectionRequests(device);

                // Step 2: Manage active sessions
                await ManageActiveSessions(device);

                // Step 3: Clean up expired sessions
                await CleanupExpiredSessions();

                // Step 4: Update security monitoring
                await UpdateSecurityMonitoring();

                // Step 5: Process protocol-specific management tasks
                await ProcessManagementTasks(device);

                // Update management state
                if (_state is ManagementProtocolState managementState)
                {
                    managementState.ActiveSessions = _activeSessions.Count;
                    managementState.IsListening = IsListening;
                    managementState.ListeningPort = ListeningPort;
                }

                LogProtocolEvent($"Management cycle completed: {_activeSessions.Count} active sessions");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error in management operation: {ex.Message}");
                _metrics.RecordError($"Management operation failed: {ex.Message}");
            }
        }

        // Abstract and virtual methods for protocol-specific implementation

        /// <summary>
        /// Start listening for incoming connections
        /// Override to implement protocol-specific listening logic
        /// </summary>
        protected virtual void StartListening()
        {
            IsListening = true;
            LogProtocolEvent($"Started listening on port {ListeningPort}");
        }

        /// <summary>
        /// Stop listening for incoming connections
        /// Override to implement protocol-specific shutdown logic
        /// </summary>
        protected virtual void StopListening()
        {
            IsListening = false;
            LogProtocolEvent("Stopped listening for connections");
        }

        /// <summary>
        /// Process pending connection requests
        /// Override to implement protocol-specific connection handling
        /// </summary>
        /// <param name="device">Network device</param>
        protected abstract Task ProcessConnectionRequests(INetworkDevice device);

        /// <summary>
        /// Manage active sessions (heartbeat, data processing, etc.)
        /// Override to implement protocol-specific session management
        /// </summary>
        /// <param name="device">Network device</param>
        protected abstract Task ManageActiveSessions(INetworkDevice device);

        /// <summary>
        /// Process protocol-specific management tasks
        /// Override to implement additional management functionality
        /// </summary>
        /// <param name="device">Network device</param>
        protected virtual async Task ProcessManagementTasks(INetworkDevice device)
        {
            await Task.CompletedTask;
        }

        // Common session management functionality

        /// <summary>
        /// Create a new session if authentication succeeds
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="credentials">Authentication credentials</param>
        /// <returns>Session ID if successful, null if failed</returns>
        protected virtual async Task<string> CreateSession(string clientId, Dictionary<string, object> credentials)
        {
            try
            {
                // Check if client is blocked
                if (IsClientBlocked(clientId))
                {
                    LogProtocolEvent($"Connection rejected from blocked client: {clientId}");
                    return null;
                }

                // Check session limit
                if (_activeSessions.Count >= MaxSessions)
                {
                    LogProtocolEvent($"Connection rejected: Maximum sessions ({MaxSessions}) reached");
                    return null;
                }

                // Authenticate if required
                if (RequiresAuthentication)
                {
                    var authResult = await AuthenticateClient(clientId, credentials);
                    if (!authResult)
                    {
                        RecordFailedAttempt(clientId);
                        LogProtocolEvent($"Authentication failed for client: {clientId}");
                        return null;
                    }
                }

                // Create session
                var sessionId = Guid.NewGuid().ToString();
                var session = CreateSessionObject(sessionId, clientId, credentials);

                _activeSessions[sessionId] = session;
                _sessionStartTimes[sessionId] = DateTime.Now;

                // Reset failed attempts on successful connection
                _connectionAttempts.Remove(clientId);
                _lastConnectionAttempt.Remove(clientId);

                LogProtocolEvent($"Session created: {sessionId} for client {clientId}");
                _state.MarkStateChanged();

                return sessionId;
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Error creating session for {clientId}: {ex.Message}");
                _metrics.RecordError($"Session creation failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Terminate a session
        /// </summary>
        /// <param name="sessionId">Session to terminate</param>
        /// <param name="reason">Termination reason</param>
        protected virtual void TerminateSession(string sessionId, string reason = "Normal termination")
        {
            if (_activeSessions.TryRemove(sessionId, out var session))
            {
                _sessionStartTimes.Remove(sessionId);
                OnSessionTerminated(sessionId, session, reason);
                LogProtocolEvent($"Session terminated: {sessionId} - {reason}");
                _state.MarkStateChanged();
            }
        }

        /// <summary>
        /// Clean up expired sessions based on timeout
        /// </summary>
        protected virtual async Task CleanupExpiredSessions()
        {
            var now = DateTime.Now;
            var expiredSessions = _sessionStartTimes
                .Where(kvp => (now - kvp.Value).TotalMinutes > SessionTimeoutMinutes)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                TerminateSession(sessionId, "Session timeout");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Update security monitoring (blocked clients, failed attempts, etc.)
        /// </summary>
        protected virtual async Task UpdateSecurityMonitoring()
        {
            var now = DateTime.Now;

            // Remove expired blocks
            var expiredBlocks = _lastConnectionAttempt
                .Where(kvp => (now - kvp.Value).TotalMinutes > BlockDurationMinutes)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var clientId in expiredBlocks)
            {
                _connectionAttempts.Remove(clientId);
                _lastConnectionAttempt.Remove(clientId);
                LogProtocolEvent($"Unblocked client: {clientId}");
            }

            await Task.CompletedTask;
        }

        // Authentication and security

        /// <summary>
        /// Authenticate a client
        /// Override to implement protocol-specific authentication
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="credentials">Authentication credentials</param>
        /// <returns>True if authentication successful, false otherwise</returns>
        protected abstract Task<bool> AuthenticateClient(string clientId, Dictionary<string, object> credentials);

        /// <summary>
        /// Create a protocol-specific session object
        /// Override to create custom session objects with protocol-specific data
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientId">Client identifier</param>
        /// <param name="credentials">Authentication credentials</param>
        /// <returns>Session object</returns>
        protected virtual object CreateSessionObject(string sessionId, string clientId, Dictionary<string, object> credentials)
        {
            return new Dictionary<string, object>
            {
                ["SessionId"] = sessionId,
                ["ClientId"] = clientId,
                ["StartTime"] = DateTime.Now,
                ["LastActivity"] = DateTime.Now
            };
        }

        /// <summary>
        /// Called when a session is terminated
        /// Override to perform protocol-specific cleanup
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="session">Session object</param>
        /// <param name="reason">Termination reason</param>
        protected virtual void OnSessionTerminated(string sessionId, object session, string reason)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Record a failed authentication attempt
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        protected virtual void RecordFailedAttempt(string clientId)
        {
            _connectionAttempts[clientId] = _connectionAttempts.GetValueOrDefault(clientId, 0) + 1;
            _lastConnectionAttempt[clientId] = DateTime.Now;

            if (_connectionAttempts[clientId] >= MaxFailedAttempts)
            {
                LogProtocolEvent($"Client blocked due to failed attempts: {clientId}");
            }
        }

        /// <summary>
        /// Check if a client is currently blocked
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>True if blocked, false otherwise</returns>
        protected virtual bool IsClientBlocked(string clientId)
        {
            if (!_connectionAttempts.ContainsKey(clientId))
                return false;

            return _connectionAttempts[clientId] >= MaxFailedAttempts &&
                   _lastConnectionAttempt.ContainsKey(clientId) &&
                   (DateTime.Now - _lastConnectionAttempt[clientId]).TotalMinutes < BlockDurationMinutes;
        }

        // Public interface methods

        /// <summary>
        /// Get information about active sessions
        /// </summary>
        /// <returns>Dictionary of session information</returns>
        public Dictionary<string, object> GetActiveSessions()
        {
            var sessions = new Dictionary<string, object>();
            foreach (var kvp in _activeSessions)
            {
                sessions[kvp.Key] = kvp.Value;
            }
            return sessions;
        }

        /// <summary>
        /// Get connection statistics
        /// </summary>
        /// <returns>Dictionary containing connection metrics</returns>
        public Dictionary<string, object> GetConnectionStatistics()
        {
            return new Dictionary<string, object>
            {
                ["ActiveSessions"] = _activeSessions.Count,
                ["MaxSessions"] = MaxSessions,
                ["IsListening"] = IsListening,
                ["ListeningPort"] = ListeningPort,
                ["SessionTimeoutMinutes"] = SessionTimeoutMinutes,
                ["BlockedClients"] = _connectionAttempts.Count(kvp => kvp.Value >= MaxFailedAttempts)
            };
        }

        /// <summary>
        /// Cleanup management-specific resources on disposal
        /// </summary>
        protected override void OnDispose()
        {
            StopListening();

            // Terminate all active sessions
            foreach (var sessionId in _activeSessions.Keys.ToList())
            {
                TerminateSession(sessionId, "Protocol shutdown");
            }

            _activeSessions.Clear();
            _sessionStartTimes.Clear();
            _connectionAttempts.Clear();
            _lastConnectionAttempt.Clear();

            base.OnDispose();
        }
    }

    /// <summary>
    /// Concrete implementation of management protocol state
    /// </summary>
    public class ManagementProtocolState : BaseProtocolState, IManagementProtocolState
    {
        public int ActiveSessions { get; set; }
        public int MaxSessions { get; set; }
        public int ListeningPort { get; set; }
        public bool RequiresAuthentication { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public bool IsListening { get; set; }

        public Dictionary<string, object> GetActiveSessions()
        {
            // This would be populated by the management protocol
            return new Dictionary<string, object>();
        }

        public Dictionary<string, object> GetConnectionStatistics()
        {
            return new Dictionary<string, object>
            {
                ["ActiveSessions"] = ActiveSessions,
                ["MaxSessions"] = MaxSessions,
                ["IsListening"] = IsListening,
                ["ListeningPort"] = ListeningPort,
                ["SessionTimeoutMinutes"] = SessionTimeoutMinutes,
                ["RequiresAuthentication"] = RequiresAuthentication
            };
        }

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["ActiveSessions"] = ActiveSessions;
            baseData["MaxSessions"] = MaxSessions;
            baseData["ListeningPort"] = ListeningPort;
            baseData["RequiresAuthentication"] = RequiresAuthentication;
            baseData["SessionTimeoutMinutes"] = SessionTimeoutMinutes;
            baseData["IsListening"] = IsListening;
            return baseData;
        }
    }
}
