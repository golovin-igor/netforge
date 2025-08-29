using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Dell.Configuration;

/// <summary>
/// Interface configuration command handler with comprehensive Dell OS10 support
/// </summary>
public class InterfaceCommandHandler() : VendorAgnosticCliHandler("interface", "Configure interface parameters")
{
    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (!IsVendor(context, "Dell"))
        {
            return RequireVendor(context, "Dell");
        }

        if (!IsInMode(context, "config"))
        {
            return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
        }

        if (context.CommandParts.Length < 2)
        {
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }

        var interfaceName = string.Join(" ", context.CommandParts.Skip(1));
        interfaceName = FormatInterfaceName(interfaceName);

        var device = context.Device;
        var interfaces = device?.GetAllInterfaces();

        // Check for existing interface
        if (interfaces != null && interfaces.ContainsKey(interfaceName))
        {
            SetCurrentInterface(context, interfaceName);
            SetMode(context, "interface");
            return Success("");
        }

        // Handle VLAN interface creation
        if (interfaceName.StartsWith("vlan"))
        {
            if (int.TryParse(interfaceName.Replace("vlan", "").Trim(), out int vlanId))
            {
                var vlans = device?.GetAllVlans();
                if (vlans != null && vlans.ContainsKey(vlanId))
                {
                    if (!interfaces.ContainsKey(interfaceName))
                    {
                        interfaces[interfaceName] = CreateInterface(interfaceName, vlanId);
                    }
                    SetCurrentInterface(context, interfaceName);
                    SetMode(context, "interface");
                    return Success("");
                }
            }
        }

        // Handle port-channel interface creation
        if (interfaceName.StartsWith("port-channel"))
        {
            if (int.TryParse(interfaceName.Replace("port-channel", "").Trim(), out int channelId))
            {
                if (!interfaces.ContainsKey(interfaceName))
                {
                    interfaces[interfaceName] = CreateInterface(interfaceName);
                }
                SetCurrentInterface(context, interfaceName);
                SetMode(context, "interface");
                return Success("");
            }
        }

        return Error(CliErrorType.ExecutionError, "% Error: Interface not found");
    }

    private dynamic CreateInterface(string name, int vlanId = 0)
    {
        return new
        {
            Name = name,
            VlanId = vlanId,
            IsUp = false,
            IsShutdown = true,
            IpAddress = "",
            SubnetMask = "",
            Description = "",
            SwitchportMode = "",
            MacAddress = "00:01:e8:8b:44:56",
            RxPackets = 0,
            TxPackets = 0,
            RxBytes = 0,
            TxBytes = 0,
            Mtu = 1500,
            Bandwidth = 1000000
        };
    }

    private string FormatInterfaceName(string interfaceName)
    {
        if (interfaceName.StartsWith("eth"))
            return "ethernet" + interfaceName.Substring(3);
        if (interfaceName.StartsWith("gi"))
            return "ethernet" + interfaceName.Substring(2);
        return interfaceName;
    }
}
