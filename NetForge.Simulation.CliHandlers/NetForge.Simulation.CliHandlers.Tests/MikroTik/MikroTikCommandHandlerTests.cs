using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.MikroTik
{
    public class MikroTikCommandHandlerTests
    {
        [Fact]
        public async Task RouterOSHandlerShouldHandlePathCommands()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system identity print");

            // Assert
            Assert.Contains("name: TestRouter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerShouldHandlePingCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1");

            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerShouldHandleQuitCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            await device.ProcessCommandAsync("/interface ethernet");

            // Act
            var output = await device.ProcessCommandAsync("quit");

            // Assert
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
            Assert.Equal("", output);
        }

        [Fact]
        public async Task RouterOSHandlerShouldHandleHelpCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("?");

            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerShouldHandleExportCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            await device.ProcessCommandAsync("/system identity set name=TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("export");

            // Assert
            Assert.Contains("/system identity", output);
            Assert.Contains("name=TestRouter2", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerShouldHandlePutCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("put [/system identity get name]");

            // Assert
            Assert.Contains("TestRouter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerWithInvalidPathShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/invalid/path");

            // Assert
            Assert.Contains("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("invalid command");

            // Assert
            Assert.Contains("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerWithIncompleteCommandShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system identity");

            // Assert
            Assert.Contains("expected end of command", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task RouterOSHandlerWithInvalidParameterShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system identity set invalid=value");

            // Assert
            Assert.Contains("bad parameter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }
    }
}
