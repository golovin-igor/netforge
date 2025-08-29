using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Extreme.Basic
{
    /// <summary>
    /// Extreme enable command handler
    /// </summary>
    public class EnableCommandHandler : VendorAgnosticCliHandler
    {
        public EnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            if (IsInMode(context, "privileged"))
            {
                return Success("");
            }

            SetMode(context, "privileged");
            return Success("");
        }
    }

    /// <summary>
    /// Extreme ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Send ping packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need IP address");
            }

            var targetIp = context.CommandParts[1];

            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, $"% Invalid IP address: {targetIp}");
            }

            var output = new StringBuilder();
            output.AppendLine($"PING {targetIp}");
            output.AppendLine($"64 bytes from {targetIp}: time=1.2 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.1 ms");
            output.AppendLine($"64 bytes from {targetIp}: time=1.0 ms");
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("3 packets transmitted, 3 received, 0% packet loss");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Extreme traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need IP address");
            }

            var targetIp = context.CommandParts[1];

            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {targetIp}");
            }

            var output = new StringBuilder();
            output.AppendLine($"traceroute to {targetIp}:");
            output.AppendLine("1  192.168.1.1  1.2 ms  1.1 ms  1.0 ms");
            output.AppendLine($"2  {targetIp}  2.1 ms  2.0 ms  1.9 ms");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Extreme write command handler
    /// </summary>
    public class WriteCommandHandler : VendorAgnosticCliHandler
    {
        public WriteCommandHandler() : base("write", "Save configuration")
        {
            AddAlias("wr");
            AddAlias("copy running-config startup-config");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            return Success("Configuration saved successfully.");
        }
    }

    /// <summary>
    /// Extreme reload command handler
    /// </summary>
    public class ReloadCommandHandler : VendorAgnosticCliHandler
    {
        public ReloadCommandHandler() : base("reload", "Restart the device")
        {
            AddAlias("reboot");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            return Success("Proceed with reload? [confirm] y\nReload initiated...");
        }
    }

    /// <summary>
    /// Extreme history command handler
    /// </summary>
    public class HistoryCommandHandler() : VendorAgnosticCliHandler("history", "Show command history")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            var output = new StringBuilder();
            output.AppendLine("Command History:");
            output.AppendLine("  1  show version");
            output.AppendLine("  2  show interfaces");
            output.AppendLine("  3  ping 127.0.0.1");
            output.AppendLine("  4  history");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Extreme copy command handler
    /// </summary>
    public class CopyCommandHandler() : VendorAgnosticCliHandler("copy", "Copy files or configuration")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Extreme"))
            {
                return RequireVendor(context, "Extreme");
            }

            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need source and destination");
            }

            var source = context.CommandParts[1];
            var destination = context.CommandParts[2];

            return Success($"Copying {source} to {destination}... [OK]");
        }
    }
}
