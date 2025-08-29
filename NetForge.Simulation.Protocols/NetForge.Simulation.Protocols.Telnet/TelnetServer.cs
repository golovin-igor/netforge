using System.Net;
using System.Net.Sockets;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// TCP server for handling Telnet connections
    /// </summary>
    public class TelnetServer : IDisposable
    {
        private readonly INetworkDevice _device;
        private readonly TelnetConfig _config;
        private readonly TelnetSessionManager _sessionManager;
        private TcpListener? _tcpListener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _disposed = false;

        /// <summary>
        /// Event fired when a new connection is received
        /// </summary>
        public event EventHandler<TelnetConnectionEventArgs>? ConnectionReceived;

        /// <summary>
        /// Event fired when a command is received from any session
        /// </summary>
        public event EventHandler<TelnetCommandEventArgs>? CommandReceived;

        /// <summary>
        /// Whether the server is currently running
        /// </summary>
        public bool IsRunning => _isRunning && !_disposed;

        /// <summary>
        /// Port the server is listening on
        /// </summary>
        public int Port => _config.Port;

        public TelnetServer(INetworkDevice device, TelnetConfig config, TelnetSessionManager sessionManager)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            // Subscribe to session events
            _sessionManager.SessionCreated += OnSessionCreated;
            _sessionManager.SessionDisconnected += OnSessionDisconnected;
        }

        /// <summary>
        /// Start the Telnet server
        /// </summary>
        public void Start()
        {
            if (_disposed || _isRunning)
                return;

            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, _config.Port);
                _cancellationTokenSource = new CancellationTokenSource();

                _tcpListener.Start();
                _isRunning = true;

                // Start accepting connections in the background
                _ = Task.Run(AcceptConnectionsAsync);

                _device.AddLogEntry($"Telnet server started on port {_config.Port}");
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Failed to start Telnet server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop the Telnet server
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning || _disposed)
                return;

            try
            {
                _isRunning = false;
                _cancellationTokenSource?.Cancel();

                // Stop accepting new connections
                _tcpListener?.Stop();

                // Disconnect all sessions
                _sessionManager.DisconnectAllSessions();

                _device.AddLogEntry("Telnet server stopped");
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error stopping Telnet server: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get server statistics
        /// </summary>
        /// <returns>Server statistics</returns>
        public Dictionary<string, object> GetServerStatistics()
        {
            var sessionStats = _sessionManager.GetSessionStatistics();

            return new Dictionary<string, object>
            {
                ["IsRunning"] = IsRunning,
                ["Port"] = Port,
                ["MaxSessions"] = _config.MaxSessions,
                ["SessionTimeout"] = _config.SessionTimeoutMinutes,
                ["RequireAuthentication"] = _config.RequireAuthentication,
                ["LogCommands"] = _config.LogCommands,
                ["SessionStatistics"] = sessionStats
            };
        }

        private async Task AcceptConnectionsAsync()
        {
            if (_tcpListener == null || _cancellationTokenSource == null)
                return;

            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();

                    if (!_sessionManager.CanAcceptNewSession())
                    {
                        // Too many sessions, reject the connection
                        tcpClient.Close();
                        _device.AddLogEntry($"Telnet connection rejected: maximum sessions ({_config.MaxSessions}) reached");
                        continue;
                    }

                    var session = new TelnetSession(tcpClient);

                    // Subscribe to session command events
                    session.CommandReceived += OnSessionCommandReceived;

                    if (_sessionManager.AddSession(session))
                    {
                        _device.AddLogEntry($"New Telnet connection from {session.ClientEndpoint} (Session: {session.SessionId})");

                        // Send welcome banner
                        if (!string.IsNullOrEmpty(_config.BannerMessage))
                        {
                            await session.SendResponse(_config.BannerMessage);
                        }

                        // Send initial prompt or authentication request
                        if (_config.RequireAuthentication && !session.IsAuthenticated)
                        {
                            await session.SendResponse("Username: ");
                        }
                        else
                        {
                            await session.SendPrompt(_device.GetHostname() ?? _device.Name);
                        }

                        OnConnectionReceived(session);
                    }
                    else
                    {
                        // Failed to add session
                        session.Disconnect();
                        session.Dispose();
                        _device.AddLogEntry($"Failed to add Telnet session from {tcpClient.Client.RemoteEndPoint}");
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Expected when stopping the server
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning) // Only log if we're still supposed to be running
                    {
                        _device.AddLogEntry($"Error accepting Telnet connection: {ex.Message}");
                    }
                }
            }
        }

        private void OnConnectionReceived(TelnetSession session)
        {
            ConnectionReceived?.Invoke(this, new TelnetConnectionEventArgs(session));
        }

        private void OnSessionCommandReceived(object? sender, TelnetCommandEventArgs e)
        {
            if (_config.LogCommands)
            {
                _device.AddLogEntry($"Telnet[{e.Session.SessionId}]: {e.Command}");
            }

            CommandReceived?.Invoke(this, e);
        }

        private void OnSessionCreated(object? sender, TelnetSessionEventArgs e)
        {
            _device.AddLogEntry($"Telnet session {e.Session.SessionId} created");
        }

        private void OnSessionDisconnected(object? sender, TelnetSessionEventArgs e)
        {
            _device.AddLogEntry($"Telnet session {e.Session.SessionId} disconnected");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_isRunning)
                {
                    _ = StopAsync();
                }

                _tcpListener?.Stop();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                // Unsubscribe from events
                if (_sessionManager != null)
                {
                    _sessionManager.SessionCreated -= OnSessionCreated;
                    _sessionManager.SessionDisconnected -= OnSessionDisconnected;
                }
            }
        }
    }

    /// <summary>
    /// Event arguments for Telnet connection events
    /// </summary>
    public class TelnetConnectionEventArgs(TelnetSession session) : EventArgs
    {
        public TelnetSession Session { get; } = session;
    }
}
