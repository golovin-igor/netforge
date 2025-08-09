using NetSim.Simulation.Devices;
using Xunit;
// For DeviceMode
// For NetworkDevice, CiscoDevice etc.

// Added for Xunit

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class CommonCommandHandlerTests
    {
        [Fact]
        public void CommonEnableHandler_ShouldEnterPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("enable");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public void CommonEnableHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("en");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public void CommonEnableHandler_WhenAlreadyPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable"); // Enter privileged mode first
            
            // Act
            var output = device.ProcessCommand("enable");
            
            // Assert
            // Most handlers return empty string when already in privileged mode
            Assert.Equal("TestRouter#", output);
            Assert.Equal("privileged", device.GetCurrentMode());
        }

        [Fact]
        public void CommonDisableHandler_ShouldExitPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable"); // Enter privileged mode first
            
            // Act
            var output = device.ProcessCommand("disable");
            
            // Assert
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", device.GetPrompt());
            Assert.Equal("TestRouter>", output);
        }

        [Fact]
        public void CommonDisableHandler_WhenNotPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("disable");
            
            // Assert
            // Disable from user mode typically just returns the prompt
            Assert.Equal("TestRouter>", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public void CommonPingHandler_ShouldExecutePing()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1");
            
            // Assert
            // In test environment without network initialization, expect network error
            Assert.Contains("Network not initialized", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public void CommonPingHandler_WithInvalidDestination_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping invalid-ip");
            
            // Assert
            Assert.Contains("Invalid IP address", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public void CommonPingHandler_WithoutDestination_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Contains("Usage: ping <destination>", output);
            Assert.Contains("TestRouter>", output);
        }

        [Fact]
        public void CommonExitHandler_ShouldExitCurrentMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act & Assert - Exit interface mode
            var output1 = device.ProcessCommand("exit");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestRouter(config)#", output1);
            
            // Exit config mode
            var output2 = device.ProcessCommand("exit");
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", output2);
            
            // Exit privileged mode
            var output3 = device.ProcessCommand("exit");
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", output3);
        }

        [Fact]
        public void CommonExitHandler_FromUserMode_ShouldStayInUserMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("exit");
            
            // Assert
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", output);
        }
    }
} 
