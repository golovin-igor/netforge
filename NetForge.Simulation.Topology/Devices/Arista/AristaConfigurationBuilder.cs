using System.Text;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Topology.Devices.Arista
{
    /// <summary>
    /// Builds Arista-specific running configuration from device state
    /// </summary>
    public class AristaConfigurationBuilder
    {
        private readonly INetworkDevice _device;
        private readonly IConfigurationProvider _configProvider;

        public AristaConfigurationBuilder(INetworkDevice device, IConfigurationProvider configProvider)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        }

        /// <summary>
        /// Builds the complete running configuration in Arista EOS format
        /// </summary>
        public string BuildRunningConfiguration()
        {
            var config = new StringBuilder();

            // Header
            config.AppendLine("! Command: show running-config");
            config.AppendLine($"! device: {_device.GetHostname()} (vEOS, EOS-{_device.GetSoftwareVersion() ?? "4.27.1F"})");
            config.AppendLine("!");
            config.AppendLine("! boot system flash:/vEOS-lab.swi");
            config.AppendLine("!");

            // Transceiver settings
            config.AppendLine("transceiver qsfp default-mode 4x10G");
            config.AppendLine("!");

            // Service routing protocols model
            config.AppendLine("service routing protocols model multi-agent");
            config.AppendLine("!");

            // Hostname
            config.AppendLine($"hostname {_device.GetHostname()}");
            config.AppendLine("!");

            // Spanning tree
            BuildSpanningTreeConfiguration(config);

            // VLANs
            BuildVlanConfiguration(config);

            // Interfaces
            BuildInterfaceConfiguration(config);

            // Routing
            BuildRoutingConfiguration(config);

            // Management
            BuildManagementConfiguration(config);

            // End
            config.AppendLine("!");
            config.AppendLine("end");

            return config.ToString();
        }

        private void BuildSpanningTreeConfiguration(StringBuilder config)
        {
            var stpConfig = _configProvider.GetStpConfiguration();
            if (stpConfig != null)
            {
                var mode = stpConfig.Mode switch
                {
                    "rapid-pvst" => "rapid-pvst",
                    "mst" => "mstp",
                    "pvst" => "pvst",
                    _ => "mstp"  // Arista default
                };

                config.AppendLine($"spanning-tree mode {mode}");

                if (stpConfig.DefaultPriority != 32768)
                {
                    config.AppendLine($"spanning-tree priority {stpConfig.DefaultPriority}");
                }

                foreach (var vlanPriority in stpConfig.VlanPriorities ?? new Dictionary<int, int>())
                {
                    config.AppendLine($"spanning-tree vlan {vlanPriority.Key} priority {vlanPriority.Value}");
                }

                config.AppendLine("!");
            }
        }

        private void BuildVlanConfiguration(StringBuilder config)
        {
            var vlans = _device.GetVlans();
            if (vlans != null && vlans.Any())
            {
                foreach (var vlan in vlans.OrderBy(v => v.Id))
                {
                    config.AppendLine($"vlan {vlan.Id}");
                    if (!string.IsNullOrEmpty(vlan.Name))
                    {
                        config.AppendLine($"   name {vlan.Name}");
                    }
                    config.AppendLine("!");
                }
            }
        }

        private void BuildInterfaceConfiguration(StringBuilder config)
        {
            var interfaces = _device.GetInterfaces();
            if (interfaces != null)
            {
                foreach (var iface in interfaces.OrderBy(i => i.Name))
                {
                    config.AppendLine($"interface {iface.Name}");

                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        config.AppendLine($"   description {iface.Description}");
                    }

                    // Physical interface settings
                    if (iface.Name.StartsWith("Ethernet"))
                    {
                        config.AppendLine("   no switchport");
                    }

                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        config.AppendLine($"   ip address {iface.IpAddress} {iface.SubnetMask}");
                    }

                    // VLAN configuration for switch interfaces
                    if (iface.VlanId > 0)
                    {
                        if (iface.IsTrunk)
                        {
                            config.AppendLine("   switchport mode trunk");
                            if (iface.AllowedVlans?.Any() == true)
                            {
                                var vlanList = string.Join(",", iface.AllowedVlans);
                                config.AppendLine($"   switchport trunk allowed vlan {vlanList}");
                            }
                        }
                        else
                        {
                            config.AppendLine("   switchport access vlan " + iface.VlanId);
                            config.AppendLine("   switchport mode access");
                        }
                    }

                    if (iface.IsShutdown)
                    {
                        config.AppendLine("   shutdown");
                    }
                    else
                    {
                        config.AppendLine("   no shutdown");
                    }

                    config.AppendLine("!");
                }
            }

            // Management interface
            config.AppendLine("interface Management1");
            config.AppendLine("   ip address dhcp");
            config.AppendLine("   no shutdown");
            config.AppendLine("!");

            // Loopback interfaces
            config.AppendLine("interface Loopback0");
            var routerId = _configProvider.GetOspfConfiguration()?.RouterId ??
                          _configProvider.GetBgpConfiguration()?.RouterId;
            if (!string.IsNullOrEmpty(routerId))
            {
                config.AppendLine($"   ip address {routerId}/32");
            }
            config.AppendLine("!");
        }

        private void BuildRoutingConfiguration(StringBuilder config)
        {
            // IP routing
            config.AppendLine("ip routing");
            config.AppendLine("!");

            // Static routes
            var routes = _device.GetRoutingTable();
            if (routes != null && routes.Any())
            {
                foreach (var route in routes.Where(r => r.Protocol == "static"))
                {
                    config.AppendLine($"ip route {route.Destination} {route.NextHop}");
                }
                config.AppendLine("!");
            }

            // OSPF
            BuildOspfConfiguration(config);

            // BGP
            BuildBgpConfiguration(config);
        }

        private void BuildOspfConfiguration(StringBuilder config)
        {
            var ospfConfig = _configProvider.GetOspfConfiguration();
            if (ospfConfig != null && ospfConfig.ProcessId > 0)
            {
                config.AppendLine($"router ospf {ospfConfig.ProcessId}");

                if (!string.IsNullOrEmpty(ospfConfig.RouterId))
                {
                    config.AppendLine($"   router-id {ospfConfig.RouterId}");
                }

                // Process each network
                foreach (var network in ospfConfig.Networks ?? Enumerable.Empty<NetworkConfig>())
                {
                    config.AppendLine($"   network {network.Network} {network.Wildcard} area {network.Area}");
                }

                // Default information originate
                config.AppendLine("   default-information originate");

                // Maximum paths
                config.AppendLine("   maximum-paths 4");

                config.AppendLine("!");
            }
        }

        private void BuildBgpConfiguration(StringBuilder config)
        {
            var bgpConfig = _configProvider.GetBgpConfiguration();
            if (bgpConfig != null && bgpConfig.LocalAs > 0)
            {
                config.AppendLine($"router bgp {bgpConfig.LocalAs}");

                if (!string.IsNullOrEmpty(bgpConfig.RouterId))
                {
                    config.AppendLine($"   router-id {bgpConfig.RouterId}");
                }

                // Networks
                foreach (var network in bgpConfig.Networks ?? Enumerable.Empty<string>())
                {
                    config.AppendLine($"   network {network}");
                }

                // Neighbors
                foreach (var neighbor in bgpConfig.Neighbors ?? new Dictionary<string, NeighborConfig>())
                {
                    config.AppendLine($"   neighbor {neighbor.Key} remote-as {neighbor.Value.RemoteAs}");

                    if (!string.IsNullOrEmpty(neighbor.Value.Description))
                    {
                        config.AppendLine($"   neighbor {neighbor.Key} description {neighbor.Value.Description}");
                    }

                    // Enable neighbor
                    if (neighbor.Value.State != "Idle (Admin)")
                    {
                        config.AppendLine($"   neighbor {neighbor.Key} send-community");
                        config.AppendLine($"   neighbor {neighbor.Key} maximum-routes 12000");
                    }
                    else
                    {
                        config.AppendLine($"   neighbor {neighbor.Key} shutdown");
                    }
                }

                // Redistribution
                config.AppendLine("   redistribute connected");

                config.AppendLine("!");
            }
        }

        private void BuildManagementConfiguration(StringBuilder config)
        {
            // Management API HTTP
            config.AppendLine("management api http-commands");
            config.AppendLine("   no shutdown");
            config.AppendLine("   vrf default");
            config.AppendLine("      no shutdown");
            config.AppendLine("!");

            // SSH
            var sshConfig = _configProvider.GetSshConfiguration();
            if (sshConfig != null)
            {
                config.AppendLine("management ssh");
                config.AppendLine("   idle-timeout 0");
                config.AppendLine("!");
            }

            // LLDP
            var lldpConfig = _configProvider.GetLldpConfiguration();
            if (lldpConfig != null && lldpConfig.Enabled)
            {
                config.AppendLine("lldp run");
                config.AppendLine("!");
            }
        }
    }
}