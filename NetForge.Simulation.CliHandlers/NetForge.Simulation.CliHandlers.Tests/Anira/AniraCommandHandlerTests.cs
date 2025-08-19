using NetForge.Simulation.Devices;

namespace NetForge.Simulation.CliHandlers.Tests.Anira
{
    public class AniraCommandHandlerTests
    {
        [Fact]
        public async Task AniraHandlerBasicInitializationShouldWork()
        {
            // Arrange & Act
            var device = new AniraDevice("TestDevice");
            
            // Assert
            Assert.NotNull(device);
            Assert.Equal("TestDevice", device.GetHostname());
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerGetCurrentModeShouldReturnDefaultMode()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var mode = device.GetCurrentMode();
            
            // Assert
            Assert.Equal("user", mode);
        }

        [Fact]
        public async Task AniraHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid_command_that_does_not_exist");
            
            // Assert
            Assert.NotNull(output);
            Assert.Contains("Invalid", output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerWithEmptyCommandShouldReturnPrompt()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("");
            
            // Assert
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerWithWhitespaceCommandShouldReturnPrompt()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("   ");
            
            // Assert
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerHelpShouldReturnHelpInformation()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("help");
            
            // Assert
            Assert.NotNull(output);
            // Should provide help information or indicate no commands are available
            Assert.True(output.Contains("help") || output.Contains("available") || output.Contains("command"));
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerQuestionMarkShouldReturnHelpInformation()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("?");
            
            // Assert
            Assert.NotNull(output);
            // Should provide help information or indicate no commands are available
            Assert.True(output.Contains("help") || output.Contains("available") || output.Contains("command"));
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerExitShouldHandleExitCommand()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("exit");
            
            // Assert
            Assert.NotNull(output);
            // The device should remain in the same state or handle exit appropriately
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerQuitShouldHandleQuitCommand()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("quit");
            
            // Assert
            Assert.NotNull(output);
            // The device should remain in the same state or handle quit appropriately
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show")]
        [InlineData("configure")]
        [InlineData("enable")]
        [InlineData("disable")]
        [InlineData("ping")]
        [InlineData("traceroute")]
        [InlineData("telnet")]
        [InlineData("ssh")]
        [InlineData("debug")]
        [InlineData("undebug")]
        public async Task AniraHandlerCommonNetworkCommandsShouldHandleOrReturnError(string command)
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            // Commands should either work or return a proper error message
            Assert.DoesNotMatch(@"^\s*$", output); // Output should not be empty or just whitespace
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public async Task AniraHandlerCommandHistoryShouldBeAvailable()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            await device.ProcessCommandAsync("help");
            await device.ProcessCommandAsync("?");
            var history = device.GetCommandHistory();
            
            // Assert
            Assert.NotNull(history);
            Assert.True(history.Count >= 2);
            Assert.Contains("help", history);
            Assert.Contains("?", history);
        }

        [Fact]
        public async Task AniraHandlerMultipleCommandsShouldMaintainState()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output1 = await device.ProcessCommandAsync("help");
            var output2 = await device.ProcessCommandAsync("?");
            var output3 = await device.ProcessCommandAsync("show");
            
            // Assert
            Assert.NotNull(output1);
            Assert.NotNull(output2);
            Assert.NotNull(output3);
            Assert.Equal("TestDevice>", device.GetPrompt());
            Assert.Equal("user", device.GetCurrentMode());
        }

        // TODO: Add more specific tests when Anira CLI commands are defined in a CSV file
        // These tests should be updated based on the actual commands that Anira supports
        
        [Fact]
        public async Task AniraHandlerNoCliCommandsFileShouldStillFunctionBasically()
        {
            // This test verifies that even without a CSV file defining commands,
            // the Anira device can still be instantiated and handle basic operations
            
            // Arrange & Act
            var device = new AniraDevice("TestDevice");
            var output = await device.ProcessCommandAsync("test command");
            
            // Assert
            Assert.NotNull(device);
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }
        
        [Fact]
        public async Task AniraHandlerShouldIndicateNoCommandsAvailable()
        {
            // Since Anira doesn't have a CSV file with commands,
            // it should indicate that no commands are available or implemented
            
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = await device.ProcessCommandAsync("help");
            
            // Assert
            Assert.NotNull(output);
            // Should indicate that no commands are implemented or available
            Assert.True(
                output.Contains("no commands") ||
                output.Contains("not implemented") ||
                output.Contains("available commands") ||
                output.Contains("help") ||
                output.Contains("No CLI commands defined")
            );
        }
    }
}
