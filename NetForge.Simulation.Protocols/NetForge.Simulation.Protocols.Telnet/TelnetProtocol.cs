using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// Telnet protocol implementation for network device management
    /// This is a special management protocol that provides actual network access to simulated devices
    /// </summary>
    public class TelnetProtocol : BaseProtocol
    {
        private TelnetServer? _telnetServer;
        private readonly TelnetSessionManager _sessionManager = new();

        public override ProtocolType Type => ProtocolType.TELNET;
        public override string Name => "Telnet Protocol";
        public override string Version => "1.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new TelnetState();
        }

        protected override void OnInitialized()
        {
            var telnetConfig = GetTelnetConfig();
            if (telnetConfig.IsEnabled)
            {
                StartTelnetServer(telnetConfig);
            }
        }

        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var telnetState = (TelnetState)_state;
            var telnetConfig = GetTelnetConfig();

            if (!telnetConfig.IsEnabled)
            {
                await StopTelnetServer();
                telnetState.IsActive = false;
                telnetState.UpdateServerStatus(false);
                return;
            }

            // Update active sessions and cleanup
            await _sessionManager.UpdateSessions();

            // Update telnet state
            telnetState.UpdateSessionActivity(_sessionManager.GetActiveSessions().Count);
            telnetState.SessionStatistics = _sessionManager.GetSessionStatistics();

            // Ensure server is running if it should be
            if (telnetConfig.IsEnabled && (_telnetServer == null || !_telnetServer.IsRunning))
            {
                StartTelnetServer(telnetConfig);
            }
        }

        private void StartTelnetServer(TelnetConfig config)
        {
            try
            {
                _telnetServer = new TelnetServer(_device, config, _sessionManager);
                _telnetServer.ConnectionReceived += OnTelnetConnectionReceived;
                _telnetServer.CommandReceived += OnTelnetCommandReceived;
                _telnetServer.Start();

                var telnetState = (TelnetState)_state;
                telnetState.UpdateServerStatus(true, config.Port);
                telnetState.IsActive = true;

                LogProtocolEvent($"Telnet server started on port {config.Port}");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Failed to start Telnet server: {ex.Message}");
                var telnetState = (TelnetState)_state;
                telnetState.UpdateServerStatus(false);
                telnetState.IsActive = false;
            }
        }

        private async Task StopTelnetServer()
        {
            if (_telnetServer != null)
            {
                try
                {
                    await _telnetServer.StopAsync();
                    _telnetServer.Dispose();
                    _telnetServer = null;

                    LogProtocolEvent("Telnet server stopped");
                }
                catch (Exception ex)
                {
                    LogProtocolEvent($"Error stopping Telnet server: {ex.Message}");
                }
            }
        }

        private void OnTelnetConnectionReceived(object? sender, TelnetConnectionEventArgs e)
        {
            var session = e.Session;
            LogProtocolEvent($"New Telnet connection from {session.ClientEndpoint} (Session: {session.SessionId})");

            var telnetState = (TelnetState)_state;
            telnetState.RecordNewConnection();
        }

        private async void OnTelnetCommandReceived(object? sender, TelnetCommandEventArgs e)
        {
            var session = e.Session;
            var command = e.Command;

            LogProtocolEvent($"Telnet command from {session.ClientEndpoint}[{session.SessionId}]: {command}");

            try
            {
                // Route to CLI handlers - this is the key integration point
                var response = await ProcessTelnetCommand(session, command);
                await session.SendResponse(response);

                // Send new prompt after command execution
                await session.SendPrompt(_device.GetHostname() ?? _device.Name);
            }
            catch (Exception ex)
            {
                await session.SendResponse($"% Error: {ex.Message}\r\n");
                await session.SendPrompt(_device.GetHostname() ?? _device.Name);
                LogProtocolEvent($"Error processing Telnet command: {ex.Message}");
            }
        }

        private async Task<string> ProcessTelnetCommand(TelnetSession session, string command)
        {
            var telnetConfig = GetTelnetConfig();

            // Handle authentication if required
            if (telnetConfig.RequireAuthentication && !session.IsAuthenticated)
            {
                return await ProcessAuthenticationCommand(session, command, telnetConfig);
            }

            // Handle special Telnet commands
            if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                session.Disconnect();
                return "Connection closed by foreign host.\r\n";
            }

            if (command.Equals("logout", StringComparison.OrdinalIgnoreCase))
            {
                session.Disconnect();
                return "Logout\r\n";
            }

            // Route to the device's CLI processing system
            // Note: We'll need to extend NetworkDevice to support Telnet sessions
            // For now, use the existing command processing
            try
            {
                var result = await _device.ProcessCommandAsync(command);
                return result + "\r\n";
            }
            catch (Exception ex)
            {
                return $"% Error: {ex.Message}\r\n";
            }
        }

        private async Task<string> ProcessAuthenticationCommand(TelnetSession session, string command, TelnetConfig config)
        {
            // Simple authentication - in real implementation this would be more sophisticated
            if (string.IsNullOrEmpty(session.Username))
            {
                // User is entering username
                if (session.Authenticate(command, "", config.Username, config.Password))
                {
                    return "Password: ";
                }
                else
                {
                    return "Username: ";
                }
            }
            else
            {
                // User is entering password
                if (session.Authenticate(session.Username, command, config.Username, config.Password))
                {
                    return $"Welcome to {_device.GetHostname() ?? _device.Name}\r\n";
                }
                else
                {
                    return "% Login invalid\r\nUsername: ";
                }
            }
        }

        private TelnetConfig GetTelnetConfig()
        {
            var config = _device.GetTelnetConfiguration() as TelnetConfig;
            return config ?? new TelnetConfig { IsEnabled = true, Port = 23 };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetTelnetConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is TelnetConfig telnetConfig)
            {
                _device.SetTelnetConfiguration(telnetConfig);

                // Restart server if configuration changed
                _ = Task.Run(async () =>
                {
                    await StopTelnetServer();
                    if (telnetConfig.IsEnabled)
                    {
                        StartTelnetServer(telnetConfig);
                    }
                });
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // All vendors support Telnet for management
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "F5", "Fortinet" };
        }

        /// <summary>
        /// Get Telnet-specific statistics
        /// </summary>
        /// <returns>Telnet statistics</returns>
        public Dictionary<string, object> GetTelnetStatistics()
        {
            var telnetState = (TelnetState)_state;
            var serverStats = _telnetServer?.GetServerStatistics() ?? new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["ProtocolState"] = telnetState.GetStateData(),
                ["ServerStatistics"] = serverStats,
                ["Configuration"] = GetTelnetConfig()
            };
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            // Telnet doesn't have traditional neighbors, but we can use this for session cleanup
            LogProtocolEvent($"Telnet session {neighborId} was removed due to timeout");
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            // Use session timeout from configuration
            var config = GetTelnetConfig();
            return config.SessionTimeoutMinutes * 60;
        }

        protected override void OnDispose()
        {
            _ = StopTelnetServer();
            _sessionManager?.Dispose();
        }
    }
}
