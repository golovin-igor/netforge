using System.Text;
using NetForge.Player.Core;
using NetForge.Player.Services;

namespace NetForge.Player.Commands;

/// <summary>
/// List all network devices in the current topology
/// </summary>
public class ListDevicesCommand : IPlayerCommand
{
    public string Name => "list";
    public string Description => "List network devices and their status";
    public string Usage => "list [devices|interfaces|connections]";

    public async Task<CommandResult> ExecuteAsync(CommandContext context)
    {
        var args = context.Arguments;
        var networkManager = context.ServiceProvider.GetService<INetworkManager>();
        
        if (networkManager == null)
        {
            return CommandResult.Fail("Network manager not available");
        }

        var listType = args.Length > 0 ? args[0].ToLowerInvariant() : "devices";
        
        return listType switch
        {
            "devices" => await ListDevicesAsync(networkManager),
            "interfaces" => await ListInterfacesAsync(networkManager),
            "connections" => await ListConnectionsAsync(networkManager),
            _ => CommandResult.Fail($"Unknown list type: '{args[0]}'. Use 'devices', 'interfaces', or 'connections'.")
        };
    }

    private async Task<CommandResult> ListDevicesAsync(INetworkManager networkManager)
    {
        var devices = await networkManager.GetAllDevicesAsync();
        
        if (!devices.Any())
        {
            return CommandResult.Ok("No devices found. Use 'create device' to add devices.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Network Devices:");
        sb.AppendLine("=================");
        sb.AppendLine();
        
        // Table headers
        sb.AppendLine($"{"Name",-20} {"Type",-15} {"Vendor",-10} {"Model",-15} {"Status",-10} {"Interfaces",-12}");
        sb.AppendLine(new string('-', 85));

        foreach (var device in devices.OrderBy(d => d.Name))
        {
            var interfaceCount = device.GetInterfaces()?.Count() ?? 0;
            var status = device.IsRunning ? "Running" : "Stopped";
            
            sb.AppendLine($"{device.Name,-20} {device.DeviceType,-15} {device.Vendor,-10} {device.Model,-15} {status,-10} {interfaceCount,-12}");
        }

        sb.AppendLine();
        sb.AppendLine($"Total devices: {devices.Count()}");

        return CommandResult.Ok(sb.ToString());
    }

    private async Task<CommandResult> ListInterfacesAsync(INetworkManager networkManager)
    {
        var devices = await networkManager.GetAllDevicesAsync();
        
        if (!devices.Any())
        {
            return CommandResult.Ok("No devices found.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Network Interfaces:");
        sb.AppendLine("==================");
        sb.AppendLine();
        
        foreach (var device in devices.OrderBy(d => d.Name))
        {
            var interfaces = device.GetInterfaces() ?? Enumerable.Empty<INetworkInterface>();
            
            if (!interfaces.Any())
                continue;
                
            sb.AppendLine($"Device: {device.Name} ({device.Vendor} {device.Model})");
            sb.AppendLine($"{"Interface",-15} {"Type",-10} {"Status",-8} {"IP Address",-15} {"Description",-20}");
            sb.AppendLine(new string('-', 75));
            
            foreach (var iface in interfaces.OrderBy(i => i.Name))
            {
                var status = iface.IsEnabled ? "Up" : "Down";
                var ipAddress = iface.IpAddress?.ToString() ?? "N/A";
                
                sb.AppendLine($"{iface.Name,-15} {iface.InterfaceType,-10} {status,-8} {ipAddress,-15} {iface.Description,-20}");
            }
            
            sb.AppendLine();
        }

        return CommandResult.Ok(sb.ToString());
    }

    private async Task<CommandResult> ListConnectionsAsync(INetworkManager networkManager)
    {
        var topology = await networkManager.GetTopologyAsync();
        
        if (topology?.Links == null || !topology.Links.Any())
        {
            return CommandResult.Ok("No connections found.");
        }

        var sb = new StringBuilder();
        sb.AppendLine("Network Connections:");
        sb.AppendLine("===================");
        sb.AppendLine();
        
        sb.AppendLine($"{"Source Device",-20} {"Source Interface",-15} {"Destination Device",-20} {"Dest Interface",-15} {"Status",-8}");
        sb.AppendLine(new string('-', 85));

        foreach (var link in topology.Links.OrderBy(l => l.SourceDevice.Name).ThenBy(l => l.DestinationDevice.Name))
        {
            var status = link.IsActive ? "Active" : "Down";
            
            sb.AppendLine($"{link.SourceDevice.Name,-20} {link.SourceInterface.Name,-15} {link.DestinationDevice.Name,-20} {link.DestinationInterface.Name,-15} {status,-8}");
        }

        sb.AppendLine();
        sb.AppendLine($"Total connections: {topology.Links.Count()}");

        return CommandResult.Ok(sb.ToString());
    }
}

/// <summary>
/// Command with aliases for listing devices
/// </summary>
public class ListCommand : ListDevicesCommand, IHasAliases
{
    public string[] Aliases => ["ls", "show"];
}