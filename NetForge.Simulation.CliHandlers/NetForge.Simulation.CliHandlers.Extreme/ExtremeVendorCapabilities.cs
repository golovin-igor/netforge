using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.CliHandlers.Extreme
{
    public class ExtremeVendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));

        public string GetRunningConfiguration()
        {
            var interfaces = _device.GetAllInterfaces();
            var vlans = _device.GetAllVlans();

            var config = "!\n";
            config += $"hostname {_device.GetHostname()}\n";
            config += "!\n";

            foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
            {
                config += $"vlan {vlan.Id}\n";
                if (!string.IsNullOrEmpty(vlan.Name))
                    config += $" name {vlan.Name}\n";
                config += "!\n";
            }

            foreach (var iface in interfaces.Values.OrderBy(i => i.Name))
            {
                config += $"interface {iface.Name}\n";
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    config += $" ip address {iface.IpAddress} {iface.SubnetMask}\n";
                if (iface.IsShutdown)
                    config += " shutdown\n";
                config += "!\n";
            }

            config += "end\n";
            return config;
        }

        public string GetStartupConfiguration()
        {
            return GetRunningConfiguration();
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
            var extremeModes = new[] { "user", "privileged", "config", "interface", "vlan" };
            return extremeModes.Contains(mode.ToLower());
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

        public string GetInterfaceStatus(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface == null) return "Interface not found";

            return $"Interface {iface.Name} is {(iface.IsUp ? "up" : "down")}, " +
                   $"line protocol is {(iface.IsUp && !iface.IsShutdown ? "up" : "down")}";
        }

        public bool SetInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface == null) return false;

            iface.IpAddress = ipAddress;
            iface.SubnetMask = subnetMask;
            _device.ForceUpdateConnectedRoutes();
            return true;
        }

        public bool SetHostname(string hostname)
        {
            _device.SetHostname(hostname);
            return true;
        }

        public bool CreateOrSelectVlan(int vlanId)
        {
            var vlans = _device.GetAllVlans();
            if (!vlans.ContainsKey(vlanId))
            {
                vlans[vlanId] = new VlanConfig(vlanId);
            }
            return true;
        }

        public bool SetVlanName(int vlanId, string name)
        {
            var vlans = _device.GetAllVlans();
            if (vlans.ContainsKey(vlanId))
            {
                vlans[vlanId].Name = name;
                return true;
            }
            return false;
        }

        public bool ShutdownInterface(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface == null) return false;

            iface.IsShutdown = true;
            iface.IsUp = false;
            return true;
        }

        public bool NoShutdownInterface(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface == null) return false;

            iface.IsShutdown = false;
            iface.IsUp = true;
            return true;
        }

        public bool SetCurrentInterface(string interfaceName)
        {
            _device.SetCurrentInterface(interfaceName);
            return true;
        }

        public string GetCurrentInterface()
        {
            return _device.GetCurrentInterface();
        }

        public bool SaveConfiguration()
        {
            _device.AddLogEntry("Configuration saved");
            return true;
        }

        public bool ReloadDevice()
        {
            _device.AddLogEntry("Device reloading...");
            return true;
        }

        public bool SupportsFeature(string feature)
        {
            var supportedFeatures = new[] { "vlan_support", "interface_configuration", "ip_routing" };
            return supportedFeatures.Contains(feature);
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            return commandParts.Length > 0;
        }

        public string FormatInterfaceName(string interfaceName)
        {
            return interfaceName;
        }

        // Interface configuration methods
        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            return SetInterfaceIp(interfaceName, ipAddress, subnetMask);
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
            if (shutdown)
                return ShutdownInterface(interfaceName);
            else
                return NoShutdownInterface(interfaceName);
        }

        // Routing protocol methods
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
            if (_device.GetEigrpConfiguration() == null)
                _device.SetEigrpConfiguration(new EigrpConfig(asNumber));
            return true;
        }

        public bool SetCurrentRouterProtocol(string protocol)
        {
            return true;
        }

        // ACL methods
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

        // VLAN methods
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

        // STP methods
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

        // Port Channel methods
        public bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            return true;
        }

        // CDP methods
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

        // Interface state methods
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
            return AddInterfaceToVlan(interfaceName, vlanId);
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
            if (iface == null)
                return false;

            switch (property.ToLower())
            {
                case "description":
                    iface.Description = value.ToString() ?? "";
                    break;
                case "shutdown":
                    iface.IsShutdown = Convert.ToBoolean(value);
                    iface.IsUp = !iface.IsShutdown;
                    break;
            }
            return true;
        }
    }
}
