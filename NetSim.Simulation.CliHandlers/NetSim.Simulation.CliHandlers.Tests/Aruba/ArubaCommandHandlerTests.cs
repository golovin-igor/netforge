using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaCommandHandlerTests
    {
        [Fact]
        public async Task ConfigureHandlerShouldEnterConfigMode()
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
        public async Task ConfigureHandlerWithAliasShouldWork()
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
        public async Task ConfigureHandlerWhenNotInManagerModeShouldReturnError()
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
        public async Task HostnameHandlerShouldChangeHostname()
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
        public async Task HostnameHandlerWhenNotInConfigModeShouldReturnError()
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
        public async Task HostnameHandlerWithoutNameShouldReturnError()
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
        public async Task ExitHandlerShouldExitCurrentMode()
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
        public async Task ExitHandlerFromManagerModeShouldStayInManagerMode()
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
        public async Task WriteHandlerShouldSaveConfiguration()
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
        public async Task WriteHandlerWithoutMemoryShouldStillSave()
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
        public async Task ReloadHandlerShouldPromptForConfirmation()
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
        public async Task ReloadHandlerWithAliasShouldWork()
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
        public async Task ReloadHandlerWhenNotInManagerModeShouldReturnError()
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
