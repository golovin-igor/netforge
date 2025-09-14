using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NetForge.Player.Core;

namespace NetForge.Player.Commands;

/// <summary>
/// Display help information for available commands
/// </summary>
public class HelpCommand : IPlayerCommand
{
    public string Name => "help";
    public string Description => "Display help information for available commands";
    public string Usage => "help [command]";

    public async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        var args = context.Arguments;
        var commandProcessor = context.ServiceProvider.GetRequiredService<ICommandProcessor>();
        
        if (commandProcessor == null)
        {
            return CommandResult.Fail("Command processor not available");
        }

        if (args.Length == 0)
        {
            // Show general help
            return await ShowGeneralHelpAsync(commandProcessor);
        }
        else
        {
            // Show help for specific command
            return await ShowCommandHelpAsync(commandProcessor, args[0]);
        }
    }

    private async Task<CommandResult> ShowGeneralHelpAsync(ICommandProcessor commandProcessor)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Available Commands:");
        sb.AppendLine("==================");
        sb.AppendLine();

        var commands = commandProcessor.GetAllCommands()
            .OrderBy(c => c.Name)
            .ToList();

        var maxNameLength = commands.Max(c => c.Name.Length);

        foreach (var command in commands)
        {
            var paddedName = command.Name.PadRight(maxNameLength + 2);
            sb.AppendLine($"{paddedName} {command.Description}");
        }

        sb.AppendLine();
        sb.AppendLine("Use 'help <command>' for detailed information about a specific command.");
        sb.AppendLine("Use 'history' to view command history.");
        sb.AppendLine("Use 'exit' or 'quit' to leave the application.");

        return CommandResult.Ok(sb.ToString());
    }

    private async Task<CommandResult> ShowCommandHelpAsync(ICommandProcessor commandProcessor, string commandName)
    {
        var commands = commandProcessor.GetAllCommands();
        var command = commands.FirstOrDefault(c => 
            c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command == null)
        {
            return CommandResult.Fail($"Unknown command: '{commandName}'. Use 'help' to see available commands.");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Command: {command.Name}");
        sb.AppendLine($"Description: {command.Description}");
        sb.AppendLine($"Usage: {command.Usage}");

        // Show aliases if available
        if (command is IHasAliases aliasedCommand && aliasedCommand.Aliases.Length > 0)
        {
            sb.AppendLine($"Aliases: {string.Join(", ", aliasedCommand.Aliases)}");
        }

        return CommandResult.Ok(sb.ToString());
    }
}