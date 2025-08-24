using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CommandHandlers.Common
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
        public async Task CommonHistoryCommandHandlerWithEmptyHistoryShouldReturnEmptyMessage()
        {
            // Act
            var result = await _testDevice.ProcessCommandAsync("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("No commands in history", result);
        }

        [Fact]
        public async Task CommonHistoryCommandHandlerWithCommandHistoryShouldDisplayHistory()
        {
            // Arrange - Add some commands to history
            await _testDevice.ProcessCommandAsync("show version");
            await _testDevice.ProcessCommandAsync("show interfaces");
            await _testDevice.ProcessCommandAsync("configure terminal");

            // Act
            var result = await _testDevice.ProcessCommandAsync("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("show version", result);
            Assert.Contains("show interfaces", result);
            Assert.Contains("configure terminal", result);
        }

        [Fact]
        public async Task CommonHistoryRecallHandlerWithEmptyHistoryShouldReturnError()
        {
            // Act
            var result = await _testDevice.ProcessCommandAsync("!!");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("No history available", result);
        }

        [Fact]
        public async Task CommonHistoryRecallHandlerWithHistoryShouldRecallLastCommand()
        {
            // Arrange - Add a command to history
            await _testDevice.ProcessCommandAsync("show version");

            // Act
            var result = await _testDevice.ProcessCommandAsync("!!");

            // Assert
            Assert.NotNull(result);
            // The result should contain the output of "show version" command
            Assert.Contains("IOS", result);
        }

        [Fact]
        public async Task CommonHistoryNumberRecallHandlerWithInvalidNumberShouldReturnError()
        {
            // Act
            var result = await _testDevice.ProcessCommandAsync("! 999");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("History number out of range", result);
        }

        [Fact]
        public async Task CommonHistoryNumberRecallHandlerWithValidNumberShouldRecallCommand()
        {
            // Arrange
            await _testDevice.ProcessCommandAsync("show version");
            await _testDevice.ProcessCommandAsync("show interfaces");

            // Act
            var result = await _testDevice.ProcessCommandAsync("! 1");

            // Assert
            Assert.NotNull(result);
            // Should recall the first command (show version)
            Assert.Contains("IOS", result);
        }

        [Fact]
        public async Task CommonHistoryNumberRecallHandlerWithInvalidFormatShouldReturnError()
        {
            // Act
            var result = await _testDevice.ProcessCommandAsync("! abc");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid history number", result);
        }

        [Fact]
        public async Task CommonHistoryNumberRecallHandlerWithMissingParameterShouldReturnError()
        {
            // Act
            var result = await _testDevice.ProcessCommandAsync("!");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Incomplete command", result);
        }

        [Fact]
        public async Task CommonHistoryHandlersShouldBeVendorAgnostic()
        {
            // Act
            var ciscoResult = await _testDevice.ProcessCommandAsync("history");
            var juniperResult = await _juniperDevice.ProcessCommandAsync("history");

            // Assert
            Assert.NotNull(ciscoResult);
            Assert.NotNull(juniperResult);

            // Both should work regardless of vendor
            Assert.Contains("No commands in history", ciscoResult);
            Assert.Contains("No commands in history", juniperResult);
        }

        [Fact]
        public async Task CommonHistoryHandlersShouldHandleDeviceProcessingErrors()
        {
            // Arrange - Add a command that would cause an error when processed
            await _testDevice.ProcessCommandAsync("invalid-command-that-fails");

            // Act
            var result = await _testDevice.ProcessCommandAsync("!!");

            // Assert
            Assert.NotNull(result);
            // Should still attempt to recall the command, even if it might fail
        }

        [Fact]
        public async Task CommonHistoryHandlersShouldWorkWithLongHistory()
        {
            // Arrange - Add many commands to test history limit handling
            for (int i = 1; i <= 50; i++)
            {
                await _testDevice.ProcessCommandAsync($"show test-command-{i}");
            }

            // Act
            var result = await _testDevice.ProcessCommandAsync("history");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("show test-command-", result);

            // Should display history without errors
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length > 0);
        }
    }
}
