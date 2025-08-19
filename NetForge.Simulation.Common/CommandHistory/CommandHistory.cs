using System.Collections;
using System.Text;

namespace NetForge.Simulation.Common.CommandHistory
{
    /// <summary>
    /// Manages command history for network devices with recall functionality
    /// </summary>
    public class CommandHistory(int maxSize = 1000): IReadOnlyCollection<string>
    {
        private readonly List<CommandHistoryEntry> _history = [];
        private int _nextCommandNumber = 1;

        /// <summary>
        /// Gets the total number of commands in history
        /// </summary>
        public int Count => _history.Count;

        /// <summary>
        /// Gets the maximum history size
        /// </summary>
        public int MaxSize => maxSize;

        /// <summary>
        /// Adds a command to the history
        /// </summary>
        /// <param name="command">The command to add</param>
        /// <param name="deviceMode">The device mode when command was executed</param>
        /// <param name="success">Whether the command executed successfully</param>
        public void AddCommand(string command, string deviceMode = "", bool success = true)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            // Don't add history recall commands to prevent recursive history
            if (IsHistoryRecallCommand(command))
                return;

            // Remove oldest entries if we exceed max size
            while (_history.Count >= maxSize)
            {
                _history.RemoveAt(0);
            }

            var entry = new CommandHistoryEntry
            {
                CommandNumber = _nextCommandNumber++,
                Command = command.Trim(),
                Timestamp = DateTime.UtcNow,
                DeviceMode = deviceMode,
                Success = success
            };

            _history.Add(entry);
        }

        /// <summary>
        /// Gets the last executed command
        /// </summary>
        /// <returns>The last command or null if history is empty</returns>
        public string GetLastCommand()
        {
            return _history.LastOrDefault()?.Command;
        }

        /// <summary>
        /// Gets a command by its number in history
        /// </summary>
        /// <param name="commandNumber">The command number</param>
        /// <returns>The command or null if not found</returns>
        public string GetCommandByNumber(int commandNumber)
        {
            var entry = _history.FirstOrDefault(h => h.CommandNumber == commandNumber);
            return entry?.Command;
        }

        /// <summary>
        /// Gets the most recent command that starts with the specified string
        /// </summary>
        /// <param name="prefix">The prefix to search for</param>
        /// <returns>The command or null if not found</returns>
        public string GetCommandByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return null;

            var entry = _history.LastOrDefault(h => h.Command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            return entry?.Command;
        }

        /// <summary>
        /// Gets a command by relative position (negative numbers for previous commands)
        /// </summary>
        /// <param name="relativePosition">Position relative to current (-1 = last, -2 = second to last, etc.)</param>
        /// <returns>The command or null if not found</returns>
        public string GetCommandByRelativePosition(int relativePosition)
        {
            if (relativePosition >= 0 || _history.Count == 0)
                return null;

            int index = _history.Count + relativePosition;
            if (index < 0 || index >= _history.Count)
                return null;

            return _history[index].Command;
        }

        /// <summary>
        /// Processes a history recall command and returns the recalled command
        /// </summary>
        /// <param name="recallCommand">The recall command (e.g., "!!", "!10", "!sh")</param>
        /// <returns>The recalled command or null if not found</returns>
        public string ProcessRecallCommand(string recallCommand)
        {
            if (string.IsNullOrEmpty(recallCommand) || !recallCommand.StartsWith("!"))
                return null;

            if (recallCommand == "!!")
            {
                // Recall last command
                return GetLastCommand();
            }
            else if (recallCommand.Length > 1)
            {
                var parameter = recallCommand.Substring(1);
                
                // Try to parse as command number
                if (int.TryParse(parameter, out int commandNumber))
                {
                    return GetCommandByNumber(commandNumber);
                }
                else
                {
                    // Treat as prefix search
                    return GetCommandByPrefix(parameter);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all history entries for display
        /// </summary>
        /// <param name="count">Number of recent entries to return (0 for all)</param>
        /// <returns>List of history entries</returns>
        public List<CommandHistoryEntry> GetHistory(int count = 0)
        {
            if (count <= 0)
                return new List<CommandHistoryEntry>(_history);

            return _history.Skip(Math.Max(0, _history.Count - count)).ToList();
        }

        /// <summary>
        /// Builds a formatted history display string
        /// </summary>
        /// <param name="count">Number of recent entries to display (0 for all)</param>
        /// <param name="showTimestamp">Whether to include timestamps</param>
        /// <param name="showMode">Whether to include device mode</param>
        /// <returns>Formatted history string</returns>
        public string BuildHistoryDisplay(int count = 0, bool showTimestamp = false, bool showMode = false)
        {
            var entries = GetHistory(count);
            if (!entries.Any())
                return "No commands in history.\n";

            var output = new StringBuilder();
            
            foreach (var entry in entries)
            {
                var line = new StringBuilder();
                line.Append($"{entry.CommandNumber,5}  ");
                
                if (showTimestamp)
                {
                    line.Append($"{entry.Timestamp:HH:mm:ss} ");
                }
                
                if (showMode && !string.IsNullOrEmpty(entry.DeviceMode))
                {
                    line.Append($"[{entry.DeviceMode}] ");
                }
                
                line.Append(entry.Command);
                
                if (!entry.Success)
                {
                    line.Append(" (failed)");
                }

                output.Append(line.ToString()).AppendLine();
            }

            return output.ToString();
        }

        /// <summary>
        /// Searches history for commands containing the specified text
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="caseSensitive">Whether search should be case sensitive</param>
        /// <returns>List of matching history entries</returns>
        public List<CommandHistoryEntry> SearchHistory(string searchText, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(searchText))
                return new List<CommandHistoryEntry>();

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            return _history.Where(h => h.Command.Contains(searchText, comparison)).ToList();
        }

        /// <summary>
        /// Clears all command history
        /// </summary>
        public void Clear()
        {
            _history.Clear();
            _nextCommandNumber = 1;
        }

        /// <summary>
        /// Removes commands older than the specified date
        /// </summary>
        /// <param name="olderThan">Date threshold</param>
        /// <returns>Number of commands removed</returns>
        public int RemoveOlderThan(DateTime olderThan)
        {
            int initialCount = _history.Count;
            _history.RemoveAll(h => h.Timestamp < olderThan);
            return initialCount - _history.Count;
        }

        /// <summary>
        /// Gets statistics about the command history
        /// </summary>
        /// <returns>History statistics</returns>
        public CommandHistoryStats GetStatistics()
        {
            if (!_history.Any())
            {
                return new CommandHistoryStats
                {
                    TotalCommands = 0,
                    SuccessfulCommands = 0,
                    FailedCommands = 0,
                    UniqueCommands = 0,
                    MostUsedCommand = null,
                    OldestCommand = null,
                    NewestCommand = null
                };
            }

            var successfulCount = _history.Count(h => h.Success);
            var failedCount = _history.Count - successfulCount;
            var uniqueCommands = _history.Select(h => h.Command).Distinct().Count();
            var mostUsed = _history.GroupBy(h => h.Command)
                                  .OrderByDescending(g => g.Count())
                                  .First().Key;

            return new CommandHistoryStats
            {
                TotalCommands = _history.Count,
                SuccessfulCommands = successfulCount,
                FailedCommands = failedCount,
                UniqueCommands = uniqueCommands,
                MostUsedCommand = mostUsed,
                OldestCommand = _history.First().Timestamp,
                NewestCommand = _history.Last().Timestamp
            };
        }

        /// <summary>
        /// Exports history to a simple text format
        /// </summary>
        /// <returns>History as text</returns>
        public string ExportToText()
        {
            var output = new StringBuilder();
            output.Append("# Command History Export").AppendLine();
            output.Append($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").AppendLine();
            output.Append($"# Total Commands: {_history.Count}").AppendLine();
            output.AppendLine();

            foreach (var entry in _history)
            {
                output.Append($"{entry.CommandNumber}\t{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\t{entry.DeviceMode}\t{entry.Success}\t{entry.Command}").AppendLine();
            }

            return output.ToString();
        }

        /// <summary>
        /// Checks if a command is a history recall command
        /// </summary>
        /// <param name="command">The command to check</param>
        /// <returns>True if it's a history recall command</returns>
        private bool IsHistoryRecallCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            var trimmed = command.Trim();
            return trimmed.StartsWith("!") || 
                   trimmed.Equals("history", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.StartsWith("history ", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _history.Select(entry => entry.Command).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a single command history entry
    /// </summary>
    public class CommandHistoryEntry
    {
        public int CommandNumber { get; set; }
        public string Command { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string DeviceMode { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// Statistics about command history usage
    /// </summary>
    public class CommandHistoryStats
    {
        public int TotalCommands { get; set; }
        public int SuccessfulCommands { get; set; }
        public int FailedCommands { get; set; }
        public int UniqueCommands { get; set; }
        public string? MostUsedCommand { get; set; }
        public DateTime? OldestCommand { get; set; }
        public DateTime? NewestCommand { get; set; }

        public double SuccessRate => TotalCommands > 0 ? (double)SuccessfulCommands / TotalCommands * 100 : 0;
    }
} 
