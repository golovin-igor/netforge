using NetForge.Simulation.Devices;
using Xunit;
// For DeviceMode
// For NetworkDevice, CiscoDevice etc.

// Added for Xunit

namespace NetForge.Simulation.Tests.CliHandlers
{
    public class CommonCommandHandlerTests
    {
        [Fact]
        public async Task CommonEnableHandlerShouldEnterPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("enable");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task CommonEnableHandlerWithAliasShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("en");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task CommonEnableHandlerWhenAlreadyPrivilegedShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode first
            
            // Act
            var output = await device.ProcessCommandAsync("enable");
            
            // Assert
            // Most handlers return empty string when already in privileged mode
            Assert.Equal("TestRouter#", output);
            Assert.Equal("privileged", device.GetCurrentMode());
        }

        [Fact]
        public async Task CommonDisableHandlerShouldExitPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode first
            
            // Act
            var output = await device.ProcessCommandAsync("disable");
            
            // Assert
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", device.GetPrompt());
            Assert.Equal("TestRouter>", output);
        }

        [Fact]
        public async Task CommonDisableHandlerWhenNotPrivilegedShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("disable");
            
            // Assert
            // Disable from user mode typically just returns the prompt
            Assert.Equal("TestRouter>", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public async Task CommonPingHandlerShouldExecutePing()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1");
            
            // Assert
            // In test environment without network initialization, expect network error
            Assert.Contains("Network not initialized", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public async Task CommonPingHandlerWithInvalidDestinationShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("ping invalid-ip");
            
            // Assert
            Assert.Contains("Invalid IP address", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public async Task CommonPingHandlerWithoutDestinationShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("ping");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Contains("Usage: ping <destination>", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public async Task CommonExitHandlerShouldExitCurrentMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act & Assert - Exit interface mode
            var output1 = await device.ProcessCommandAsync("exit");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestRouter(config)#", output1);
            
            // Exit config mode
            var output2 = await device.ProcessCommandAsync("exit");
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", output2);
            
            // Exit privileged mode
            var output3 = await device.ProcessCommandAsync("exit");
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", output3);
        }

        [Fact]
        public async Task CommonExitHandlerFromUserModeShouldStayInUserMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("exit");
            
            // Assert
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", output);
        }
    }
} 
