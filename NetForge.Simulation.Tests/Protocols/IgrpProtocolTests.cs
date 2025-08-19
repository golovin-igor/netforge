using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Events;
using NetForge.Simulation.Protocols.Implementations;
using NetForge.Simulation.Protocols.Routing;
using Xunit;

namespace NetForge.Simulation.Tests.Protocols
{
    /// <summary>
    /// LEGACY TESTS: IGRP protocol tests for old implementation in NetForge.Simulation.Common.
    /// These tests are for the legacy IGRP implementation that will be migrated to the new protocol architecture.
    /// Once IGRP is migrated to NetForge.Simulation.Protocols.IGRP, create new tests in NetForge.Simulation.Protocols.Tests.
    /// </summary>
    [Trait("Category", "Legacy")]
    public class IgrpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly CiscoDevice _device;
        private readonly IgrpProtocol _igrpProtocol;

        public IgrpProtocolTests()
        {
            _network = new Network();
            _device = new CiscoDevice("TestRouter");
            _network.AddDeviceAsync(_device).Wait();
            _igrpProtocol = new IgrpProtocol();
        }

        public void Dispose()
        {
            // No explicit disposal needed for test devices
        }

        [Fact]
        public void IgrpProtocol_Initialize_ShouldLogConfigurationNotFound()
        {
            // Arrange & Act
            _igrpProtocol.Initialize(_device);

            // Assert
            var logs = _device.GetLogEntries();
            Assert.Contains(logs, log => log.Contains("IGRP configuration not found"));
        }

        [Fact]
        public void IgrpProtocol_Initialize_WithConfiguration_ShouldLogSuccess()
        {
            // Arrange
            var config = new IgrpConfig(100);
            _device.SetIgrpConfiguration(config);

            // Act
            _igrpProtocol.Initialize(_device);

            // Assert
            var logs = _device.GetLogEntries();
            Assert.Contains(logs, log => log.Contains("Successfully initialized with IGRP AS 100"));
        }

        [Fact]
        public async Task IgrpProtocol_UpdateState_WithNoConfiguration_ShouldClearRoutes()
        {
            // Arrange
            _igrpProtocol.Initialize(_device);

            // Act
            await _igrpProtocol.UpdateState(_device);

            // Assert
            var logs = _device.GetLogEntries();
            Assert.Contains(logs, log => log.Contains("IGRP configuration missing or not enabled"));
        }

        [Fact]
        public async Task IgrpProtocol_UpdateState_WithConfiguration_ShouldUpdateRoutes()
        {
            // Arrange
            var config = new IgrpConfig(100);
            config.Networks.Add("192.168.1.0");
            _device.SetIgrpConfiguration(config);
            _igrpProtocol.Initialize(_device);

            // Act
            await _igrpProtocol.UpdateState(_device);

            // Assert
            var logs = _device.GetLogEntries();
            Assert.Contains(logs, log => log.Contains("Updating IGRP state for AS 100"));
        }
    }
} 
