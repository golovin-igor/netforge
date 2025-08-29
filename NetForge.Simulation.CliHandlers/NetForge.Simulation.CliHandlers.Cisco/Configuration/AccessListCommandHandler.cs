using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'access-list' command in configuration mode
/// </summary>
public class AccessListCommandHandler() : VendorAgnosticCliHandler("access-list", "Configure access control lists")
{
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

        if (context.CommandParts.Length < 2)
        {
            return Error(CliErrorType.IncompleteCommand,
                "% Incomplete command. Usage: access-list <number> <permit|deny> ...");
        }

        if (!int.TryParse(context.CommandParts[1], out int aclNumber))
        {
            return Error(CliErrorType.InvalidParameter,
                "% Invalid access list number");
        }

        // Validate ACL number range
        if (!IsValidAclNumber(aclNumber))
        {
            return Error(CliErrorType.InvalidParameter,
                "% Invalid access list number. Use 1-99 (standard), 100-199 (extended), or 1300-2699 (extended)");
        }

        var device = context.Device;
        if (device == null)
        {
            return Error(CliErrorType.ExecutionError, "% Device not available");
        }

        // Process the ACL entry if more parameters provided
        if (context.CommandParts.Length > 2)
        {
            return ProcessAclEntry(context.CommandParts, context, device, aclNumber);
        }

        // Log ACL creation
        device.AddLogEntry($"Access list {aclNumber} created/modified");
        return Success("");
    }

    private CliResult ProcessAclEntry(string[] commandParts, ICliContext context, INetworkDevice device, int aclNumber)
    {
        try
        {
            var entry = new AclEntry();
            int index = 2; // Start after "access-list <number>"

            // Action (permit/deny)
            if (index >= commandParts.Length)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            entry.Action = commandParts[index].ToLower();
            if (entry.Action != "permit" && entry.Action != "deny")
            {
                return Error(CliErrorType.InvalidParameter,
                    "% Invalid action. Use permit or deny");
            }

            index++;

            // Protocol (for extended ACLs) or source (for standard ACLs)
            if (index < commandParts.Length)
            {
                var part = commandParts[index].ToLower();
                if (IsProtocol(part))
                {
                    entry.Protocol = part;
                    index++;
                }
                else
                {
                    entry.Protocol = "ip"; // Default for standard ACLs
                }
            }

            // Parse source address
            if (!ParseAddressSpec(commandParts, ref index, out string sourceAddr, out string sourceWildcard))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid source address specification");
            }

            entry.SourceAddress = sourceAddr;
            entry.SourceWildcard = sourceWildcard;

            // For extended ACLs, parse destination
            if (IsExtendedAcl(aclNumber) && index < commandParts.Length)
            {
                if (!ParseAddressSpec(commandParts, ref index, out string destAddr, out string destWildcard))
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid destination address specification");
                }

                entry.DestAddress = destAddr;
                entry.DestWildcard = destWildcard;
            }

            // Get vendor context and add ACL entry to device state
            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                // Add ACL entry to device state
                if (!vendorContext.Capabilities.AddAclEntry(aclNumber, entry))
                {
                    return Error(CliErrorType.ExecutionError, "% Failed to add ACL entry");
                }

                // Add to running configuration
                var configLine = $"access-list {aclNumber} {string.Join(" ", commandParts.Skip(2))}";
                vendorContext.Capabilities.AppendToRunningConfig(configLine);
            }

            // For Cisco devices, also update the actual device state
            // Use reflection or type checking to avoid direct dependencies
            var deviceType = device.GetType();
            if (deviceType.Name == "CiscoDevice")
            {
                try
                {
                    // Use reflection to call CiscoDevice methods
                    var addAclEntryMethod = deviceType.GetMethod("AddAclEntry");
                    var appendConfigMethod = deviceType.GetMethod("AppendToRunningConfig");

                    if (addAclEntryMethod != null)
                    {
                        addAclEntryMethod.Invoke(device, new object[] { aclNumber, entry });
                    }

                    if (appendConfigMethod != null)
                    {
                        var configLine = $"access-list {aclNumber} {string.Join(" ", commandParts.Skip(2))}";
                        appendConfigMethod.Invoke(device, new object[] { configLine });
                    }
                }
                catch (Exception ex)
                {
                    device.AddLogEntry($"Warning: Could not update device state: {ex.Message}");
                }
            }

            device.AddLogEntry($"Access list {aclNumber} entry added: {entry.Action} {entry.Protocol} {entry.SourceAddress}");

            return Success("");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error processing ACL entry: {ex.Message}");
            return Error(CliErrorType.ExecutionError, $"% Error processing ACL entry: {ex.Message}");
        }
    }

    private bool ParseAddressSpec(string[] commandParts, ref int index, out string address, out string wildcard)
    {
        address = "";
        wildcard = "";

        if (index >= commandParts.Length)
            return false;

        if (commandParts[index].ToLower() == "any")
        {
            address = "any";
            wildcard = "255.255.255.255";
            index++;
            return true;
        }
        else if (commandParts[index].ToLower() == "host" && index + 1 < commandParts.Length)
        {
            address = commandParts[index + 1];
            wildcard = "0.0.0.0";
            index += 2;
            return true;
        }
        else
        {
            address = commandParts[index];
            if (index + 1 < commandParts.Length && IsWildcardMask(commandParts[index + 1]))
            {
                wildcard = commandParts[index + 1];
                index += 2;
            }
            else
            {
                wildcard = "0.0.0.0";
                index++;
            }

            return true;
        }
    }

    private bool IsValidAclNumber(int number)
    {
        // Standard ACLs: 1-99
        // Extended ACLs: 100-199, 1300-2699
        return (number >= 1 && number <= 99) ||
               (number >= 100 && number <= 199) ||
               (number >= 1300 && number <= 2699);
    }

    private bool IsExtendedAcl(int number)
    {
        return (number >= 100 && number <= 199) || (number >= 1300 && number <= 2699);
    }

    private bool IsProtocol(string part)
    {
        var protocols = new[] { "ip", "tcp", "udp", "icmp", "eigrp", "gre", "ospf" };
        return protocols.Contains(part.ToLower());
    }

    private bool IsWildcardMask(string str)
    {
        var parts = str.Split('.');
        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                return false;
        }

        return true;
    }
}
