using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Broadcom
{
    /// <summary>
    /// Broadcom-specific vendor capabilities implementation
    /// </summary>
    public class BroadcomVendorCapabilities : IVendorCapabilities
    {
        private readonly NetworkDevice _device;

        public BroadcomVendorCapabilities(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public string GetRunningConfiguration()
        {
            return "! Running configuration for " + _device.Name;
        }

        public string GetStartupConfiguration()
        {
            return "! Startup configuration for " + _device.Name;
        }

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
            var broadcomModes = new[] { "user", "privileged", "config", "interface", "vlan" };
            return broadcomModes.Contains(mode.ToLower());
        }

        public IEnumerable<string> GetAvailableModes()
        {
            return new[] { "user", "privileged", "config", "interface", "vlan" };
        }

        public string FormatCommandOutput(string command, object? data = null)
        {
            return data?.ToString() ?? "";
        }

        public string GetVendorErrorMessage(string errorType, string? context = null)
        {
            return errorType.ToLower() switch
            {
                "invalid_command" => "% Invalid command.",
                "incomplete_command" => "% Incomplete command.",
                "invalid_parameter" => "% Invalid parameter.",
                "invalid_mode" => "% Command not available in current mode.",
                "permission_denied" => "% Permission denied.",
                "syntax_error" => "% Syntax error.",
                _ => $"% Error: {errorType}"
            };
        }

        public string FormatInterfaceName(string interfaceName)
        {
            return interfaceName
                .Replace("gi", "GigabitEthernet")
                .Replace("fa", "FastEthernet")
                .Replace("eth", "Ethernet")
                .Replace("vlan", "Vlan");
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            if (commandParts.Length == 0)
                return false;

            var firstCommand = commandParts[0].ToLower();
            var currentMode = _device.GetCurrentMode().ToLower();

            return (currentMode, firstCommand) switch
            {
                ("user", "enable" or "ping" or "show" or "exit") => true,
                ("privileged", "configure" or "show" or "ping" or "write" or "reload" or "disable" or "exit") => true,
                ("config", "interface" or "vlan" or "hostname" or "exit") => true,
                ("interface", "ip" or "shutdown" or "no" or "description" or "exit") => true,
                ("vlan", "name" or "exit") => true,
                _ => false
            };
        }

        public bool SupportsFeature(string feature)
        {
            var broadcomFeatures = new[] { "vlan_support", "interface_configuration", "ip_routing", "stp" };
            return broadcomFeatures.Contains(feature.ToLower());
        }

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
                iface.IpAddress = null;
                iface.SubnetMask = null;
                return true;
            }
            return false;
        }

        public bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                if (direction.ToLower() == "in")
                    iface.IncomingAccessList = aclNumber;
                else if (direction.ToLower() == "out")
                    iface.OutgoingAccessList = aclNumber;
                return true;
            }
            return false;
        }

        public bool RemoveAccessGroup(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.IncomingAccessList = null;
                iface.OutgoingAccessList = null;
                return true;
            }
            return false;
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

        public bool CreateOrSelectVlan(int vlanId)
        {
            return vlanId >= 1 && vlanId <= 4094;
        }

        public bool InitializeOspf(int processId)
        {
            return processId >= 1 && processId <= 65535;
        }

        public bool InitializeBgp(int asNumber)
        {
            return asNumber >= 1 && asNumber <= 65535;
        }

        public bool InitializeRip()
        {
            return true;
        }

        public bool InitializeEigrp(int asNumber)
        {
            return asNumber >= 1 && asNumber <= 65535;
        }

        public bool SetCurrentRouterProtocol(string protocol)
        {
            return true;
        }

        public bool AddAclEntry(int aclNumber, object aclEntry)
        {
            return true;
        }

        public bool SetCurrentAclNumber(int aclNumber)
        {
            return true;
        }

        public int GetCurrentAclNumber()
        {
            return 0;
        }

        public bool AppendToRunningConfig(string configLine)
        {
            return true;
        }

        public bool AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            var iface = _device.GetInterface(interfaceName);
            return iface != null && vlanId >= 1 && vlanId <= 4094;
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

        public bool SetStpMode(string mode)
        {
            return true;
        }

        public bool SetStpVlanPriority(int vlanId, int priority)
        {
            return true;
        }

        public bool SetStpPriority(int priority)
        {
            return true;
        }

        public bool EnablePortfast(string interfaceName)
        {
            return true;
        }

        public bool DisablePortfast(string interfaceName)
        {
            return true;
        }

        public bool EnablePortfastDefault()
        {
            return true;
        }

        public bool EnableBpduGuard(string interfaceName)
        {
            return true;
        }

        public bool DisableBpduGuard(string interfaceName)
        {
            return true;
        }

        public bool EnableBpduGuardDefault()
        {
            return true;
        }

        public bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            return true;
        }

        public bool EnableCdpGlobal()
        {
            return true;
        }

        public bool DisableCdpGlobal()
        {
            return true;
        }

        public bool EnableCdpInterface(string interfaceName)
        {
            return true;
        }

        public bool DisableCdpInterface(string interfaceName)
        {
            return true;
        }

        public bool SetCdpTimer(int seconds)
        {
            return true;
        }

        public bool SetCdpHoldtime(int seconds)
        {
            return true;
        }

        public bool SetHostname(string hostname)
        {
            _device.SetHostname(hostname);
            return true;
        }

        public bool SetInterfaceDescription(string interfaceName, string description)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.Description = description;
                return true;
            }
            return false;
        }

        public bool SetSwitchportMode(string interfaceName, string mode)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.SwitchportMode = mode;
                return true;
            }
            return false;
        }

        public bool SetInterfaceVlan(string interfaceName, int vlanId)
        {
            return true;
        }

        public bool SetCurrentInterface(string interfaceName)
        {
            _device.SetCurrentInterface(interfaceName);
            return true;
        }

        public bool SetInterfaceState(string interfaceName, string state)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.IsUp = state.ToLower() == "up";
                iface.IsShutdown = state.ToLower() == "down";
                return true;
            }
            return false;
        }

        public bool SetInterface(string interfaceName, string property, object value)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                return true;
            }
            return false;
        }

        public bool SaveConfiguration()
        {
            return true;
        }

        public bool ReloadDevice()
        {
            return true;
        }


    }
} 
