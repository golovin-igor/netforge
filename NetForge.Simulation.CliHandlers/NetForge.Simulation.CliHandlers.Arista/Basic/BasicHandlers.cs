using System.Text;
using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Arista.Basic
{
    /// <summary>
    /// Arista enable command handler
    /// </summary>
    public class EnableCommandHandler : VendorAgnosticCliHandler
    {
        public EnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            // Check if already in privileged mode
            if (IsInMode(context, "privileged"))
            {
                return Success(""); // Already in privileged mode
            }

            // Move to privileged mode
            SetMode(context, "privileged");

            return Success("");
        }
    }

    /// <summary>
    /// Arista ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Send ping packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var targetIp = context.CommandParts[1];

            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {targetIp}");
            }

            // Simulate ping result (Arista/Cisco-style hybrid)
            var pingCount = 5;
            var packetSize = 100;

            var output = new StringBuilder();
            output.AppendLine($"Type escape sequence to abort.");
            output.AppendLine($"Sending {pingCount}, {packetSize}-byte ICMP Echos to {targetIp}, timeout is 2 seconds:");
            output.AppendLine("!!!!!");
            output.AppendLine($"Success rate is 100 percent ({pingCount}/{pingCount}), round-trip min/avg/max = 1/1/4 ms");
            output.AppendLine($"{pingCount} packets transmitted, {pingCount} packets received, 0% packet loss");

            return Success(output.ToString());
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }

    /// <summary>
    /// Arista write command handler
    /// </summary>
    public class WriteCommandHandler() : VendorAgnosticCliHandler("write", "Write configuration to memory")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                // Log configuration save
                device.AddLogEntry("Configuration saved to startup-config");

                return Success("Copy completed successfully.\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error saving configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Arista reload command handler
    /// </summary>
    public class ReloadCommandHandler() : VendorAgnosticCliHandler("reload", "Reload the device")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                var output = new StringBuilder();
                output.AppendLine("Proceed with reload? [confirm]");
                output.AppendLine("% Reloading system...");

                // Log device reload
                device.AddLogEntry("Device reload initiated");

                // Reset interface states
                var interfaces = device.GetAllInterfaces();
                foreach (var iface in interfaces.Values)
                {
                    // Reset interface counters
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                }

                device.AddLogEntry("Device reload completed");

                return Success(output.ToString());
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error during reload: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Arista history command handler
    /// </summary>
    public class HistoryCommandHandler() : VendorAgnosticCliHandler("history", "Display command history")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            // Get command history
            var history = GetCommandHistory(context);

            var output = new StringBuilder();
            for (int i = 0; i < history.Count; i++)
            {
                output.AppendLine($"{i + 1,3}  {history[i]}");
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Arista copy command handler
    /// </summary>
    public class CopyCommandHandler() : VendorAgnosticCliHandler("copy", "Copy configuration or files")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need source and destination");
            }

            var source = context.CommandParts[1];
            var destination = context.CommandParts[2];

            return (source, destination) switch
            {
                ("running-config", "startup-config") => HandleCopyRunningToStartup(context),
                ("startup-config", "running-config") => HandleCopyStartupToRunning(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Invalid copy operation: {source} to {destination}")
            };
        }

        private CliResult HandleCopyRunningToStartup(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                device.AddLogEntry("Running configuration copied to startup configuration");

                return Success("Copy completed successfully.\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying running-config to startup-config: {ex.Message}");
            }
        }

        private CliResult HandleCopyStartupToRunning(ICliContext context)
        {
            try
            {
                var device = context.Device;

                device.AddLogEntry("Startup configuration loaded to running configuration");

                return Success("Copy completed successfully.\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying startup-config to running-config: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Arista traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Arista"))
            {
                return RequireVendor(context, "Arista");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need destination address");
            }

            var targetIp = context.CommandParts[1];

            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {targetIp}");
            }

            // Simulate traceroute result (Arista style)
            var output = new StringBuilder();
            output.AppendLine($"traceroute to {targetIp} (56 data bytes, 30 hops max)");
            output.AppendLine($" 1  192.168.1.1  1.234 ms  1.123 ms  1.045 ms");
            output.AppendLine($" 2  10.0.0.1  2.456 ms  2.234 ms  2.123 ms");
            output.AppendLine($" 3  {targetIp}  3.678 ms  3.456 ms  3.234 ms");

            return Success(output.ToString());
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }
}
