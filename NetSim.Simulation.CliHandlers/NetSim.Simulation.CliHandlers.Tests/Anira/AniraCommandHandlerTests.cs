using NetSim.Simulation.Devices;

namespace NetSim.Simulation.CliHandlers.Tests.Anira
{
    public class AniraCommandHandlerTests
    {
        [Fact]
        public void AniraHandler_BasicInitialization_ShouldWork()
        {
            // Arrange & Act
            var device = new AniraDevice("TestDevice");
            
            // Assert
            Assert.NotNull(device);
            Assert.Equal("TestDevice", device.GetHostname());
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_GetCurrentMode_ShouldReturnDefaultMode()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var mode = device.GetCurrentMode();
            
            // Assert
            Assert.Equal("user", mode);
        }

        [Fact]
        public void AniraHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("invalid_command_that_does_not_exist");
            
            // Assert
            Assert.NotNull(output);
            Assert.Contains("Invalid", output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_WithEmptyCommand_ShouldReturnPrompt()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("");
            
            // Assert
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_WithWhitespaceCommand_ShouldReturnPrompt()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("   ");
            
            // Assert
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_Help_ShouldReturnHelpInformation()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("help");
            
            // Assert
            Assert.NotNull(output);
            // Should provide help information or indicate no commands are available
            Assert.True(output.Contains("help") || output.Contains("available") || output.Contains("command"));
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_QuestionMark_ShouldReturnHelpInformation()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("?");
            
            // Assert
            Assert.NotNull(output);
            // Should provide help information or indicate no commands are available
            Assert.True(output.Contains("help") || output.Contains("available") || output.Contains("command"));
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_Exit_ShouldHandleExitCommand()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("exit");
            
            // Assert
            Assert.NotNull(output);
            // The device should remain in the same state or handle exit appropriately
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_Quit_ShouldHandleQuitCommand()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("quit");
            
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
        public void AniraHandler_CommonNetworkCommands_ShouldHandleOrReturnError(string command)
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            // Commands should either work or return a proper error message
            Assert.DoesNotMatch(@"^\s*$", output); // Output should not be empty or just whitespace
            Assert.Equal("TestDevice>", device.GetPrompt());
        }

        [Fact]
        public void AniraHandler_CommandHistory_ShouldBeAvailable()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            device.ProcessCommand("help");
            device.ProcessCommand("?");
            var history = device.GetCommandHistory();
            
            // Assert
            Assert.NotNull(history);
            Assert.True(history.Count >= 2);
            Assert.Contains("help", history);
            Assert.Contains("?", history);
        }

        [Fact]
        public void AniraHandler_MultipleCommands_ShouldMaintainState()
        {
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output1 = device.ProcessCommand("help");
            var output2 = device.ProcessCommand("?");
            var output3 = device.ProcessCommand("show");
            
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
        public void AniraHandler_NoCliCommandsFile_ShouldStillFunctionBasically()
        {
            // This test verifies that even without a CSV file defining commands,
            // the Anira device can still be instantiated and handle basic operations
            
            // Arrange & Act
            var device = new AniraDevice("TestDevice");
            var output = device.ProcessCommand("test command");
            
            // Assert
            Assert.NotNull(device);
            Assert.NotNull(output);
            Assert.Equal("TestDevice>", device.GetPrompt());
        }
        
        [Fact]
        public void AniraHandler_ShouldIndicateNoCommandsAvailable()
        {
            // Since Anira doesn't have a CSV file with commands,
            // it should indicate that no commands are available or implemented
            
            // Arrange
            var device = new AniraDevice("TestDevice");
            
            // Act
            var output = device.ProcessCommand("help");
            
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
