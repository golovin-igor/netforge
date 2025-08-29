using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Handles 'spanning-tree' command in configuration mode
    /// </summary>
    public class SpanningTreeCommandHandler : VendorAgnosticCliHandler
    {
        public SpanningTreeCommandHandler() : base("spanning-tree", "Configure spanning tree")
        {
            AddSubHandler("mode", new SpanningTreeModeHandler());
            AddSubHandler("vlan", new SpanningTreeVlanHandler());
            AddSubHandler("portfast", new SpanningTreePortfastHandler());
            AddSubHandler("priority", new SpanningTreePriorityHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config") && !IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% This command can only be used in config or interface mode");
            }

            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    public class SpanningTreeModeHandler : VendorAgnosticCliHandler
    {
        public SpanningTreeModeHandler() : base("mode", "Set spanning-tree mode")
        {
            AddSubHandler("pvst", new SpanningTreeModePvstHandler());
            AddSubHandler("rapid-pvst", new SpanningTreeModeRapidPvstHandler());
            AddSubHandler("mst", new SpanningTreeModeMstHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    public class SpanningTreeModePvstHandler() : VendorAgnosticCliHandler("pvst", "Per-VLAN spanning tree mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.SetStpMode("pvst"))
                {
                    vendorContext.Capabilities.AppendToRunningConfig("spanning-tree mode pvst");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error setting STP mode");
        }
    }

    public class SpanningTreeModeRapidPvstHandler() : VendorAgnosticCliHandler("rapid-pvst", "Rapid per-VLAN spanning tree mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.SetStpMode("rapid-pvst"))
                {
                    vendorContext.Capabilities.AppendToRunningConfig("spanning-tree mode rapid-pvst");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error setting STP mode");
        }
    }

    public class SpanningTreeModeMstHandler() : VendorAgnosticCliHandler("mst", "Multiple spanning tree mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.SetStpMode("mst"))
                {
                    vendorContext.Capabilities.AppendToRunningConfig("spanning-tree mode mst");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error setting STP mode");
        }
    }

    public class SpanningTreeVlanHandler() : VendorAgnosticCliHandler("vlan", "VLAN spanning tree configuration")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: spanning-tree vlan <vlan-id> priority <priority>");
            }

            if (!int.TryParse(context.CommandParts[1], out int vlanId))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid VLAN ID");
            }

            if (context.CommandParts.Length >= 4 && context.CommandParts[2].ToLower() == "priority")
            {
                if (!int.TryParse(context.CommandParts[3], out int priority))
                {
                    return Error(CliErrorType.InvalidParameter, "% Invalid priority value");
                }

                if (priority % 4096 != 0 || priority < 0 || priority > 61440)
                {
                    return Error(CliErrorType.InvalidParameter, 
                        "% Bridge priority must be in increments of 4096");
                }

                var vendorContext = GetVendorContext(context);
                if (vendorContext?.Capabilities != null)
                {
                    if (vendorContext.Capabilities.SetStpVlanPriority(vlanId, priority))
                    {
                        vendorContext.Capabilities.AppendToRunningConfig($"spanning-tree vlan {vlanId} priority {priority}");
                        return Success("");
                    }
                }
            }
            else if (context.CommandParts.Length >= 4 && context.CommandParts[2].ToLower() == "root")
            {
                // Handle spanning-tree vlan <vlan-id> root primary/secondary
                var rootType = context.CommandParts[3].ToLower();
                if (rootType == "primary" || rootType == "secondary")
                {
                    var priority = rootType == "primary" ? 24576 : 28672;
                    
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        if (vendorContext.Capabilities.SetStpVlanPriority(vlanId, priority))
                        {
                            vendorContext.Capabilities.AppendToRunningConfig($"spanning-tree vlan {vlanId} root {rootType}");
                            return Success("");
                        }
                    }
                }
            }

            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    public class SpanningTreePortfastHandler : VendorAgnosticCliHandler
    {
        public SpanningTreePortfastHandler() : base("portfast", "Configure portfast on interface")
        {
            AddSubHandler("default", new SpanningTreePortfastDefaultHandler());
            AddSubHandler("bpduguard", new SpanningTreePortfastBpduguardHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // In interface mode
            if (IsInMode(context, "interface"))
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (!string.IsNullOrEmpty(currentInterface))
                {
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        if (vendorContext.Capabilities.EnablePortfast(currentInterface))
                        {
                            vendorContext.Capabilities.AppendToRunningConfig(" spanning-tree portfast");
                            return Success("");
                        }
                    }
                }
            }
            
            // In global config mode, need sub-command
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    public class SpanningTreePortfastDefaultHandler() : VendorAgnosticCliHandler("default", "Enable portfast by default on all access ports")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.EnablePortfastDefault())
                {
                    vendorContext.Capabilities.AppendToRunningConfig("spanning-tree portfast default");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error enabling PortFast default");
        }
    }

    public class SpanningTreePortfastBpduguardHandler : VendorAgnosticCliHandler
    {
        public SpanningTreePortfastBpduguardHandler() : base("bpduguard", "Configure BPDU guard")
        {
            AddSubHandler("default", new SpanningTreePortfastBpduguardDefaultHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // In interface mode
            if (IsInMode(context, "interface"))
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (!string.IsNullOrEmpty(currentInterface))
                {
                    var vendorContext = GetVendorContext(context);
                    if (vendorContext?.Capabilities != null)
                    {
                        if (vendorContext.Capabilities.EnableBpduGuard(currentInterface))
                        {
                            vendorContext.Capabilities.AppendToRunningConfig(" spanning-tree bpduguard enable");
                            return Success("");
                        }
                    }
                }
            }

            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    public class SpanningTreePortfastBpduguardDefaultHandler() : VendorAgnosticCliHandler("default", "Enable BPDU guard by default")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.EnableBpduGuardDefault())
                {
                    vendorContext.Capabilities.AppendToRunningConfig("spanning-tree portfast bpduguard default");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error enabling BPDU Guard default");
        }
    }

    public class SpanningTreePriorityHandler() : VendorAgnosticCliHandler("priority", "Set bridge priority")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: spanning-tree priority <priority>");
            }

            if (!int.TryParse(context.CommandParts[1], out int priority))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid priority value");
            }

            if (priority % 4096 != 0 || priority < 0 || priority > 61440)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Bridge priority must be in increments of 4096");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var vendorContext = GetVendorContext(context);
            if (vendorContext?.Capabilities != null)
            {
                if (vendorContext.Capabilities.SetStpPriority(priority))
                {
                    vendorContext.Capabilities.AppendToRunningConfig($"spanning-tree priority {priority}");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error setting STP priority");
        }
    }
} 
