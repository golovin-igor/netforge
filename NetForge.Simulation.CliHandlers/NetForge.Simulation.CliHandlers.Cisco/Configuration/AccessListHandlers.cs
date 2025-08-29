using System.Linq;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Handles 'access-list' command in configuration mode
    /// </summary>
    public class AccessListCommandHandler() : VendorAgnosticCliHandler("access-list", "Configure access control lists")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

            var device = context.Device as NetworkDevice;
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

        private CliResult ProcessAclEntry(string[] commandParts, CliContext context, NetworkDevice device, int aclNumber)
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

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

    public class IpAccessListStandardHandler() : VendorAgnosticCliHandler("standard", "Standard IP access list")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: ip access-list standard <name|number>");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            var aclIdentifier = context.CommandParts[1];
            int aclNumber;

            // Try to parse as number, otherwise use hash of name for standard range
            if (!int.TryParse(aclIdentifier, out aclNumber))
            {
                aclNumber = Math.Abs(aclIdentifier.GetHashCode()) % 99 + 1; // Standard ACL range 1-99
            }

            // Validate standard ACL number range
            if (aclNumber < 1 || aclNumber > 99)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Standard access list number must be in range 1-99");
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
                    vendorContext.Capabilities.AppendToRunningConfig($"ip access-list standard {aclIdentifier}");
                }
                
                device.AddLogEntry($"Entering standard access list configuration for {aclIdentifier}");
                
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

    public class IpAccessListExtendedHandler() : VendorAgnosticCliHandler("extended", "Extended IP access list")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: ip access-list extended <name|number>");
            }

            var device = context.Device as NetworkDevice;
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

    /// <summary>
    /// Handles 'permit' command in ACL configuration mode
    /// </summary>
    public class PermitCommandHandler() : VendorAgnosticCliHandler("permit", "Permit packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

            var device = context.Device as NetworkDevice;
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

        private CliResult ProcessAclModeEntry(string[] commandParts, CliContext context, NetworkDevice device, int aclNumber)
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
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

            var device = context.Device as NetworkDevice;
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

        private CliResult ProcessAclModeEntry(string[] commandParts, CliContext context, NetworkDevice device, int aclNumber)
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
