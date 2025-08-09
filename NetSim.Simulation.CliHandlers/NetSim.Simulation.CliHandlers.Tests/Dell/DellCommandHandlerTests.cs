using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Dell
{
    public class DellCommandHandlerTests
    {
        [Fact]
        public async Task DellOperationalHandler_RequestCommand_ShouldProcessRequest()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("request system reload");
            
            // Assert
            Assert.Contains("System will be reloaded", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task DellOperationalHandler_RequestCommand_WithoutParameters_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("request");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task DellOperationalHandler_FileCommand_ShouldProcessFile()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("file show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task DellOperationalHandler_BootCommand_ShouldProcessBoot()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("boot system");
            
            // Assert
            Assert.Contains("System boot information", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task DellOperationalHandler_SystemCommand_ShouldProcessSystem()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("system show");
            
            // Assert
            Assert.Contains("System information", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task DellOperationalHandler_WhenNotPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("request system reload");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public async Task DellPingHandler_ShouldExecutePing()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task DellPingHandler_WithCount_ShouldUseSpecifiedCount()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1 -c 3");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("3 packets transmitted", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task DellPingHandler_WithSize_ShouldUseSpecifiedSize()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1 -s 100");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("100 bytes", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task DellPingHandler_WithInvalidIp_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping invalid-ip");
            
            // Assert
            Assert.Contains("Invalid IP address", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task DellPingHandler_WithoutDestination_ShouldReturnError()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Contains("Usage: ping <destination>", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }
    }
} 
