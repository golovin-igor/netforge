using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.MikroTik
{
    public class MikroTikCommandHandlerTests
    {
        [Fact]
        public void RouterOSHandler_ShouldHandlePathCommands()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("/system identity print");
            
            // Assert
            Assert.Contains("name: TestRouter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_ShouldHandlePingCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_ShouldHandleQuitCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            device.ProcessCommand("/interface ethernet");
            
            // Act
            var output = device.ProcessCommand("quit");
            
            // Assert
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
            Assert.Equal("", output);
        }

        [Fact]
        public void RouterOSHandler_ShouldHandleHelpCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("?");
            
            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_ShouldHandleExportCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            device.ProcessCommand("/system identity set name=TestRouter2");
            
            // Act
            var output = device.ProcessCommand("export");
            
            // Assert
            Assert.Contains("/system identity", output);
            Assert.Contains("name=TestRouter2", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_ShouldHandlePutCommand()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("put [/system identity get name]");
            
            // Assert
            Assert.Contains("TestRouter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("/invalid/path");
            
            // Assert
            Assert.Contains("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_WithIncompleteCommand_ShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("/system identity");
            
            // Assert
            Assert.Contains("expected end of command", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public void RouterOSHandler_WithInvalidParameter_ShouldReturnError()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("/system identity set invalid=value");
            
            // Assert
            Assert.Contains("bad parameter", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }
    }
} 
