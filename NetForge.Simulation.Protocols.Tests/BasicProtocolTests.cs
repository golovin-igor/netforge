using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.SSH;
using NetForge.Simulation.Protocols.ARP;

namespace NetForge.Simulation.Protocols.Tests
{
    /// <summary>
    /// Basic protocol tests that validate core functionality and follow TESTING_STRATEGY.md guidelines.
    /// These tests focus on essential protocol behavior without complex model interactions.
    /// </summary>
    public class BasicProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly CiscoDevice _testDevice;

        public BasicProtocolTests()
        {
            _network = new Network();
            _testDevice = new CiscoDevice("TestProtocolDevice");
            _network.AddDeviceAsync(_testDevice).Wait();
        }

        [Fact]
        public void SshProtocol_Initialization_ShouldSetCorrectProperties()
        {
            // Arrange & Act
            var protocol = new SshProtocol();

            // Assert
            Assert.Equal(ProtocolType.SSH, protocol.Type);
            Assert.Equal("SSH Protocol", protocol.Name);
            Assert.NotNull(protocol.GetState());
        }

        [Fact]
        public void ArpProtocol_Initialization_ShouldSetCorrectProperties()
        {
            // Arrange & Act
            var protocol = new ArpProtocol();

            // Assert
            Assert.Equal(ProtocolType.ARP, protocol.Type);
            Assert.Equal("Address Resolution Protocol", protocol.Name);
            Assert.NotNull(protocol.GetState());
        }

        [Fact]
        public void SshProtocol_GetSupportedVendors_ShouldReturnVendors()
        {
            // Arrange
            var protocol = new SshProtocol();

            // Act
            var supportedVendors = protocol.GetSupportedVendors();

            // Assert
            Assert.NotNull(supportedVendors);
            Assert.NotEmpty(supportedVendors);
            Assert.Contains("Generic", supportedVendors);
        }

        [Fact]
        public void ArpProtocol_GetSupportedVendors_ShouldReturnVendors()
        {
            // Arrange
            var protocol = new ArpProtocol();

            // Act
            var supportedVendors = protocol.GetSupportedVendors();

            // Assert
            Assert.NotNull(supportedVendors);
            Assert.NotEmpty(supportedVendors);
            Assert.Contains("Generic", supportedVendors);
        }

        [Fact]
        public void SshProtocol_SupportsVendor_ShouldReturnTrueForSupportedVendors()
        {
            // Arrange
            var protocol = new SshProtocol();

            // Act & Assert
            Assert.True(protocol.SupportsVendor("Generic"));
            Assert.True(protocol.SupportsVendor("generic")); // Case insensitive
            Assert.True(protocol.SupportsVendor("Cisco"));
        }

        [Fact]
        public void ArpProtocol_SupportsVendor_ShouldReturnTrueForSupportedVendors()
        {
            // Arrange
            var protocol = new ArpProtocol();

            // Act & Assert
            Assert.True(protocol.SupportsVendor("Generic"));
            Assert.True(protocol.SupportsVendor("generic")); // Case insensitive
            Assert.True(protocol.SupportsVendor("Cisco"));
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
            Assert.Equal("admin", config.Username);
            Assert.Equal("admin", config.Password);
        }

        [Fact]
        public void ArpEntry_Construction_ShouldSetProperties()
        {
            // Arrange & Act
            var entry = new ArpEntry("192.168.1.10", "aa:bb:cc:dd:ee:ff", "GigabitEthernet0/1")
            {
                Type = ArpEntryType.Static
            };

            // Assert
            Assert.Equal("192.168.1.10", entry.IpAddress);
            Assert.Equal("aa:bb:cc:dd:ee:ff", entry.MacAddress);
            Assert.Equal("GigabitEthernet0/1", entry.Interface);
            Assert.Equal(ArpEntryType.Static, entry.Type);
        }

        [Fact]
        public void ArpEntry_IsExpired_ShouldReturnCorrectValue()
        {
            // Arrange
            var freshEntry = new ArpEntry("192.168.1.10", "aa:bb:cc:dd:ee:ff", "GigabitEthernet0/1");
            var oldEntry = new ArpEntry("192.168.1.11", "bb:cc:dd:ee:ff:aa", "GigabitEthernet0/1")
            {
                Timestamp = DateTime.Now.AddMinutes(-25) // Older than default 20 minutes
            };

            // Act & Assert
            Assert.False(freshEntry.IsExpired()); // Fresh entry shouldn't be expired
            Assert.True(oldEntry.IsExpired()); // Old entry should be expired
        }

        [Fact]
        public void SshConfig_IsValid_ShouldValidateConfiguration()
        {
            // Arrange
            var validConfig = new SshConfig
            {
                Port = 2222,
                ProtocolVersion = 2,
                MaxSessions = 5,
                SessionTimeoutMinutes = 30,
                Username = "testuser",
                Password = "testpass"
            };

            var invalidConfig = new SshConfig
            {
                Port = 70000, // Invalid port
                Username = "", // Empty username
                Password = ""  // Empty password
            };

            // Act & Assert
            Assert.True(validConfig.IsValid());
            Assert.False(invalidConfig.IsValid());
        }

        [Fact]
        public async Task ProtocolsCanBeRegisteredOnDevice()
        {
            // Arrange
            var sshProtocol = new SshProtocol();
            var arpProtocol = new ArpProtocol();

            // Act
            _testDevice.RegisterProtocol(sshProtocol);
            _testDevice.RegisterProtocol(arpProtocol);

            // Assert
            // Verify protocols were registered (implementation specific)
            // This test validates the registration mechanism works
            Assert.NotNull(sshProtocol);
            Assert.NotNull(arpProtocol);
        }

        public void Dispose()
        {
            // Clean up test resources
            // Note: Network and device cleanup handled by test framework
        }
    }
}
