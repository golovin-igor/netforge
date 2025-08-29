using NetForge.Simulation.Common;
using System.Text;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Aruba
{
    /// <summary>
    /// Vendor-specific capabilities for Aruba devices
    /// </summary>
    public class ArubaVendorCapabilities(INetworkDevice device) : IVendorCapabilities
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));

        public string GetRunningConfiguration()
        {
            var config = new StringBuilder();
            config.AppendLine("! Aruba Switch Configuration");
            config.AppendLine("!");
            config.AppendLine($"hostname {_device.Name}");
            config.AppendLine("!");

            // Add interface configurations
            foreach (var iface in _device.GetAllInterfaces().Values)
            {
                config.AppendLine($"interface {FormatInterfaceName(iface.Name)}");
                if (!string.IsNullOrEmpty(iface.IpAddress))
                {
                    config.AppendLine($" ip address {iface.IpAddress} {iface.SubnetMask}");
                }
                if (iface.IsShutdown)
                {
                    config.AppendLine(" shutdown");
                }
                if (!string.IsNullOrEmpty(iface.Description))
                {
                    config.AppendLine($" description {iface.Description}");
                }
                config.AppendLine("!");
            }

            config.AppendLine("end");
            return config.ToString();
        }

        public string GetStartupConfiguration()
        {
            return GetRunningConfiguration(); // Simplified - in real device these could differ
        }

        public bool SetHostname(string hostname)
        {
            try
            {
                _device.AddLogEntry($"Hostname changed to {hostname}");
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error setting hostname: {ex.Message}");
                return false;
            }
        }

        public bool SetInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.IpAddress = ipAddress;
                    iface.SubnetMask = subnetMask;
                    _device.ForceUpdateConnectedRoutes();
                    _device.ParentNetwork?.UpdateProtocols();
                    _device.AddLogEntry($"Interface {interfaceName} IP set to {ipAddress}/{subnetMask}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error setting interface IP: {ex.Message}");
                return false;
            }
        }

        public bool SetInterfaceDescription(string interfaceName, string description)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.Description = description;
                    _device.AddLogEntry($"Interface {interfaceName} description set to: {description}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error setting interface description: {ex.Message}");
                return false;
            }
        }

        public bool ShutdownInterface(string interfaceName)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.IsShutdown = true;
                    iface.IsUp = false;
                    _device.ParentNetwork?.UpdateProtocols();
                    _device.AddLogEntry($"Interface {interfaceName} shutdown");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error shutting down interface: {ex.Message}");
                return false;
            }
        }

        public bool NoShutdownInterface(string interfaceName)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.IsShutdown = false;
                    iface.IsUp = true;
                    _device.ParentNetwork?.UpdateProtocols();
                    _device.AddLogEntry($"Interface {interfaceName} no shutdown");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error bringing up interface: {ex.Message}");
                return false;
            }
        }

        public string GetInterfaceConfiguration(string interfaceName)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface == null)
                return $"% Interface {interfaceName} not found";

            var config = new StringBuilder();
            config.AppendLine($"interface {FormatInterfaceName(interfaceName)}");

            if (!string.IsNullOrEmpty(iface.Description))
                config.AppendLine($" description {iface.Description}");

            if (!string.IsNullOrEmpty(iface.IpAddress))
                config.AppendLine($" ip address {iface.IpAddress} {iface.SubnetMask}");

            if (iface.IsShutdown)
                config.AppendLine(" shutdown");

            return config.ToString();
        }

        public string FormatInterfaceName(string interfaceName)
        {
            // Aruba uses format like "1/1/1" for stacked switches or "1" for simple ports
            if (interfaceName.StartsWith("eth"))
            {
                var number = interfaceName.Replace("eth", "");
                return number;
            }
            return interfaceName;
        }

        public string GetVendorSpecificError(string errorType)
        {
            return errorType switch
            {
                "invalid_command" => "Invalid input -> {0}",
                "incomplete_command" => "% Incomplete command",
                "invalid_interface" => "% Invalid interface",
                "access_denied" => "% Access denied",
                "invalid_vlan" => "% Invalid VLAN",
                "invalid_ip" => "% Invalid IP address",
                _ => "% Error"
            };
        }

        // Required interface methods
        public bool SetInterfaceVlan(string interfaceName, int vlanId)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.VlanId = vlanId;
                    _device.AddLogEntry($"Interface {interfaceName} VLAN set to {vlanId}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error setting interface VLAN: {ex.Message}");
                return false;
            }
        }

        public bool SetCurrentInterface(string interfaceName)
        {
            // Set current interface for configuration context
            _currentInterface = interfaceName;
            _device.AddLogEntry($"Current interface set to {interfaceName}");
            return true;
        }

        public bool SetInterfaceState(string interfaceName, string state)
        {
            return state.ToLower() switch
            {
                "up" => NoShutdownInterface(interfaceName),
                "down" => ShutdownInterface(interfaceName),
                _ => false
            };
        }

        public bool SetInterface(string interfaceName, string property, object value)
        {
            // Generic interface property setter
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface == null) return false;

                switch (property.ToLower())
                {
                    case "description":
                        return SetInterfaceDescription(interfaceName, value.ToString() ?? "");
                    case "shutdown":
                        return bool.Parse(value.ToString() ?? "false") ? ShutdownInterface(interfaceName) : NoShutdownInterface(interfaceName);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool SaveConfiguration()
        {
            // Aruba configuration saving
            _device.AddLogEntry("Configuration saved to startup-config");
            return true;
        }

        public bool LoadConfiguration()
        {
            // Aruba configuration loading
            _device.AddLogEntry("Configuration loaded from startup-config");
            return true;
        }

        // Additional required interface methods
        public void SetDeviceMode(string mode)
        {
            _device.SetCurrentMode(mode);
        }

        public string GetDeviceMode()
        {
            return _device.GetCurrentMode();
        }

        public bool SupportsMode(string mode)
        {
            var supportedModes = new[] { "user", "privileged", "config", "interface", "vlan" };
            return supportedModes.Contains(mode.ToLower());
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
            return GetVendorSpecificError(errorType);
        }

        public bool SupportsFeature(string feature)
        {
            var supportedFeatures = new[] { "vlan_support", "interface_configuration", "ip_routing", "access_lists", "port_security", "spanning_tree", "link_aggregation" };
            return supportedFeatures.Contains(feature);
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            // Basic Aruba syntax validation
            return commandParts.Length > 0;
        }

        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            return SetInterfaceIp(interfaceName, ipAddress, subnetMask);
        }

        public bool RemoveInterfaceIp(string interfaceName)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.IpAddress = null;
                    iface.SubnetMask = null;
                    _device.ForceUpdateConnectedRoutes();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction)
        {
            _device.AddLogEntry($"Applied ACL {aclNumber} to interface {interfaceName} {direction}");
            return true;
        }

        public bool RemoveAccessGroup(string interfaceName)
        {
            _device.AddLogEntry($"Removed ACL from interface {interfaceName}");
            return true;
        }

        public bool SetInterfaceShutdown(string interfaceName, bool shutdown)
        {
            return shutdown ? ShutdownInterface(interfaceName) : NoShutdownInterface(interfaceName);
        }

        public bool CreateOrSelectVlan(int vlanId)
        {
            _device.AddLogEntry($"Created/selected VLAN {vlanId}");
            return true;
        }

        public bool InitializeOspf(int processId)
        {
            _device.AddLogEntry($"Initialized OSPF process {processId}");
            return true;
        }

        public bool InitializeBgp(int asNumber)
        {
            _device.AddLogEntry($"Initialized BGP AS {asNumber}");
            return true;
        }

        public bool InitializeRip()
        {
            _device.AddLogEntry("Initialized RIP");
            return true;
        }

        public bool InitializeEigrp(int asNumber)
        {
            _device.AddLogEntry($"Initialized EIGRP AS {asNumber}");
            return true;
        }

        public bool SetCurrentRouterProtocol(string protocol)
        {
            _device.AddLogEntry($"Set current router protocol to {protocol}");
            return true;
        }

        public bool AddAclEntry(int aclNumber, object aclEntry)
        {
            _device.AddLogEntry($"Added ACL entry to access-list {aclNumber}");
            return true;
        }

        public bool SetCurrentAclNumber(int aclNumber)
        {
            _device.AddLogEntry($"Set current ACL number to {aclNumber}");
            return true;
        }

        public int GetCurrentAclNumber()
        {
            return 1; // Default ACL number
        }

        public bool AppendToRunningConfig(string configLine)
        {
            _device.AddLogEntry($"Added to running config: {configLine}");
            return true;
        }

        public bool AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            return SetInterfaceVlan(interfaceName, vlanId);
        }

        public bool VlanExists(int vlanId)
        {
            return vlanId >= 1 && vlanId <= 4094;
        }

        public bool SetVlanName(int vlanId, string name)
        {
            _device.AddLogEntry($"Set VLAN {vlanId} name to {name}");
            return true;
        }

        public bool SetStpMode(string mode)
        {
            _device.AddLogEntry($"Set STP mode to {mode}");
            return true;
        }

        public bool SetStpVlanPriority(int vlanId, int priority)
        {
            _device.AddLogEntry($"Set STP priority {priority} for VLAN {vlanId}");
            return true;
        }

        public bool SetStpPriority(int priority)
        {
            _device.AddLogEntry($"Set global STP priority to {priority}");
            return true;
        }

        public bool EnablePortfast(string interfaceName)
        {
            _device.AddLogEntry($"Enabled PortFast on {interfaceName}");
            return true;
        }

        public bool DisablePortfast(string interfaceName)
        {
            _device.AddLogEntry($"Disabled PortFast on {interfaceName}");
            return true;
        }

        public bool EnablePortfastDefault()
        {
            _device.AddLogEntry("Enabled PortFast default");
            return true;
        }

        public bool EnableBpduGuard(string interfaceName)
        {
            _device.AddLogEntry($"Enabled BPDU Guard on {interfaceName}");
            return true;
        }

        public bool DisableBpduGuard(string interfaceName)
        {
            _device.AddLogEntry($"Disabled BPDU Guard on {interfaceName}");
            return true;
        }

        public bool EnableBpduGuardDefault()
        {
            _device.AddLogEntry("Enabled BPDU Guard default");
            return true;
        }

        public bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
        {
            _device.AddLogEntry($"Created/updated port-channel {channelId} with interface {interfaceName} in {mode} mode");
            return true;
        }

        public bool EnableCdpGlobal()
        {
            _device.AddLogEntry("Enabled CDP globally");
            return true;
        }

        public bool DisableCdpGlobal()
        {
            _device.AddLogEntry("Disabled CDP globally");
            return true;
        }

        public bool EnableCdpInterface(string interfaceName)
        {
            _device.AddLogEntry($"Enabled CDP on {interfaceName}");
            return true;
        }

        public bool DisableCdpInterface(string interfaceName)
        {
            _device.AddLogEntry($"Disabled CDP on {interfaceName}");
            return true;
        }

        public bool SetCdpTimer(int seconds)
        {
            _device.AddLogEntry($"Set CDP timer to {seconds} seconds");
            return true;
        }

        public bool SetCdpHoldtime(int seconds)
        {
            _device.AddLogEntry($"Set CDP holdtime to {seconds} seconds");
            return true;
        }

        public bool SetSwitchportMode(string interfaceName, string mode)
        {
            _device.AddLogEntry($"Set {interfaceName} switchport mode to {mode}");
            return true;
        }

        public bool ReloadDevice()
        {
            _device.AddLogEntry("Device reloading...");
            return true;
        }

        // VLAN Port Management methods (migrated from old handlers)
        public bool AddTaggedPort(int vlanId, string interfaceName)
        {
            try
            {
                var vlans = _device.GetAllVlans();
                if (!vlans.ContainsKey(vlanId))
                {
                    _device.AddLogEntry($"VLAN {vlanId} does not exist");
                    return false;
                }

                var vlan = vlans[vlanId];
                if (!vlan.TaggedPorts.Contains(interfaceName))
                {
                    vlan.TaggedPorts.Add(interfaceName);
                    _device.AddLogEntry($"Added {interfaceName} as tagged port to VLAN {vlanId}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error adding tagged port: {ex.Message}");
                return false;
            }
        }

        public bool AddUntaggedPort(int vlanId, string interfaceName)
        {
            try
            {
                var vlans = _device.GetAllVlans();
                if (!vlans.ContainsKey(vlanId))
                {
                    _device.AddLogEntry($"VLAN {vlanId} does not exist");
                    return false;
                }

                var vlan = vlans[vlanId];
                if (!vlan.UntaggedPorts.Contains(interfaceName))
                {
                    vlan.UntaggedPorts.Add(interfaceName);
                    _device.AddLogEntry($"Added {interfaceName} as untagged port to VLAN {vlanId}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error adding untagged port: {ex.Message}");
                return false;
            }
        }

        public bool RemoveTaggedPort(int vlanId, string interfaceName)
        {
            try
            {
                var vlans = _device.GetAllVlans();
                if (!vlans.ContainsKey(vlanId))
                {
                    return false;
                }

                var vlan = vlans[vlanId];
                var removed = vlan.TaggedPorts.Remove(interfaceName);
                if (removed)
                {
                    _device.AddLogEntry($"Removed {interfaceName} as tagged port from VLAN {vlanId}");
                }
                return removed;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error removing tagged port: {ex.Message}");
                return false;
            }
        }

        public bool RemoveUntaggedPort(int vlanId, string interfaceName)
        {
            try
            {
                var vlans = _device.GetAllVlans();
                if (!vlans.ContainsKey(vlanId))
                {
                    return false;
                }

                var vlan = vlans[vlanId];
                var removed = vlan.UntaggedPorts.Remove(interfaceName);
                if (removed)
                {
                    _device.AddLogEntry($"Removed {interfaceName} as untagged port from VLAN {vlanId}");
                }
                return removed;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error removing untagged port: {ex.Message}");
                return false;
            }
        }

        // Static Route Management (migrated from old handlers)
        public bool AddStaticRoute(string network, string mask, string nextHop, int metric = 1)
        {
            try
            {
                _device.AddStaticRoute(network, mask, nextHop, metric);
                _device.AddLogEntry($"Added static route: {network}/{mask} via {nextHop}");
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error adding static route: {ex.Message}");
                return false;
            }
        }

        public bool RemoveStaticRoute(string network, string mask)
        {
            try
            {
                _device.RemoveStaticRoute(network, mask);
                _device.AddLogEntry($"Removed static route: {network}/{mask}");
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error removing static route: {ex.Message}");
                return false;
            }
        }

        // Interface Counter Management (migrated from old handlers)
        public bool ClearInterfaceCounters()
        {
            try
            {
                var interfaces = _device.GetAllInterfaces();
                foreach (var iface in interfaces.Values)
                {
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                }
                _device.AddLogEntry("All interface counters cleared");
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error clearing counters: {ex.Message}");
                return false;
            }
        }

        public bool ClearInterfaceCounters(string interfaceName)
        {
            try
            {
                var iface = _device.GetInterface(interfaceName);
                if (iface != null)
                {
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                    _device.AddLogEntry($"Interface {interfaceName} counters cleared");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error clearing interface counters: {ex.Message}");
                return false;
            }
        }

        // Configuration Management
        public bool SaveConfigurationToMemory()
        {
            try
            {
                // In Aruba, "write memory" saves the running config to startup config
                // Since NetworkDevice doesn't have a specific SaveConfig method,
                // we'll log the action and consider it successful
                _device.AddLogEntry("Configuration saved to startup-config (write memory)");
                return true;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error saving configuration: {ex.Message}");
                return false;
            }
        }

        // Current VLAN context management
                private int _currentVlanId = 1;
        private string _currentInterface = "";

        public bool SetCurrentVlan(int vlanId)
        {
            _currentVlanId = vlanId;
            return true;
        }

        public int GetCurrentVlan()
        {
            return _currentVlanId;
        }

        public string GetCurrentInterface()
        {
            return _currentInterface;
        }

        public void SetCurrentInterfaceContext(string interfaceName)
        {
            _currentInterface = interfaceName;
        }
    }
}
