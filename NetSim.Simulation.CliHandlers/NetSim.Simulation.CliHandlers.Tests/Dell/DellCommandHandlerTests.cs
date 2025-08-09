using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Dell
{
    public class DellCommandHandlerTests
    {
        [Fact]
        public void DellOperationalHandler_RequestCommand_ShouldProcessRequest()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("request system reload");
            
            // Assert
            Assert.Contains("System will be reloaded", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellOperationalHandler_RequestCommand_WithoutParameters_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("request");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellOperationalHandler_FileCommand_ShouldProcessFile()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("file show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellOperationalHandler_BootCommand_ShouldProcessBoot()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("boot system");
            
            // Assert
            Assert.Contains("System boot information", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellOperationalHandler_SystemCommand_ShouldProcessSystem()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("system show");
            
            // Assert
            Assert.Contains("System information", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellOperationalHandler_WhenNotPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("request system reload");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public void DellPingHandler_ShouldExecutePing()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void DellPingHandler_WithCount_ShouldUseSpecifiedCount()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1 -c 3");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("3 packets transmitted", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void DellPingHandler_WithSize_ShouldUseSpecifiedSize()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1 -s 100");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("100 bytes", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void DellPingHandler_WithInvalidIp_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping invalid-ip");
            
            // Assert
            Assert.Contains("Invalid IP address", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void DellPingHandler_WithoutDestination_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Contains("Usage: ping <destination>", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }
    }
} 
