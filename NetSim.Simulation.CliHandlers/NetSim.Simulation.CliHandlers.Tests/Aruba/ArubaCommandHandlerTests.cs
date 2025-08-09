using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaCommandHandlerTests
    {
        [Fact]
        public async Task ConfigureHandler_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("configure");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }

        [Fact]
        public async Task ConfigureHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("config");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }

        [Fact]
        public async Task ConfigureHandler_WhenNotInManagerMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("configure");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public async Task HostnameHandler_ShouldChangeHostname()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch", device.GetHostname());
            Assert.Equal("NewSwitch(config)#", output);
        }

        [Fact]
        public async Task HostnameHandler_WhenNotInConfigMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("TestSwitch", device.GetHostname());
        }

        [Fact]
        public async Task HostnameHandler_WithoutName_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("TestSwitch", device.GetHostname());
        }

        [Fact]
        public async Task ExitHandler_ShouldExitCurrentMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act & Assert - Exit interface mode
            var output1 = await device.ProcessCommandAsync("exit");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", output1);
            
            // Exit config mode
            var output2 = await device.ProcessCommandAsync("exit");
            Assert.Equal("manager", device.GetCurrentMode());
            Assert.Equal("TestSwitch#", output2);
        }

        [Fact]
        public async Task ExitHandler_FromManagerMode_ShouldStayInManagerMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("exit");
            
            // Assert
            Assert.Equal("manager", device.GetCurrentMode());
            Assert.Equal("TestSwitch#", output);
        }

        [Fact]
        public async Task WriteHandler_ShouldSaveConfiguration()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("write memory");
            
            // Assert
            Assert.Contains("Configuration saved successfully", output);
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task WriteHandler_WithoutMemory_ShouldStillSave()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("write");
            
            // Assert
            Assert.Contains("Configuration saved successfully", output);
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task ReloadHandler_ShouldPromptForConfirmation()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("reload");
            
            // Assert
            Assert.Contains("System configuration has been modified", output);
            Assert.Contains("Save?", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ReloadHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("reboot");
            
            // Assert
            Assert.Contains("System configuration has been modified", output);
            Assert.Contains("Save?", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ReloadHandler_WhenNotInManagerMode_ShouldReturnError()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("reload");
            
            // Assert
            Assert.Contains("Invalid input:", output);
            Assert.Equal("user", device.GetCurrentMode());
        }
    }
} 
