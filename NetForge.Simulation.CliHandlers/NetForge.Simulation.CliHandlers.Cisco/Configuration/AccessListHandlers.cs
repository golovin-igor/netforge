using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Handles 'permit' command in ACL configuration mode
    /// </summary>
    public class PermitCommandHandler() : VendorAgnosticCliHandler("permit", "Permit packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "acl"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in ACL configuration mode");
            }

            var device = context.Device;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            // Reconstruct command with permit at the beginning
            var fullParts = new[] { "permit" }.Concat(context.CommandParts.Skip(1)).ToArray();

            // Get current ACL number from vendor context
            var vendorContext = GetVendorContext(context);
            int aclNumber = 1; // Default
            if (vendorContext?.Capabilities != null)
            {
                aclNumber = vendorContext.Capabilities.GetCurrentAclNumber();
            }

            return ProcessAclModeEntry(fullParts, context, device, aclNumber);
        }

        private CliResult ProcessAclModeEntry(string[] commandParts, ICliContext context, INetworkDevice device, int aclNumber)
        {
            try
            {
                var entry = new AclEntry();
                int index = 0;

                // Action
                entry.Action = commandParts[0].ToLower();
                index++;

                // Protocol or source
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
                        entry.Protocol = "ip";
                    }
                }

                // Parse source
                if (!ParseAddressSpec(commandParts, ref index, out string sourceAddr, out string sourceWildcard))
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid source address specification");
                }

                entry.SourceAddress = sourceAddr;
                entry.SourceWildcard = sourceWildcard;

                // Parse destination for extended ACLs
                if (IsExtendedAcl(aclNumber) && index < commandParts.Length)
                {
                    if (!ParseAddressSpec(commandParts, ref index, out string destAddr, out string destWildcard))
                    {
                        return Error(CliErrorType.InvalidParameter, "% Invalid destination address specification");
                    }

                    entry.DestAddress = destAddr;
                    entry.DestWildcard = destWildcard;
                }

                device.AddLogEntry($"ACL {aclNumber} permit entry added");

                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error processing permit entry: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error processing permit entry: {ex.Message}");
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

    /// <summary>
    /// Handles 'deny' command in ACL configuration mode
    /// </summary>
    public class DenyCommandHandler() : VendorAgnosticCliHandler("deny", "Deny packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "acl"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command can only be used in ACL configuration mode");
            }

            var device = context.Device;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            // Reconstruct command with deny at the beginning
            var fullParts = new[] { "deny" }.Concat(context.CommandParts.Skip(1)).ToArray();

            // Get current ACL number from vendor context
            var vendorContext = GetVendorContext(context);
            int aclNumber = 1; // Default
            if (vendorContext?.Capabilities != null)
            {
                aclNumber = vendorContext.Capabilities.GetCurrentAclNumber();
            }

            return ProcessAclModeEntry(fullParts, context, device, aclNumber);
        }

        private CliResult ProcessAclModeEntry(string[] commandParts, ICliContext context, INetworkDevice device, int aclNumber)
        {
            try
            {
                var entry = new AclEntry();
                int index = 0;

                // Action
                entry.Action = commandParts[0].ToLower();
                index++;

                // Protocol or source
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
                        entry.Protocol = "ip";
                    }
                }

                // Parse source
                if (!ParseAddressSpec(commandParts, ref index, out string sourceAddr, out string sourceWildcard))
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid source address specification");
                }

                entry.SourceAddress = sourceAddr;
                entry.SourceWildcard = sourceWildcard;

                // Parse destination for extended ACLs
                if (IsExtendedAcl(aclNumber) && index < commandParts.Length)
                {
                    if (!ParseAddressSpec(commandParts, ref index, out string destAddr, out string destWildcard))
                    {
                        return Error(CliErrorType.InvalidParameter, "% Invalid destination address specification");
                    }

                    entry.DestAddress = destAddr;
                    entry.DestWildcard = destWildcard;
                }

                device.AddLogEntry($"ACL {aclNumber} deny entry added");

                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error processing deny entry: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error processing deny entry: {ex.Message}");
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

    // Helper class for ACL entries
    public class AclEntry
    {
        public string Action { get; set; } = "";
        public string Protocol { get; set; } = "ip";
        public string SourceAddress { get; set; } = "";
        public string SourceWildcard { get; set; } = "";
        public string DestAddress { get; set; } = "";
        public string DestWildcard { get; set; } = "";
    }
}
