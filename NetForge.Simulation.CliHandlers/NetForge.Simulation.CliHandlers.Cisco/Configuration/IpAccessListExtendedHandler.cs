using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class IpAccessListExtendedHandler() : VendorAgnosticCliHandler("extended", "Extended IP access list")
{
    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (context.CommandParts.Length < 2)
        {
            return Error(CliErrorType.IncompleteCommand,
                "% Incomplete command. Usage: ip access-list extended <name|number>");
        }

        var device = context.Device;
        if (device == null)
        {
            return Error(CliErrorType.ExecutionError, "% Device not available");
        }

        var aclIdentifier = context.CommandParts[1];
        int aclNumber;

        // Try to parse as number, otherwise use hash of name for extended range
        if (!int.TryParse(aclIdentifier, out aclNumber))
        {
            aclNumber = Math.Abs(aclIdentifier.GetHashCode()) % 100 + 100; // Extended ACL range 100-199
        }

        // Validate extended ACL number range
        if (aclNumber < 100 || aclNumber > 199)
        {
            return Error(CliErrorType.InvalidParameter,
                "% Extended access list number must be in range 100-199");
        }

        try
        {
            // Set ACL mode for named access list configuration
            SetMode(context, "acl");

            // Get vendor context and set current ACL number
            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                vendorContext.Capabilities.SetCurrentAclNumber(aclNumber);
                vendorContext.Capabilities.AppendToRunningConfig($"ip access-list extended {aclIdentifier}");
            }

            device.AddLogEntry($"Entering extended access list configuration for {aclIdentifier}");

            return Success("");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error entering ACL configuration: {ex.Message}");
            return Error(CliErrorType.ExecutionError,
                $"% Error entering ACL configuration: {ex.Message}");
        }
    }
}
