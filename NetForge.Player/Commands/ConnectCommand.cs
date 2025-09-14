using NetForge.Player.Core;
using NetForge.Player.Services;

namespace NetForge.Player.Commands;

/// <summary>
/// Connect to a device console session
/// </summary>
public class ConnectCommand : IPlayerCommand, ISupportsCompletion
{
    public string Name => "connect";
    public string Description => "Connect to a device console or terminal session";
    public string Usage => "connect <device_name> [--protocol <telnet|ssh|console>]";

    public async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        var args = context.Arguments;
        
        if (args.Length < 1)
        {
            return CommandResult.Fail("Device name is required. Usage: " + Usage);
        }

        var deviceName = args[0];
        var parameters = ParseParameters(args.Skip(1).ToArray());
        var protocol = parameters.GetValueOrDefault("protocol", "console").ToLowerInvariant();
        
        var networkManager = context.ServiceProvider.GetService<INetworkManager>();
        var sessionManager = context.ServiceProvider.GetService<ISessionManager>();
        
        if (networkManager == null)
        {
            return CommandResult.Fail("Network manager not available");
        }
        
        if (sessionManager == null)
        {
            return CommandResult.Fail("Session manager not available");
        }

        try
        {
            // Find the device
            var device = await networkManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                return CommandResult.Fail($"Device '{deviceName}' not found. Use 'list devices' to see available devices.");
            }

            // Check if device is running
            if (!device.IsRunning)
            {
                return CommandResult.Fail($"Device '{deviceName}' is not running. Start the device first.");
            }

            // Validate protocol
            if (!IsValidProtocol(protocol))
            {
                return CommandResult.Fail($"Invalid protocol: '{protocol}'. Valid protocols: console, telnet, ssh");
            }

            // Create or get existing session
            var session = await sessionManager.CreateSessionAsync(device, protocol);
            
            if (session == null)
            {
                return CommandResult.Fail($"Failed to create session to device '{deviceName}' using {protocol}");
            }

            // Start the terminal session
            Console.WriteLine($"Connecting to {deviceName} via {protocol}...");
            Console.WriteLine("Press Ctrl+] to disconnect and return to NetForge console.\n");
            
            await session.StartInteractiveSessionAsync();
            
            return CommandResult.Ok($"Disconnected from {deviceName}");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail($"Failed to connect to device: {ex.Message}");
        }
    }

    private Dictionary<string, string> ParseParameters(string[] args)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i].Substring(2);
                var value = args[i + 1];
                parameters[key] = value;
                i++; // Skip the value in next iteration
            }
        }
        
        return parameters;
    }

    private bool IsValidProtocol(string protocol)
    {
        return protocol switch
        {
            "console" or "telnet" or "ssh" => true,
            _ => false
        };
    }

    public List<string> GetCompletions(string[] currentArgs, string partialInput)
    {
        // First argument should be device name
        if (currentArgs.Length == 0)
        {
            // TODO: Get actual device names from network manager
            // For now, return common device naming patterns
            return ["router1", "switch1", "firewall1", "server1"];
        }
        
        // Check for protocol parameter
        if (currentArgs.Length > 1 && currentArgs[^2] == "--protocol")
        {
            return ["console", "telnet", "ssh"];
        }
        
        // Offer protocol parameter
        if (currentArgs.Length >= 1)
        {
            return ["--protocol"];
        }
        
        return new List<string>();
    }
}

/// <summary>
/// Alias for connect command
/// </summary>
public class TelnetCommand : ConnectCommand, IHasAliases
{
    public new string Name => "telnet";
    public new string Description => "Connect to a device via Telnet (alias for connect --protocol telnet)";
    public new string Usage => "telnet <device_name>";
    
    public string[] Aliases => ["ssh"];
    
    public new async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        // Force telnet protocol
        var args = context.Arguments.Concat(["--protocol", "telnet"]).ToArray();
        var newContext = new CommandContext
        {
            CommandName = context.CommandName,
            Arguments = args,
            RawInput = context.RawInput,
            ServiceProvider = context.ServiceProvider
        };
        
        return await base.ExecuteAsync(newContext);
    }
}