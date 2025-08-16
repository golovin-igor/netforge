using System.Net;
using System.Net.Sockets;
using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.SSH
{
    /// <summary>
    /// SSH server for handling incoming SSH connections
    /// </summary>
    public class SshServer : IDisposable
    {
        private readonly NetworkDevice _device;
        private readonly SshConfig _config;
        private readonly SshSessionManager _sessionManager;
        private TcpListener? _listener;
        private bool _isRunning;
        private CancellationTokenSource? _cancellationTokenSource;
        
        // Events
        public event EventHandler<SshConnectionEventArgs>? ConnectionReceived;
        public event EventHandler<SshCommandEventArgs>? CommandReceived;
        public event EventHandler<SshAuthenticationEventArgs>? AuthenticationSucceeded;
        public event EventHandler<SshAuthenticationEventArgs>? AuthenticationFailed;
        
        public bool IsRunning => _isRunning;
        
        public SshServer(NetworkDevice device, SshConfig config, SshSessionManager sessionManager)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }
        
        /// <summary>
        /// Start the SSH server
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;
            
            try
            {
                _listener = new TcpListener(IPAddress.Any, _config.Port);
                _listener.Start();
                
                _cancellationTokenSource = new CancellationTokenSource();
                _isRunning = true;
                
                // Start accepting connections in background
                _ = Task.Run(AcceptConnectionsAsync, _cancellationTokenSource.Token);
                
                _device.AddLogEntry($"SSH server started on port {_config.Port}");
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Failed to start SSH server: {ex.Message}");
                _isRunning = false;
                throw;
            }
        }
        
        /// <summary>
        /// Stop the SSH server
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
                return;
            
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            
            try
            {
                _listener?.Stop();
                
                // Close all active sessions
                await _sessionManager.CloseAllSessionsAsync();
                
                _device.AddLogEntry("SSH server stopped");
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error stopping SSH server: {ex.Message}");
            }
        }
        
        private async Task AcceptConnectionsAsync()
        {
            var token = _cancellationTokenSource?.Token ?? CancellationToken.None;
            
            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    if (_listener == null)
                        break;
                    
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    
                    // Check session limits
                    if (_sessionManager.GetActiveSessions().Count >= _config.MaxSessions)
                    {
                        _device.AddLogEntry($"SSH connection rejected from {tcpClient.Client.RemoteEndPoint}: Session limit reached");
                        tcpClient.Close();
                        continue;
                    }
                    
                    // Handle connection in background
                    _ = Task.Run(() => HandleConnectionAsync(tcpClient, token), token);
                }
                catch (ObjectDisposedException)
                {
                    // Server is being shut down
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        _device.AddLogEntry($"Error accepting SSH connection: {ex.Message}");
                    }
                }
            }
        }
        
        private async Task HandleConnectionAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            var clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";
            SshSession? session = null;
            
            try
            {
                // Create session
                session = new SshSession(tcpClient, _config, _device);
                _sessionManager.AddSession(session);
                
                // Raise connection event
                ConnectionReceived?.Invoke(this, new SshConnectionEventArgs(session));
                
                // Send banner
                await session.SendMessage(_config.BannerMessage);
                
                // Start authentication process
                var authenticated = await HandleAuthenticationAsync(session, cancellationToken);
                
                if (authenticated)
                {
                    AuthenticationSucceeded?.Invoke(this, new SshAuthenticationEventArgs(session, session.Username ?? "unknown"));
                    
                    // Send welcome message and initial prompt
                    await session.SendMessage($"Welcome to {_device.GetHostname() ?? _device.Name}\r\n");
                    await session.SendPrompt(_device.GetHostname() ?? _device.Name);
                    
                    // Handle commands
                    await HandleCommandsAsync(session, cancellationToken);
                }
                else
                {
                    await session.SendMessage("Authentication failed. Connection closed.\r\n");
                }
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error handling SSH connection from {clientEndpoint}: {ex.Message}");
            }
            finally
            {
                if (session != null)
                {
                    _sessionManager.RemoveSession(session);
                    session.Dispose();
                }
                
                try
                {
                    tcpClient.Close();
                }
                catch
                {
                    // Ignore errors when closing
                }
            }
        }
        
        private async Task<bool> HandleAuthenticationAsync(SshSession session, CancellationToken cancellationToken)
        {
            if (!_config.RequireAuthentication)
            {
                session.SetAuthenticated("anonymous");
                return true;
            }
            
            var attempts = 0;
            
            while (attempts < _config.MaxAuthAttempts && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Send username prompt
                    await session.SendMessage("Username: ");
                    var username = await session.ReadLineAsync(cancellationToken);
                    
                    if (string.IsNullOrWhiteSpace(username))
                        continue;
                    
                    // Send password prompt
                    await session.SendMessage("Password: ");
                    var password = await session.ReadLineAsync(cancellationToken, echoInput: false);
                    
                    // Validate credentials
                    if (ValidateCredentials(username, password))
                    {
                        session.SetAuthenticated(username);
                        return true;
                    }
                    else
                    {
                        attempts++;
                        AuthenticationFailed?.Invoke(this, new SshAuthenticationEventArgs(session, username, "Invalid credentials"));
                        
                        if (attempts < _config.MaxAuthAttempts)
                        {
                            await session.SendMessage("Authentication failed. Please try again.\r\n\r\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _device.AddLogEntry($"Authentication error for session {session.SessionId}: {ex.Message}");
                    break;
                }
            }
            
            return false;
        }
        
        private bool ValidateCredentials(string username, string password)
        {
            // Simple credential validation - in a real implementation this would be more sophisticated
            return string.Equals(username, _config.Username, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(password, _config.Password, StringComparison.Ordinal);
        }
        
        private async Task HandleCommandsAsync(SshSession session, CancellationToken cancellationToken)
        {
            try
            {
                while (session.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var command = await session.ReadLineAsync(cancellationToken);
                    
                    if (string.IsNullOrWhiteSpace(command))
                        continue;
                    
                    // Raise command event for processing
                    CommandReceived?.Invoke(this, new SshCommandEventArgs(session, command));
                    
                    // Note: Response is handled by the protocol class
                }
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error handling commands for session {session.SessionId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get server statistics
        /// </summary>
        /// <returns>Dictionary of server statistics</returns>
        public Dictionary<string, object> GetServerStatistics()
        {
            return new Dictionary<string, object>
            {
                ["IsRunning"] = _isRunning,
                ["ListeningPort"] = _config.Port,
                ["ActiveSessions"] = _sessionManager.GetActiveSessions().Count,
                ["MaxSessions"] = _config.MaxSessions,
                ["TotalConnections"] = _sessionManager.GetTotalConnectionCount(),
                ["ProtocolVersion"] = _config.ProtocolVersion,
                ["AuthenticationRequired"] = _config.RequireAuthentication
            };
        }
        
        public void Dispose()
        {
            _ = StopAsync();
            _cancellationTokenSource?.Dispose();
            _listener = null;
        }
    }
    
    // Event argument classes
    public class SshConnectionEventArgs : EventArgs
    {
        public SshSession Session { get; }
        
        public SshConnectionEventArgs(SshSession session)
        {
            Session = session;
        }
    }
    
    public class SshCommandEventArgs : EventArgs
    {
        public SshSession Session { get; }
        public string Command { get; }
        
        public SshCommandEventArgs(SshSession session, string command)
        {
            Session = session;
            Command = command;
        }
    }
    
    public class SshAuthenticationEventArgs : EventArgs
    {
        public SshSession Session { get; }
        public string Username { get; }
        public string? FailureReason { get; }
        
        public SshAuthenticationEventArgs(SshSession session, string username, string? failureReason = null)
        {
            Session = session;
            Username = username;
            FailureReason = failureReason;
        }
    }
}