using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Handles 'switchport' command in interface configuration mode
    /// </summary>
    public class SwitchportCommandHandler : VendorAgnosticCliHandler
    {
        public SwitchportCommandHandler() : base("switchport", "Configure switchport parameters")
        {
            AddSubHandler("mode", new SwitchportModeHandler());
            AddSubHandler("access", new SwitchportAccessHandler());
            AddSubHandler("trunk", new SwitchportTrunkHandler());
            AddSubHandler("voice", new SwitchportVoiceHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% This command can only be used in interface configuration mode");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            // Basic switchport command without parameters
            if (context.CommandParts.Length == 1)
            {
                try
                {
                    var currentInterface = context.Device.GetCurrentInterface();
                    if (string.IsNullOrEmpty(currentInterface))
                    {
                        return Error(CliErrorType.InvalidMode, "% No interface selected");
                    }

                    var iface = device.GetInterface(currentInterface);
                    if (iface != null)
                    {
                        // Enable switchport mode (default to access)
                        iface.SwitchportMode = "access";
                        device.AddLogEntry($"Switchport enabled on interface {currentInterface}");
                        return Success("");
                    }
                    
                    return Error(CliErrorType.ExecutionError, "% Interface not found");
                }
                catch (Exception ex)
                {
                    device.AddLogEntry($"Error enabling switchport: {ex.Message}");
                    return Error(CliErrorType.ExecutionError, $"% Error enabling switchport: {ex.Message}");
                }
            }

            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    /// <summary>
    /// Handles 'switchport mode' command
    /// </summary>
    public class SwitchportModeHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeHandler() : base("mode", "Set switchport mode")
        {
            AddSubHandler("access", new SwitchportModeAccessHandler());
            AddSubHandler("trunk", new SwitchportModeTrunkHandler());
            AddSubHandler("dynamic", new SwitchportModeDynamicHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: access, trunk, dynamic");
        }
    }

    public class SwitchportModeAccessHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeAccessHandler() : base("access", "Set switchport to access mode")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    iface.SwitchportMode = "access";
                    
                    // Update running configuration
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        vendorContext.Capabilities.AppendToRunningConfig(" switchport mode access");
                    }
                    
                    device.AddLogEntry($"Interface {currentInterface} set to access mode");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting access mode: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting access mode: {ex.Message}");
            }
        }
    }

    public class SwitchportModeTrunkHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeTrunkHandler() : base("trunk", "Set switchport to trunk mode")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    iface.SwitchportMode = "trunk";
                    
                    // Update running configuration
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        vendorContext.Capabilities.AppendToRunningConfig(" switchport mode trunk");
                    }
                    
                    device.AddLogEntry($"Interface {currentInterface} set to trunk mode");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting trunk mode: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting trunk mode: {ex.Message}");
            }
        }
    }

    public class SwitchportModeDynamicHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeDynamicHandler() : base("dynamic", "Set switchport to dynamic mode")
        {
            AddSubHandler("auto", new SwitchportModeDynamicAutoHandler());
            AddSubHandler("desirable", new SwitchportModeDynamicDesirableHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: auto, desirable");
        }
    }

    public class SwitchportModeDynamicAutoHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeDynamicAutoHandler() : base("auto", "Set to dynamic auto mode")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    iface.SwitchportMode = "dynamic auto";
                    device.AddLogEntry($"Interface {currentInterface} set to dynamic auto mode");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting dynamic auto mode: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting dynamic auto mode: {ex.Message}");
            }
        }
    }

    public class SwitchportModeDynamicDesirableHandler : VendorAgnosticCliHandler
    {
        public SwitchportModeDynamicDesirableHandler() : base("desirable", "Set to dynamic desirable mode")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    iface.SwitchportMode = "dynamic desirable";
                    device.AddLogEntry($"Interface {currentInterface} set to dynamic desirable mode");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting dynamic desirable mode: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting dynamic desirable mode: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles 'switchport access' command
    /// </summary>
    public class SwitchportAccessHandler : VendorAgnosticCliHandler
    {
        public SwitchportAccessHandler() : base("access", "Set access mode configuration")
        {
            AddSubHandler("vlan", new SwitchportAccessVlanHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: vlan");
        }
    }

    public class SwitchportAccessVlanHandler : VendorAgnosticCliHandler
    {
        public SwitchportAccessVlanHandler() : base("vlan", "Set access VLAN")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: switchport access vlan <vlan-id>");
            }

            if (!int.TryParse(context.CommandParts[1], out int vlanId))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid VLAN ID");
            }

            if (vlanId < 1 || vlanId > 4094)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% VLAN ID must be between 1 and 4094");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    // Get vendor context for device operations
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        // Create VLAN if it doesn't exist (Cisco IOS auto-creates VLANs)
                        if (!vendorContext.Capabilities.VlanExists(vlanId))
                        {
                            vendorContext.Capabilities.CreateOrSelectVlan(vlanId);
                        }

                        // Set interface to access mode and assign to VLAN
                        vendorContext.Capabilities.SetInterfaceVlan(currentInterface, vlanId);
                        vendorContext.Capabilities.SetSwitchportMode(currentInterface, "access");
                        
                        // Add interface to VLAN
                        vendorContext.Capabilities.AddInterfaceToVlan(currentInterface, vlanId);
                        
                        // Update running configuration
                        vendorContext.Capabilities.AppendToRunningConfig($" switchport access vlan {vlanId}");
                    }
                    
                    // For Cisco devices, also update the actual device state
                    var deviceType = device.GetType();
                    if (deviceType.Name == "CiscoDevice")
                    {
                        try
                        {
                            // Use reflection to call CiscoDevice methods
                            var createVlanMethod = deviceType.GetMethod("CreateOrSelectVlan");
                            var addInterfaceToVlanMethod = deviceType.GetMethod("AddInterfaceToVlan");
                            var appendConfigMethod = deviceType.GetMethod("AppendToRunningConfig");
                            var vlanExistsMethod = deviceType.GetMethod("VlanExists");
                            
                            // Check if VLAN exists and create if needed
                            if (vlanExistsMethod != null && createVlanMethod != null)
                            {
                                var vlanExists = (bool)vlanExistsMethod.Invoke(device, new object[] { vlanId });
                                if (!vlanExists)
                                {
                                    createVlanMethod.Invoke(device, new object[] { vlanId });
                                }
                            }
                            
                            // Add interface to VLAN
                            if (addInterfaceToVlanMethod != null)
                            {
                                addInterfaceToVlanMethod.Invoke(device, new object[] { currentInterface, vlanId });
                            }
                            
                            // Update running configuration
                            if (appendConfigMethod != null)
                            {
                                appendConfigMethod.Invoke(device, new object[] { $" switchport access vlan {vlanId}" });
                            }
                        }
                        catch (Exception ex)
                        {
                            device.AddLogEntry($"Warning: Could not update device state: {ex.Message}");
                        }
                    }
                    
                    // Also update the interface object directly
                    iface.VlanId = vlanId;
                    iface.SwitchportMode = "access";
                    
                    device.AddLogEntry($"Interface {currentInterface} assigned to access VLAN {vlanId}");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting access VLAN: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting access VLAN: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles 'switchport trunk' command
    /// </summary>
    public class SwitchportTrunkHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkHandler() : base("trunk", "Set trunk mode configuration")
        {
            AddSubHandler("encapsulation", new SwitchportTrunkEncapsulationHandler());
            AddSubHandler("allowed", new SwitchportTrunkAllowedHandler());
            AddSubHandler("native", new SwitchportTrunkNativeHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: encapsulation, allowed, native");
        }
    }

    public class SwitchportTrunkEncapsulationHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkEncapsulationHandler() : base("encapsulation", "Set trunk encapsulation")
        {
            AddSubHandler("dot1q", new SwitchportTrunkEncapsulationDot1qHandler());
            AddSubHandler("isl", new SwitchportTrunkEncapsulationIslHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: dot1q, isl");
        }
    }

    public class SwitchportTrunkEncapsulationDot1qHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkEncapsulationDot1qHandler() : base("dot1q", "Set 802.1Q encapsulation")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                device.AddLogEntry($"Interface {currentInterface} trunk encapsulation set to 802.1Q");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting trunk encapsulation: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting trunk encapsulation: {ex.Message}");
            }
        }
    }

    public class SwitchportTrunkEncapsulationIslHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkEncapsulationIslHandler() : base("isl", "Set ISL encapsulation")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                device.AddLogEntry($"Interface {currentInterface} trunk encapsulation set to ISL");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting trunk encapsulation: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting trunk encapsulation: {ex.Message}");
            }
        }
    }

    public class SwitchportTrunkAllowedHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkAllowedHandler() : base("allowed", "Set allowed VLANs on trunk")
        {
            AddSubHandler("vlan", new SwitchportTrunkAllowedVlanHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: vlan");
        }
    }

    public class SwitchportTrunkAllowedVlanHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkAllowedVlanHandler() : base("vlan", "Set allowed VLANs")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: switchport trunk allowed vlan <vlan-list>");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var vlanList = context.CommandParts[1];
                var currentInterface = context.Device.GetCurrentInterface();
                
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                // Validate VLAN list format (basic validation)
                if (!IsValidVlanList(vlanList))
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid VLAN list format");
                }

                device.AddLogEntry($"Interface {currentInterface} trunk allowed VLANs set to: {vlanList}");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting trunk allowed VLANs: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting trunk allowed VLANs: {ex.Message}");
            }
        }

        private bool IsValidVlanList(string vlanList)
        {
            // Basic validation for VLAN list (all, none, 1-4094, ranges like 10-20, etc.)
            if (vlanList.ToLower() == "all" || vlanList.ToLower() == "none")
                return true;

            // Check for comma-separated list with ranges
            var parts = vlanList.Split(',');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains('-'))
                {
                    var range = trimmed.Split('-');
                    if (range.Length != 2)
                        return false;
                    
                    if (!int.TryParse(range[0], out int start) || !int.TryParse(range[1], out int end))
                        return false;
                    
                    if (start < 1 || end > 4094 || start > end)
                        return false;
                }
                else
                {
                    if (!int.TryParse(trimmed, out int vlan) || vlan < 1 || vlan > 4094)
                        return false;
                }
            }

            return true;
        }
    }

    public class SwitchportTrunkNativeHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkNativeHandler() : base("native", "Set native VLAN on trunk")
        {
            AddSubHandler("vlan", new SwitchportTrunkNativeVlanHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: vlan");
        }
    }

    public class SwitchportTrunkNativeVlanHandler : VendorAgnosticCliHandler
    {
        public SwitchportTrunkNativeVlanHandler() : base("vlan", "Set native VLAN")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: switchport trunk native vlan <vlan-id>");
            }

            if (!int.TryParse(context.CommandParts[1], out int vlanId))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid VLAN ID");
            }

            if (vlanId < 1 || vlanId > 4094)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% VLAN ID must be between 1 and 4094");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                device.AddLogEntry($"Interface {currentInterface} trunk native VLAN set to {vlanId}");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting trunk native VLAN: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting trunk native VLAN: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles 'switchport voice' command
    /// </summary>
    public class SwitchportVoiceHandler : VendorAgnosticCliHandler
    {
        public SwitchportVoiceHandler() : base("voice", "Set voice VLAN configuration")
        {
            AddSubHandler("vlan", new SwitchportVoiceVlanHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: vlan");
        }
    }

    public class SwitchportVoiceVlanHandler : VendorAgnosticCliHandler
    {
        public SwitchportVoiceVlanHandler() : base("vlan", "Set voice VLAN")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: switchport voice vlan <vlan-id|dot1p|none|untagged>");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                var vlanParam = context.CommandParts[1].ToLower();
                var currentInterface = context.Device.GetCurrentInterface();
                
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                if (vlanParam == "dot1p" || vlanParam == "none" || vlanParam == "untagged")
                {
                    device.AddLogEntry($"Interface {currentInterface} voice VLAN set to {vlanParam}");
                    return Success("");
                }
                else if (int.TryParse(vlanParam, out int vlanId))
                {
                    if (vlanId < 1 || vlanId > 4094)
                    {
                        return Error(CliErrorType.InvalidParameter, 
                            "% VLAN ID must be between 1 and 4094");
                    }

                    device.AddLogEntry($"Interface {currentInterface} voice VLAN set to {vlanId}");
                    return Success("");
                }
                else
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid voice VLAN parameter");
                }
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting voice VLAN: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting voice VLAN: {ex.Message}");
            }
        }
    }
} 
