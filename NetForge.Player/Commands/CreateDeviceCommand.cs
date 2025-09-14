using System.Text;
using NetForge.Player.Core;
using NetForge.Player.Services;

namespace NetForge.Player.Commands;

/// <summary>
/// Create a new network device in the topology
/// </summary>
public class CreateDeviceCommand : IPlayerCommand, ISupportsCompletion
{
    public string Name => "create";
    public string Description => "Create a new network device";
    public string Usage => "create device <name> --type <type> --vendor <vendor> [--model <model>] [--interfaces <count>]";

    public async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        var args = context.Arguments;
        
        if (args.Length < 1)
        {
            return CommandResult.Error("Usage: " + Usage);
        }

        var subCommand = args[0].ToLowerInvariant();
        
        return subCommand switch
        {
            "device" => await CreateDeviceAsync(context, args.Skip(1).ToArray()),
            "connection" => await CreateConnectionAsync(context, args.Skip(1).ToArray()),
            "interface" => await CreateInterfaceAsync(context, args.Skip(1).ToArray()),
            _ => CommandResult.Error($"Unknown create type: '{args[0]}'. Use 'device', 'connection', or 'interface'.")
        };
    }

    private async Task<CommandResult> CreateDeviceAsync(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            return CommandResult.Error("Device name is required. Usage: create device <name> --type <type> --vendor <vendor>");
        }

        var deviceName = args[0];
        var parameters = ParseParameters(args.Skip(1).ToArray());
        
        // Validate required parameters
        if (!parameters.ContainsKey("type"))
        {
            return CommandResult.Error("Device type is required. Use --type <router|switch|firewall|server>");
        }
        
        if (!parameters.ContainsKey("vendor"))
        {
            return CommandResult.Error("Vendor is required. Use --vendor <cisco|juniper|arista|f5|mikrotik>");
        }

        var networkManager = context.ServiceProvider.GetService<INetworkManager>();
        if (networkManager == null)
        {
            return CommandResult.Error("Network manager not available");
        }

        try
        {
            // Parse device type
            if (!Enum.TryParse<DeviceType>(parameters["type"], true, out var deviceType))
            {
                return CommandResult.Error($"Invalid device type: '{parameters["type"]}'. Valid types: Router, Switch, Firewall, Server");
            }

            var vendor = parameters["vendor"];
            var model = parameters.GetValueOrDefault("model", "Generic");
            
            // Parse interface count
            var interfaceCount = 2; // Default
            if (parameters.ContainsKey("interfaces"))
            {
                if (!int.TryParse(parameters["interfaces"], out interfaceCount) || interfaceCount < 1)
                {
                    return CommandResult.Error("Interface count must be a positive integer");
                }
            }

            // Create device
            var device = await networkManager.CreateDeviceAsync(deviceName, deviceType, vendor, model);
            
            // Add interfaces
            for (int i = 1; i <= interfaceCount; i++)
            {
                var interfaceName = deviceType switch
                {
                    DeviceType.Router => $"GigabitEthernet0/{i}",
                    DeviceType.Switch => $"FastEthernet0/{i}",
                    DeviceType.Firewall => i == 1 ? "outside" : i == 2 ? "inside" : $"dmz{i - 2}",
                    DeviceType.Server => $"eth{i - 1}",
                    _ => $"Interface{i}"
                };
                
                await device.AddInterfaceAsync(interfaceName, InterfaceType.Ethernet);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Device '{deviceName}' created successfully.");
            sb.AppendLine($"Type: {deviceType}");
            sb.AppendLine($"Vendor: {vendor}");
            sb.AppendLine($"Model: {model}");
            sb.AppendLine($"Interfaces: {interfaceCount}");
            sb.AppendLine();
            sb.AppendLine("Use 'list devices' to see all devices or 'connect <device>' to access its console.");

            return CommandResult.Success(sb.ToString());
        }
        catch (Exception ex)
        {
            return CommandResult.Error($"Failed to create device: {ex.Message}");
        }
    }

    private async Task<CommandResult> CreateConnectionAsync(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            return CommandResult.Error("Usage: create connection <source_device:interface> <dest_device:interface>");
        }

        var networkManager = context.ServiceProvider.GetService<INetworkManager>();
        if (networkManager == null)
        {
            return CommandResult.Error("Network manager not available");
        }

        try
        {
            var sourceParts = args[0].Split(':');
            var destParts = args[1].Split(':');
            
            if (sourceParts.Length != 2 || destParts.Length != 2)
            {
                return CommandResult.Error("Connection format: <device:interface> <device:interface>");
            }

            var sourceDevice = sourceParts[0];
            var sourceInterface = sourceParts[1];
            var destDevice = destParts[0];
            var destInterface = destParts[1];

            await networkManager.CreateConnectionAsync(sourceDevice, sourceInterface, destDevice, destInterface);

            return CommandResult.Success($"Connection created: {sourceDevice}:{sourceInterface} ↔ {destDevice}:{destInterface}");
        }
        catch (Exception ex)
        {
            return CommandResult.Error($"Failed to create connection: {ex.Message}");
        }
    }

    private async Task<CommandResult> CreateInterfaceAsync(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            return CommandResult.Error("Usage: create interface <device> <interface_name> [--type <type>]");
        }

        var deviceName = args[0];
        var interfaceName = args[1];
        var parameters = ParseParameters(args.Skip(2).ToArray());
        
        var networkManager = context.ServiceProvider.GetService<INetworkManager>();
        if (networkManager == null)
        {
            return CommandResult.Error("Network manager not available");
        }

        try
        {
            var device = await networkManager.GetDeviceAsync(deviceName);
            if (device == null)
            {
                return CommandResult.Error($"Device '{deviceName}' not found");
            }

            var interfaceType = InterfaceType.Ethernet; // Default
            if (parameters.ContainsKey("type"))
            {
                if (!Enum.TryParse<InterfaceType>(parameters["type"], true, out interfaceType))
                {
                    return CommandResult.Error($"Invalid interface type: '{parameters["type"]}'.");
                }
            }

            await device.AddInterfaceAsync(interfaceName, interfaceType);

            return CommandResult.Success($"Interface '{interfaceName}' added to device '{deviceName}'");
        }
        catch (Exception ex)
        {
            return CommandResult.Error($"Failed to create interface: {ex.Message}");
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

    public List<string> GetCompletions(string[] currentArgs, string partialInput)
    {
        if (currentArgs.Length == 0)
        {
            return ["device", "connection", "interface"];
        }

        var subCommand = currentArgs[0].ToLowerInvariant();
        
        return subCommand switch
        {
            "device" => GetDeviceCompletions(currentArgs.Skip(1).ToArray()),
            "connection" => GetConnectionCompletions(currentArgs.Skip(1).ToArray()),
            "interface" => GetInterfaceCompletions(currentArgs.Skip(1).ToArray()),
            _ => new List<string>()
        };
    }

    private List<string> GetDeviceCompletions(string[] args)
    {
        // Simple completions for device creation parameters
        if (args.Length > 1 && args[^2] == "--type")
        {
            return ["router", "switch", "firewall", "server"];
        }
        
        if (args.Length > 1 && args[^2] == "--vendor")
        {
            return ["cisco", "juniper", "arista", "f5", "mikrotik", "nokia", "paloalto"];
        }
        
        return ["--type", "--vendor", "--model", "--interfaces"];
    }

    private List<string> GetConnectionCompletions(string[] args)
    {
        // TODO: Get actual device and interface names from network manager
        return new List<string>();
    }

    private List<string> GetInterfaceCompletions(string[] args)
    {
        if (args.Length > 1 && args[^2] == "--type")
        {
            return ["ethernet", "serial", "loopback", "tunnel"];
        }
        
        return ["--type"];
    }
}