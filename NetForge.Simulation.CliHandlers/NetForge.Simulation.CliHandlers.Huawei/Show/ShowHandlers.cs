using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;

namespace NetForge.Simulation.CliHandlers.Huawei.Show
{
    /// <summary>
    /// Huawei show command handler
    /// </summary>
    public class ShowCommandHandler : VendorAgnosticCliHandler
    {
        public ShowCommandHandler() : base("show", "Display device information")
        {
            AddAlias("sh");
            AddAlias("sho");
            AddAlias("display"); // Huawei uses display instead of show
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Huawei"))
            {
                return RequireVendor(context, "Huawei");
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
                "interface" => HandleShowInterface(context),
                "arp" => HandleShowArp(context),
                "vlan" => HandleShowVlan(context),
                "current-configuration" => HandleShowCurrentConfiguration(context),
                "device" => HandleShowDevice(context),
                "cpu-usage" => HandleShowCpuUsage(context),
                "alarm" => HandleShowAlarm(context),
                "ip" => HandleShowIp(context),
                "mac-address" => HandleShowMacAddress(context),
                "isis" => HandleShowIsis(context),
                "bgp" => HandleShowBgp(context),
                "stp" => HandleShowStp(context),
                "rip" => HandleShowRip(context),
                "ospf" => HandleShowOspf(context),
                "memory-usage" => HandleShowMemoryUsage(context),
                "power" => HandleShowPower(context),
                "fan" => HandleShowFan(context),
                "temperature" => HandleShowTemperature(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid show option: {option}")
            };
        }
        
        private CliResult HandleShowVersion(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine($"Huawei Network Device");
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

        private CliResult HandleShowArp(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var table = device?.GetArpTableOutput();
            if (string.IsNullOrEmpty(table))
                return Success("ARP table is empty.\n");
            return Success(table);
        }
        
        private CliResult HandleShowInterface(CliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return HandleShowInterfaces(context); // Default to show all interfaces
            }
            
            var interfaceOption = context.CommandParts[2];
            return interfaceOption switch
            {
                "brief" => HandleShowInterfaceBrief(context),
                _ => HandleShowInterfaces(context)
            };
        }
        
        private CliResult HandleShowInterfaceBrief(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var interfaces = device.GetAllInterfaces();
            
            output.AppendLine("Interface     Status   IP              Protocol  Description");
            output.AppendLine("------------- -------- --------------- --------- -----------");
            
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-13} {status,-8} {iface.IpAddress,-15} {protocol,-9} Interface");
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowVlan(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var vlans = device.GetAllVlans();
            
            output.AppendLine("VLAN ID  Name                Status   Ports");
            output.AppendLine("-------- ------------------- -------- --------");
            
            if (vlans.Count == 0)
            {
                output.AppendLine("1        default             active   All");
            }
            else
            {
                foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
                {
                    output.AppendLine($"{vlan.Id,-8} {vlan.Name,-19} active   All");
                }
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowCurrentConfiguration(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("#");
            output.AppendLine($"sysname {device?.Name}");
            output.AppendLine("#");
            output.AppendLine("vlan batch 1");
            output.AppendLine("#");
            output.AppendLine("cluster enable");
            output.AppendLine("ntdp enable");
            output.AppendLine("#");
            output.AppendLine("interface GigabitEthernet0/0/1");
            output.AppendLine(" port link-type access");
            output.AppendLine(" port default vlan 1");
            output.AppendLine("#");
            output.AppendLine("return");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowDevice(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("Device information:");
            output.AppendLine($"Device name: {device?.Name}");
            output.AppendLine("Device type: S5700-28C-EI-24S");
            output.AppendLine("Hardware version: VER.B");
            output.AppendLine("Software version: V200R019C00SPC500");
            output.AppendLine("Manufacture date: 2023-01-01");
            output.AppendLine("Serial number: 210235A29GH123456789");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowCpuUsage(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("CPU usage statistics:");
            output.AppendLine("CPU Usage(5 seconds): 5%");
            output.AppendLine("CPU Usage(1 minute): 7%");
            output.AppendLine("CPU Usage(5 minutes): 6%");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowAlarm(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Current alarms:");
            output.AppendLine("No alarms currently active");
            output.AppendLine("");
            output.AppendLine("Recent cleared alarms:");
            output.AppendLine("None");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIp(CliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command - need IP option");
            }
            
            var ipOption = context.CommandParts[2];
            return ipOption switch
            {
                "interface" => HandleShowIpInterface(context),
                "routing-table" => HandleShowIpRoutingTable(context),
                _ => Error(CliErrorType.InvalidCommand, $"% Invalid IP option: {ipOption}")
            };
        }
        
        private CliResult HandleShowIpInterface(CliContext context)
        {
            if (context.CommandParts.Length > 3 && context.CommandParts[3] == "brief")
            {
                return HandleShowIpInterfaceBrief(context);
            }
            
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
                output.AppendLine($"Interface: {iface.Name}");
                output.AppendLine($"  Internet address: {iface.IpAddress}/{iface.SubnetMask}");
                output.AppendLine($"  Physical address: {iface.MacAddress}");
                output.AppendLine($"  Link status: {(iface.IsUp ? "Up" : "Down")}");
                output.AppendLine("");
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpInterfaceBrief(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }
            
            var interfaces = device.GetAllInterfaces();
            
            output.AppendLine("Interface                IP Address      Status          Protocol");
            output.AppendLine("------------------------ --------------- --------------- --------");
            
            foreach (var kvp in interfaces)
            {
                var iface = kvp.Value;
                var status = iface.IsUp ? "up" : "down";
                var protocol = iface.IsUp ? "up" : "down";
                output.AppendLine($"{iface.Name,-24} {iface.IpAddress,-15} {status,-15} {protocol}");
            }
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIpRoutingTable(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Routing Table:");
            output.AppendLine("Destination/Mask    Proto  Pre  Cost      NextHop         Interface");
            output.AppendLine("0.0.0.0/0           Static 60   0         192.168.1.1     GE0/0/1");
            output.AppendLine("192.168.1.0/24      Direct 0    0         192.168.1.100   GE0/0/1");
            output.AppendLine("127.0.0.0/8         Direct 0    0         127.0.0.1       InLoop0");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowMacAddress(CliContext context)
        {
            var device = context.Device as NetworkDevice;
            var output = new StringBuilder();
            
            output.AppendLine("MAC address table:");
            output.AppendLine("VLAN    MAC Address      Type      Interface");
            output.AppendLine("------- ---------------- --------- ---------");
            output.AppendLine("1       0001.0203.0405   Dynamic   GE0/0/1");
            output.AppendLine("1       0006.0708.090a   Dynamic   GE0/0/2");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowIsis(CliContext context)
        {
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "peer")
            {
                var output = new StringBuilder();
                output.AppendLine("IS-IS peer information:");
                output.AppendLine("System ID       Interface   Circuit Id   State  Holdtime  Type");
                output.AppendLine("0000.0000.0001  GE0/0/1     01           Up     27        L2");
                return Success(output.ToString());
            }
            
            return Error(CliErrorType.InvalidCommand, "% Invalid ISIS option");
        }
        
        private CliResult HandleShowBgp(CliContext context)
        {
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "peer")
            {
                var output = new StringBuilder();
                output.AppendLine("BGP peer information:");
                output.AppendLine("Peer            AS      State      PfxRcd   Uptime");
                output.AppendLine("192.168.1.1     65001   Established  100    01:23:45");
                return Success(output.ToString());
            }
            
            return Error(CliErrorType.InvalidCommand, "% Invalid BGP option");
        }
        
        private CliResult HandleShowStp(CliContext context)
        {
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "brief")
            {
                var output = new StringBuilder();
                output.AppendLine("STP Brief Information:");
                output.AppendLine("MSTID   Port                        Role  STP State     Protected");
                output.AppendLine("0       GigabitEthernet0/0/1        Desg  Forwarding    No");
                output.AppendLine("0       GigabitEthernet0/0/2        Desg  Forwarding    No");
                return Success(output.ToString());
            }
            
            return Error(CliErrorType.InvalidCommand, "% Invalid STP option");
        }
        
        private CliResult HandleShowRip(CliContext context)
        {
            if (context.CommandParts.Length > 3 && context.CommandParts[2] == "1" && context.CommandParts[3] == "neighbor")
            {
                var output = new StringBuilder();
                output.AppendLine("RIP neighbor information:");
                output.AppendLine("Neighbor         Interface        State");
                output.AppendLine("192.168.1.1      GE0/0/1          Up");
                return Success(output.ToString());
            }
            
            return Error(CliErrorType.InvalidCommand, "% Invalid RIP option");
        }
        
        private CliResult HandleShowOspf(CliContext context)
        {
            if (context.CommandParts.Length > 2 && context.CommandParts[2] == "peer")
            {
                var output = new StringBuilder();
                output.AppendLine("OSPF peer information:");
                output.AppendLine("Neighbor         State    Interface            Dead Time");
                output.AppendLine("192.168.1.1      Full     GigabitEthernet0/0/1 00:00:35");
                return Success(output.ToString());
            }
            
            return Error(CliErrorType.InvalidCommand, "% Invalid OSPF option");
        }
        
        private CliResult HandleShowMemoryUsage(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Memory usage statistics:");
            output.AppendLine("Total Memory: 512 MB");
            output.AppendLine("Used Memory:  256 MB (50%)");
            output.AppendLine("Free Memory:  256 MB (50%)");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowPower(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Power supply information:");
            output.AppendLine("Power Supply 1: Normal");
            output.AppendLine("  Status: Present");
            output.AppendLine("  Type: AC power supply");
            output.AppendLine("  Power: 150W");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowFan(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Fan information:");
            output.AppendLine("Fan 1: Normal");
            output.AppendLine("  Status: Present");
            output.AppendLine("  Speed: 3000 RPM");
            output.AppendLine("Fan 2: Normal");
            output.AppendLine("  Status: Present");
            output.AppendLine("  Speed: 3000 RPM");
            
            return Success(output.ToString());
        }
        
        private CliResult HandleShowTemperature(CliContext context)
        {
            var output = new StringBuilder();
            output.AppendLine("Temperature information:");
            output.AppendLine("System temperature: 35°C");
            output.AppendLine("CPU temperature: 40°C");
            output.AppendLine("Status: Normal");
            
            return Success(output.ToString());
        }
    }
}
