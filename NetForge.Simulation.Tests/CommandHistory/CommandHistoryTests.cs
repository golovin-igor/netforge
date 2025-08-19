using Xunit;

namespace NetForge.Simulation.Tests.CommandHistory
{
    public class CommandHistoryTests
    {
        [Fact]
        public void CommandHistory_DefaultConstructor_ShouldInitializeWithEmptyHistory()
        {
            // Act
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Assert
            Assert.Empty(history.GetHistory());
            Assert.Equal(0, history.Count);
        }

        [Fact]
        public void AddCommand_SingleCommand_ShouldAddToHistory()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command = "show version";

            // Act
            history.AddCommand(command);

            // Assert
            Assert.Single(history.GetHistory());
            Assert.Equal(command, history.GetHistory()[0].Command);
            Assert.Equal(1, history.Count);
        }

        [Fact]
        public void AddCommand_MultipleCommands_ShouldAddInOrder()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command1 = "show version";
            var command2 = "show interfaces";
            var command3 = "show ip route";

            // Act
            history.AddCommand(command1);
            history.AddCommand(command2);
            history.AddCommand(command3);

            // Assert
            var commands = history.GetHistory();
            Assert.Equal(3, commands.Count);
            Assert.Equal(command1, commands[0].Command);
            Assert.Equal(command2, commands[1].Command);
            Assert.Equal(command3, commands[2].Command);
            Assert.Equal(3, history.Count);
        }

        [Fact]
        public void AddCommand_WithNullCommand_ShouldNotAddToHistory()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Act
            history.AddCommand(null);

            // Assert
            Assert.Empty(history.GetHistory());
            Assert.Equal(0, history.Count);
        }

        [Fact]
        public void AddCommand_WithEmptyCommand_ShouldNotAddToHistory()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Act
            history.AddCommand("");
            history.AddCommand("   ");

            // Assert
            Assert.Empty(history.GetHistory());
            Assert.Equal(0, history.Count);
        }

        [Fact]
        public void AddCommand_WithWhitespaceCommand_ShouldTrimAndAdd()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command = "  show version  ";

            // Act
            history.AddCommand(command);

            // Assert
            Assert.Single(history.GetHistory());
            Assert.Equal("show version", history.GetHistory()[0].Command);
        }

        [Fact]
        public void AddCommand_DuplicateCommands_ShouldAddBothInstances()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command = "show version";

            // Act
            history.AddCommand(command);
            history.AddCommand(command);

            // Assert
            Assert.Equal(2, history.GetHistory().Count);
            Assert.Equal(command, history.GetHistory()[0].Command);
            Assert.Equal(command, history.GetHistory()[1].Command);
        }

        [Fact]
        public void AddCommand_ShouldSetTimestamp()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command = "show version";
            var beforeTime = DateTime.UtcNow;

            // Act
            history.AddCommand(command);
            var afterTime = DateTime.UtcNow;

            // Assert
            var entry = history.GetHistory()[0];
            Assert.InRange(entry.Timestamp, beforeTime, afterTime);
        }

        [Fact]
        public void AddCommand_ShouldSetCommandNumber()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Act
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");

            // Assert
            var commands = history.GetHistory();
            Assert.Equal(1, commands[0].CommandNumber);
            Assert.Equal(2, commands[1].CommandNumber);
            Assert.Equal(3, commands[2].CommandNumber);
        }

        [Fact]
        public void GetCommandByNumber_ValidNumber_ShouldReturnCorrectCommand()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");

            // Act & Assert
            Assert.Equal("command1", history.GetCommandByNumber(1));
            Assert.Equal("command2", history.GetCommandByNumber(2));
            Assert.Equal("command3", history.GetCommandByNumber(3));
        }

        [Fact]
        public void GetCommandByNumber_InvalidNumber_ShouldReturnNull()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");

            // Act & Assert
            Assert.Null(history.GetCommandByNumber(0));
            Assert.Null(history.GetCommandByNumber(2));
            Assert.Null(history.GetCommandByNumber(10));
        }

        [Fact]
        public void GetCommandByNumber_EmptyHistory_ShouldReturnNull()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Act & Assert
            Assert.Null(history.GetCommandByNumber(1));
        }

        [Fact]
        public void GetLastCommand_WithHistory_ShouldReturnLastCommand()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");

            // Act
            var lastCommand = history.GetLastCommand();

            // Assert
            Assert.Equal("command3", lastCommand);
        }

        [Fact]
        public void GetLastCommand_EmptyHistory_ShouldReturnNull()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();

            // Act
            var lastCommand = history.GetLastCommand();

            // Assert
            Assert.Null(lastCommand);
        }

        [Fact]
        public void Clear_ShouldRemoveAllCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");

            // Act
            history.Clear();

            // Assert
            Assert.Empty(history.GetHistory());
            Assert.Equal(0, history.Count);
            Assert.Null(history.GetLastCommand());
        }

        [Fact]
        public void GetHistory_ValidCount_ShouldReturnCorrectCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");
            history.AddCommand("command4");
            history.AddCommand("command5");

            // Act
            var recent = history.GetHistory(3);

            // Assert
            Assert.Equal(3, recent.Count);
            Assert.Equal("command3", recent[0].Command);
            Assert.Equal("command4", recent[1].Command);
            Assert.Equal("command5", recent[2].Command);
        }

        [Fact]
        public void GetHistory_CountGreaterThanHistory_ShouldReturnAllCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");

            // Act
            var recent = history.GetHistory(5);

            // Assert
            Assert.Equal(2, recent.Count);
            Assert.Equal("command1", recent[0].Command);
            Assert.Equal("command2", recent[1].Command);
        }

        [Fact]
        public void GetHistory_ZeroCount_ShouldReturnAllCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");
            history.AddCommand("command2");

            // Act
            var recent = history.GetHistory(0);

            // Assert
            Assert.Equal(2, recent.Count);
            Assert.Equal("command1", recent[0].Command);
            Assert.Equal("command2", recent[1].Command);
        }

        [Fact]
        public void GetHistory_NegativeCount_ShouldReturnAllCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("command1");

            // Act
            var recent = history.GetHistory(-1);

            // Assert
            Assert.Single(recent);
            Assert.Equal("command1", recent[0].Command);
        }

        [Fact]
        public void SearchHistory_ValidPattern_ShouldReturnMatchingCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("show version");
            history.AddCommand("show interfaces");
            history.AddCommand("configure terminal");
            history.AddCommand("show ip route");
            history.AddCommand("exit");

            // Act
            var matches = history.SearchHistory("show");

            // Assert
            Assert.Equal(3, matches.Count);
            Assert.Equal("show version", matches[0].Command);
            Assert.Equal("show interfaces", matches[1].Command);
            Assert.Equal("show ip route", matches[2].Command);
        }

        [Fact]
        public void SearchHistory_CaseInsensitive_ShouldReturnMatchingCommands()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("Show Version");
            history.AddCommand("SHOW INTERFACES");
            history.AddCommand("configure terminal");

            // Act
            var matches = history.SearchHistory("show", caseSensitive: false);

            // Assert
            Assert.Equal(2, matches.Count);
            Assert.Equal("Show Version", matches[0].Command);
            Assert.Equal("SHOW INTERFACES", matches[1].Command);
        }

        [Fact]
        public void SearchHistory_CaseSensitive_ShouldReturnExactMatches()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("show version");
            history.AddCommand("Show Version");
            history.AddCommand("SHOW INTERFACES");

            // Act
            var matches = history.SearchHistory("show", caseSensitive: true);

            // Assert
            Assert.Single(matches);
            Assert.Equal("show version", matches[0].Command);
        }

        [Fact]
        public void SearchHistory_NoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("configure terminal");
            history.AddCommand("exit");

            // Act
            var matches = history.SearchHistory("show");

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public void SearchHistory_NullPattern_ShouldReturnEmptyList()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("show version");

            // Act
            var matches = history.SearchHistory(null);

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public void SearchHistory_EmptyPattern_ShouldReturnEmptyList()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand("show version");

            // Act
            var matches = history.SearchHistory("");

            // Assert
            Assert.Empty(matches);
        }

        [Fact]
        public void CommandHistoryEntry_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            var command = "show version";
            var beforeTime = DateTime.UtcNow;

            // Act
            history.AddCommand(command);
            var afterTime = DateTime.UtcNow;

            // Assert
            var entry = history.GetHistory()[0];
            Assert.Equal(command, entry.Command);
            Assert.Equal(1, entry.CommandNumber);
            Assert.InRange(entry.Timestamp, beforeTime, afterTime);
        }

        [Fact]
        public void CommandHistory_MaxSize_ShouldLimitHistorySize()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory(maxSize: 3);

            // Act
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3");
            history.AddCommand("command4"); // Should remove command1
            history.AddCommand("command5"); // Should remove command2

            // Assert
            Assert.Equal(3, history.Count);
            var commands = history.GetHistory();
            Assert.Equal("command3", commands[0].Command);
            Assert.Equal("command4", commands[1].Command);
            Assert.Equal("command5", commands[2].Command);
        }

        [Fact]
        public void CommandHistory_WithMaxSize_CommandNumbersShouldContinue()
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory(maxSize: 2);

            // Act
            history.AddCommand("command1");
            history.AddCommand("command2");
            history.AddCommand("command3"); // Should remove command1

            // Assert
            var commands = history.GetHistory();
            Assert.Equal(2, commands[0].CommandNumber); // command2
            Assert.Equal(3, commands[1].CommandNumber); // command3
        }

        [Theory]
        [InlineData("show version", "show", true)]
        [InlineData("show interfaces", "inter", true)]
        [InlineData("configure terminal", "config", true)]
        [InlineData("show version", "route", false)]
        [InlineData("Show Version", "show", true)] // Case insensitive
        [InlineData("SHOW INTERFACES", "show", true)] // Case insensitive
        public void SearchHistory_VariousPatterns_ShouldReturnCorrectResults(string command, string pattern, bool shouldMatch)
        {
            // Arrange
            var history = new Simulation.Common.CommandHistory.CommandHistory();
            history.AddCommand(command);

            // Act
            var matches = history.SearchHistory(pattern);

            // Assert
            if (shouldMatch)
            {
                Assert.Single(matches);
                Assert.Equal(command, matches[0].Command);
            }
            else
            {
                Assert.Empty(matches);
            }
        }
    }
} 
