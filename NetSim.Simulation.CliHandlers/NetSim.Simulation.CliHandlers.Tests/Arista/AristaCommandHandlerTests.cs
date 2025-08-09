using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaCommandHandlerTests
    {
        [Fact]
        public void ShowVersionHandler_ShouldReturnVersionInfo()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("Arista DCS-7050TX-64", output);
            Assert.Contains("Software image version: 4.25.3F", output);
            Assert.Contains("TestSwitch>", output); // Should have prompt at end
        }
        
        [Fact]
        public void PingHandler_ShouldSimulatePing()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("5 packets transmitted", output);
            Assert.Contains("packet loss", output);
            Assert.Contains("TestSwitch>", output); // Should have prompt at end
        }
        
        [Fact]
        public void EnableHandler_ShouldChangeModeCorrectly()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act & Assert - Start in user mode
            Assert.Equal("TestSwitch>", device.GetPrompt());
            Assert.Equal("user", device.GetMode());
            
            var output = device.ProcessCommand("enable");
            
            // Check that mode changed
            Assert.Equal("privileged", device.GetMode());
            Assert.Equal("TestSwitch#", device.GetPrompt());
            Assert.Equal("TestSwitch#", output); // Output should be just the new prompt
        }
        
        [Fact]
        public void ConfigureHandler_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Equal("config", device.GetMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            Assert.Equal("TestSwitch(config)#", output);
        }
        
        [Fact]
        public void HelpCommand_ShouldShowAvailableCommands()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("?");
            
            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Contains("enable", output);
            Assert.Contains("show", output);
            Assert.Contains("ping", output);
        }
        
        [Fact]
        public void ShowHelp_ShouldShowSubcommands()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show ?");
            
            // Assert
            Assert.Contains("running-config", output);
            Assert.Contains("version", output);
            Assert.Contains("interfaces", output);
            Assert.Contains("vlan", output);
            Assert.Contains("ip", output);
            Assert.Contains("lldp", output); // Arista uses LLDP instead of CDP
        }
        
        [Fact]
        public void InvalidCommand_ShouldFallbackToLegacy()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("Invalid input", output);
        }
        
        [Fact]
        public void WriteMemory_ShouldSaveConfig()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("write memory");
            
            // Assert
            Assert.Contains("Copy completed successfully", output);
        }
        
        [Fact]
        public void CopyRunningStartup_ShouldSaveConfig()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand("copy running-config startup-config");
            
            // Assert
            Assert.Contains("Copy completed successfully", output);
        }
        
        [Fact]
        public void ShowLldpNeighbors_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show lldp neighbors");
            
            // Assert
            // Should not error, even if no neighbors
            Assert.Contains("TestSwitch>", output);
        }
    }
} 
