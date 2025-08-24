using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Base
{
    /// <summary>
    /// Manages CLI handlers for a device
    /// </summary>
    public class CliHandlerManager
    {
        private readonly List<ICliHandler> handlers;
        private readonly NetworkDevice device;

        public CliHandlerManager(NetworkDevice device)
        {
            this.device = device;
            this.handlers = new List<ICliHandler>();
        }

        /// <summary>
        /// Registers a CLI handler
        /// </summary>
        public void RegisterHandler(ICliHandler handler)
        {
            handlers.Add(handler);
        }

        /// <summary>
        /// Asynchronously processes a command using the registered handlers
        /// </summary>
        /// <returns>The command result</returns>
        public async Task<CliResult> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return CliResult.Ok();

            try
            {
                // Check if this is a help request (ends with ?)
                bool isHelpRequest = command.TrimEnd().EndsWith("?");
                if (isHelpRequest)
                {
                    // Remove the ? and any trailing space
                    command = command.TrimEnd().TrimEnd('?').TrimEnd();
                }

                // Split the command into parts
                var parts = SplitCommand(command);

                // Create the context
                var context = new CliContext(device, parts, command)
                {
                    IsHelpRequest = isHelpRequest
                };

                // If this is a help request, show available commands
                if (isHelpRequest)
                {
                    return CliResult.Ok(GetHelpText(context));
                }

                if (parts.Length == 0)
                    return CliResult.Ok();

                // Try to find a handler that can handle this command
                foreach (var handler in handlers)
                {
                    if (handler.CanHandle(context))
                    {
                        return await handler.HandleAsync(context);
                    }
                }

                // No handler found - try to find similar commands
                var suggestions = GetSuggestions(command);
                var errorMsg = "% Invalid input detected at '^' marker.";
                if (suggestions.Length > 0)
                {
                    errorMsg += "\nDid you mean one of these?\n  " + string.Join("\n  ", suggestions);
                }

                return CliResult.Failed(CliErrorType.InvalidCommand, errorMsg, suggestions);
            }
            catch (Exception ex)
            {
                return CliResult.Failed(
                    CliErrorType.ExecutionError,
                    $"Error processing command: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Gets completions for tab completion
        /// </summary>
        public List<string> GetCompletions(string command)
        {
            var completions = new List<string>();

            // Handle empty command
            if (string.IsNullOrWhiteSpace(command))
            {
                // Return all available command names
                foreach (var handler in handlers)
                {
                    var info = handler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        completions.Add(info.Value.Item1);
                    }
                }
                return completions.Distinct().OrderBy(x => x).ToList();
            }

            // Split the command into parts
            var parts = SplitCommand(command);

            // Create the context
            var context = new CliContext(device, parts, command);

            // If we have only one part, do fuzzy matching on all handlers
            if (parts.Length == 1)
            {
                var partialCommand = parts[0];
                var allHandlers = handlers.ToList();

                // First, add exact prefix matches
                var exactMatches = allHandlers.Where(h => {
                    var info = h.GetCommandInfo();
                    return info.HasValue && info.Value.Item1.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase);
                }).ToList();

                foreach (var handler in exactMatches)
                {
                    completions.AddRange(handler.GetCompletions(context));
                }

                // If no exact matches, try fuzzy matching
                if (exactMatches.Count == 0)
                {
                    var fuzzyMatches = allHandlers.Where(h => {
                        var info = h.GetCommandInfo();
                        if (!info.HasValue) return false;
                        return CalculateFuzzyScore(partialCommand, info.Value.Item1) > 0.5;
                    }).ToList();

                    foreach (var handler in fuzzyMatches)
                    {
                        completions.AddRange(handler.GetCompletions(context));
                    }
                }

                // Also add direct command name matches
                foreach (var handler in handlers)
                {
                    var info = handler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        var cmdName = info.Value.Item1;
                        if (cmdName.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            completions.Add(cmdName);
                        }
                    }
                }
            }
            else
            {
                // Multiple parts - find handlers that can handle the prefix
                foreach (var handler in handlers)
                {
                    if (handler.CanHandlePrefix(context))
                    {
                        completions.AddRange(handler.GetCompletions(context));
                    }
                }
            }

            return completions.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Calculate fuzzy match score for completion
        /// </summary>
        private double CalculateFuzzyScore(string input, string target)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
                return 0;

            input = input.ToLowerInvariant();
            target = target.ToLowerInvariant();

            // Exact match
            if (input == target)
                return 1.0;

            // Starts with match
            if (target.StartsWith(input))
                return 0.9;

            // Contains match
            if (target.Contains(input))
                return 0.7;

            // Levenshtein distance based scoring
            var distance = LevenshteinDistance(input, target);
            var maxLength = Math.Max(input.Length, target.Length);
            var similarity = 1.0 - ((double)distance / maxLength);

            return similarity;
        }

        /// <summary>
        /// Gets help text for a specific command
        /// </summary>
        public string GetCommandHelp(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return GetGeneralHelp();
            }

            // Check if this is a help request (ends with ?)
            bool isHelpRequest = command.TrimEnd().EndsWith("?");
            if (isHelpRequest)
            {
                // Remove the ? and any trailing space
                command = command.TrimEnd().TrimEnd('?').TrimEnd();
            }

            // Split the command into parts
            var parts = SplitCommand(command);

            // Create the context for help
            var context = new CliContext(device, parts, command)
            {
                IsHelpRequest = true
            };

            return GetHelpText(context);
        }

        /// <summary>
        /// Gets general help text when no specific command is provided
        /// </summary>
        private string GetGeneralHelp()
        {
            var help = new List<string>();
            var allCommands = new Dictionary<string, string>(); // cmd -> description

            foreach (var handler in handlers)
            {
                var info = handler.GetCommandInfo();
                if (info.HasValue)
                {
                    var (cmd, desc) = info.Value;
                    if (!allCommands.ContainsKey(cmd))
                    {
                        allCommands[cmd] = desc;
                    }
                }
            }

            if (allCommands.Any())
            {
                help.Add("Available commands:");
                foreach (var (cmd, desc) in allCommands.OrderBy(x => x.Key))
                {
                    help.Add($"  {cmd,-15} {desc}");
                }
            }
            else
            {
                help.Add("No commands available.");
            }

            return string.Join("\n", help);
        }

        /// <summary>
        /// Gets help text for the current command
        /// </summary>
        private string GetHelpText(CliContext context)
        {
            var help = new List<string>();
            var foundHandler = false;
            var allSubCommands = new Dictionary<string, string>(); // cmd -> description

            // If no command parts, show general help
            if (context.CommandParts.Length == 0)
            {
                return GetGeneralHelp();
            }

            foreach (var handler in handlers)
            {
                if (handler.CanHandlePrefix(context))
                {
                    foundHandler = true;

                    // If this is a help request for a specific handler, use its contextual help
                    if (context.IsHelpRequest && handler.CanHandle(context))
                    {
                        // Use the handler's own contextual help method
                        var handlerType = handler.GetType();
                        var getContextualHelpMethod = handlerType.GetMethod("GetContextualHelp",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (getContextualHelpMethod != null)
                        {
                            var contextualHelp = getContextualHelpMethod.Invoke(handler, new object[] { context }) as string;
                            if (!string.IsNullOrEmpty(contextualHelp))
                            {
                                return contextualHelp;
                            }
                        }
                    }

                    var subCommands = handler.GetSubCommands(context);

                    // Collect all sub-commands from all handlers
                    foreach (var (cmd, desc) in subCommands)
                    {
                        // Use the first description found for each command
                        if (!allSubCommands.ContainsKey(cmd))
                        {
                            allSubCommands[cmd] = desc;
                        }
                    }
                }
            }

            if (foundHandler && allSubCommands.Any())
            {
                help.Add("Available options:");
                foreach (var (cmd, desc) in allSubCommands.OrderBy(x => x.Key))
                {
                    help.Add($"  {cmd,-15} {desc}");
                }

                // Add additional help information
                help.Add("");
                help.Add("Use '?' after any command for context-sensitive help");
                help.Add("Use <TAB> for command completion");
            }
            else if (!foundHandler)
            {
                help.Add("% Invalid input detected at '^' marker.");
                help.Add("");
                help.Add("Available commands:");

                // Show available root commands
                foreach (var handler in handlers)
                {
                    var info = handler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        var (cmd, desc) = info.Value;
                        help.Add($"  {cmd,-15} {desc}");
                    }
                }
            }

            return string.Join("\n", help) + "\n";
        }

        /// <summary>
        /// Split command into parts, handling quotes
        /// </summary>
        private string[] SplitCommand(string command)
        {
            var parts = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < command.Length; i++)
            {
                char c = command[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current);
            }

            return parts.ToArray();
        }

        /// <summary>
        /// Check if a handler could potentially handle a command
        /// </summary>
        private bool CouldHandle(ICliHandler handler, CliContext context)
        {
            var info = handler.GetCommandInfo();
            if (!info.HasValue || context.CommandParts.Length == 0)
                return false;

            return info.Value.Item1.StartsWith(context.CommandParts[0], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get command suggestions for an invalid command
        /// </summary>
        private string[] GetSuggestions(string command)
        {
            var suggestions = new List<string>();
            var parts = SplitCommand(command);

            if (parts.Length > 0)
            {
                // Find similar commands
                foreach (var handler in handlers)
                {
                    var info = handler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        var similarity = CalculateSimilarity(parts[0], info.Value.Item1);
                        if (similarity >= 0.7) // 70% similarity threshold
                        {
                            suggestions.Add(info.Value.Item1);
                        }
                    }
                }
            }

            return suggestions.ToArray();
        }

        /// <summary>
        /// Calculate similarity between two strings (Levenshtein distance based)
        /// </summary>
        private double CalculateSimilarity(string s1, string s2)
        {
            var distance = LevenshteinDistance(s1.ToLowerInvariant(), s2.ToLowerInvariant());
            var maxLength = Math.Max(s1.Length, s2.Length);
            return 1 - ((double)distance / maxLength);
        }

        /// <summary>
        /// Calculate Levenshtein distance between two strings
        /// </summary>
        private static int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            // First row and column
            for (int i = 0; i <= s1.Length; i++) matrix[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) matrix[0, j] = j;

            // Fill in the rest of the matrix
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[s1.Length, s2.Length];
        }
    }
}

