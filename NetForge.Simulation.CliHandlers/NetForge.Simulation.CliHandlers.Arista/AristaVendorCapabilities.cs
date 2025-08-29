using NetForge.Simulation.Common;
using System.Text;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.CliHandlers.Arista
{
    /// <summary>
    /// Arista-specific vendor capabilities implementation
    /// </summary>
    public class AristaVendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));

        public string VendorName => "Arista";
        public string VendorVersion => "EOS 4.x";
        public string DefaultConfigurationMode => "config";
        public string DefaultPrivilegedMode => "privileged";
        public string DefaultUserMode => "user";

        // Configuration methods
        public string GetRunningConfiguration()
        {
            var config = new StringBuilder();
            config.AppendLine("! Arista EOS Configuration");
            config.AppendLine($"hostname {_device.GetHostname()}");

            foreach (var iface in _device.GetAllInterfaces().Values)
            {
                config.AppendLine($"interface {iface.Name}");
                if (!string.IsNullOrEmpty(iface.Description))
                    config.AppendLine($"   description {iface.Description}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    config.AppendLine($"   ip address {iface.IpAddress}/{GetCidrFromMask(iface.SubnetMask)}");
                if (iface.IsShutdown)
                    config.AppendLine("   shutdown");
                else
                    config.AppendLine("   no shutdown");
                config.AppendLine("   exit");
            }

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
                "interface" => true,
                "router" => true,
                "vlan" => true,
                _ => false
            };
        }

        public IEnumerable<string> GetAvailableModes()
        {
            return new[] { "user", "privileged", "config", "interface", "router", "vlan" };
        }

        // Output formatting
        public string FormatCommandOutput(string command, object? data = null)
        {
            return data?.ToString() ?? "";
        }

        public string GetVendorErrorMessage(string errorType, string? context = null)
        {
            return $"% {errorType}";
        }

        public string FormatInterfaceName(string interfaceName)
        {
            // Arista uses Ethernet format
            if (interfaceName.StartsWith("eth"))
                return "Ethernet" + interfaceName.Substring(3);
            if (interfaceName.StartsWith("e"))
                return "Ethernet" + interfaceName.Substring(1);
            return interfaceName;
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            return commandParts.Length > 0;
        }

        // Interface configuration
        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.IpAddress = ipAddress;
                iface.SubnetMask = subnetMask;
                return true;
            }
            return false;
        }

        public bool RemoveInterfaceIp(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.IpAddress = "";
                iface.SubnetMask = "";
                return true;
            }
            return false;
        }

        public bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction)
        {
            // Arista ACL implementation placeholder
            return true;
        }

        public bool RemoveAccessGroup(string interfaceName)
        {
            // Arista ACL removal placeholder
            return true;
        }

        public bool SetInterfaceShutdown(string interfaceName, bool shutdown)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
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
            if (_device.GetOspfConfiguration() == null)
                _device.SetOspfConfiguration(new OspfConfig(processId));
            return true;
        }

        public bool InitializeBgp(int asNumber)
        {
            if (_device.GetBgpConfiguration() == null)
                _device.SetBgpConfiguration(new BgpConfig(asNumber));
            return true;
        }

        public bool InitializeRip()
        {
            if (_device.GetRipConfiguration() == null)
                _device.SetRipConfiguration(new RipConfig());
            return true;
        }

        public bool InitializeEigrp(int asNumber)
        {
            // EIGRP not typically supported on Arista EOS
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
            // Arista ACL entry implementation placeholder
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
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.VlanId = vlanId;
                return true;
            }
            return false;
        }

        public bool VlanExists(int vlanId)
        {
            return _device.GetVlan(vlanId) != null;
        }

        public bool SetVlanName(int vlanId, string name)
        {
            var vlan = _device.GetVlan(vlanId);
            if (vlan != null)
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
            if (!_device.GetPortChannels().ContainsKey(channelId))
            {
                _device.GetPortChannels()[channelId] = new PortChannel(channelId);
            }
            return true;
        }

        // CDP methods (LLDP used instead on Arista)
        public bool EnableCdpGlobal()
        {
            // Arista uses LLDP instead of CDP
            return false;
        }

        public bool DisableCdpGlobal()
        {
            // Arista uses LLDP instead of CDP
            return false;
        }

        public bool EnableCdpInterface(string interfaceName)
        {
            // Arista uses LLDP instead of CDP
            return false;
        }

        public bool DisableCdpInterface(string interfaceName)
        {
            // Arista uses LLDP instead of CDP
            return false;
        }

        public bool SetCdpTimer(int seconds)
        {
            // Arista uses LLDP instead of CDP
            return false;
        }

        public bool SetCdpHoldtime(int seconds)
        {
            // Arista uses LLDP instead of CDP
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
            if (_device.GetAllInterfaces().TryGetValue(interfaceName, out var iface))
            {
                iface.Description = description;
                return true;
            }
            return false;
        }

        public bool SetSwitchportMode(string interfaceName, string mode)
        {
            if (_device.GetAllInterfaces().TryGetValue(interfaceName, out var iface))
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
            if (_device.GetAllInterfaces().TryGetValue(interfaceName, out var iface))
            {
                iface.IsUp = state.ToLower() == "up";
                iface.IsShutdown = state.ToLower() == "down";
                return true;
            }
            return false;
        }

        public bool SetInterface(string interfaceName, string property, object value)
        {
            if (!_device.GetAllInterfaces().TryGetValue(interfaceName, out var iface))
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

        public bool SupportsFeature(string feature)
        {
            return feature.ToLower() switch
            {
                "vlan" => true,
                "routing" => true,
                "switching" => true,
                "acl" => true,
                "stp" => true,
                "lacp" => true,
                "vrf" => true,
                "bgp" => true,
                "ospf" => true,
                "isis" => true,
                "mpls" => true,
                "lldp" => true,
                "snmp" => true,
                "ssh" => true,
                "ntp" => true,
                "logging" => true,
                "aaa" => true,
                _ => false
            };
        }

        public IEnumerable<string> GetSupportedModes()
        {
            return new[] { "user", "privileged", "config", "interface", "router", "vlan" };
        }

        public IEnumerable<string> GetSupportedCommands()
        {
            return new[]
            {
                "show", "configure", "interface", "router", "enable", "exit", "ping", "write", "reload",
                "hostname", "ip", "vlan", "no", "shutdown", "description", "switchport"
            };
        }

        public string GetPromptForMode(string mode)
        {
            var hostname = _device.Name;
            return mode.ToLower() switch
            {
                "user" => $"{hostname}>",
                "privileged" => $"{hostname}#",
                "config" => $"{hostname}(config)#",
                "interface" => $"{hostname}(config-if)#",
                "router" => $"{hostname}(config-router)#",
                "vlan" => $"{hostname}(config-vlan)#",
                _ => $"{hostname}#{mode}#"
            };
        }

        public bool IsValidTransition(string fromMode, string toMode)
        {
            return (fromMode.ToLower(), toMode.ToLower()) switch
            {
                ("user", "privileged") => true,
                ("privileged", "user") => true,
                ("privileged", "config") => true,
                ("config", "privileged") => true,
                ("config", "interface") => true,
                ("interface", "config") => true,
                ("config", "router") => true,
                ("router", "config") => true,
                ("config", "vlan") => true,
                ("vlan", "config") => true,
                _ => false
            };
        }

        public string GetModeDescription(string mode)
        {
            return mode.ToLower() switch
            {
                "user" => "User EXEC mode",
                "privileged" => "Privileged EXEC mode",
                "config" => "Global configuration mode",
                "interface" => "Interface configuration mode",
                "router" => "Router configuration mode",
                "vlan" => "VLAN configuration mode",
                _ => $"Unknown mode: {mode}"
            };
        }

        public IEnumerable<string> GetCommandsForMode(string mode)
        {
            return mode.ToLower() switch
            {
                "user" => new[] { "enable", "ping", "show", "exit" },
                "privileged" => new[] { "configure", "show", "ping", "write", "reload", "exit" },
                "config" => new[] { "interface", "router", "hostname", "ip", "vlan", "exit" },
                "interface" => new[] { "ip", "shutdown", "no", "description", "switchport", "exit" },
                "router" => new[] { "network", "neighbor", "router-id", "version", "exit" },
                "vlan" => new[] { "name", "state", "exit" },
                _ => Array.Empty<string>()
            };
        }

        public bool RequiresPrivilegedMode(string command)
        {
            var privilegedCommands = new[] { "configure", "write", "reload", "copy", "erase" };
            return privilegedCommands.Contains(command.ToLower());
        }

        public bool RequiresConfigurationMode(string command)
        {
            var configCommands = new[] { "hostname", "interface", "router", "vlan", "ip route" };
            return configCommands.Any(cmd => command.ToLower().StartsWith(cmd));
        }

        public string GetCommandSyntax(string command)
        {
            return command.ToLower() switch
            {
                "show" => "show <option>",
                "configure" => "configure terminal",
                "interface" => "interface <interface-name>",
                "router" => "router <protocol>",
                "ip" => "ip <option>",
                "vlan" => "vlan <vlan-id>",
                "ping" => "ping <ip-address>",
                _ => $"{command} - syntax not available"
            };
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
