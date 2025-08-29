using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Dell.Basic
{
    /// <summary>
    /// Dell enable command handler
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
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
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
    /// Dell ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Send ping packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
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
    /// Dell disable command handler
    /// </summary>
    public class DisableCommandHandler : VendorAgnosticCliHandler
    {
        public DisableCommandHandler() : base("disable", "Exit privileged mode")
        {
            AddAlias("dis");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            if (IsInMode(context, "user"))
            {
                return Success("");
            }

            SetMode(context, "user");
            return Success("");
        }
    }

    /// <summary>
    /// Dell traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
            AddAlias("trace");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need destination IP");
            }

            var targetIp = context.CommandParts[1];

            if (!System.Net.IPAddress.TryParse(targetIp, out _))
            {
                return Error(CliErrorType.InvalidParameter, $"% Invalid IP address: {targetIp}");
            }

            var output = new StringBuilder();
            output.AppendLine($"traceroute to {targetIp}, 30 hops max, 40 byte packets");
            output.AppendLine($" 1  192.168.1.1  1.2 ms  1.1 ms  1.0 ms");
            output.AppendLine($" 2  {targetIp}  2.3 ms  2.1 ms  2.0 ms");

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Dell write command handler
    /// </summary>
    public class WriteCommandHandler : VendorAgnosticCliHandler
    {
        public WriteCommandHandler() : base("write", "Write configuration to memory")
        {
            AddAlias("wr");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("Building configuration...\n[OK]");
        }
    }

    /// <summary>
    /// Dell reload command handler
    /// </summary>
    public class ReloadCommandHandler() : VendorAgnosticCliHandler("reload", "Restart the system")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("System restart requested...\n");
        }
    }

    /// <summary>
    /// Dell history command handler
    /// </summary>
    public class HistoryCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryCommandHandler() : base("history", "Show command history")
        {
            AddAlias("hist");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("Command history:\n  1  show version\n  2  show ip interface brief\n  3  show running-config\n");
        }
    }

    /// <summary>
    /// Dell help command handler
    /// </summary>
    public class HelpCommandHandler : VendorAgnosticCliHandler
    {
        public HelpCommandHandler() : base("help", "Show help information")
        {
            AddAlias("?");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("Help may be requested at any point in a command by entering a question mark '?'.\n");
        }
    }

    /// <summary>
    /// Dell copy command handler
    /// </summary>
    public class CopyCommandHandler() : VendorAgnosticCliHandler("copy", "Copy files")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("Source filename []?\n");
        }
    }

    /// <summary>
    /// Dell clear command handler
    /// </summary>
    public class ClearCommandHandler() : VendorAgnosticCliHandler("clear", "Clear screen or counters")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }

            return Success("Cleared.\n");
        }
    }
}
