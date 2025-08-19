using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers
{
    /// <summary>
    /// Base abstract class for CLI handlers providing common functionality
    /// </summary>
    public abstract class BaseCliHandler : ICliHandler
    {
        protected readonly Dictionary<string, ICliHandler> SubHandlers;
        protected readonly List<string> Aliases;
        protected readonly string CommandName;
        protected readonly string HelpText;
        protected readonly List<string> ApplicableModes;

        protected BaseCliHandler(string commandName, string helpText)
        {
            CommandName = commandName;
            HelpText = helpText;
            SubHandlers = new Dictionary<string, ICliHandler>(StringComparer.OrdinalIgnoreCase);
            Aliases = new List<string>();
            ApplicableModes = new List<string>();
        }

        /// <summary>
        /// Add modes where this command is applicable. If empty, applies to all modes.
        /// </summary>
        protected void AddApplicableMode(string mode)
        {
            ApplicableModes.Add(mode);
        }

        public virtual bool CanHandle(CliContext context)
        {
            if (context.CommandParts.Length == 0)
                return false;

            // Check if we're in an applicable mode
            if (ApplicableModes.Any() && !ApplicableModes.Contains(context.CurrentMode))
                return false;

            var firstPart = context.CommandParts[0];
            
            // Check if this matches the command name or an alias
            return CommandName.Equals(firstPart, StringComparison.OrdinalIgnoreCase) ||
                   Aliases.Any(alias => alias.Equals(firstPart, StringComparison.OrdinalIgnoreCase));
        }

        public virtual CliResult Handle(CliContext context)
            => HandleAsync(context).GetAwaiter().GetResult();

        public virtual async Task<CliResult> HandleAsync(CliContext context)
        {
            try
            {
                // If this is a help request, return help information
                if (context.IsHelpRequest)
                {
                    return Success(GetContextualHelp(context));
                }

                // If we have sub-handlers and more command parts, try to delegate
                if (context.CommandParts.Length > 1 && SubHandlers.Count > 0)
                {
                    var remainingParts = context.CommandParts.Skip(1).ToArray();
                    var subContext = new CliContext(context.Device, remainingParts, context.FullCommand)
                    {
                        IsHelpRequest = context.IsHelpRequest,
                        Parameters = context.Parameters
                    };

                    foreach (var handler in SubHandlers.Values)
                    {
                        if (handler.CanHandle(subContext))
                        {
                            return await handler.HandleAsync(subContext);
                        }
                    }
                }

                // Otherwise, handle the command ourselves
                return await ExecuteCommandAsync(context);
            }
            catch (Exception ex)
            {
                return CliResult.Failed(
                    CliErrorType.ExecutionError,
                    $"Error executing command: {ex.Message}"
                );
            }
        }

        protected abstract Task<CliResult> ExecuteCommandAsync(CliContext context);

        public virtual string GetHelp()
        {
            return HelpText;
        }

        public virtual List<string> GetCompletions(CliContext context)
        {
            var completions = new List<string>();

            // If we're at the first level, return sub-handler command names with fuzzy matching
            if (context.CommandParts.Length <= 1)
            {
                var allSubCommands = SubHandlers.Keys.ToList();
                
                // If we have a partial command, filter by fuzzy matching
                if (context.CommandParts.Length == 1)
                {
                    var partialCommand = context.CommandParts[0];
                    if (!string.IsNullOrEmpty(partialCommand))
                    {
                        // Add exact matches first
                        var exactMatches = allSubCommands.Where(cmd => 
                            cmd.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase)).ToList();
                        completions.AddRange(exactMatches);
                        
                        // Add fuzzy matches if no exact matches found
                        if (exactMatches.Count == 0)
                        {
                            var fuzzyMatches = allSubCommands.Where(cmd => 
                                GetFuzzyMatchScore(partialCommand, cmd) > 0.5).ToList();
                            completions.AddRange(fuzzyMatches);
                        }
                    }
                    else
                    {
                        completions.AddRange(allSubCommands);
                    }
                }
                else
                {
                    completions.AddRange(allSubCommands);
                }
                
                // Add vendor-specific completions if available
                if (context.VendorContext != null)
                {
                    var vendorCompletions = context.VendorContext.GetCommandCompletions(context.CommandParts);
                    completions.AddRange(vendorCompletions);
                }
            }
            else if (context.CommandParts.Length > 1)
            {
                // Try to find a sub-handler for deeper completions
                var subCommand = context.CommandParts[1];
                var remainingParts = context.CommandParts.Skip(1).ToArray();
                var subContext = new CliContext(context.Device, remainingParts, context.FullCommand)
                {
                    IsHelpRequest = context.IsHelpRequest,
                    Parameters = context.Parameters
                };
                
                // First try exact match
                if (SubHandlers.TryGetValue(subCommand, out var exactHandler))
                {
                    return exactHandler.GetCompletions(subContext);
                }
                
                // Then try fuzzy matching for sub-handlers
                var fuzzyMatches = SubHandlers.Where(kvp => 
                    kvp.Key.StartsWith(subCommand, StringComparison.OrdinalIgnoreCase) ||
                    GetFuzzyMatchScore(subCommand, kvp.Key) > 0.7).ToList();
                
                if (fuzzyMatches.Count == 1)
                {
                    return fuzzyMatches.First().Value.GetCompletions(subContext);
                }
                else if (fuzzyMatches.Count > 1)
                {
                    // Multiple matches, return the matching sub-command names
                    completions.AddRange(fuzzyMatches.Select(kvp => kvp.Key));
                }
                
                // Fall back to checking if any handler can handle this prefix
                foreach (var handler in SubHandlers.Values)
                {
                    if (handler.CanHandlePrefix(subContext))
                    {
                        var subCompletions = handler.GetCompletions(subContext);
                        completions.AddRange(subCompletions);
                    }
                }
            }

            return completions.Distinct().OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Calculate fuzzy match score between two strings
        /// </summary>
        protected virtual double GetFuzzyMatchScore(string input, string target)
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
            var distance = CalculateLevenshteinDistance(input, target);
            var maxLength = Math.Max(input.Length, target.Length);
            var similarity = 1.0 - ((double)distance / maxLength);
            
            return similarity;
        }

        /// <summary>
        /// Calculate Levenshtein distance between two strings
        /// </summary>
        protected virtual int CalculateLevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// Adds a sub-handler to this command handler
        /// </summary>
        protected void AddSubHandler(string key, ICliHandler handler)
        {
            SubHandlers[key] = handler;
        }

        /// <summary>
        /// Adds an alias for this command
        /// </summary>
        protected void AddAlias(string alias)
        {
            Aliases.Add(alias);
        }
        
        public Dictionary<string, ICliHandler> GetSubHandlers()
        {
            return SubHandlers;
        }
        
        public (string, string)? GetCommandInfo()
        {
            return (CommandName, HelpText);
        }
        
        public virtual bool CanHandlePrefix(CliContext context)
        {
            if (context.CommandParts.Length == 0)
                return false;
                
            // Check if the first part matches our command or alias (exact or prefix match)
            var firstPart = context.CommandParts[0];
            bool matches = CommandName.Equals(firstPart, StringComparison.OrdinalIgnoreCase) ||
                          CommandName.StartsWith(firstPart, StringComparison.OrdinalIgnoreCase) ||
                          Aliases.Any(alias => alias.Equals(firstPart, StringComparison.OrdinalIgnoreCase) ||
                                             alias.StartsWith(firstPart, StringComparison.OrdinalIgnoreCase));
                          
            if (!matches)
            {
                // Try fuzzy matching for better completion experience
                var fuzzyScore = GetFuzzyMatchScore(firstPart, CommandName);
                if (fuzzyScore < 0.5)
                {
                    fuzzyScore = Aliases.Max(alias => GetFuzzyMatchScore(firstPart, alias));
                }
                matches = fuzzyScore >= 0.5;
            }
                          
            if (!matches)
                return false;
                
            // If we have more parts, check if we have sub-handlers that could handle them
            if (context.CommandParts.Length > 1 && SubHandlers.Count > 0)
            {
                var subCommand = context.CommandParts[1];
                
                // Check if any sub-handler can handle the prefix
                return SubHandlers.Any(kvp => 
                    kvp.Key.StartsWith(subCommand, StringComparison.OrdinalIgnoreCase) ||
                    GetFuzzyMatchScore(subCommand, kvp.Key) > 0.5);
            }
            
            return true; // We can handle this prefix
        }
        
        public virtual List<(string, string)> GetSubCommands(CliContext context)
        {
            var commands = new List<(string, string)>();
            
            // Add our own command if we're at the root
            if (context.CommandParts.Length <= 1)
            {
                commands.Add((CommandName, HelpText));
            }
            
            // Add sub-handler commands
            if (context.CommandParts.Length <= 1)
            {
                foreach (var handler in SubHandlers.Values)
                {
                    var info = handler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        commands.Add(info.Value);
                    }
                }
            }
            else
            {
                // Try to find a sub-handler for deeper completions
                var subCommand = context.CommandParts[1];
                var remainingParts = context.CommandParts.Skip(1).ToArray();
                var subContext = new CliContext(context.Device, remainingParts, context.FullCommand)
                {
                    IsHelpRequest = context.IsHelpRequest,
                    Parameters = context.Parameters
                };
                
                // First try exact match
                if (SubHandlers.TryGetValue(subCommand, out var exactHandler))
                {
                    return exactHandler.GetSubCommands(subContext);
                }
                
                // Then try fuzzy matching
                var fuzzyMatches = SubHandlers.Where(kvp => 
                    kvp.Key.StartsWith(subCommand, StringComparison.OrdinalIgnoreCase) ||
                    GetFuzzyMatchScore(subCommand, kvp.Key) > 0.7).ToList();
                
                if (fuzzyMatches.Count == 1)
                {
                    return fuzzyMatches.First().Value.GetSubCommands(subContext);
                }
                else if (fuzzyMatches.Count > 1)
                {
                    // Multiple matches, return the matching sub-commands
                    foreach (var match in fuzzyMatches)
                    {
                        var info = match.Value.GetCommandInfo();
                        if (info.HasValue)
                        {
                            commands.Add(info.Value);
                        }
                    }
                }
                
                // Fall back to checking all handlers
                foreach (var handler in SubHandlers.Values)
                {
                    if (handler.CanHandlePrefix(subContext))
                    {
                        var subCommands = handler.GetSubCommands(subContext);
                        commands.AddRange(subCommands);
                    }
                }
            }
            
            return commands.Distinct().OrderBy(c => c.Item1).ToList();
        }

        /// <summary>
        /// Helper method to create a success result
        /// </summary>
        protected CliResult Success(string output = "") => CliResult.Ok(output);

        /// <summary>
        /// Helper method to create an error result
        /// </summary>
        protected CliResult Error(CliErrorType error, string message = "", string[]? suggestions = null) => 
            CliResult.Failed(error, message, suggestions);

        /// <summary>
        /// Helper method for invalid command errors
        /// </summary>
        protected CliResult InvalidCommand(string message = "% Invalid input detected at '^' marker.") => 
            Error(CliErrorType.InvalidCommand, message);

        /// <summary>
        /// Helper method for incomplete command errors
        /// </summary>
        protected CliResult IncompleteCommand(string message = "Incomplete command") => 
            Error(CliErrorType.IncompleteCommand, message);

        /// <summary>
        /// Helper method for invalid parameter errors
        /// </summary>
        protected CliResult InvalidParameter(string message = "Invalid parameter") => 
            Error(CliErrorType.InvalidParameter, message);

        /// <summary>
        /// Helper method for invalid mode errors
        /// </summary>
        protected CliResult InvalidMode(string message = "Command not available in current mode") => 
            Error(CliErrorType.InvalidMode, message);

        /// <summary>
        /// Helper method for permission denied errors
        /// </summary>
        protected CliResult PermissionDenied(string message = "Permission denied") => 
            Error(CliErrorType.PermissionDenied, message);

        /// <summary>
        /// Gets contextual help information based on command context
        /// </summary>
        protected virtual string GetContextualHelp(CliContext context)
        {
            var help = new List<string>();
            
            // Add command description
            help.Add($"{CommandName} - {HelpText}");
            
            // If we have sub-handlers, show available options
            if (SubHandlers.Count > 0)
            {
                help.Add("");
                help.Add("Available options:");
                
                foreach (var kvp in SubHandlers.OrderBy(x => x.Key))
                {
                    var subHandler = kvp.Value;
                    var info = subHandler.GetCommandInfo();
                    if (info.HasValue)
                    {
                        var (subCmd, subDesc) = info.Value;
                        help.Add($"  {subCmd,-15} {subDesc}");
                    }
                }
            }
            
            // Add vendor-specific help if available
            if (context.VendorContext != null)
            {
                var vendorHelp = context.VendorContext.GetCommandHelp(context.FullCommand);
                if (!string.IsNullOrEmpty(vendorHelp) && !vendorHelp.Contains("not available"))
                {
                    help.Add("");
                    help.Add("Vendor-specific information:");
                    help.Add(vendorHelp);
                }
            }
            
            // Add syntax information based on command parts
            if (context.CommandParts.Length > 1)
            {
                help.Add("");
                help.Add("Syntax:");
                help.Add($"  {string.Join(" ", context.CommandParts)} [options]");
            }
            
            // Add mode information
            var currentMode = context.CurrentMode;
            if (!string.IsNullOrEmpty(currentMode))
            {
                help.Add("");
                help.Add($"Current mode: {currentMode}");
            }
            
            return string.Join("\n", help);
        }
    }
} 
