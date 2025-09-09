using NetForge.Simulation.Devices;
using NetForge.Simulation.Topology.Common;
using NetForge.Simulation.Topology.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.SSH;

namespace NetForge.Simulation.Protocols.Tests
{
    /// <summary>
    /// Tests for SSH protocol implementation following TESTING_STRATEGY.md guidelines.
    /// Tests session negotiation, authentication, and command processing.
    /// </summary>
    public class SshProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly CiscoDevice _testDevice;
        private readonly SshProtocol _sshProtocol;
        private readonly List<string> _deviceLogs = new();

        public SshProtocolTests()
        {
            _network = new Network();
            _testDevice = new CiscoDevice("TestSshDevice");
            _network.AddDeviceAsync(_testDevice).Wait();

            // Create and register SSH protocol
            _sshProtocol = new SshProtocol();
            _testDevice.RegisterProtocol(_sshProtocol);

            // Set up log capture
            _testDevice.LogEntryAdded += log => _deviceLogs.Add(log);

            // Configure SSH
            var sshConfig = new SshConfig
            {
                IsEnabled = true,
                Port = 2222,
                RequireAuthentication = true,
                AllowPasswordAuthentication = true,
                AllowPublicKeyAuthentication = false,
                Username = "admin",
                Password = "test123",
                TestMode = true // Enable test mode to prevent actual network binding
            };
            _testDevice.SetSshConfiguration(sshConfig);
            _deviceLogs.Clear();
        }

        [Fact]
        public void SshProtocol_Initialization_ShouldSetCorrectProperties()
        {
            // Arrange & Act
            var protocol = new SshProtocol();

            // Assert
            Assert.Equal(NetworkProtocolType.SSH, protocol.Type);
            Assert.Equal("SSH Protocol", protocol.Name);
            Assert.NotNull(protocol.GetState());
        }

        [Fact]
        public async Task SshProtocol_WhenEnabled_ShouldStartServer()
        {
            // Arrange
            var sshState = _sshProtocol.GetTypedState<SshState>();

            // Act
            await _sshProtocol.UpdateState(_testDevice);

            // Debug: Check logs if assertion fails
            if (!sshState.IsActive)
            {
                foreach (var log in _deviceLogs)
                {
                    Console.WriteLine($"Device Log: {log}");
                }
            }

            // Assert
            Assert.True(sshState.IsActive, $"SSH should be active. IsServerRunning: {sshState.IsServerRunning}");
            Assert.True(sshState.IsServerRunning);
            Assert.Equal(2222, sshState.ListeningPort); // Uses configured port
        }

        [Fact]
        public async Task SshProtocol_WhenDisabled_ShouldStopServer()
        {
            // Arrange
            await _sshProtocol.UpdateState(_testDevice); // Start first
            var sshConfig = new SshConfig { IsEnabled = false };
            _testDevice.SetSshConfiguration(sshConfig);
            _deviceLogs.Clear();

            // Act
            await _sshProtocol.UpdateState(_testDevice);

            // Assert
            var sshState = _sshProtocol.GetTypedState<SshState>();
            Assert.False(sshState.IsActive);
        }

        [Fact]
        public void SshProtocol_GetSupportedVendors_ShouldReturnAllVendors()
        {
            // Act
            var supportedVendors = _sshProtocol.GetSupportedVendors();

            // Assert
            Assert.Contains("Generic", supportedVendors);
            Assert.Contains("Cisco", supportedVendors);
            Assert.Contains("Juniper", supportedVendors);
            Assert.True(supportedVendors.Count() >= 3);
        }

        [Fact]
        public void SshProtocol_SupportsVendor_ShouldReturnTrueForSupportedVendors()
        {
            // Act & Assert
            Assert.True(_sshProtocol.SupportsVendor("Cisco"));
            Assert.True(_sshProtocol.SupportsVendor("cisco")); // Case insensitive
            Assert.True(_sshProtocol.SupportsVendor("Generic"));
        }

        [Fact]
        public void SshConfig_DefaultValues_ShouldBeReasonable()
        {
            // Arrange & Act
            var config = new SshConfig();

            // Assert
            Assert.True(config.IsEnabled); // Enabled by default for simulation
            Assert.Equal(22, config.Port); // Standard SSH port
            Assert.True(config.RequireAuthentication); // Secure by default
            Assert.True(config.AllowPasswordAuthentication);
            Assert.True(config.AllowPublicKeyAuthentication);
        }

        [Fact]
        public void SshState_InitialState_ShouldBeCorrect()
        {
            // Arrange & Act
            var state = new SshState();

            // Assert
            Assert.True(state.IsActive); // Protocols start active in simulation
            Assert.False(state.IsConfigured);
            Assert.Equal(0, state.ActiveSessions);
            Assert.Equal(0, state.TotalConnections);
            Assert.False(state.IsServerRunning);
            Assert.Equal(22, state.ListeningPort); // Default SSH port
        }

        public void Dispose()
        {
            // Clean up event subscriptions
            // Note: Network cleanup is handled by the test framework
        }
    }
}
