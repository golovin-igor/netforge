using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaCommandHandlerTests
    {
        [Fact]
        public async Task ShowVersionHandlerShouldReturnVersionInfo()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Arista DCS-7050TX-64", output);
            Assert.Contains("Software image version: 4.25.3F", output);
            Assert.Contains("TestSwitch>", output); // Should have prompt at end
        }
        
        [Fact]
        public async Task PingHandlerShouldSimulatePing()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("5 packets transmitted", output);
            Assert.Contains("packet loss", output);
            Assert.Contains("TestSwitch>", output); // Should have prompt at end
        }
        
        [Fact]
        public async Task EnableHandlerShouldChangeModeCorrectly()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act & Assert - Start in user mode
            Assert.Equal("TestSwitch>", device.GetPrompt());
            Assert.Equal("user", device.GetMode());
            
            var output = await device.ProcessCommandAsync("enable");
            
            // Check that mode changed
            Assert.Equal("privileged", device.GetMode());
            Assert.Equal("TestSwitch#", device.GetPrompt());
            Assert.Equal("TestSwitch#", output); // Output should be just the new prompt
        }
        
        [Fact]
        public async Task ConfigureHandlerShouldEnterConfigMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("configure");
            
            // Assert
            Assert.Equal("config", device.GetMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }
        
        [Fact]
        public async Task HelpCommandShouldShowAvailableCommands()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("?");
            
            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Contains("enable", output);
            Assert.Contains("show", output);
            Assert.Contains("ping", output);
        }
        
        [Fact]
        public async Task ShowHelpShouldShowSubcommands()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show ?");
            
            // Assert
            Assert.Contains("running-config", output);
            Assert.Contains("version", output);
            Assert.Contains("interfaces", output);
            Assert.Contains("vlan", output);
            Assert.Contains("ip", output);
            Assert.Contains("lldp", output); // Arista uses LLDP instead of CDP
        }
        
        [Fact]
        public async Task InvalidCommandShouldFallbackToLegacy()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid command");
            
            // Assert
            Assert.Contains("Invalid input", output);
        }
        
        [Fact]
        public async Task WriteMemoryShouldSaveConfig()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("write memory");
            
            // Assert
            Assert.Contains("Copy completed successfully", output);
        }
        
        [Fact]
        public async Task CopyRunningStartupShouldSaveConfig()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("copy running-config startup-config");
            
            // Assert
            Assert.Contains("Copy completed successfully", output);
        }
        
        [Fact]
        public async Task ShowLldpNeighborsShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show lldp neighbors");
            
            // Assert
            // Should not error, even if no neighbors
            Assert.Contains("TestSwitch>", output);
        }
    }
} 
