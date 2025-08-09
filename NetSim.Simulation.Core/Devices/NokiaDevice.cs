using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Core;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;

namespace NetSim.Simulation.Devices
{
    /// <summary>
    /// Nokia SR OS device implementation
    /// </summary>
    public class NokiaDevice : NetworkDevice
    {
        private string currentContext = "";
        private string currentPort = "";
        private string currentRouterInterface = "";
        private string currentVlan = "";
        private string currentRouterProtocol = "";
        
        public NokiaDevice(string name) : base(name)
        {
            Vendor = "Nokia";
            Hostname = name;
            // Nokia devices start in admin mode
            base.CurrentMode = DeviceModeExtensions.FromModeString("admin");

            // Register common protocols for Nokia (SR OS) devices
            RegisterProtocol(new OspfProtocol());
            RegisterProtocol(new BgpProtocol());
            RegisterProtocol(new IsisProtocol());
            RegisterProtocol(new RipProtocol());
            RegisterProtocol(new StpProtocol());
            RegisterProtocol(new LldpProtocol());
            RegisterProtocol(new VrrpProtocol());
            RegisterProtocol(new ArpProtocol());
        }
        
        protected override void InitializeDefaultInterfaces()
        {
            // Add default ports for a Nokia router
            Interfaces["1/1/1"] = new InterfaceConfig("1/1/1", this);
            Interfaces["1/1/2"] = new InterfaceConfig("1/1/2", this);
            Interfaces["1/1/3"] = new InterfaceConfig("1/1/3", this);
            Interfaces["1/1/4"] = new InterfaceConfig("1/1/4", this);
        }
        
        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Nokia handlers to ensure they are available for tests
            var registry = new NetSim.Simulation.CliHandlers.Nokia.NokiaHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }
        
        public override string GetPrompt()
        {
            var mode = GetCurrentMode();
            var context = string.IsNullOrEmpty(currentContext) ? "" : $"[{currentContext}]";
            var port = string.IsNullOrEmpty(currentPort) ? "" : $"[Port {currentPort}]";
            var router = string.IsNullOrEmpty(currentRouterInterface) ? "" : $"[Router {currentRouterInterface}]";
            var vlan = string.IsNullOrEmpty(currentVlan) ? "" : $"[VLAN {currentVlan}]";
            var protocol = string.IsNullOrEmpty(currentRouterProtocol) ? "" : $"[{currentRouterProtocol}]";
            
            // Handle basic privileged mode vs complex configuration contexts
            if (mode == "privileged" && string.IsNullOrEmpty(currentContext) && string.IsNullOrEmpty(currentPort) && 
                string.IsNullOrEmpty(currentRouterInterface) && string.IsNullOrEmpty(currentVlan) && string.IsNullOrEmpty(currentRouterProtocol))
            {
                // Basic privileged mode: A:TestRouter# 
                return $"A:{Name}# ";
            }
            else if (mode == "config" && string.IsNullOrEmpty(currentContext) && string.IsNullOrEmpty(currentPort) && 
                string.IsNullOrEmpty(currentRouterInterface) && string.IsNullOrEmpty(currentVlan) && string.IsNullOrEmpty(currentRouterProtocol))
            {
                // Basic config mode: A:TestRouter(config)#
                return $"A:{Name}(config)# ";
            }
            else if (mode == "admin" && string.IsNullOrEmpty(currentContext) && string.IsNullOrEmpty(currentPort) && 
                     string.IsNullOrEmpty(currentRouterInterface) && string.IsNullOrEmpty(currentVlan) && string.IsNullOrEmpty(currentRouterProtocol))
            {
                // Basic admin mode: A:TestRouter>admin# 
                return $"A:{Name}>admin# ";
            }
            else if (!string.IsNullOrEmpty(currentContext) || !string.IsNullOrEmpty(currentPort) || 
                     !string.IsNullOrEmpty(currentRouterInterface) || !string.IsNullOrEmpty(currentVlan) || !string.IsNullOrEmpty(currentRouterProtocol))
            {
                // Complex configuration contexts: A:TestRouter>config>router>ospf#
                var contextPath = new StringBuilder();
                if (!string.IsNullOrEmpty(currentContext))
                {
                    contextPath.Append(">").Append(currentContext.Replace("configure", "config"));
                }
                if (!string.IsNullOrEmpty(currentRouterInterface))
                {
                    contextPath.Append(">router>interface");
                }
                if (!string.IsNullOrEmpty(currentRouterProtocol))
                {
                    contextPath.Append(">router>").Append(currentRouterProtocol);
                }
                return $"A:{Name}{contextPath}#";
            }
            else
            {
                // Default user mode: TestRouter>
                return $"{Name}>";
            }
        }
        
        // Helper methods for command handlers
        public string GetCurrentContext() => currentContext;
        public void SetContext(string context) => currentContext = context;
        public void SetRouterProtocol(string protocol) => currentRouterProtocol = protocol;
        public void SetRouterInterface(string iface) => currentRouterInterface = iface;
        public void SetCurrentPort(string port) => currentPort = port;
        public void SetCurrentVlan(string vlan) => currentVlan = vlan;
        
        public string GetMode() => base.CurrentMode.ToModeString();
        public new void SetCurrentMode(string mode) => base.CurrentMode = DeviceModeExtensions.FromModeString(mode);

        // Nokia-specific ping simulation
        private string SimulateNokiaPing(string destination)
        {
            var sb = new StringBuilder();
            var random = new Random();
            
            for (int i = 0; i < 5; i++)
            {
                var time = Math.Round(random.NextDouble() * 10 + 1, 3);
                sb.AppendLine($"64 bytes from {destination}: icmp_seq={i + 1} ttl=64 time={time} ms");
            }
            
            return sb.ToString();
        }

        public override string ProcessCommand(string command)
        {
            // Use the command handler manager for all command processing
            if (CommandManager != null)
            {
                var result = CommandManager.ProcessCommand(command);
                
                // If command was handled, return the result 
                if (result != null)
                {
                    // Check if result already ends with prompt
                    var prompt = GetPrompt();
                    if (result.Output.EndsWith(prompt))
                    {
                        return result.Output;
                    }
                    else
                    {
                        return result.Output + prompt;
                    }
                    
                }
            }
            
            // If no handler found, return Nokia error
            return "Error: Invalid command" + GetPrompt();
        }

        // Context navigation methods that handlers can use
        public void NavigateToConfigureContext()
        {
            currentContext = "configure";
        }
        
        public void ExitCurrentContext()
        {
            if (!string.IsNullOrEmpty(currentRouterProtocol))
                currentRouterProtocol = "";
            else if (!string.IsNullOrEmpty(currentVlan))
                currentVlan = "";
            else if (!string.IsNullOrEmpty(currentRouterInterface))
                currentRouterInterface = "";
            else if (!string.IsNullOrEmpty(currentPort))
                currentPort = "";
            else if (currentContext == "configure")
                currentContext = "";
        }
        
        public void BackToRoot()
        {
            currentContext = "";
            currentPort = "";
            currentRouterInterface = "";
            currentVlan = "";
            currentRouterProtocol = "";
        }

        // Protocol initialization helpers
        public void InitializeOspf(int processId)
        {
            if (OspfConfig == null)
            {
                OspfConfig = new OspfConfig(processId);
            }
        }

        public void InitializeBgp(int asNumber)
        {
            if (BgpConfig == null)
            {
                BgpConfig = new BgpConfig(asNumber);
            }
        }

        public void CreateOrSelectVlan(int vlanId)
        {
            if (!Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId] = new VlanConfig(vlanId);
            }
        }

        public Dictionary<string, RoutingPolicy> GetRoutingPolicies()
        {
            return RoutingPolicies ?? new Dictionary<string, RoutingPolicy>();
        }

        public Dictionary<string, PrefixList> GetPrefixLists()
        {
            return PrefixLists ?? new Dictionary<string, PrefixList>();
        }

        public Dictionary<string, BgpCommunity> GetCommunities()
        {
            return BgpCommunities ?? new Dictionary<string, BgpCommunity>();
        }

        public Dictionary<string, AsPathGroup> GetAsPathGroups()
        {
            return AsPathGroups ?? new Dictionary<string, AsPathGroup>();
        }
        
        // Keep the complex context processing methods for backwards compatibility
        // but these can be called by command handlers if needed
        private string ProcessRootContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "show":
                    if (parts.Length > 1)
                    {
                        return ProcessShowCommand(parts);
                    }
                    return "Error: Invalid command\n";
                    
                case "ping":
                    if (parts.Length > 1)
                    {
                        return SimulateNokiaPing(parts[1]);
                    }
                    return "Error: Invalid command\n";
                    
                case "admin":
                    if (parts.Length > 1)
                    {
                        if (parts[1].ToLower() == "save")
                        {
                            return "\nWriting configuration to cf3:\\config.cfg\nSaving configuration ... OK\nCompleted.\n";
                        }
                        else if (parts[1].ToLower() == "reboot")
                        {
                            return "\nAre you want to reboot (y/n)? ";
                        }
                    }
                    return "Error: Invalid command\n";
                    
                case "clear":
                    if (parts.Length > 2 && parts[1].ToLower() == "port" && parts.Length > 3 && parts[3].ToLower() == "statistics")
                    {
                        var portName = parts[2];
                        if (Interfaces.ContainsKey(portName))
                        {
                            var iface = Interfaces[portName];
                            iface.RxPackets = 0;
                            iface.TxPackets = 0;
                            iface.RxBytes = 0;
                            iface.TxBytes = 0;
                            return $"\nCleared statistics for port {portName}\n";
                        }
                        return "Error: Invalid port\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessConfigureContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "port":
                    if (parts.Length > 1)
                    {
                        currentPort = parts[1];
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "router":
                    if (parts.Length > 1)
                    {
                        currentContext = "configure router";
                        if (parts[1].ToLower() == "interface" && parts.Length > 2)
                        {
                            currentRouterInterface = parts[2].Trim('"');
                            return "\n";
                        }
                        else if (parts[1].ToLower() == "ospf" || parts[1].ToLower() == "bgp" || parts[1].ToLower() == "rip")
                        {
                            currentRouterProtocol = parts[1].ToLower();
                            InitializeRoutingProtocol(currentRouterProtocol);
                            return "\n";
                        }
                    }
                    else
                    {
                        currentContext = "configure router";
                    }
                    return "\n";
                    
                case "vlan":
                    if (parts.Length > 1)
                    {
                        currentContext = "configure vlan";
                        currentVlan = parts[1];
                        if (int.TryParse(currentVlan, out int vlanId))
                        {
                            if (!Vlans.ContainsKey(vlanId))
                            {
                                Vlans[vlanId] = new VlanConfig(vlanId);
                            }
                        }
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "system":
                    currentContext = "configure system";
                    return "\n";
                    
                default:
                    // Handle commands within contexts
                    if (!string.IsNullOrEmpty(currentPort))
                        return ProcessPortContext(parts);
                    else if (currentContext == "configure router")
                        return ProcessRouterContext(parts);
                    else if (currentContext == "configure vlan")
                        return ProcessVlanContext(parts);
                    else if (currentContext == "configure system")
                        return ProcessSystemContext(parts);
                    
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessPortContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            if (!Interfaces.ContainsKey(currentPort))
            {
                Interfaces[currentPort] = new InterfaceConfig(currentPort, this);
            }
            
            var iface = Interfaces[currentPort];
            
            switch (cmd)
            {
                case "shutdown":
                    iface.IsShutdown = true;
                    iface.IsUp = false;
                    RunningConfig.AppendLine($"    port {currentPort}");
                    RunningConfig.AppendLine("        shutdown");
                    ParentNetwork?.UpdateProtocols();
                    return "\n";
                    
                case "no":
                    if (parts.Length > 1 && parts[1].ToLower() == "shutdown")
                    {
                        iface.IsShutdown = false;
                        iface.IsUp = true;
                        RunningConfig.AppendLine($"    port {currentPort}");
                        RunningConfig.AppendLine("        no shutdown");
                        ParentNetwork?.UpdateProtocols();
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "description":
                    if (parts.Length > 1)
                    {
                        iface.Description = string.Join(" ", parts.Skip(1)).Trim('"');
                        RunningConfig.AppendLine($"    port {currentPort}");
                        RunningConfig.AppendLine($"        description \"{iface.Description}\"");
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "ethernet":
                    RunningConfig.AppendLine($"    port {currentPort}");
                    RunningConfig.AppendLine("        ethernet");
                    return "\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessRouterContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            if (!string.IsNullOrEmpty(currentRouterInterface))
            {
                return ProcessRouterInterfaceContext(parts);
            }
            else if (!string.IsNullOrEmpty(currentRouterProtocol))
            {
                return ProcessRoutingProtocolContext(parts);
            }
            
            switch (cmd)
            {
                case "interface":
                    if (parts.Length > 1)
                    {
                        currentRouterInterface = parts[1].Trim('"');
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "static-route":
                    if (parts.Length > 3 && parts[2].ToLower() == "next-hop")
                    {
                        var routeParts = parts[1].Split('/');
                        if (routeParts.Length == 2)
                        {
                            var network = routeParts[0];
                            var mask = CidrToMask(int.Parse(routeParts[1]));
                            var nextHop = parts[3];
                            
                            var route = new Route(network, mask, nextHop, "", "Static");
                            route.Metric = 1;
                            RoutingTable.Add(route);
                            
                            RunningConfig.AppendLine($"        static-route {parts[1]} next-hop {nextHop}");
                            return "\n";
                        }
                    }
                    return "Error: Invalid command\n";
                    
                case "ospf":
                case "bgp":
                case "rip":
                    currentRouterProtocol = cmd;
                    InitializeRoutingProtocol(cmd);
                    return "\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessRouterInterfaceContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            // Create a virtual interface for router interfaces
            var ifaceName = $"router-{currentRouterInterface}";
            if (!Interfaces.ContainsKey(ifaceName))
            {
                Interfaces[ifaceName] = new InterfaceConfig(ifaceName, this);
            }
            
            var iface = Interfaces[ifaceName];
            
            switch (cmd)
            {
                case "address":
                    if (parts.Length > 1)
                    {
                        var addressParts = parts[1].Split('/');
                        if (addressParts.Length == 2)
                        {
                            iface.IpAddress = addressParts[0];
                            iface.SubnetMask = CidrToMask(int.Parse(addressParts[1]));
                            RunningConfig.AppendLine($"        interface \"{currentRouterInterface}\"");
                            RunningConfig.AppendLine($"            address {parts[1]}");
                            UpdateConnectedRoutes();
                            ParentNetwork?.UpdateProtocols();
                            return "\n";
                        }
                    }
                    return "Error: Invalid command\n";
                    
                case "port":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"        interface \"{currentRouterInterface}\"");
                        RunningConfig.AppendLine($"            port {parts[1]}");
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessRoutingProtocolContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (currentRouterProtocol)
            {
                case "ospf":
                    return ProcessOspfContext(parts);
                case "bgp":
                    return ProcessBgpContext(parts);
                case "rip":
                    return ProcessRipContext(parts);
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessOspfContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "area":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"        ospf");
                        RunningConfig.AppendLine($"            area {parts[1]}");
                        if (parts.Length > 2 && parts[2].ToLower() == "interface" && parts.Length > 3)
                        {
                            var ifaceName = parts[3].Trim('"');
                            if (int.TryParse(parts[1], out int area))
                            {
                                OspfConfig.Interfaces[ifaceName] = new OspfInterface(ifaceName, area);
                                RunningConfig.AppendLine($"                interface \"{ifaceName}\"");
                                ParentNetwork?.UpdateProtocols();
                            }
                        }
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessBgpContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "group":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"        bgp");
                        RunningConfig.AppendLine($"            group \"{parts[1]}\"");
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "neighbor":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"                neighbor {parts[1]}");
                        if (parts.Length > 3 && parts[2].ToLower() == "peer-as")
                        {
                            if (int.TryParse(parts[3], out int peerAs))
                            {
                                var neighbor = new BgpNeighbor(parts[1], peerAs);
                                BgpConfig.Neighbors[parts[1]] = neighbor;
                                RunningConfig.AppendLine($"                    peer-as {peerAs}");
                                ParentNetwork?.UpdateProtocols();
                            }
                        }
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessRipContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "group":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"        rip");
                        RunningConfig.AppendLine($"            group \"{parts[1]}\"");
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                case "neighbor":
                    if (parts.Length > 1)
                    {
                        RunningConfig.AppendLine($"                neighbor {parts[1]}");
                        RipConfig.Networks.Add(parts[1]);
                        ParentNetwork?.UpdateProtocols();
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private string ProcessVlanContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            if (int.TryParse(currentVlan, out int vlanId) && Vlans.ContainsKey(vlanId))
            {
                var vlan = Vlans[vlanId];
                
                switch (cmd)
                {
                    case "name":
                        if (parts.Length > 1)
                        {
                            vlan.Name = string.Join(" ", parts.Skip(1)).Trim('"');
                            RunningConfig.AppendLine($"    vlan {vlanId}");
                            RunningConfig.AppendLine($"        name \"{vlan.Name}\"");
                            return "\n";
                        }
                        return "Error: Invalid command\n";
                        
                    default:
                        return "Error: Invalid command\n";
                }
            }
            
            return "Error: Invalid command\n";
        }
        
        private string ProcessSystemContext(string[] parts)
        {
            var cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "name":
                    if (parts.Length > 1)
                    {
                        Hostname = parts[1].Trim('"');
                        RunningConfig.AppendLine("    system");
                        RunningConfig.AppendLine($"        name \"{Hostname}\"");
                        return "\n";
                    }
                    return "Error: Invalid command\n";
                    
                default:
                    return "Error: Invalid command\n";
            }
        }
        
        private void InitializeRoutingProtocol(string protocol)
        {
            switch (protocol)
            {
                case "ospf":
                    if (OspfConfig == null)
                        OspfConfig = new OspfConfig(0);
                    break;
                case "bgp":
                    if (BgpConfig == null)
                        BgpConfig = new BgpConfig(65000);
                    break;
                case "rip":
                    if (RipConfig == null)
                        RipConfig = new RipConfig();
                    break;
            }
        }
        
        private string ProcessShowCommand(string[] parts)
        {
            if (parts.Length < 2)
                return "Error: Invalid command\n";
                
            var output = new StringBuilder();
            
            switch (parts[1].ToLower())
            {
                case "configuration":
                    output.AppendLine("===============================================================================");
                    output.AppendLine("TiMOS-B-14.0.R4 both/x86_64 Nokia 7750 SR Copyright (c) 2000-2016 Nokia.");
                    output.AppendLine("All rights reserved. All use subject to applicable license agreements.");
                    output.AppendLine("Built on Thu Jul 28 17:32:13 PDT 2016 by builder in /rel14.0/b1/R4/panos/main");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("# Generated FRI JAN 01 00:00:00 2021 UTC\n");
                    output.AppendLine("exit all");
                    output.AppendLine("configure");
                    output.AppendLine("#--------------------------------------------------");
                    output.AppendLine("echo \"System Configuration\"");
                    output.AppendLine("#--------------------------------------------------");
                    output.AppendLine("    system");
                    output.AppendLine($"        name \"{Hostname}\"");
                    output.AppendLine("    exit");
                    
                    if (RunningConfig.Length > 0)
                    {
                        output.Append(RunningConfig.ToString());
                    }
                    
                    output.AppendLine("exit all");
                    output.AppendLine("\n# Finished FRI JAN 01 00:00:00 2021 UTC");
                    break;
                    
                case "router":
                    if (parts.Length > 2)
                    {
                        switch (parts[2].ToLower())
                        {
                            case "route-table":
                                output.AppendLine("===============================================================================");
                                output.AppendLine("Route Table (Router: Base)");
                                output.AppendLine("===============================================================================");
                                output.AppendLine("Dest Prefix[Flags]                            Type    Proto     Age        Pref");
                                output.AppendLine("      Next Hop[Interface Name]                                    Metric");
                                output.AppendLine("-------------------------------------------------------------------------------");
                                
                                foreach (var route in RoutingTable.OrderBy(r => r.Network))
                                {
                                    var cidr = MaskToCidr(route.Mask);
                                    var type = route.Protocol == "Connected" ? "Local" : "Remote";
                                    var proto = route.Protocol.ToLower();
                                    
                                    output.AppendLine($"{route.Network}/{cidr,-42} {type,-7} {proto,-9} 00h00m00s  {route.AdminDistance}");
                                    output.AppendLine($"       {route.NextHop,-45} {route.Metric}");
                                }
                                
                                output.AppendLine("-------------------------------------------------------------------------------");
                                output.AppendLine($"No. of Routes: {RoutingTable.Count}");
                                break;
                                
                            case "interface":
                                output.AppendLine("===============================================================================");
                                output.AppendLine("Interface Table (Router: Base)");
                                output.AppendLine("===============================================================================");
                                output.AppendLine("Interface-Name                   Adm       Opr(v4/v6)  Mode    Port/SapId");
                                output.AppendLine("   IP-Address                                                  PfxState");
                                output.AppendLine("-------------------------------------------------------------------------------");
                                
                                foreach (var iface in Interfaces.Values.Where(i => i.Name.StartsWith("router-")))
                                {
                                    var adminState = iface.IsShutdown ? "Down" : "Up";
                                    var operState = iface.IsUp ? "Up" : "Down";
                                    var displayName = iface.Name.Replace("router-", "");
                                    
                                    output.AppendLine($"{displayName,-32} {adminState,-9} {operState,-11} Network");
                                    if (!string.IsNullOrEmpty(iface.IpAddress))
                                    {
                                        var cidr = MaskToCidr(iface.SubnetMask);
                                        output.AppendLine($"   {iface.IpAddress}/{cidr,-44} n/a");
                                    }
                                }
                                
                                output.AppendLine("-------------------------------------------------------------------------------");
                                output.AppendLine("Interfaces : " + Interfaces.Values.Count(i => i.Name.StartsWith("router-")));
                                break;
                        }
                    }
                    break;
                    
                case "port":
                    if (parts.Length > 2)
                    {
                        var portName = parts[2];
                        if (Interfaces.ContainsKey(portName))
                        {
                            var port = Interfaces[portName];
                            output.AppendLine("===============================================================================");
                            output.AppendLine($"Ethernet Interface");
                            output.AppendLine("===============================================================================");
                            output.AppendLine($"Description        : {port.Description}");
                            output.AppendLine($"Interface          : {portName}");
                            output.AppendLine($"Admin State        : {(port.IsShutdown ? "down" : "up")}");
                            output.AppendLine($"Oper State         : {(port.IsUp ? "up" : "down")}");
                            output.AppendLine($"Physical Link      : {(port.IsUp ? "Yes" : "No")}");
                            output.AppendLine($"IfIndex            : 35684352");
                            output.AppendLine("===============================================================================");
                        }
                    }
                    break;
                    
                case "system":
                    if (parts.Length > 2 && parts[2].ToLower() == "information")
                    {
                        output.AppendLine("===============================================================================");
                        output.AppendLine("System Information");
                        output.AppendLine("===============================================================================");
                        output.AppendLine($"System Name            : {Hostname}");
                        output.AppendLine("System Type            : 7750 SR-12");
                        output.AppendLine("System Version         : B-14.0.R4");
                        output.AppendLine("System Up Time         : 0 days, 00:00:00 (hr:min:sec)");
                        output.AppendLine("===============================================================================");
                    }
                    break;
                    
                case "version":
                    output.AppendLine("===============================================================================");
                    output.AppendLine("TiMOS-B-14.0.R4 both/x86_64 Nokia 7750 SR Copyright (c) 2000-2016 Nokia.");
                    output.AppendLine("All rights reserved. All use subject to applicable license agreements.");
                    output.AppendLine("Built on Thu Jul 28 17:32:13 PDT 2016 by builder in /rel14.0/b1/R4/panos/main");
                    output.AppendLine("===============================================================================");
                    break;
                    
                case "log":
                    if (parts.Length > 2 && parts[2].ToLower() == "99")
                    {
                        output.AppendLine("===============================================================================");
                        output.AppendLine("Event Log 99");
                        output.AppendLine("===============================================================================");
                        output.AppendLine("Description : Default System Log");
                        output.AppendLine("Memory Log contents  [size=500   next event=6  (wrapped)]");
                        output.AppendLine("-------------------------------------------------------------------------------");
                        output.AppendLine($"5 2021/01/15 10:24:13.39 UTC MINOR: SYSTEM #2002 Base {Hostname}");
                        output.AppendLine("\"System configured from Console\"");
                        output.AppendLine("");
                        output.AppendLine($"4 2021/01/15 10:23:45.12 UTC MINOR: PORT #2001 Base {Hostname}");
                        output.AppendLine("\"Port 1/1/1 is now operationally up\"");
                        output.AppendLine("");
                        output.AppendLine($"3 2021/01/15 10:23:30.00 UTC MAJOR: SYSTEM #2006 Base {Hostname}");
                        output.AppendLine("\"System startup\"");
                        output.AppendLine("===============================================================================");
                    }
                    break;
                    
                case "service":
                    if (parts.Length > 2 && parts[2].ToLower() == "vlan")
                    {
                        output.AppendLine("===============================================================================");
                        output.AppendLine("VLAN Summary");
                        output.AppendLine("===============================================================================");
                        output.AppendLine("VLAN ID  VLAN Name                        Type         Adm  Opr  MTU");
                        output.AppendLine("-------------------------------------------------------------------------------");
                        
                        foreach (var vlan in Vlans.Values.OrderBy(v => v.Id))
                        {
                            output.AppendLine($"{vlan.Id,-8} {vlan.Name,-32} service      Up   Up   1514");
                        }
                        
                        output.AppendLine("-------------------------------------------------------------------------------");
                        output.AppendLine($"Number of VLANs : {Vlans.Count}");
                        output.AppendLine("===============================================================================");
                    }
                    break;
                    
                case "arp":
                    output.AppendLine("===============================================================================");
                    output.AppendLine("ARP Table (Router: Base)");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("IP Address      MAC Address       Expiry    Type   Interface");
                    output.AppendLine("-------------------------------------------------------------------------------");
                    
                    foreach (var iface in Interfaces.Values)
                    {
                        if (!string.IsNullOrEmpty(iface.IpAddress))
                        {
                            output.AppendLine($"{iface.IpAddress,-15} aa:bb:cc:00:01:00 00h00m00s Local  {iface.Name}");
                        }
                    }
                    
                    output.AppendLine("-------------------------------------------------------------------------------");
                    output.AppendLine($"No. of ARP Entries: {Interfaces.Values.Count(i => !string.IsNullOrEmpty(i.IpAddress))}");
                    output.AppendLine("===============================================================================");
                    break;
                    
                case "lag":
                    output.AppendLine("===============================================================================");
                    output.AppendLine("Link Aggregation Group Table");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("LAG    Adm    Opr    Port-         Port-  Num.   Num.   Act/   MC-");
                    output.AppendLine("Id           State  Threshold     Type   Ports  Active Stdby  Act");
                    output.AppendLine("-------------------------------------------------------------------------------");
                    
                    foreach (var pc in PortChannels)
                    {
                        output.AppendLine($"lag-{pc.Key,-3} up     up     0             ether  {pc.Value.MemberInterfaces.Count,-6} {pc.Value.MemberInterfaces.Count,-6} 0/0    N/A");
                    }
                    
                    output.AppendLine("-------------------------------------------------------------------------------");
                    output.AppendLine($"Total LAGs : {PortChannels.Count}");
                    output.AppendLine("===============================================================================");
                    break;
            }
            
            // Handle OSPF and BGP show commands
            if (parts.Length > 3 && parts[1].ToLower() == "router" && parts[2].ToLower() == "ospf")
            {
                if (parts[3].ToLower() == "neighbor")
                {
                    output.AppendLine("===============================================================================");
                    output.AppendLine("OSPF Instance 0 All Neighbors");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("Interface-Name                   Rtr Id          State      Pri  Dead Time");
                    output.AppendLine("  Nbr IP Addr");
                    output.AppendLine("-------------------------------------------------------------------------------");
                    
                    if (OspfConfig != null)
                    {
                        foreach (var neighbor in OspfConfig.Neighbors)
                        {
                            output.AppendLine($"{neighbor.Interface,-32} {neighbor.NeighborId,-15} {neighbor.State,-10} {neighbor.Priority,-4} 00:00:38");
                            output.AppendLine($"  {neighbor.IpAddress}");
                        }
                    }
                    
                    output.AppendLine("-------------------------------------------------------------------------------");
                    output.AppendLine($"No. of Neighbors: {OspfConfig?.Neighbors.Count ?? 0}");
                    output.AppendLine("===============================================================================");
                }
            }
            else if (parts.Length > 3 && parts[1].ToLower() == "router" && parts[2].ToLower() == "bgp")
            {
                if (parts[3].ToLower() == "summary")
                {
                    output.AppendLine("===============================================================================");
                    output.AppendLine("BGP Router ID:0.0.0.0        AS:65000       Local AS:65000");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("BGP Summary");
                    output.AppendLine("===============================================================================");
                    output.AppendLine("Neighbor");
                    output.AppendLine("                   AS PktRcvd InQ  Up/Down   State|Rcv/Act/Sent (Addr Family)");
                    output.AppendLine("                      PktSent OutQ");
                    output.AppendLine("-------------------------------------------------------------------------------");
                    
                    if (BgpConfig != null)
                    {
                        foreach (var neighbor in BgpConfig.Neighbors.Values)
                        {
                            output.AppendLine($"{neighbor.IpAddress}");
                            output.AppendLine($"                {neighbor.RemoteAs,5}       1    0 00:00:00   {neighbor.State,-12} {neighbor.ReceivedRoutes.Count}/{neighbor.ReceivedRoutes.Count}/0");
                            output.AppendLine($"                            1    0");
                        }
                    }
                    
                    output.AppendLine("-------------------------------------------------------------------------------");
                    output.AppendLine("===============================================================================");
                }
            }
            
            return output.ToString();
        }
    }
} 
