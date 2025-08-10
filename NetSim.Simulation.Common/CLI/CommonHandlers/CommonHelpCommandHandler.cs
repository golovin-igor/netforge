using System.Text;

namespace NetSim.Simulation.CliHandlers.Common
{
    public class CommonHelpCommandHandler : BaseCliHandler
    {
        public CommonHelpCommandHandler() : base("help", "Show help information")
        {
            AddAlias("?");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var output = new StringBuilder();
            output.Append("Available commands:").AppendLine();
            output.Append("  help, ?                  - Show this help message").AppendLine();
            output.Append("  enable                   - Enter privileged mode").AppendLine();
            output.Append("  disable                  - Exit privileged mode").AppendLine();
            output.Append("  exit                     - Exit current mode or configuration context").AppendLine();
            output.Append("  ping <destination>       - Send ICMP echo request to destination").AppendLine();
            output.Append("  show <option>            - Show various system information").AppendLine();
            output.Append("  configure terminal       - Enter configuration mode").AppendLine();
            output.Append("  interface <interface>    - Configure interface").AppendLine();
            output.Append("  router <protocol>        - Configure routing protocol").AppendLine();
            output.Append("  ip route                 - Configure static route").AppendLine();
            output.Append("  hostname <name>          - Set device hostname").AppendLine();
            output.Append("  no <command>             - Negate a command or set default").AppendLine();

            return Success(output.ToString());
        }
    }
} 

