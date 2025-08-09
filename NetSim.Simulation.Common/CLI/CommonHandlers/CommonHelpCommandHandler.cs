using System.Text;

namespace NetSim.Simulation.CliHandlers.Common
{
    public class CommonHelpCommandHandler : BaseCliHandler
    {
        public CommonHelpCommandHandler() : base("help", "Show help information")
        {
            AddAlias("?");
        }

        protected override CliResult ExecuteCommand(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Available commands:");
            output.AppendLine("  help, ?                  - Show this help message");
            output.AppendLine("  enable                   - Enter privileged mode");
            output.AppendLine("  disable                  - Exit privileged mode");
            output.AppendLine("  exit                     - Exit current mode or configuration context");
            output.AppendLine("  ping <destination>       - Send ICMP echo request to destination");
            output.AppendLine("  show <option>            - Show various system information");
            output.AppendLine("  configure terminal       - Enter configuration mode");
            output.AppendLine("  interface <interface>    - Configure interface");
            output.AppendLine("  router <protocol>        - Configure routing protocol");
            output.AppendLine("  ip route                 - Configure static route");
            output.AppendLine("  hostname <name>          - Set device hostname");
            output.AppendLine("  no <command>             - Negate a command or set default");

            return Success(output.ToString());
        }
    }
} 

