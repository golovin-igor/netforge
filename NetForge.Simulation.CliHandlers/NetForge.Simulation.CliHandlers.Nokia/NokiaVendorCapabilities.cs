using System.Text;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using PortChannelConfig = NetForge.Simulation.Common.Configuration.PortChannel;

namespace NetForge.Simulation.CliHandlers.Nokia
{
    /// <summary>
    /// Nokia-specific vendor capabilities implementation for SR OS
    /// </summary>
    public class NokiaVendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));

        public string VendorName => "Nokia";
        public string CliStyle => "SR OS";

        // Configuration methods
        public string GetRunningConfiguration()
        {
            var config = new StringBuilder();
            config.AppendLine("# TiMOS-B-14.0.R4 both/x86_64 Nokia 7750 SR");
            config.AppendLine("# Generated configuration");
            config.AppendLine("configure");
            config.AppendLine($"    system name \"{_device.GetHostname()}\"");

            foreach (var iface in _device.GetAllInterfaces().Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress) || !string.IsNullOrEmpty(iface.Description))
                {
                    config.AppendLine($"    router interface \"{iface.Name}\"");
                    if (!string.IsNullOrEmpty(iface.IpAddress))
                    {
                        var cidr = GetCidrFromMask(iface.SubnetMask);
                        config.AppendLine($"        address {iface.IpAddress}/{cidr}");
                    }
                    if (!string.IsNullOrEmpty(iface.Description))
                    {
                        config.AppendLine($"        description \"{iface.Description}\"");
                    }
                    if (iface.IsShutdown)
                    {
                        config.AppendLine("        shutdown");
                    }
                    config.AppendLine("        exit");
                }
            }

            config.AppendLine("exit all");
            return config.ToString();
        }

        public string GetStartupConfiguration()
        {
            return GetRunningConfiguration();
        }

        // Device mode methods
        public void SetDeviceMode(string mode)
        {
            _device.SetMode(mode);
        }

        public string GetDeviceMode()
        {
            return _device.GetCurrentMode();
        }

        public bool SupportsMode(string mode)
        {
            return mode.ToLower() switch
            {
                "user" => true,
                "privileged" => true,
                "config" => true,
                "configure" => true,
                "router" => true,
                "interface" => true,
                _ => false
            };
        }

        public IEnumerable<string> GetAvailableModes()
        {
            return new[] { "user", "privileged", "config", "configure", "router", "interface" };
        }

        // Output formatting
        public string FormatCommandOutput(string command, object? data = null)
        {
            return data?.ToString() ?? "";
        }

        public string GetVendorErrorMessage(string errorType, string? context = null)
        {
            return $"Error: {errorType}";
        }

        public string FormatInterfaceName(string interfaceName)
        {
            // Nokia SR OS uses interface names like "1/1/1"
            return interfaceName;
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            return commandParts.Length > 0;
        }

        public bool SupportsFeature(string feature)
        {
            return feature.ToLower() switch
            {
                "vlan_support" => true,
                "interface_configuration" => true,
                "ip_routing" => true,
                "ospf" => true,
                "bgp" => true,
                "isis" => true,
                "mpls" => true,
                _ => false
            };
        }

        // Interface configuration
        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.IpAddress = ipAddress;
                iface.SubnetMask = subnetMask;
                return true;
            }
            return false;
        }

        public bool RemoveInterfaceIp(string interfaceName)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.IpAddress = "";
                iface.SubnetMask = "";
                return true;
            }
            return false;
        }

        public bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction)
        {
            // Nokia SR OS ACL implementation placeholder
            return true;
        }

        public bool RemoveAccessGroup(string interfaceName)
        {
            // Nokia SR OS ACL removal placeholder
            return true;
        }

        public bool SetInterfaceShutdown(string interfaceName, bool shutdown)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.IsShutdown = shutdown;
                iface.IsUp = !shutdown;
                return true;
            }
            return false;
        }

        // VLAN methods
        public bool CreateOrSelectVlan(int vlanId)
        {
            var vlans = _device.GetAllVlans();
            if (!vlans.ContainsKey(vlanId))
            {
                vlans[vlanId] = new VlanConfig(vlanId);
            }
            return true;
        }

        // Protocol initialization
        public bool InitializeOspf(int processId)
        {
            var ospfConfig = _device.GetOspfConfiguration();
            if (ospfConfig == null)
            {
                // Initialize OSPF configuration - placeholder implementation
                // The actual initialization would be handled by the device
            }
            return true;
        }

        public bool InitializeBgp(int asNumber)
        {
            var bgpConfig = _device.GetBgpConfiguration();
            if (bgpConfig == null)
            {
                // Initialize BGP configuration - placeholder implementation
                // The actual initialization would be handled by the device
            }
            return true;
        }

        public bool InitializeRip()
        {
            var ripConfig = _device.GetRipConfiguration();
            if (ripConfig == null)
            {
                // Initialize RIP configuration - placeholder implementation
                // The actual initialization would be handled by the device
            }
            return true;
        }

        public bool InitializeEigrp(int asNumber)
        {
            // EIGRP not typically supported on Nokia SR OS
            return false;
        }

        public bool SetCurrentRouterProtocol(string protocol)
        {
            // Protocol context setting placeholder
            return true;
        }

        // ACL methods
        public bool AddAclEntry(int aclNumber, object aclEntry)
        {
            // Nokia SR OS ACL entry implementation placeholder
            return true;
        }

        public bool SetCurrentAclNumber(int aclNumber)
        {
            // ACL context setting placeholder
            return true;
        }

        public int GetCurrentAclNumber()
        {
            return 0; // Default ACL number
        }

        // Configuration management
        public bool AppendToRunningConfig(string configLine)
        {
            // Append configuration line placeholder
            return true;
        }

        // VLAN management
        public bool AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            CreateOrSelectVlan(vlanId);
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.VlanId = vlanId;
                return true;
            }
            return false;
        }

        public bool VlanExists(int vlanId)
        {
            return _device.GetAllVlans().ContainsKey(vlanId);
        }

        public bool SetVlanName(int vlanId, string name)
        {
            var vlans = _device.GetAllVlans();
            if (vlans.TryGetValue(vlanId, out var vlan))
            {
                vlan.Name = name;
                return true;
            }
            return false;
        }

        // STP methods
        public bool SetStpMode(string mode)
        {
            // STP mode setting placeholder
            return true;
        }

        public bool SetStpVlanPriority(int vlanId, int priority)
        {
            // STP VLAN priority placeholder
            return true;
        }

        public bool SetStpPriority(int priority)
        {
            // STP priority placeholder
            return true;
        }

        public bool EnablePortfast(string interfaceName)
        {
            // Portfast enable placeholder
            return true;
        }

        public bool DisablePortfast(string interfaceName)
        {
            // Portfast disable placeholder
            return true;
        }

        public bool EnablePortfastDefault()
        {
            // Global portfast enable placeholder
            return true;
        }

        public bool EnableBpduGuard(string interfaceName)
        {
            // BPDU guard enable placeholder
            return true;
        }

        public bool DisableBpduGuard(string interfaceName)
        {
            // BPDU guard disable placeholder
            return true;
        }

        public bool EnableBpduGuardDefault()
        {
            // Global BPDU guard enable placeholder
            return true;
        }

        // Port channel methods
        public bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            var portChannels = _device.GetPortChannels();
            if (!portChannels.ContainsKey(channelId))
            {
                portChannels[channelId] = new PortChannelConfig(channelId);
            }
            return true;
        }

        // CDP methods (not supported on Nokia, but required by interface)
        public bool EnableCdpGlobal()
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        public bool DisableCdpGlobal()
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        public bool EnableCdpInterface(string interfaceName)
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        public bool DisableCdpInterface(string interfaceName)
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        public bool SetCdpTimer(int seconds)
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        public bool SetCdpHoldtime(int seconds)
        {
            // CDP not supported on Nokia SR OS
            return false;
        }

        // Basic device configuration
        public bool SetHostname(string hostname)
        {
            _device.SetHostname(hostname);
            return true;
        }

        public bool SetInterfaceDescription(string interfaceName, string description)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.Description = description;
                return true;
            }
            return false;
        }

        public bool SetSwitchportMode(string interfaceName, string mode)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.SwitchportMode = mode;
                return true;
            }
            return false;
        }

        public bool SetInterfaceVlan(string interfaceName, int vlanId)
        {
            return AddInterfaceToVlan(interfaceName, vlanId);
        }

        public bool SetCurrentInterface(string interfaceName)
        {
            _device.SetCurrentInterface(interfaceName);
            return true;
        }

        public bool SetInterfaceState(string interfaceName, string state)
        {
            var interfaces = _device.GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var iface))
            {
                iface.IsUp = state.ToLower() == "up";
                iface.IsShutdown = state.ToLower() == "down";
                return true;
            }
            return false;
        }

        public bool SetInterface(string interfaceName, string property, object value)
        {
            var interfaces = _device.GetAllInterfaces();
            if (!interfaces.TryGetValue(interfaceName, out var iface))
                return false;

            switch (property.ToLower())
            {
                case "description":
                    iface.Description = value.ToString() ?? "";
                    return true;
                case "shutdown":
                    iface.IsShutdown = Convert.ToBoolean(value);
                    iface.IsUp = !iface.IsShutdown;
                    return true;
                default:
                    return false;
            }
        }

        // Device management
        public bool SaveConfiguration()
        {
            // Save configuration placeholder
            return true;
        }

        public bool ReloadDevice()
        {
            // Device reload placeholder
            return true;
        }

        // Helper methods
        private int GetCidrFromMask(string subnetMask)
        {
            if (string.IsNullOrEmpty(subnetMask))
                return 24;

            var parts = subnetMask.Split('.');
            if (parts.Length != 4)
                return 24;

            int cidr = 0;
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int octet))
                {
                    cidr += CountBits(octet);
                }
            }
            return cidr;
        }

        private int CountBits(int value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }
    }
}
