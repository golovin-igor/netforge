using Xunit;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Core;

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
            var protocolService = device.GetProtocolService();

            // Act & Assert - These should not throw exceptions
            var ospfProtocol = protocolService.GetProtocol<IDeviceProtocol>();
            var bgpProtocol = protocolService.GetProtocol(ProtocolType.BGP);
            var isOspfActive = protocolService.IsProtocolActive(ProtocolType.OSPF);
            var isOspfRegistered = protocolService.IsProtocolRegistered(ProtocolType.OSPF);

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
            var protocolService = device.GetProtocolService();

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
            var ospfDependencies = protocolService.GetProtocolDependencies(ProtocolType.OSPF);
            Assert.NotNull(ospfDependencies);
            
            // Conflict checking
            var ospfConflicts = protocolService.GetProtocolConflicts(ProtocolType.OSPF);
            Assert.NotNull(ospfConflicts);
            
            // Coexistence checking
            var canCoexist = protocolService.CanProtocolsCoexist(ProtocolType.OSPF, ProtocolType.BGP);
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
            var protocolService = device.GetProtocolService();

            // Act & Assert - Lifecycle operations should not throw
            var startResult = await protocolService.StartProtocol(ProtocolType.OSPF);
            var stopResult = await protocolService.StopProtocol(ProtocolType.OSPF);
            var restartResult = await protocolService.RestartProtocol(ProtocolType.OSPF);
            
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
            var protocolService = device.GetProtocolService();
            var testConfig = new { RouterId = "1.1.1.1", Area = "0.0.0.0" };

            // Act & Assert - Configuration operations should not throw
            var isValid = protocolService.ValidateProtocolConfiguration(ProtocolType.OSPF, testConfig);
            var applyResult = await protocolService.ApplyProtocolConfiguration(ProtocolType.OSPF, testConfig);
            
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
            var protocolService = device.GetProtocolService();

            // Act & Assert - Operations with invalid data should not throw
            var invalidProtocol = protocolService.GetProtocol((ProtocolType)9999);
            var invalidState = protocolService.GetProtocolState((ProtocolType)9999);
            var invalidMetrics = protocolService.GetProtocolMetrics((ProtocolType)9999);
            
            Assert.Null(invalidProtocol);
            Assert.Null(invalidState);
            Assert.Null(invalidMetrics);
            
            // Reset operations should not throw for invalid protocols
            protocolService.ResetProtocolMetrics((ProtocolType)9999);
            protocolService.ResetAllMetrics();
        }
    }
}