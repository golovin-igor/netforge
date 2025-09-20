using System.Text;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Topology.Devices.Juniper
{
    /// <summary>
    /// Builds Juniper-specific running configuration from device state
    /// </summary>
    public class JuniperConfigurationBuilder
    {
        private readonly JuniperDevice _device;
        private readonly IConfigurationProvider _configProvider;

        public JuniperConfigurationBuilder(JuniperDevice device, IConfigurationProvider configProvider)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        }

        /// <summary>
        /// Builds the complete running configuration in Junos format
        /// </summary>
        public string BuildRunningConfiguration()
        {
            var config = new StringBuilder();

            // Juniper uses hierarchical configuration structure
            config.AppendLine("## Last commit: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            config.AppendLine("version " + (_device.GetVersion() ?? "21.1R1"));

            // System configuration
            BuildSystemConfiguration(config);

            // Interfaces
            BuildInterfaceConfiguration(config);

            // Routing options
            BuildRoutingOptions(config);

            // Protocols
            BuildProtocolsConfiguration(config);

            // Policy options
            BuildPolicyOptions(config);

            // Firewall
            BuildFirewallConfiguration(config);

            // VLANs
            BuildVlanConfiguration(config);

            return config.ToString();
        }

        private void BuildSystemConfiguration(StringBuilder config)
        {
            config.AppendLine("system {");
            config.AppendLine($"    host-name {_device.GetHostname()};");
            config.AppendLine("    root-authentication {");
            config.AppendLine("        encrypted-password \"$6$encrypted\";");
            config.AppendLine("    }");
            config.AppendLine("    services {");

            var sshConfig = _configProvider.GetSshConfiguration();
            if (sshConfig != null)
            {
                config.AppendLine("        ssh {");
                config.AppendLine("            protocol-version v2;");
                config.AppendLine("        }");
            }

            config.AppendLine("        netconf {");
            config.AppendLine("            ssh;");
            config.AppendLine("        }");
            config.AppendLine("    }");
            config.AppendLine("    syslog {");
            config.AppendLine("        user * {");
            config.AppendLine("            any emergency;");
            config.AppendLine("        }");
            config.AppendLine("        file messages {");
            config.AppendLine("            any notice;");
            config.AppendLine("            authorization info;");
            config.AppendLine("        }");
            config.AppendLine("    }");
            config.AppendLine("}");
        }

        private void BuildInterfaceConfiguration(StringBuilder config)
        {
            var interfaces = _device.GetInterfaces();
            if (interfaces != null && interfaces.Any())
            {
                config.AppendLine("interfaces {");

                foreach (var iface in interfaces.OrderBy(i => i.Name))
                {
                    config.AppendLine($"    {iface.Name} {{");

                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        config.AppendLine($"        description \"{iface.Description}\";");
                    }

                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        // Convert subnet mask to CIDR for Juniper
                        var cidr = CalculateCidr(iface.SubnetMask);
                        config.AppendLine("        unit 0 {");
                        config.AppendLine("            family inet {");
                        config.AppendLine($"                address {iface.IpAddress}/{cidr};");
                        config.AppendLine("            }");
                        config.AppendLine("        }");
                    }

                    if (iface.IsShutdown)
                    {
                        config.AppendLine("        disable;");
                    }

                    // VLAN configuration for switch interfaces
                    if (iface.VlanId > 0)
                    {
                        config.AppendLine("        unit 0 {");
                        config.AppendLine("            family ethernet-switching {");

                        if (iface.SwitchportMode == "trunk")
                        {
                            config.AppendLine("                interface-mode trunk;");
                            // Note: AllowedVlans not available in current interface - would need separate VLAN management
                        }
                        else
                        {
                            config.AppendLine("                interface-mode access;");
                            config.AppendLine($"                vlan members {iface.VlanId};");
                        }

                        config.AppendLine("            }");
                        config.AppendLine("        }");
                    }

                    config.AppendLine("    }");
                }

                config.AppendLine("}");
            }
        }

        private void BuildRoutingOptions(StringBuilder config)
        {
            config.AppendLine("routing-options {");

            // Static routes
            var routes = _device.GetRoutingTable();
            if (routes != null && routes.Any())
            {
                foreach (var route in routes.Where(r => r.Protocol == "static"))
                {
                    config.AppendLine($"    static route {route.Destination} next-hop {route.NextHop};");
                }
            }

            // Router ID
            var ospfConfig = _configProvider.GetOspfConfiguration();
            if (ospfConfig != null && !string.IsNullOrEmpty(ospfConfig.RouterId))
            {
                config.AppendLine($"    router-id {ospfConfig.RouterId};");
            }
            else
            {
                var bgpConfig = _configProvider.GetBgpConfiguration();
                if (bgpConfig != null && !string.IsNullOrEmpty(bgpConfig.RouterId))
                {
                    config.AppendLine($"    router-id {bgpConfig.RouterId};");
                }
            }

            config.AppendLine("    autonomous-system {");
            var bgpConf = _configProvider.GetBgpConfiguration();
            if (bgpConf != null && bgpConf.LocalAs > 0)
            {
                config.AppendLine($"        {bgpConf.LocalAs};");
            }
            config.AppendLine("    }");

            config.AppendLine("}");
        }

        private void BuildProtocolsConfiguration(StringBuilder config)
        {
            config.AppendLine("protocols {");

            // OSPF
            BuildOspfConfiguration(config);

            // BGP
            BuildBgpConfiguration(config);

            // LLDP
            var lldpConfig = _configProvider.GetLldpConfiguration();
            if (lldpConfig != null && lldpConfig.Enabled)
            {
                config.AppendLine("    lldp {");
                config.AppendLine("        interface all;");
                config.AppendLine("    }");
            }

            // STP
            BuildStpConfiguration(config);

            config.AppendLine("}");
        }

        private void BuildOspfConfiguration(StringBuilder config)
        {
            var ospfConfig = _configProvider.GetOspfConfiguration();
            if (ospfConfig != null && ospfConfig.ProcessId > 0)
            {
                config.AppendLine("    ospf {");

                // Group areas and interfaces
                var areaGroups = new Dictionary<int, List<string>>();

                foreach (var network in ospfConfig.Networks ?? Enumerable.Empty<NetworkConfig>())
                {
                    if (!areaGroups.ContainsKey(network.Area))
                    {
                        areaGroups[network.Area] = new List<string>();
                    }

                    // In Juniper, we specify interfaces in areas
                    // This is simplified - normally would map network to interface
                    areaGroups[network.Area].Add($"{network.Network}");
                }

                foreach (var area in areaGroups)
                {
                    config.AppendLine($"        area {area.Key} {{");
                    foreach (var network in area.Value)
                    {
                        config.AppendLine($"            interface {network};");
                    }
                    config.AppendLine("        }");
                }

                config.AppendLine("    }");
            }
        }

        private void BuildBgpConfiguration(StringBuilder config)
        {
            var bgpConfig = _configProvider.GetBgpConfiguration();
            if (bgpConfig != null && bgpConfig.LocalAs > 0)
            {
                config.AppendLine("    bgp {");
                config.AppendLine($"        group internal {{");
                config.AppendLine("            type internal;");
                config.AppendLine($"            local-address {bgpConfig.RouterId};");

                foreach (var neighbor in bgpConfig.Neighbors ?? new Dictionary<string, BgpNeighbor>())
                {
                    if (neighbor.Value.RemoteAs == bgpConfig.LocalAs)
                    {
                        config.AppendLine($"            neighbor {neighbor.Key} {{");
                        if (!string.IsNullOrEmpty(neighbor.Value.Description))
                        {
                            config.AppendLine($"                description \"{neighbor.Value.Description}\";");
                        }
                        config.AppendLine("            }");
                    }
                }

                config.AppendLine("        }");

                // External BGP neighbors
                var externalNeighbors = bgpConfig.Neighbors?.Where(n => n.Value.RemoteAs != bgpConfig.LocalAs);
                if (externalNeighbors?.Any() == true)
                {
                    config.AppendLine("        group external {");
                    config.AppendLine("            type external;");

                    foreach (var neighbor in externalNeighbors)
                    {
                        config.AppendLine($"            neighbor {neighbor.Key} {{");
                        config.AppendLine($"                peer-as {neighbor.Value.RemoteAs};");
                        if (!string.IsNullOrEmpty(neighbor.Value.Description))
                        {
                            config.AppendLine($"                description \"{neighbor.Value.Description}\";");
                        }
                        config.AppendLine("            }");
                    }

                    config.AppendLine("        }");
                }

                config.AppendLine("    }");
            }
        }

        private void BuildStpConfiguration(StringBuilder config)
        {
            var stpConfig = _configProvider.GetStpConfiguration();
            if (stpConfig != null)
            {
                string protocol = stpConfig.Mode switch
                {
                    "rapid-pvst" => "rstp",
                    "mst" => "mstp",
                    _ => "stp"
                };

                config.AppendLine($"    {protocol} {{");

                if (stpConfig.DefaultPriority != 32768)
                {
                    config.AppendLine($"        bridge-priority {stpConfig.DefaultPriority};");
                }

                // Interface configuration would go here
                config.AppendLine("        interface all;");

                config.AppendLine("    }");
            }
        }

        private void BuildPolicyOptions(StringBuilder config)
        {
            // Policy configuration would go here
            config.AppendLine("policy-options {");
            config.AppendLine("}");
        }

        private void BuildFirewallConfiguration(StringBuilder config)
        {
            // Firewall filters would go here
            config.AppendLine("firewall {");
            config.AppendLine("}");
        }

        private void BuildVlanConfiguration(StringBuilder config)
        {
            var vlans = _device.GetVlans();
            if (vlans != null && vlans.Any())
            {
                config.AppendLine("vlans {");

                foreach (var vlan in vlans.OrderBy(v => v.Id))
                {
                    config.AppendLine($"    vlan-{vlan.Id} {{");
                    if (!string.IsNullOrEmpty(vlan.Name))
                    {
                        config.AppendLine($"        description \"{vlan.Name}\";");
                    }
                    config.AppendLine($"        vlan-id {vlan.Id};");
                    config.AppendLine("    }");
                }

                config.AppendLine("}");
            }
        }

        private int CalculateCidr(string subnetMask)
        {
            if (string.IsNullOrEmpty(subnetMask)) return 24;

            var maskParts = subnetMask.Split('.');
            if (maskParts.Length != 4) return 24;

            int cidr = 0;
            foreach (var part in maskParts)
            {
                if (byte.TryParse(part, out byte octet))
                {
                    cidr += CountBits(octet);
                }
            }

            return cidr;
        }

        private int CountBits(byte value)
        {
            int count = 0;
            while (value > 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }
    }
}