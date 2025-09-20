using System.Text;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Topology.Devices.Cisco
{
    /// <summary>
    /// Builds Cisco-specific running configuration from device state
    /// </summary>
    public class CiscoConfigurationBuilder
    {
        private readonly INetworkDevice _device;
        private readonly IConfigurationProvider _configProvider;

        public CiscoConfigurationBuilder(INetworkDevice device, IConfigurationProvider configProvider)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        }

        /// <summary>
        /// Builds the complete running configuration in Cisco IOS format
        /// </summary>
        public string BuildRunningConfiguration()
        {
            var config = new StringBuilder();

            // Header
            config.AppendLine("!");
            config.AppendLine($"! Last configuration change at {DateTime.Now:HH:mm:ss zzz ddd MMM dd yyyy}");
            config.AppendLine("!");
            config.AppendLine($"version {_device.GetSoftwareVersion() ?? "15.7"}");
            config.AppendLine("service timestamps debug datetime msec");
            config.AppendLine("service timestamps log datetime msec");
            config.AppendLine("no service password-encryption");
            config.AppendLine("!");

            // Hostname
            config.AppendLine($"hostname {_device.GetHostname()}");
            config.AppendLine("!");

            // Boot and platform settings
            config.AppendLine("boot-start-marker");
            config.AppendLine("boot-end-marker");
            config.AppendLine("!");

            // Global settings
            BuildGlobalSettings(config);

            // VLANs
            BuildVlanConfiguration(config);

            // Interfaces
            BuildInterfaceConfiguration(config);

            // Routing protocols
            BuildRoutingProtocolConfiguration(config);

            // Spanning Tree
            BuildSpanningTreeConfiguration(config);

            // Discovery protocols
            BuildDiscoveryProtocolConfiguration(config);

            // Management
            BuildManagementConfiguration(config);

            // Access lists
            BuildAccessListConfiguration(config);

            // Line configuration
            BuildLineConfiguration(config);

            // End
            config.AppendLine("!");
            config.AppendLine("end");

            return config.ToString();
        }

        private void BuildGlobalSettings(StringBuilder config)
        {
            config.AppendLine("!");
            config.AppendLine("no aaa new-model");
            config.AppendLine("!");
            config.AppendLine("ip cef");
            config.AppendLine("no ipv6 cef");
            config.AppendLine("!");
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
                        config.AppendLine($" name {vlan.Name}");
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
                        config.AppendLine($" description {iface.Description}");
                    }

                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        config.AppendLine($" ip address {iface.IpAddress} {iface.SubnetMask}");
                    }
                    else
                    {
                        config.AppendLine(" no ip address");
                    }

                    if (iface.IsShutdown)
                    {
                        config.AppendLine(" shutdown");
                    }
                    else
                    {
                        config.AppendLine(" no shutdown");
                    }

                    // VLAN configuration for switch interfaces
                    if (iface.VlanId > 0)
                    {
                        if (iface.IsTrunk)
                        {
                            config.AppendLine(" switchport mode trunk");
                            if (iface.AllowedVlans?.Any() == true)
                            {
                                var vlanList = string.Join(",", iface.AllowedVlans);
                                config.AppendLine($" switchport trunk allowed vlan {vlanList}");
                            }
                        }
                        else
                        {
                            config.AppendLine(" switchport mode access");
                            config.AppendLine($" switchport access vlan {iface.VlanId}");
                        }
                    }

                    config.AppendLine("!");
                }
            }
        }

        private void BuildRoutingProtocolConfiguration(StringBuilder config)
        {
            // OSPF
            var ospfConfig = _configProvider.GetOspfConfiguration();
            if (ospfConfig != null && ospfConfig.ProcessId > 0)
            {
                config.AppendLine($"router ospf {ospfConfig.ProcessId}");

                if (!string.IsNullOrEmpty(ospfConfig.RouterId))
                {
                    config.AppendLine($" router-id {ospfConfig.RouterId}");
                }

                foreach (var network in ospfConfig.Networks ?? Enumerable.Empty<NetworkConfig>())
                {
                    config.AppendLine($" network {network.Network} {network.Wildcard} area {network.Area}");
                }

                config.AppendLine("!");
            }

            // BGP
            var bgpConfig = _configProvider.GetBgpConfiguration();
            if (bgpConfig != null && bgpConfig.LocalAs > 0)
            {
                config.AppendLine($"router bgp {bgpConfig.LocalAs}");

                if (!string.IsNullOrEmpty(bgpConfig.RouterId))
                {
                    config.AppendLine($" bgp router-id {bgpConfig.RouterId}");
                }

                foreach (var neighbor in bgpConfig.Neighbors ?? new Dictionary<string, NeighborConfig>())
                {
                    config.AppendLine($" neighbor {neighbor.Key} remote-as {neighbor.Value.RemoteAs}");

                    if (!string.IsNullOrEmpty(neighbor.Value.Description))
                    {
                        config.AppendLine($" neighbor {neighbor.Key} description {neighbor.Value.Description}");
                    }

                    if (neighbor.Value.State == "Idle (Admin)")
                    {
                        config.AppendLine($" neighbor {neighbor.Key} shutdown");
                    }
                }

                foreach (var network in bgpConfig.Networks ?? Enumerable.Empty<string>())
                {
                    config.AppendLine($" network {network}");
                }

                config.AppendLine("!");
            }

            // RIP
            var ripConfig = _configProvider.GetRipConfiguration();
            if (ripConfig != null && (ripConfig.Networks?.Any() == true))
            {
                config.AppendLine("router rip");

                if (ripConfig.Version > 0)
                {
                    config.AppendLine($" version {ripConfig.Version}");
                }

                foreach (var network in ripConfig.Networks)
                {
                    config.AppendLine($" network {network}");
                }

                if (!ripConfig.AutoSummary)
                {
                    config.AppendLine(" no auto-summary");
                }

                config.AppendLine("!");
            }

            // EIGRP
            var eigrpConfig = _configProvider.GetEigrpConfiguration();
            if (eigrpConfig != null && eigrpConfig.AsNumber > 0)
            {
                config.AppendLine($"router eigrp {eigrpConfig.AsNumber}");

                foreach (var network in eigrpConfig.Networks ?? Enumerable.Empty<string>())
                {
                    config.AppendLine($" network {network}");
                }

                if (!eigrpConfig.AutoSummary)
                {
                    config.AppendLine(" no auto-summary");
                }

                config.AppendLine("!");
            }
        }

        private void BuildSpanningTreeConfiguration(StringBuilder config)
        {
            var stpConfig = _configProvider.GetStpConfiguration();
            if (stpConfig != null)
            {
                if (!string.IsNullOrEmpty(stpConfig.Mode))
                {
                    config.AppendLine($"spanning-tree mode {stpConfig.Mode}");
                }

                if (stpConfig.DefaultPriority != 32768) // Default STP priority
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

        private void BuildDiscoveryProtocolConfiguration(StringBuilder config)
        {
            var cdpConfig = _configProvider.GetCdpConfiguration();
            if (cdpConfig != null)
            {
                if (cdpConfig.Enabled)
                {
                    config.AppendLine("cdp run");

                    if (cdpConfig.Timer != 60) // Default CDP timer
                    {
                        config.AppendLine($"cdp timer {cdpConfig.Timer}");
                    }

                    if (cdpConfig.Holdtime != 180) // Default CDP holdtime
                    {
                        config.AppendLine($"cdp holdtime {cdpConfig.Holdtime}");
                    }
                }
                else
                {
                    config.AppendLine("no cdp run");
                }

                config.AppendLine("!");
            }
        }

        private void BuildManagementConfiguration(StringBuilder config)
        {
            // SSH configuration
            var sshConfig = _configProvider.GetSshConfiguration();
            if (sshConfig != null)
            {
                config.AppendLine("ip ssh version 2");
                config.AppendLine("!");
            }

            // SNMP configuration
            var snmpConfig = _configProvider.GetSnmpConfiguration();
            if (snmpConfig != null)
            {
                // Basic SNMP configuration
                config.AppendLine("!");
            }
        }

        private void BuildAccessListConfiguration(StringBuilder config)
        {
            // Access lists would go here
            config.AppendLine("!");
        }

        private void BuildLineConfiguration(StringBuilder config)
        {
            // Console line
            config.AppendLine("line con 0");
            config.AppendLine(" logging synchronous");
            config.AppendLine("!");

            // VTY lines
            config.AppendLine("line vty 0 4");
            config.AppendLine(" transport input ssh telnet");
            config.AppendLine("line vty 5 15");
            config.AppendLine(" transport input ssh telnet");
            config.AppendLine("!");
        }
    }
}