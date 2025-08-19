using System.Text;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Basic
{
    /// <summary>
    /// Vendor-agnostic help command handler
    /// </summary>
    public class HelpCommandHandler : VendorAgnosticCliHandler
    {
        public HelpCommandHandler() : base("help", "Show help information")
        {
            AddAlias("?");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var output = new StringBuilder();
            
            // Get vendor-specific help format
            var vendorContext = GetVendorContext(context);
            
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
            
            // Add vendor-specific help if available
            if (vendorContext != null)
            {
                var vendorHelp = vendorContext.GetCommandHelp("");
                if (!string.IsNullOrEmpty(vendorHelp) && vendorHelp != "Available commands:")
                {
                    output.AppendLine();
                    output.AppendLine($"Vendor-specific help ({vendorContext.VendorName}):");
                    output.AppendLine(vendorHelp);
                }
            }

            return Success(output.ToString());
        }
    }
} 
