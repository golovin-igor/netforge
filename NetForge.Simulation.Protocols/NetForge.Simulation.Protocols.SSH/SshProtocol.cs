using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.SSH
{
    /// <summary>
    /// SSH protocol implementation for secure network device management
    /// This is a special management protocol that provides encrypted access to simulated devices
    /// </summary>
    public class SshProtocol : BaseProtocol
    {
        private SshServer? _sshServer;
        private readonly SshSessionManager _sessionManager;

        public override ProtocolType Type => ProtocolType.SSH;
        public override string Name => "SSH Protocol";
        public override string Version => "2.0.0";

        public SshProtocol()
        {
            _sessionManager = new SshSessionManager();
        }

        protected override BaseProtocolState CreateInitialState()
        {
            return new SshState();
        }

        protected override void OnInitialized()
        {
            var sshConfig = GetSshConfig();
            if (sshConfig.IsEnabled)
            {
                StartSshServer(sshConfig);
            }
        }

        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var sshState = (SshState)_state;
            var sshConfig = GetSshConfig();

            if (!sshConfig.IsEnabled)
            {
                await StopSshServer();
                sshState.IsActive = false;
                sshState.UpdateServerStatus(false);
                return;
            }

            // Update active sessions and cleanup
            await _sessionManager.UpdateSessions();

            // Update SSH state
            sshState.UpdateSessionActivity(_sessionManager.GetActiveSessions().Count);
            sshState.SessionStatistics = _sessionManager.GetSessionStatistics();

            // Ensure server is running if it should be
            if (sshConfig.IsEnabled && (_sshServer == null || !_sshServer.IsRunning))
            {
                StartSshServer(sshConfig);
            }
        }

        private void StartSshServer(SshConfig config)
        {
            try
            {
                _sshServer = new SshServer(_device, config, _sessionManager);
                _sshServer.ConnectionReceived += OnSshConnectionReceived;
                _sshServer.CommandReceived += OnSshCommandReceived;
                _sshServer.AuthenticationFailed += OnSshAuthenticationFailed;
                _sshServer.AuthenticationSucceeded += OnSshAuthenticationSucceeded;
                _sshServer.Start();

                var sshState = (SshState)_state;
                sshState.UpdateServerStatus(true, config.Port, config.ProtocolVersion);
                sshState.IsActive = true;

                // Generate and set host key fingerprint
                var fingerprint = GenerateHostKeyFingerprint();
                sshState.SetHostKeyFingerprint(fingerprint);

                LogProtocolEvent($"SSH server started on port {config.Port} (Protocol version {config.ProtocolVersion})");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Failed to start SSH server: {ex.Message}");
                var sshState = (SshState)_state;
                sshState.UpdateServerStatus(false);
                sshState.IsActive = false;
            }
        }

        private async Task StopSshServer()
        {
            if (_sshServer != null)
            {
                try
                {
                    await _sshServer.StopAsync();
                    _sshServer.Dispose();
                    _sshServer = null;

                    LogProtocolEvent("SSH server stopped");
                }
                catch (Exception ex)
                {
                    LogProtocolEvent($"Error stopping SSH server: {ex.Message}");
                }
            }
        }

        private void OnSshConnectionReceived(object? sender, SshConnectionEventArgs e)
        {
            var session = e.Session;
            LogProtocolEvent($"New SSH connection from {session.ClientEndpoint} (Session: {session.SessionId})");

            var sshState = (SshState)_state;
            sshState.RecordNewConnection();
            sshState.UpdateSessionEncryption(session.SessionId, session.EncryptionAlgorithm);
        }

        private void OnSshAuthenticationSucceeded(object? sender, SshAuthenticationEventArgs e)
        {
            var session = e.Session;
            var username = e.Username;

            LogProtocolEvent($"SSH authentication succeeded for {username} from {session.ClientEndpoint}");

            var sshState = (SshState)_state;
            sshState.RecordSuccessfulAuthentication(username, session.ClientEndpoint);
        }

        private void OnSshAuthenticationFailed(object? sender, SshAuthenticationEventArgs e)
        {
            var session = e.Session;
            var username = e.Username;
            var reason = e.FailureReason ?? "Invalid credentials";

            LogProtocolEvent($"SSH authentication failed for {username} from {session.ClientEndpoint}: {reason}");

            var sshState = (SshState)_state;
            sshState.RecordFailedAuthentication(username, session.ClientEndpoint, reason);
        }

        private async void OnSshCommandReceived(object? sender, SshCommandEventArgs e)
        {
            var session = e.Session;
            var command = e.Command;

            LogProtocolEvent($"SSH command from {session.ClientEndpoint}[{session.SessionId}]: {command}");

            try
            {
                // Route to CLI handlers - this is the key integration point
                var response = await ProcessSshCommand(session, command);
                await session.SendResponse(response);

                // Send new prompt after command execution
                await session.SendPrompt(_device.GetHostname() ?? _device.Name);
            }
            catch (Exception ex)
            {
                await session.SendResponse($"% Error: {ex.Message}\r\n");
                await session.SendPrompt(_device.GetHostname() ?? _device.Name);
                LogProtocolEvent($"Error processing SSH command: {ex.Message}");
            }
        }

        private async Task<string> ProcessSshCommand(SshSession session, string command)
        {
            // Handle special SSH commands
            if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                command.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                session.Disconnect();
                return "Connection closed.\r\n";
            }

            if (command.Equals("logout", StringComparison.OrdinalIgnoreCase))
            {
                session.Disconnect();
                return "Logout\r\n";
            }

            // Route to the device's CLI processing system
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

        private string GenerateHostKeyFingerprint()
        {
            // Generate a pseudo-random host key fingerprint for display purposes
            var random = new Random();
            var bytes = new byte[16];
            random.NextBytes(bytes);

            return string.Join(":", bytes.Select(b => b.ToString("x2")));
        }

        private SshConfig GetSshConfig()
        {
            var config = _device.GetSshConfiguration() as SshConfig;
            return config ?? new SshConfig { IsEnabled = true, Port = 22 };
        }

        protected override object GetProtocolConfiguration()
        {
            return GetSshConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is SshConfig sshConfig)
            {
                _device.SetSshConfiguration(sshConfig);

                // Restart server if configuration changed
                _ = Task.Run(async () =>
                {
                    await StopSshServer();
                    if (sshConfig.IsEnabled)
                    {
                        StartSshServer(sshConfig);
                    }
                });
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // All vendors support SSH for management
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "F5", "Fortinet" };
        }

        /// <summary>
        /// Get SSH-specific statistics
        /// </summary>
        /// <returns>SSH statistics</returns>
        public Dictionary<string, object> GetSshStatistics()
        {
            var sshState = (SshState)_state;
            var serverStats = _sshServer?.GetServerStatistics() ?? new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["ProtocolState"] = sshState.GetStateData(),
                ["ServerStatistics"] = serverStats,
                ["Configuration"] = GetSshConfig()
            };
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            // SSH doesn't have traditional neighbors, but we can use this for session cleanup
            LogProtocolEvent($"SSH session {neighborId} was removed due to timeout");

            var sshState = (SshState)_state;
            sshState.RemoveSessionEncryption(neighborId);
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            // Use session timeout from configuration
            var config = GetSshConfig();
            return config.SessionTimeoutMinutes * 60;
        }

        protected override void OnDispose()
        {
            _ = StopSshServer();
            _sessionManager?.Dispose();
        }
    }
}
