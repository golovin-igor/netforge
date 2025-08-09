using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.F5.Basic
{
    /// <summary>
    /// F5 BIG-IP help command handler
    /// </summary>
    public class HelpCommandHandler : VendorAgnosticCliHandler
    {
        public HelpCommandHandler() : base("help", "Show help information")
        {
            AddAlias("?");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "F5"))
            {
                return RequireVendor(context, "F5");
            }
            
            var helpText = @"F5 BIG-IP Command Help
======================

Basic Commands:
  enable     - Enter privileged mode
  disable    - Exit privileged mode
  exit       - Exit current mode
  help       - Show this help
  ?          - Show this help
  ping       - Ping a host

Show Commands:
  show version        - Show system version
  show running-config - Show current configuration
  show interfaces     - Show interface status
  show ltm pool      - Show LTM pools
  show ltm virtual    - Show LTM virtual servers
  show ltm node       - Show LTM nodes
  show gtm            - Show GTM information
  show asm            - Show ASM information
  show apm            - Show APM information
  show net            - Show network configuration
  show sys            - Show system information

F5 Specific Commands:
  tmsh                - Enter TMSH mode
  bash                - Enter bash shell
  list                - List objects
  create              - Create objects
  modify              - Modify objects
  delete              - Delete objects";
            
            return Success(helpText);
        }
    }
} 
