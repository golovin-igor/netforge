using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.CliHandlers;

namespace NetSim.Simulation.CliHandlers.Juniper.Show
{
    /// <summary>
    /// Juniper show command handler
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
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need show option");
            }
            
            var option = context.CommandParts[1];
            
            return option switch
            {
                "version" => HandleShowVersion(context),
                "configuration" => HandleShowConfiguration(context),
                "interfaces" => HandleShowInterfaces(context),
                "route" => HandleShowRoute(context),
                _ => Error(CliErrorType.InvalidCommand, 
                    $"% Invalid show option: {option}")
            };
        }
        
        private CliResult HandleShowVersion(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("Hostname: " + device?.Name);
            output.AppendLine("Model: mx240");
            output.AppendLine("Junos: 20.4R3.8");
            output.AppendLine("JUNOS Base OS boot [20.4R3.8]");
            output.AppendLine("JUNOS Base OS Software Suite [20.4R3.8]");
            output.AppendLine("JUNOS Crypto Software Suite [20.4R3.8]");
            output.AppendLine("JUNOS Packet Forwarding Engine Support (MX Common) [20.4R3.8]");
            output.AppendLine("JUNOS Routing Software Suite [20.4R3.8]");
            output.AppendLine("JUNOS Web Management [20.4R3.8]");
            output.AppendLine("JUNOS py-extensions [20.4R3.8]");
            output.AppendLine("JUNOS py-base [20.4R3.8]");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowConfiguration(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("## Last commit: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " UTC by admin");
            output.AppendLine("version 20.4R3.8;");
            output.AppendLine("system {");
            output.AppendLine("    host-name " + device?.Name + ";");
            output.AppendLine("    root-authentication {");
            output.AppendLine("        encrypted-password \"$6$...\";");
            output.AppendLine("    }");
            output.AppendLine("    login {");
            output.AppendLine("        user admin {");
            output.AppendLine("            uid 2000;");
            output.AppendLine("            class super-user;");
            output.AppendLine("            authentication {");
            output.AppendLine("                encrypted-password \"$6$...\";");
            output.AppendLine("            }");
            output.AppendLine("        }");
            output.AppendLine("    }");
            output.AppendLine("}");
            
            // Show interface configurations
            if (device != null)
            {
                var interfaces = device.GetAllInterfaces();
                output.AppendLine("interfaces {");
                foreach (var kvp in interfaces)
                {
                    var iface = kvp.Value;
                    output.AppendLine($"    {iface.Name} {{");
                    
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        output.AppendLine("        unit 0 {");
                        output.AppendLine("            family inet {");
                        output.AppendLine($"                address {iface.IpAddress}/{iface.SubnetMask};");
                        output.AppendLine("            }");
                        output.AppendLine("        }");
                    }
                    
                    if (iface.IsShutdown)
                    {
                        output.AppendLine("        disable;");
                    }
                    
                    output.AppendLine("    }");
                }
                output.AppendLine("}");
            }
            
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
            
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                
                output.AppendLine($"Physical interface: {iface.Name}, Enabled, Physical link is {status}");
                output.AppendLine($"  Interface index: 128, SNMP ifIndex: 501");
                output.AppendLine($"  Link-level type: Ethernet, MTU: {iface.Mtu}, Speed: {iface.Speed}");
                output.AppendLine($"  Device flags   : Enabled BROADCAST RUNNING MULTICAST");
                output.AppendLine($"  Interface flags: SNMP-Traps Internal: 0x0");
                output.AppendLine($"  Link flags     : None");
                output.AppendLine($"  CoS queues     : 8 supported, 8 maximum usable queues");
                output.AppendLine($"  Current address: aa:bb:cc:dd:ee:ff, Hardware address: aa:bb:cc:dd:ee:ff");
                output.AppendLine($"  Last flapped   : Never");
                output.AppendLine($"    Input  bytes  :            {iface.RxBytes}");
                output.AppendLine($"    Output bytes  :            {iface.TxBytes}");
                output.AppendLine($"    Input  packets:            {iface.RxPackets}");
                output.AppendLine($"    Output packets:            {iface.TxPackets}");
                output.AppendLine("");
                
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    output.AppendLine($"  Logical interface {iface.Name}.0 (Index 70) (SNMP ifIndex 502)");
                    output.AppendLine($"    Flags: SNMP-Traps 0x0 Encapsulation: ENET2");
                    output.AppendLine($"    Protocol inet, MTU: {iface.Mtu - 18}");
                    output.AppendLine($"      Addresses, Flags: Is-Preferred Is-Primary");
                    output.AppendLine($"        Destination: {iface.IpAddress}/{iface.SubnetMask}, Local: {iface.IpAddress}");
                    output.AppendLine("");
                }
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowRoute(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("inet.0: 5 destinations, 5 routes (5 active, 0 holddown, 0 hidden)");
            output.AppendLine("+ = Active Route, - = Last Active, * = Both");
            output.AppendLine("");
            output.AppendLine("0.0.0.0/0          *[Static/5] 1w0d 12:34:56");
            output.AppendLine("                    > to 192.168.1.1 via ge-0/0/0.0");
            output.AppendLine("192.168.1.0/24     *[Direct/0] 1w0d 12:34:56");
            output.AppendLine("                    > via ge-0/0/0.0");
            output.AppendLine("192.168.1.1/32     *[Local/0] 1w0d 12:34:56");
            output.AppendLine("                      Local via ge-0/0/0.0");
            output.AppendLine("224.0.0.0/4        *[Direct/0] 1w0d 12:34:56");
            output.AppendLine("                    > via ge-0/0/0.0");
            
            return Success(output.ToString());
        }
    }
}
