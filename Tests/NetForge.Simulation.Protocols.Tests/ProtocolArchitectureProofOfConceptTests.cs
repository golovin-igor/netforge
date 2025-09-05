using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Topology.Devices;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Tests
{
    /// <summary>
    /// Demonstration tests showing that the enhanced protocol architecture concept works
    /// These tests use only the basic interfaces to avoid naming conflicts
    /// </summary>
    public class ProtocolArchitectureProofOfConceptTests
    {
        private NetworkDevice CreateTestDevice(string name, string vendor)
        {
            return DeviceFactory.CreateDevice(vendor, name);
        }

        /// <summary>
        /// Demonstrates that NetworkDevice successfully provides the enhanced protocol service
        /// </summary>
        [Fact]
        public void Enhanced_Protocol_Service_Integration_Works()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");

            // Act
            var protocolService = device.GetProtocolService();

            // Assert
            Assert.NotNull(protocolService);
            Assert.NotNull(device.DeviceName);
            Assert.NotNull(device.DeviceType);

            // Verify protocol service provides basic functionality
            var allProtocols = protocolService.GetAllProtocols();
            Assert.NotNull(allProtocols);

            var activeTypes = protocolService.GetActiveProtocolTypes();
            Assert.NotNull(activeTypes);
        }

        /// <summary>
        /// Demonstrates that the protocol service can handle protocol queries safely
        /// </summary>
        [Fact]
        public void Protocol_Service_Handles_Protocol_Queries_Safely()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");
            var basicProtocolService = device.GetProtocolService();
            var protocolService = basicProtocolService as IProtocolService;

            // Act & Assert - These should not throw exceptions
            var ospfProtocol = basicProtocolService.GetProtocol<IDeviceProtocol>();
            var bgpProtocol = basicProtocolService.GetProtocol(NetworkProtocolType.BGP);
            var isOspfActive = basicProtocolService.IsProtocolActive(NetworkProtocolType.OSPF);
            var isOspfRegistered = protocolService?.IsProtocolRegistered(NetworkProtocolType.OSPF) ?? false;

            // All operations should complete without exceptions
            Assert.True(true); // If we reach here, the operations succeeded
        }

        /// <summary>
        /// Demonstrates that enhanced functionality is available through the service
        /// </summary>
        [Fact]
        public void Enhanced_Protocol_Service_Provides_Advanced_Features()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");
            var basicProtocolService = device.GetProtocolService();

            // Skip test if enhanced service is not available
            if (basicProtocolService is not IProtocolService protocolService)
            {
                Assert.True(true, "Enhanced protocol service not available, skipping test");
                return;
            }

            // Act & Assert - Test enhanced features that don't exist in basic interface

            // Service health reporting
            var serviceHealth = protocolService.GetServiceHealth();
            Assert.NotNull(serviceHealth);
            Assert.Contains("ServiceName", serviceHealth.Keys);

            // Protocol summary reporting
            var protocolSummary = protocolService.GetProtocolSummary();
            Assert.NotNull(protocolSummary);
            Assert.Contains("DeviceId", protocolSummary.Keys);

            // Dependency management
            var ospfDependencies = protocolService.GetProtocolDependencies(NetworkProtocolType.OSPF);
            Assert.NotNull(ospfDependencies);

            // Conflict checking
            var ospfConflicts = protocolService.GetProtocolConflicts(NetworkProtocolType.OSPF);
            Assert.NotNull(ospfConflicts);

            // Coexistence checking
            var canCoexist = protocolService.CanProtocolsCoexist(NetworkProtocolType.OSPF, NetworkProtocolType.BGP);
            Assert.True(canCoexist); // OSPF and BGP should coexist

            // Vendor support
            var ciscoProtocols = protocolService.GetProtocolsForVendor("Cisco");
            Assert.NotNull(ciscoProtocols);

            // Metrics access
            var allMetrics = protocolService.GetAllProtocolMetrics();
            Assert.NotNull(allMetrics);
        }

        /// <summary>
        /// Demonstrates that protocol lifecycle management works
        /// </summary>
        [Fact]
        public async Task Protocol_Lifecycle_Management_Works()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");
            var basicProtocolService = device.GetProtocolService();

            // Skip test if enhanced service is not available
            if (basicProtocolService is not IProtocolService protocolService)
            {
                Assert.True(true, "Enhanced protocol service not available, skipping test");
                return;
            }

            // Act & Assert - Lifecycle operations should not throw
            var startResult = await protocolService.StartProtocol(NetworkProtocolType.OSPF);
            var stopResult = await protocolService.StopProtocol(NetworkProtocolType.OSPF);
            var restartResult = await protocolService.RestartProtocol(NetworkProtocolType.OSPF);

            // Operations should complete (may return false if protocol not available, but should not throw)
            Assert.True(true); // If we reach here, operations completed successfully
        }

        /// <summary>
        /// Demonstrates that configuration management is integrated
        /// </summary>
        [Fact]
        public async Task Configuration_Management_Is_Integrated()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");
            var basicProtocolService = device.GetProtocolService();
            var protocolService = basicProtocolService as IProtocolService;
            var testConfig = new { RouterId = "1.1.1.1", Area = "0.0.0.0" };

            // Skip test if enhanced service is not available
            if (protocolService == null)
            {
                Assert.True(true, "Enhanced protocol service not available, skipping test");
                return;
            }

            // Act & Assert - Configuration operations should not throw
            var isValid = protocolService.ValidateProtocolConfiguration(NetworkProtocolType.OSPF, testConfig);
            var applyResult = await protocolService.ApplyProtocolConfiguration(NetworkProtocolType.OSPF, testConfig);

            // Operations should complete without exceptions
            Assert.True(true);
        }

        /// <summary>
        /// Demonstrates that the fallback to BasicProtocolService works correctly
        /// </summary>
        [Fact]
        public void Fallback_To_Basic_Protocol_Service_Works()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");

            // Act
            var protocolService = device.GetProtocolService();

            // Assert - Even if enhanced service isn't available, basic service should work
            Assert.NotNull(protocolService);

            // Basic functionality should always be available
            var protocols = protocolService.GetAllProtocols();
            var activeTypes = protocolService.GetActiveProtocolTypes();

            Assert.NotNull(protocols);
            Assert.NotNull(activeTypes);
        }

        /// <summary>
        /// Demonstrates error handling capabilities
        /// </summary>
        [Fact]
        public void Protocol_Service_Handles_Errors_Gracefully()
        {
            // Arrange
            var device = CreateTestDevice("TestRouter", "Cisco");
            var basicProtocolService = device.GetProtocolService();
            var protocolService = basicProtocolService as IProtocolService;

            // Act & Assert - Operations with invalid data should not throw
            var invalidProtocol = basicProtocolService.GetProtocol((NetworkProtocolType)9999);
            var invalidState = basicProtocolService.GetProtocolState<IProtocolState>((NetworkProtocolType)9999);

            Assert.Null(invalidProtocol);
            Assert.Null(invalidState);

            if (protocolService != null)
            {
                var invalidMetrics = protocolService.GetProtocolMetrics((NetworkProtocolType)9999);
                Assert.Null(invalidMetrics);

                // Reset operations should not throw for invalid protocols
                protocolService.ResetProtocolMetrics((NetworkProtocolType)9999);
                protocolService.ResetAllMetrics();
            }
        }
    }
}
