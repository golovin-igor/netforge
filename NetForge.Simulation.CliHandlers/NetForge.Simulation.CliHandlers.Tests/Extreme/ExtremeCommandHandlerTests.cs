using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Extreme
{
    public class ExtremeCommandHandlerTests
    {
        [Fact]
        public async Task ExtremeHandlerShowCommandShouldDisplayInfo()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show version");

            // Assert
            Assert.Contains("ExtremeXOS", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureCommandShouldEnterConfigMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("configure");

            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
            Assert.Equal("TestRouter(config)#", output);
        }

        [Fact]
        public async Task ExtremeHandlerExitCommandShouldExitConfigMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("exit");

            // Assert
            Assert.Equal("operational", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task ExtremeHandlerPingCommandShouldExecutePing()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1");

            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerTracerouteCommandShouldExecuteTraceroute()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("traceroute 192.168.1.1");

            // Assert
            Assert.Contains("traceroute to 192.168.1.1", output);
            Assert.Contains("hops", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerInterfaceCommandShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("interface 1");

            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestRouter(config-if-1)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-if-1)#", output);
        }

        [Fact]
        public async Task ExtremeHandlerEndCommandShouldExitToOperationalMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");

            // Act
            var output = await device.ProcessCommandAsync("end");

            // Assert
            Assert.Equal("operational", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task ExtremeHandlerSaveCommandShouldSaveConfiguration()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("configure snmp sysName TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("save");

            // Assert
            Assert.Contains("Configuration saved", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowConfigCommandShouldShowConfig()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("configure snmp sysName TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("show config");

            // Assert
            Assert.Contains("configure snmp sysName TestRouter2", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerUnconfigureCommandShouldRemoveConfiguration()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("configure snmp sysName TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("unconfigure snmp sysName");

            // Assert
            Assert.Contains("Configuration removed", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("invalid command");

            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("* TestRouter.1 # ", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerWithIncompleteCommandShouldReturnError()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("configure");

            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("* TestRouter.1 # ", device.GetPrompt());
        }

    }
}
