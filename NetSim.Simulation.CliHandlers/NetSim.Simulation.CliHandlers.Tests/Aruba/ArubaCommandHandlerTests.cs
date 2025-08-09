using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaCommandHandlerTests
    {
        [Fact]
        public void ConfigureHandler_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }

        [Fact]
        public void ConfigureHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("config");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }

        [Fact]
        public void ConfigureHandler_WhenNotInManagerMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public void HostnameHandler_ShouldChangeHostname()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("hostname NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch", device.GetHostname());
            Assert.Equal("NewSwitch(config)#", output);
        }

        [Fact]
        public void HostnameHandler_WhenNotInConfigMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("hostname NewSwitch");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("TestSwitch", device.GetHostname());
        }

        [Fact]
        public void HostnameHandler_WithoutName_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("hostname");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("TestSwitch", device.GetHostname());
        }

        [Fact]
        public void ExitHandler_ShouldExitCurrentMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act & Assert - Exit interface mode
            var output1 = device.ProcessCommand("exit");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", output1);
            
            // Exit config mode
            var output2 = device.ProcessCommand("exit");
            Assert.Equal("manager", device.GetCurrentMode());
            Assert.Equal("TestSwitch#", output2);
        }

        [Fact]
        public void ExitHandler_FromManagerMode_ShouldStayInManagerMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("exit");
            
            // Assert
            Assert.Equal("manager", device.GetCurrentMode());
            Assert.Equal("TestSwitch#", output);
        }

        [Fact]
        public void WriteHandler_ShouldSaveConfiguration()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("hostname NewSwitch");
            
            // Act
            var output = device.ProcessCommand("write memory");
            
            // Assert
            Assert.Contains("Configuration saved successfully", output);
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void WriteHandler_WithoutMemory_ShouldStillSave()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("hostname NewSwitch");
            
            // Act
            var output = device.ProcessCommand("write");
            
            // Assert
            Assert.Contains("Configuration saved successfully", output);
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void ReloadHandler_ShouldPromptForConfirmation()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("reload");
            
            // Assert
            Assert.Contains("System configuration has been modified", output);
            Assert.Contains("Save?", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ReloadHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("reboot");
            
            // Assert
            Assert.Contains("System configuration has been modified", output);
            Assert.Contains("Save?", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ReloadHandler_WhenNotInManagerMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("reload");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("user", device.GetCurrentMode());
        }
    }
} 
