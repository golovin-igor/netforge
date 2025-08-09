using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Fortinet.Show
{
    /// <summary>
    /// Fortinet show command handler
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");
        }
        
        protected override CliResult ExecuteCommand(CliContext context)
        {
            if (!IsVendor(context, "Fortinet"))
            {
                return RequireVendor(context, "Fortinet");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need show option");
            }
            
            var option = context.CommandParts[1];
            
            return option switch
            {
                "version" => HandleShowVersion(context),
                "interfaces" => HandleShowInterfaces(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid show option: {option}")
            };
        }
        
        private CliResult HandleShowVersion(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine($"Fortinet Network Device");
            output.AppendLine($"Device name: {device?.Name}");
            output.AppendLine($"Software version: 1.0");
            output.AppendLine($"Hardware: Generic");
            output.AppendLine($"Uptime: 1 day, 0 hours, 0 minutes");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowInterfaces(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var interfaces = device.GetAllInterfaces();
            
            output.AppendLine("Interface              IP-Address      Status    Protocol");
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-22} {iface.IpAddress,-15} {status,-9} {protocol}");
            }
            
            return Success(output.ToString());
        }
    }
}
