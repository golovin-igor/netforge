using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Huawei
{
    /// <summary>
    /// Huawei-specific vendor capabilities implementation
    /// </summary>
    public class HuaweiVendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));

        public string GetRunningConfiguration()
        {
            return "# Running configuration for " + _device.Name;
        }

        public string GetStartupConfiguration()
        {
            return "# Startup configuration for " + _device.Name;
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
            var huaweiModes = new[] { "user", "privileged", "config", "interface", "router", "vlan" };
            return huaweiModes.Contains(mode.ToLower());
        }

        public IEnumerable<string> GetAvailableModes()
        {
            return new[] { "user", "privileged", "config", "interface", "router", "vlan" };
        }

        public string FormatCommandOutput(string command, object? data = null)
        {
            return data?.ToString() ?? "";
        }

        public string GetVendorErrorMessage(string errorType, string? context = null)
        {
            return errorType.ToLower() switch
            {
                "invalid_command" => "% Unrecognized command found at '^' position.",
                "incomplete_command" => "% Incomplete command found at '^' position.",
                "invalid_parameter" => "% Invalid parameter found at '^' position.",
                "invalid_mode" => "% Command not available in current mode.",
                "permission_denied" => "% Permission denied.",
                "syntax_error" => "% Syntax error found at '^' position.",
                _ => $"% Error: {errorType}"
            };
        }

        public string FormatInterfaceName(string interfaceName)
        {
            return interfaceName
                .Replace("gi", "GigabitEthernet")
                .Replace("fe", "FastEthernet")
                .Replace("eth", "Ethernet")
                .Replace("vlan", "Vlanif");
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            if (commandParts.Length == 0)
                return false;

            var firstCommand = commandParts[0].ToLower();
            var currentMode = _device.GetCurrentMode().ToLower();

            return (currentMode, firstCommand) switch
            {
                ("user", "system-view" or "ping" or "display" or "quit") => true,
                ("privileged", "system-view" or "ping" or "display" or "save" or "reboot" or "quit") => true,
                ("config", "interface" or "router" or "vlan" or "sysname" or "quit") => true,
                ("interface", "ip" or "shutdown" or "undo" or "description" or "quit") => true,
                ("router", "network" or "area" or "quit") => true,
                ("vlan", "name" or "quit") => true,
                _ => false
            };
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

        public bool SupportsFeature(string feature)
        {
            return true;
        }

        // ICommandFormatter interface implementation
        public string PreprocessCommand(string command)
        {
            return command; // No preprocessing by default
        }

        public string PostprocessOutput(string output)
        {
            return output; // No postprocessing by default
        }

        public string RenderConfiguration(object configData)
        {
            return configData?.ToString() ?? "";
        }
    }
}
