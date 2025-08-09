using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Extreme
{
    public class ExtremeCommandHandlerTests
    {
        [Fact]
        public void ExtremeHandler_ShowCommand_ShouldDisplayInfo()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("ExtremeXOS", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureCommand_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
            Assert.Equal("TestRouter(config)#", output);
        }

        [Fact]
        public void ExtremeHandler_ExitCommand_ShouldExitConfigMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("exit");
            
            // Assert
            Assert.Equal("operational", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public void ExtremeHandler_PingCommand_ShouldExecutePing()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_TracerouteCommand_ShouldExecuteTraceroute()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("traceroute 192.168.1.1");
            
            // Assert
            Assert.Contains("traceroute to 192.168.1.1", output);
            Assert.Contains("hops", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_InterfaceCommand_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("interface 1");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestRouter(config-if-1)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-if-1)#", output);
        }

        [Fact]
        public void ExtremeHandler_EndCommand_ShouldExitToOperationalMode()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            
            // Act
            var output = device.ProcessCommand("end");
            
            // Assert
            Assert.Equal("operational", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public void ExtremeHandler_SaveCommand_ShouldSaveConfiguration()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("configure snmp sysName TestRouter2");
            
            // Act
            var output = device.ProcessCommand("save");
            
            // Assert
            Assert.Contains("Configuration saved", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowConfigCommand_ShouldShowConfig()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("configure snmp sysName TestRouter2");
            
            // Act
            var output = device.ProcessCommand("show config");
            
            // Assert
            Assert.Contains("configure snmp sysName TestRouter2", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_UnconfigureCommand_ShouldRemoveConfiguration()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("configure snmp sysName TestRouter2");
            
            // Act
            var output = device.ProcessCommand("unconfigure snmp sysName");
            
            // Assert
            Assert.Contains("Configuration removed", output);
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("* TestRouter.1 # ", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_WithIncompleteCommand_ShouldReturnError()
        {
            // Arrange
            var device = new ExtremeDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("* TestRouter.1 # ", device.GetPrompt());
        }

    }
} 
