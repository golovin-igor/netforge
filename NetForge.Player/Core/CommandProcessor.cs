using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetForge.Player.Core;

/// <summary>
/// Processes and executes commands in the NetForge.Player environment
/// </summary>
public class CommandProcessor : ICommandProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandProcessor> _logger;
    private readonly Dictionary<string, Type> _commandRegistry;
    private readonly CommandHistory _commandHistory;

    public CommandProcessor(IServiceProvider serviceProvider, ILogger<CommandProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _commandRegistry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        _commandHistory = new CommandHistory();

        RegisterCommands();
    }

    /// <summary>
    /// Process a command line input
    /// </summary>
    public async Task<CommandResult> ProcessCommandAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return CommandResult.Empty();
        }

        // Add to history
        _commandHistory.Add(command);

        // Handle history recall commands
        if (command.StartsWith("!"))
        {
            var historicalCommand = _commandHistory.GetByIndex(command);
            if (historicalCommand != null)
            {
                command = historicalCommand;
                _logger.LogDebug("Executing historical command: {Command}", command);
            }
            else
            {
                return CommandResult.Fail($"No command found in history matching '{command}'");
            }
        }

        try
        {
            // Parse the command line
            var tokens = TokenizeInput(command);
            if (tokens.Length == 0)
            {
                return CommandResult.Empty();
            }

            var commandName = tokens[0];
            var args = tokens.Skip(1).ToArray();

            // Special built-in commands
            switch (commandName.ToLowerInvariant())
            {
                case "history":
                    return ShowHistory();
                case "clear":
                    Console.Clear();
                    return CommandResult.Ok("Screen cleared");
                case "exit":
                case "quit":
                    return CommandResult.Exit("Goodbye!");
            }

            // Find and execute registered command
            var resolvedCommand = ResolveCommand(commandName);
            if (resolvedCommand == null)
            {
                return CommandResult.Fail($"Unknown command: '{commandName}'. Type 'help' for available commands.");
            }

            // Build execution context
            var context = new CommandContext
            {
                CommandName = commandName,
                Arguments = args,
                RawInput = command,
                ServiceProvider = _serviceProvider
            };

            // Execute the command
            _logger.LogDebug("Executing command: {Command} with {ArgCount} arguments", commandName, args.Length);
            var result = await resolvedCommand.ExecuteAsync(context);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {Input}", command);
            return CommandResult.Fail($"Command failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get command suggestions for tab completion
    /// </summary>
    public async Task<List<string>> GetCompletionsAsync(string partialCommand)
    {
        if (string.IsNullOrWhiteSpace(partialCommand))
        {
            return _commandRegistry.Keys.OrderBy(k => k).ToList();
        }

        var tokens = TokenizeInput(partialCommand);
        if (tokens.Length == 0)
        {
            return _commandRegistry.Keys.OrderBy(k => k).ToList();
        }

        // If we're still on the first token, complete command names
        if (tokens.Length == 1 && !partialCommand.EndsWith(" "))
        {
            return _commandRegistry.Keys
                .Where(k => k.StartsWith(tokens[0], StringComparison.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();
        }

        // Otherwise, delegate to the command for argument completion
        var command = ResolveCommand(tokens[0]);
        if (command is ISupportsCompletion completableCommand)
        {
            var args = tokens.Skip(1).ToArray();
            return completableCommand.GetCompletions(args, partialCommand);
        }

        return new List<string>();
    }

    /// <summary>
    /// Get help information for a command
    /// </summary>
    public CommandMetadata GetCommandHelp(string commandName)
    {
        var command = ResolveCommand(commandName);
        if (command == null)
        {
            return new CommandMetadata
            {
                Name = commandName,
                Description = "Command not found",
                Usage = $"Unknown command: {commandName}"
            };
        }

        var metadata = new CommandMetadata
        {
            Name = command.Name,
            Description = command.Description,
            Usage = command.Usage
        };

        // Add aliases if available
        if (command is IHasAliases aliasedCommand)
        {
            // TODO: Add aliases property to CommandMetadata if needed
        }

        return metadata;
    }

    /// <summary>
    /// Register a command type
    /// </summary>
    public void RegisterCommand<T>() where T : IPlayerCommand
    {
        var commandType = typeof(T);
        var tempInstance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
        _commandRegistry[tempInstance.Name] = commandType;

        // Also register any aliases
        if (tempInstance is IHasAliases aliasedCommand)
        {
            foreach (var alias in aliasedCommand.Aliases)
            {
                _commandRegistry[alias] = commandType;
            }
        }

        _logger.LogDebug("Registered command: {CommandName} ({Type})", tempInstance.Name, commandType.Name);
    }

    /// <summary>
    /// Get all registered commands
    /// </summary>
    public IEnumerable<IPlayerCommand> GetAllCommands()
    {
        var uniqueTypes = _commandRegistry.Values.Distinct();
        foreach (var commandType in uniqueTypes)
        {
            yield return (IPlayerCommand)ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
        }
    }

    private void RegisterCommands()
    {
        // Auto-discover commands from the assembly
        var commandTypes = typeof(CommandProcessor).Assembly
            .GetTypes()
            .Where(t => typeof(IPlayerCommand).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var commandType in commandTypes)
        {
            try
            {
                var method = typeof(CommandProcessor)
                    .GetMethod(nameof(RegisterCommand))!
                    .MakeGenericMethod(commandType);

                method.Invoke(this, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register command type: {Type}", commandType.Name);
            }
        }

        _logger.LogInformation("Registered {Count} commands", _commandRegistry.Count);
    }

    private IPlayerCommand? ResolveCommand(string commandName)
    {
        if (_commandRegistry.TryGetValue(commandName, out var commandType))
        {
            return (IPlayerCommand)ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
        }
        return null;
    }

    private string[] TokenizeInput(string input)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        var inQuotes = false;
        var escapeNext = false;

        foreach (var ch in input)
        {
            if (escapeNext)
            {
                currentToken.Append(ch);
                escapeNext = false;
                continue;
            }

            switch (ch)
            {
                case '\\':
                    escapeNext = true;
                    break;
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ' ':
                case '\t':
                    if (inQuotes)
                    {
                        currentToken.Append(ch);
                    }
                    else if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                    break;
                default:
                    currentToken.Append(ch);
                    break;
            }
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens.ToArray();
    }

    private CommandResult ShowHistory()
    {
        var history = _commandHistory.GetAll();
        if (!history.Any())
        {
            return CommandResult.Ok("Command history is empty");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Command History:");
        for (int i = 0; i < history.Count; i++)
        {
            sb.AppendLine($"  {i + 1,3}: {history[i]}");
        }

        return CommandResult.Ok(sb.ToString());
    }
}

/// <summary>
/// Maintains command history for the session
/// </summary>
public class CommandHistory
{
    private readonly List<string> _history = new();
    private const int MaxHistorySize = 100;

    public void Add(string command)
    {
        if (!string.IsNullOrWhiteSpace(command) && !command.StartsWith("!"))
        {
            _history.Add(command);

            // Limit history size
            while (_history.Count > MaxHistorySize)
            {
                _history.RemoveAt(0);
            }
        }
    }

    public string? GetByIndex(string indexSpec)
    {
        if (indexSpec.StartsWith("!!"))
        {
            // Get last command
            return _history.LastOrDefault();
        }

        if (indexSpec.StartsWith("!") && int.TryParse(indexSpec.Substring(1), out var index))
        {
            // Get command by index (1-based)
            if (index > 0 && index <= _history.Count)
            {
                return _history[index - 1];
            }
        }

        return null;
    }

    public IReadOnlyList<string> GetAll() => _history.AsReadOnly();
}

/// <summary>
/// Interface for commands that support tab completion
/// </summary>
public interface ISupportsCompletion
{
    List<string> GetCompletions(string[] currentArgs, string partialInput);
}

/// <summary>
/// Interface for commands with aliases
/// </summary>
public interface IHasAliases
{
    string[] Aliases { get; }
}