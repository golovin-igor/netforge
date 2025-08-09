using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CommandHandlers.Common
{
    public class CommonHistoryHandlerTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly JuniperDevice _juniperDevice;

        public CommonHistoryHandlerTests()
        {
            _testDevice = new CiscoDevice("TestRouter");
            _juniperDevice = new JuniperDevice("JuniperRouter");
        }

        [Fact]
        public void CommonHistoryCommandHandler_WithEmptyHistory_ShouldReturnEmptyMessage()
        {
            // Act
            var result = _testDevice.ProcessCommand("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("No commands in history", result);
        }

        [Fact]
        public void CommonHistoryCommandHandler_WithCommandHistory_ShouldDisplayHistory()
        {
            // Arrange - Add some commands to history
            _testDevice.ProcessCommand("show version");
            _testDevice.ProcessCommand("show interfaces");
            _testDevice.ProcessCommand("configure terminal");

            // Act
            var result = _testDevice.ProcessCommand("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("show version", result);
            Assert.Contains("show interfaces", result);
            Assert.Contains("configure terminal", result);
        }

        [Fact]
        public void CommonHistoryRecallHandler_WithEmptyHistory_ShouldReturnError()
        {
            // Act
            var result = _testDevice.ProcessCommand("!!");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("No history available", result);
        }

        [Fact]
        public void CommonHistoryRecallHandler_WithHistory_ShouldRecallLastCommand()
        {
            // Arrange - Add a command to history
            _testDevice.ProcessCommand("show version");

            // Act
            var result = _testDevice.ProcessCommand("!!");

            // Assert
            Assert.NotNull(result);
            // The result should contain the output of "show version" command
            Assert.Contains("IOS", result);
        }

        [Fact]
        public void CommonHistoryNumberRecallHandler_WithInvalidNumber_ShouldReturnError()
        {
            // Act
            var result = _testDevice.ProcessCommand("! 999");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("History number out of range", result);
        }

        [Fact]
        public void CommonHistoryNumberRecallHandler_WithValidNumber_ShouldRecallCommand()
        {
            // Arrange
            _testDevice.ProcessCommand("show version");
            _testDevice.ProcessCommand("show interfaces");

            // Act
            var result = _testDevice.ProcessCommand("! 1");

            // Assert
            Assert.NotNull(result);
            // Should recall the first command (show version)
            Assert.Contains("IOS", result);
        }

        [Fact]
        public void CommonHistoryNumberRecallHandler_WithInvalidFormat_ShouldReturnError()
        {
            // Act
            var result = _testDevice.ProcessCommand("! abc");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid history number", result);
        }

        [Fact]
        public void CommonHistoryNumberRecallHandler_WithMissingParameter_ShouldReturnError()
        {
            // Act
            var result = _testDevice.ProcessCommand("!");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Incomplete command", result);
        }

        [Fact]
        public void CommonHistoryHandlers_ShouldBeVendorAgnostic()
        {
            // Act
            var ciscoResult = _testDevice.ProcessCommand("history");
            var juniperResult = _juniperDevice.ProcessCommand("history");

            // Assert
            Assert.NotNull(ciscoResult);
            Assert.NotNull(juniperResult);
            
            // Both should work regardless of vendor
            Assert.Contains("No commands in history", ciscoResult);
            Assert.Contains("No commands in history", juniperResult);
        }

        [Fact]
        public void CommonHistoryHandlers_ShouldHandleDeviceProcessingErrors()
        {
            // Arrange - Add a command that would cause an error when processed
            _testDevice.ProcessCommand("invalid-command-that-fails");

            // Act
            var result = _testDevice.ProcessCommand("!!");

            // Assert
            Assert.NotNull(result);
            // Should still attempt to recall the command, even if it might fail
        }

        [Fact]
        public void CommonHistoryHandlers_ShouldWorkWithLongHistory()
        {
            // Arrange - Add many commands to test history limit handling
            for (int i = 1; i <= 50; i++)
            {
                _testDevice.ProcessCommand($"show test-command-{i}");
            }

            // Act
            var result = _testDevice.ProcessCommand("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("show test-command-", result);
            
            // Should display history without errors
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length > 0);
        }
    }
} 
