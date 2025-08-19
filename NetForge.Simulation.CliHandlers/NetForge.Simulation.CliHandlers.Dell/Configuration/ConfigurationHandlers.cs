using System.Text;
using NetForge.Simulation.Common;
using NetForge.Simulation.CliHandlers;

namespace NetForge.Simulation.CliHandlers.Dell.Configuration
{
    /// <summary>
    /// Dell configure terminal command handler
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (context.CommandParts.Length > 1 && context.CommandParts[1] == "terminal")
            {
                SetMode(context, "config");
                return Success("");
            }
            
            SetMode(context, "config");
            return Success("");
        }
    }

    /// <summary>
    /// Interface configuration command handler with comprehensive Dell OS10 support
    /// </summary>
    public class InterfaceCommandHandler : VendorAgnosticCliHandler
    {
        public InterfaceCommandHandler() : base("interface", "Configure interface parameters")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            var interfaceName = string.Join(" ", context.CommandParts.Skip(1));
            interfaceName = FormatInterfaceName(interfaceName);
            
            var device = context.Device as NetworkDevice;
            var interfaces = device?.GetAllInterfaces();

            // Check for existing interface
            if (interfaces != null && interfaces.ContainsKey(interfaceName))
            {
                SetCurrentInterface(context, interfaceName);
                SetMode(context, "interface");
                return Success("");
            }

            // Handle VLAN interface creation
            if (interfaceName.StartsWith("vlan"))
            {
                if (int.TryParse(interfaceName.Replace("vlan", "").Trim(), out int vlanId))
                {
                    var vlans = device?.GetAllVlans();
                    if (vlans != null && vlans.ContainsKey(vlanId))
                    {
                        if (!interfaces.ContainsKey(interfaceName))
                        {
                            interfaces[interfaceName] = CreateInterface(interfaceName, vlanId);
                        }
                        SetCurrentInterface(context, interfaceName);
                        SetMode(context, "interface");
                        return Success("");
                    }
                }
            }

            // Handle port-channel interface creation
            if (interfaceName.StartsWith("port-channel"))
            {
                if (int.TryParse(interfaceName.Replace("port-channel", "").Trim(), out int channelId))
                {
                    if (!interfaces.ContainsKey(interfaceName))
                    {
                        interfaces[interfaceName] = CreateInterface(interfaceName);
                    }
                    SetCurrentInterface(context, interfaceName);
                    SetMode(context, "interface");
                    return Success("");
                }
            }

            return Error(CliErrorType.ExecutionError, "% Error: Interface not found");
        }
        
        private dynamic CreateInterface(string name, int vlanId = 0)
        {
            return new 
            {
                Name = name,
                VlanId = vlanId,
                IsUp = false,
                IsShutdown = true,
                IpAddress = "",
                SubnetMask = "",
                Description = "",
                SwitchportMode = "",
                MacAddress = "00:01:e8:8b:44:56",
                RxPackets = 0,
                TxPackets = 0,
                RxBytes = 0,
                TxBytes = 0,
                Mtu = 1500,
                Bandwidth = 1000000
            };
        }
        
        private string FormatInterfaceName(string interfaceName)
        {
            if (interfaceName.StartsWith("eth"))
                return "ethernet" + interfaceName.Substring(3);
            if (interfaceName.StartsWith("gi"))
                return "ethernet" + interfaceName.Substring(2);
            return interfaceName;
        }
    }

    /// <summary>
    /// Interface mode configuration commands with comprehensive Dell OS10 support
    /// </summary>
    public class InterfaceModeCommandHandler : VendorAgnosticCliHandler
    {
        public InterfaceModeCommandHandler() : base("", "Interface mode commands")
        {
        }

        public override bool CanHandle(CliContext context)
        {
            return IsInMode(context, "interface");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this mode");
            }

            var cmd = context.CommandParts[0].ToLower();
            var currentInterface = GetCurrentInterface(context);
            var device = context.Device as NetworkDevice;
            var interfaces = device?.GetAllInterfaces();
            var iface = interfaces?.ContainsKey(currentInterface) == true ? interfaces[currentInterface] : null;

            if (iface == null)
            {
                return Error(CliErrorType.ExecutionError, "% Invalid command at this privilege level");
            }

            return cmd switch
            {
                "ip" => ProcessIpCommand(context, device, iface),
                "switchport" => ProcessSwitchportCommand(context, device, iface),
                "description" => ProcessDescriptionCommand(context, device, iface),
                "shutdown" => ProcessShutdownCommand(context, device, iface),
                "no" => ProcessNoCommand(context, device, iface),
                "speed" => ProcessSpeedCommand(context, device, iface),
                "duplex" => ProcessDuplexCommand(context, device, iface),
                "channel-group" => ProcessChannelGroupCommand(context, device, iface),
                "exit" => ProcessExitCommand(context),
                _ => Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level")
            };
        }

        private CliResult ProcessIpCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            if (parts[1].ToLower() == "address")
            {
                if (parts.Length >= 3)
                {
                    string ip, mask;
                    
                    if (parts.Length > 3)
                    {
                        // Traditional format: ip address 10.0.0.1 255.255.255.0
                        ip = parts[2];
                        mask = parts[3];
                    }
                    else
                    {
                        // Dell OS10 CIDR format: ip address 10.0.0.1/24
                        if (parts[2].Contains("/"))
                        {
                            var cidrParts = parts[2].Split('/');
                            if (cidrParts.Length == 2 && int.TryParse(cidrParts[1], out int cidr))
                            {
                                ip = cidrParts[0];
                                mask = CidrToMask(cidr);
                            }
                            else
                            {
                                return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");
                            }
                        }
                        else
                        {
                            return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");
                        }
                    }
                    
                    if (IsValidIpAddress(ip))
                    {
                        iface.IpAddress = ip;
                        iface.SubnetMask = mask;
                        device.AddLogEntry($"Interface {iface.Name} IP address set to {ip}/{mask}");
                        return Success("");
                    }
                }
                return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");
            }

            return Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level");
        }

        private CliResult ProcessSwitchportCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            switch (parts[1].ToLower())
            {
                case "mode":
                    if (parts.Length > 2)
                    {
                        var mode = parts[2].ToLower();
                        if (mode == "access" || mode == "trunk")
                        {
                            iface.SwitchportMode = mode;
                            return Success("");
                        }
                    }
                    return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");

                case "access":
                    if (parts.Length > 3 && parts[2].ToLower() == "vlan")
                    {
                        if (int.TryParse(parts[3], out int vlanId) && IsValidVlanId(vlanId))
                        {
                            iface.VlanId = vlanId;
                            iface.SwitchportMode = "access";
                            
                            var vlans = device.GetAllVlans();
                            if (vlans.ContainsKey(vlanId))
                            {
                                vlans[vlanId].Interfaces.Add(iface.Name);
                            }
                            return Success("");
                        }
                    }
                    return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");

                case "trunk":
                    if (parts.Length > 4 && parts[2].ToLower() == "allowed" && parts[3].ToLower() == "vlan")
                    {
                        // Handle trunk allowed VLAN configuration
                        return Success("");
                    }
                    return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");

                default:
                    return Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level");
            }
        }

        private CliResult ProcessDescriptionCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length > 1)
            {
                iface.Description = string.Join(" ", parts.Skip(1));
                return Success("");
            }
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }

        private CliResult ProcessShutdownCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            iface.IsShutdown = true;
            iface.IsUp = false;
            device.AddLogEntry($"Interface {iface.Name} shutdown");
            return Success("");
        }

        private CliResult ProcessNoCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            switch (parts[1].ToLower())
            {
                case "shutdown":
                    iface.IsShutdown = false;
                    iface.IsUp = true;
                    device.AddLogEntry($"Interface {iface.Name} no shutdown");
                    return Success("");

                case "ip":
                    if (parts.Length > 2 && parts[2].ToLower() == "address")
                    {
                        iface.IpAddress = "";
                        iface.SubnetMask = "";
                        device.AddLogEntry($"Interface {iface.Name} IP address removed");
                        return Success("");
                    }
                    return Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level");

                case "description":
                    iface.Description = "";
                    return Success("");

                default:
                    return Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level");
            }
        }

        private CliResult ProcessSpeedCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length > 1)
            {
                // Speed configuration - Dell OS10 supports auto, 10, 100, 1000, 10000, etc.
                device.AddLogEntry($"Interface {iface.Name} speed set to {parts[1]}");
                return Success("");
            }
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }

        private CliResult ProcessDuplexCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length > 1)
            {
                // Duplex configuration - full, half, auto
                device.AddLogEntry($"Interface {iface.Name} duplex set to {parts[1]}");
                return Success("");
            }
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }

        private CliResult ProcessChannelGroupCommand(CliContext context, NetworkDevice device, dynamic iface)
        {
            var parts = context.CommandParts;
            if (parts.Length > 2 && int.TryParse(parts[1], out int channelId))
            {
                var mode = parts.Length > 3 ? parts[3].ToLower() : "on";
                device.AddLogEntry($"Interface {iface.Name} added to port-channel {channelId} mode {mode}");
                return Success("");
            }
            return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");
        }

        private CliResult ProcessExitCommand(CliContext context)
        {
            SetMode(context, "config");
            SetCurrentInterface(context, "");
            return Success("");
        }

        private bool IsValidIpAddress(string ip)
        {
            return global::System.Net.IPAddress.TryParse(ip, out _);
        }

        private bool IsValidVlanId(int vlanId)
        {
            return vlanId >= 1 && vlanId <= 4094;
        }

        private string CidrToMask(int cidr)
        {
            uint mask = 0xffffffff << (32 - cidr);
            return $"{(mask >> 24) & 255}.{(mask >> 16) & 255}.{(mask >> 8) & 255}.{mask & 255}";
        }
    }

    /// <summary>
    /// Hostname configuration command handler
    /// </summary>
    public class HostnameCommandHandler : VendorAgnosticCliHandler
    {
        public HostnameCommandHandler() : base("hostname", "Set system hostname")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
            }

            if (context.CommandParts.Length > 1)
            {
                var device = context.Device as NetworkDevice;
                device.AddLogEntry($"Hostname changed to {context.CommandParts[1]}");
                return Success("");
            }
            return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
        }
    }

    /// <summary>
    /// VLAN configuration command handler with comprehensive Dell OS10 support
    /// </summary>
    public class VlanCommandHandler : VendorAgnosticCliHandler
    {
        public VlanCommandHandler() : base("vlan", "Configure VLAN parameters")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
            }

            if (context.CommandParts.Length > 1 && int.TryParse(context.CommandParts[1], out int vlanId))
            {
                if (IsValidVlanId(vlanId))
                {
                    var device = context.Device as NetworkDevice;
                    var vlans = device?.GetAllVlans();
                    if (vlans != null && !vlans.ContainsKey(vlanId))
                    {
                        vlans[vlanId] = CreateVlan(vlanId);
                    }
                    SetCurrentVlan(context, vlanId);
                    SetMode(context, "vlan");
                    return Success("");
                }
            }
            return Error(CliErrorType.InvalidParameter, "% Error: Invalid VLAN ID");
        }
        
        private dynamic CreateVlan(int vlanId)
        {
            return new 
            {
                Id = vlanId,
                Name = $"VLAN{vlanId:D4}",
                Active = true,
                Interfaces = new List<string>()
            };
        }
        
        private bool IsValidVlanId(int vlanId)
        {
            return vlanId >= 1 && vlanId <= 4094;
        }
    }

    /// <summary>
    /// Router configuration command handler for OSPF, BGP, RIP protocols
    /// </summary>
    public class RouterCommandHandler : VendorAgnosticCliHandler
    {
        public RouterCommandHandler() : base("router", "Configure routing protocols")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
            }

            var protocol = context.CommandParts[1].ToLower();
            var device = context.Device as NetworkDevice;

            switch (protocol)
            {
                case "ospf":
                    if (context.CommandParts.Length > 2 && int.TryParse(context.CommandParts[2], out int processId))
                    {
                        var ospfConfig = device?.GetOspfConfiguration();
                        if (ospfConfig == null)
                        {
                            device?.AddLogEntry($"OSPF process {processId} configured");
                        }
                        SetCurrentProtocol(context, "ospf");
                        SetMode(context, "router");
                        return Success("");
                    }
                    return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");

                case "bgp":
                    if (context.CommandParts.Length > 2 && int.TryParse(context.CommandParts[2], out int asNumber))
                    {
                        var bgpConfig = device?.GetBgpConfiguration();
                        if (bgpConfig == null)
                        {
                            device?.AddLogEntry($"BGP AS {asNumber} configured");
                        }
                        SetCurrentProtocol(context, "bgp");
                        SetMode(context, "router");
                        return Success("");
                    }
                    return Error(CliErrorType.InvalidParameter, "% Invalid command at this privilege level");

                case "rip":
                    var ripConfig = device?.GetRipConfiguration();
                    if (ripConfig == null)
                    {
                        device?.AddLogEntry("RIP configured");
                    }
                    SetCurrentProtocol(context, "rip");
                    SetMode(context, "router");
                    return Success("");

                default:
                    return Error(CliErrorType.InvalidCommand, "% Invalid routing protocol");
            }
        }
    }

    /// <summary>
    /// Router mode configuration commands for routing protocols
    /// </summary>
    public class RouterModeCommandHandler : VendorAgnosticCliHandler
    {
        public RouterModeCommandHandler() : base("", "Router mode commands")
        {
        }

        public override bool CanHandle(CliContext context)
        {
            return IsInMode(context, "router");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "router"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this mode");
            }

            var cmd = context.CommandParts[0].ToLower();
            var device = context.Device as NetworkDevice;

            return cmd switch
            {
                "router-id" => ProcessRouterIdCommand(context, device),
                "network" => ProcessNetworkCommand(context, device),
                "neighbor" => ProcessNeighborCommand(context, device),
                "exit" => ProcessExitCommand(context),
                _ => Error(CliErrorType.InvalidCommand, "% Invalid command at this privilege level")
            };
        }

        private CliResult ProcessRouterIdCommand(CliContext context, NetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length > 1)
            {
                var routerId = parts[1];
                if (IsValidIpAddress(routerId))
                {
                    var protocol = GetCurrentProtocol(context);
                    device?.AddLogEntry($"{protocol.ToUpper()} router-id set to {routerId}");
                    return Success("");
                }
            }
            return Error(CliErrorType.InvalidParameter, "% Invalid router ID");
        }

        private CliResult ProcessNetworkCommand(CliContext context, NetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length > 1)
            {
                var network = parts[1];
                var protocol = GetCurrentProtocol(context);
                
                if (protocol == "ospf" && parts.Length > 3 && parts[2] == "area")
                {
                    var area = parts[3];
                    device?.AddLogEntry($"OSPF network {network} area {area} configured");
                    return Success("");
                }
                else if (protocol == "bgp")
                {
                    device?.AddLogEntry($"BGP network {network} configured");
                    return Success("");
                }
                else if (protocol == "rip")
                {
                    device?.AddLogEntry($"RIP network {network} configured");
                    return Success("");
                }
            }
            return Error(CliErrorType.InvalidParameter, "% Invalid network configuration");
        }

        private CliResult ProcessNeighborCommand(CliContext context, NetworkDevice device)
        {
            var parts = context.CommandParts;
            if (parts.Length > 3)
            {
                var neighborIp = parts[1];
                var remoteAs = parts[3];
                
                if (IsValidIpAddress(neighborIp) && parts[2] == "remote-as")
                {
                    device?.AddLogEntry($"BGP neighbor {neighborIp} remote-as {remoteAs} configured");
                    return Success("");
                }
            }
            return Error(CliErrorType.InvalidParameter, "% Invalid neighbor configuration");
        }

        private CliResult ProcessExitCommand(CliContext context)
        {
            SetMode(context, "config");
            SetCurrentProtocol(context, "");
            return Success("");
        }

        private bool IsValidIpAddress(string ip)
        {
            return global::System.Net.IPAddress.TryParse(ip, out _);
        }
    }

    /// <summary>
    /// IP route configuration command handler
    /// </summary>
    public class IpRouteCommandHandler : VendorAgnosticCliHandler
    {
        public IpRouteCommandHandler() : base("ip", "Configure IP parameters")
        {
        }

        public override bool CanHandle(CliContext context)
        {
            return context.CommandParts.Length > 1 && context.CommandParts[1] == "route";
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, "% Invalid command at this privilege level");
            }

            var parts = context.CommandParts;
            if (parts.Length >= 4 && parts[1] == "route")
            {
                var network = parts[2];
                var nextHop = parts[3];
                
                if (IsValidNetwork(network) && IsValidIpAddress(nextHop))
                {
                    var device = context.Device as NetworkDevice;
                    device?.AddLogEntry($"Static route {network} via {nextHop} configured");
                    return Success("");
                }
            }
            
            return Error(CliErrorType.InvalidParameter, "% Invalid route configuration");
        }

        private bool IsValidNetwork(string network)
        {
            return network.Contains("/") || network.Contains(".");
        }

        private bool IsValidIpAddress(string ip)
        {
            return global::System.Net.IPAddress.TryParse(ip, out _);
        }
    }

    /// <summary>
    /// Exit command handler for configuration modes
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current configuration mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Dell"))
            {
                return RequireVendor(context, "Dell");
            }
            
            var currentMode = GetCurrentMode(context);
            
            var newMode = currentMode switch
            {
                "interface" => "config",
                "vlan" => "config", 
                "router" => "config",
                "config" => "exec",
                _ => "exec"
            };
            
            SetMode(context, newMode);
            
            // Clear context-specific settings
            if (currentMode == "interface")
            {
                SetCurrentInterface(context, "");
            }
            else if (currentMode == "router")
            {
                SetCurrentProtocol(context, "");
            }
            
            return Success("");
        }
    }
} 
