using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'ip access-list' command in configuration mode
/// </summary>
public class IpAccessListCommandHandler : VendorAgnosticCliHandler
{
    public IpAccessListCommandHandler() : base("access-list", "Configure named IP access lists")
    {
        AddSubHandler("standard", new IpAccessListStandardHandler());
        AddSubHandler("extended", new IpAccessListExtendedHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (!IsVendor(context, "Cisco"))
        {
            return RequireVendor(context, "Cisco");
        }

        if (!IsInMode(context, "config"))
        {
            return Error(CliErrorType.InvalidMode,
                "% This command can only be used in config mode");
        }

        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Usage: ip access-list {standard|extended} <name>");
    }
}
