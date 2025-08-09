using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class CommandHistoryTests
    {
        [Fact]
        public void CommandHistory_ShouldTrackExecutedCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("hostname TestDevice");
            
            // Assert
            var history = device.GetCommandHistory();
            Assert.Equal(3, history.Count);
        }
        
        [Fact]
        public void HistoryCommand_ShouldDisplayRecentCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("show version");
            device.ProcessCommand("configure terminal");
            
            // Act
            var result = device.ProcessCommand("history");
            
            // Assert
            Assert.Contains("enable", result);
            Assert.Contains("show version", result);
            Assert.Contains("configure terminal", result);
            Assert.Contains("TestRouter", result); // Should include prompt
        }
        
        [Fact]
        public void HistoryRecall_DoubleExclamation_ShouldRecallLastCommand()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("show version");
            
            // Act
            var result = device.ProcessCommand("!!");
            
            // Assert
            Assert.Contains("Recalled: show version", result);
            Assert.Contains("TestRouter", result);
        }
        
        [Fact]
        public void HistoryRecall_ByNumber_ShouldRecallSpecificCommand()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");           // Command 1
            device.ProcessCommand("show version");     // Command 2
            device.ProcessCommand("configure terminal"); // Command 3
            
            // Act
            var result = device.ProcessCommand("!2");
            
            // Assert
            Assert.Contains("Recalled: show version", result);
        }
        
        [Fact]
        public void HistorySearch_ShouldFindMatchingCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("show version");
            device.ProcessCommand("show interfaces");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("show ip route");
            
            // Act
            var result = device.ProcessCommand("history search show");
            
            // Assert
            Assert.Contains("Found 3 commands containing 'show'", result);
            Assert.Contains("show version", result);
            Assert.Contains("show interfaces", result);
            Assert.Contains("show ip route", result);
        }
        
        [Fact]
        public void HistoryStats_ShouldDisplayStatistics()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("show version");
            device.ProcessCommand("configure terminal");
            
            // Act
            var result = device.ProcessCommand("history stats");
            
            // Assert
            Assert.Contains("Command History Statistics:", result);
            Assert.Contains("Total commands:", result);
            Assert.Contains("Successful:", result);
        }
        
        [Fact]
        public void HistoryClear_ShouldClearAllHistory()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("show version");
            device.ProcessCommand("configure terminal");
            
            // Act
            var result = device.ProcessCommand("history clear");
            
            // Assert
            Assert.Contains("Cleared", result);
            Assert.Contains("commands from history", result);
        }
        
        [Fact]
        public void CommandShortcuts_ShouldExpandProperly()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            
            // Act & Assert - Test common shortcuts
            var result1 = device.ProcessCommand("conf t");
            Assert.Contains("Enter configuration commands", result1);
        }
    }
} 
