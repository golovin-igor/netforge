using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class CommandHistoryTests
    {
        [Fact]
        public async Task CommandHistoryShouldTrackExecutedCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("hostname TestDevice");
            
            // Assert
            var history = device.GetCommandHistory();
            Assert.Equal(3, history.Count);
        }
        
        [Fact]
        public async Task HistoryCommandShouldDisplayRecentCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("show version");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var result = await device.ProcessCommandAsync("history");
            
            // Assert
            Assert.Contains("enable", result);
            Assert.Contains("show version", result);
            Assert.Contains("configure terminal", result);
            Assert.Contains("TestRouter", result); // Should include prompt
        }
        
        [Fact]
        public async Task HistoryRecallDoubleExclamationShouldRecallLastCommand()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("show version");
            
            // Act
            var result = await device.ProcessCommandAsync("!!");
            
            // Assert
            Assert.Contains("Recalled: show version", result);
            Assert.Contains("TestRouter", result);
        }
        
        [Fact]
        public async Task HistoryRecallByNumberShouldRecallSpecificCommand()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");           // Command 1
            await device.ProcessCommandAsync("show version");     // Command 2
            await device.ProcessCommandAsync("configure terminal"); // Command 3
            
            // Act
            var result = await device.ProcessCommandAsync("!2");
            
            // Assert
            Assert.Contains("Recalled: show version", result);
        }
        
        [Fact]
        public async Task HistorySearchShouldFindMatchingCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("show version");
            await device.ProcessCommandAsync("show interfaces");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("show ip route");
            
            // Act
            var result = await device.ProcessCommandAsync("history search show");
            
            // Assert
            Assert.Contains("Found 3 commands containing 'show'", result);
            Assert.Contains("show version", result);
            Assert.Contains("show interfaces", result);
            Assert.Contains("show ip route", result);
        }
        
        [Fact]
        public async Task HistoryStatsShouldDisplayStatistics()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("show version");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var result = await device.ProcessCommandAsync("history stats");
            
            // Assert
            Assert.Contains("Command History Statistics:", result);
            Assert.Contains("Total commands:", result);
            Assert.Contains("Successful:", result);
        }
        
        [Fact]
        public async Task HistoryClearShouldClearAllHistory()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("show version");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var result = await device.ProcessCommandAsync("history clear");
            
            // Assert
            Assert.Contains("Cleared", result);
            Assert.Contains("commands from history", result);
        }
        
        [Fact]
        public async Task CommandShortcutsShouldExpandProperly()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act & Assert - Test common shortcuts
            var result1 = await device.ProcessCommandAsync("conf t");
            Assert.Contains("Enter configuration commands", result1);
        }
    }
} 
