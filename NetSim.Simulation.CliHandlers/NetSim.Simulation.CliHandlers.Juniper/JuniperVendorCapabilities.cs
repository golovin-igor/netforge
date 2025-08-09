using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Protocols.Routing;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Juniper-specific vendor capabilities implementation
    /// </summary>
    public class JuniperVendorCapabilities : IVendorCapabilities
    {
        private readonly NetworkDevice _device;

        public JuniperVendorCapabilities(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public string VendorName => "Juniper";
        public string VendorVersion => "JUNOS 20.x";
        public string DefaultConfigurationMode => "configuration";
        public string DefaultPrivilegedMode => "operational";
        public string DefaultUserMode => "operational";

        public bool SupportsFeature(string feature)
        {
            return feature.ToLower() switch
            {
                "vlan" => true,
                "routing" => true,
                "switching" => true,
                "mpls" => true,
                "bgp" => true,
                "ospf" => true,
                "isis" => true,
                "ldp" => true,
                "rsvp" => true,
                "lldp" => true,
                "lacp" => true,
                "snmp" => true,
                "ssh" => true,
                "commit" => true,
                "candidate-config" => true,
                _ => false
            };
        }

        public IEnumerable<string> GetSupportedModes()
        {
            return new[] { "operational", "configuration" };
        }

        public IEnumerable<string> GetSupportedCommands()
        {
            return new[]
            {
                "show", "configure", "set", "delete", "commit", "rollback", "exit", "ping",
                "edit", "up", "top", "run"
            };
        }

        public string GetPromptForMode(string mode)
        {
            var username = "admin";
            var hostname = _device.Name;
            return mode.ToLower() switch
            {
                "operational" => $"{username}@{hostname}> ",
                "configuration" => $"{username}@{hostname}# ",
                _ => $"{username}@{hostname}> "
            };
        }

        public bool IsValidTransition(string fromMode, string toMode)
        {
            return (fromMode.ToLower(), toMode.ToLower()) switch
            {
                ("operational", "configuration") => true,
                ("configuration", "operational") => true,
                _ => false
            };
        }

        public string GetModeDescription(string mode)
        {
            return mode.ToLower() switch
            {
                "operational" => "Operational mode",
                "configuration" => "Configuration mode",
                _ => $"Unknown mode: {mode}"
            };
        }

        public IEnumerable<string> GetCommandsForMode(string mode)
        {
            return mode.ToLower() switch
            {
                "operational" => new[] { "show", "ping", "configure", "exit" },
                "configuration" => new[] { "set", "delete", "show", "commit", "rollback", "exit", "edit", "up", "top" },
                _ => Array.Empty<string>()
            };
        }

        public bool RequiresPrivilegedMode(string command)
        {
            var privilegedCommands = new[] { "configure", "commit", "rollback" };
            return privilegedCommands.Contains(command.ToLower());
        }

        public bool RequiresConfigurationMode(string command)
        {
            var configCommands = new[] { "set", "delete", "commit", "rollback", "edit" };
            return configCommands.Contains(command.ToLower());
        }

        public string GetCommandSyntax(string command)
        {
            return command.ToLower() switch
            {
                "show" => "show <option>",
                "configure" => "configure",
                "set" => "set <hierarchy> <value>",
                "delete" => "delete <hierarchy>",
                "commit" => "commit",
                "rollback" => "rollback <number>",
                "ping" => "ping <ip-address>",
                _ => $"{command} - syntax not available"
            };
        }

        // Required interface methods
        public string GetRunningConfiguration()
        {
            var interfaces = _device.GetAllInterfaces();
            var vlans = _device.GetAllVlans();
            
            var config = "# Juniper Configuration\n";
            config += $"set system host-name {_device.GetHostname()}\n";
            
            foreach (var vlan in vlans.Values.OrderBy(v => v.Id))
            {
                config += $"set vlans vlan{vlan.Id} vlan-id {vlan.Id}\n";
                if (!string.IsNullOrEmpty(vlan.Name))
                    config += $"set vlans vlan{vlan.Id} description {vlan.Name}\n";
            }
            
            foreach (var iface in interfaces.Values.OrderBy(i => i.Name))
            {
                config += $"set interfaces {iface.Name} unit 0\n";
                if (!string.IsNullOrEmpty(iface.IpAddress))
                    config += $"set interfaces {iface.Name} unit 0 family inet address {iface.IpAddress}/{_device.MaskToCidr(iface.SubnetMask)}\n";
                if (iface.IsShutdown)
                    config += $"set interfaces {iface.Name} disable\n";
            }
            
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
            var juniperModes = new[] { "operational", "configuration", "shell" };
            return juniperModes.Contains(mode.ToLower());
        }

        public IEnumerable<string> GetAvailableModes()
        {
            return new[] { "operational", "configuration", "shell" };
        }

        public string FormatCommandOutput(string command, object? data = null)
        {
            return data?.ToString() ?? "";
        }

        public string GetVendorErrorMessage(string errorType, string? context = null)
        {
            return errorType.ToLower() switch
            {
                "invalid_command" => "% Unknown command.",
                "incomplete_command" => "% Incomplete command.",
                "invalid_parameter" => "% Invalid parameter.",
                "invalid_mode" => "% Command not available in current mode.",
                "permission_denied" => "% Permission denied.",
                "syntax_error" => "syntax error.",
                _ => $"error: {errorType}"
            };
        }

        public string FormatInterfaceName(string interfaceName)
        {
            return interfaceName
                .Replace("gi", "ge-")
                .Replace("fe", "fe-")
                .Replace("xe", "xe-")
                .Replace("et", "et-");
        }

        public bool ValidateVendorSyntax(string[] commandParts, string command)
        {
            if (commandParts.Length == 0)
                return false;

            var firstCommand = commandParts[0].ToLower();
            var currentMode = _device.GetCurrentMode().ToLower();

            return (currentMode, firstCommand) switch
            {
                ("operational", "configure" or "show" or "ping" or "exit") => true,
                ("configuration", "set" or "delete" or "show" or "commit" or "rollback" or "exit") => true,
                _ => false
            };
        }

        // Interface configuration methods
        public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
        {
            var iface = _device.GetInterface(interfaceName);
            if (iface != null)
            {
                iface.IpAddress = ipAddress;
                iface.SubnetMask = subnetMask;
                _device.ForceUpdateConnectedRoutes();
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

        // VLAN management methods
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

        // CDP methods (LLDP in Juniper)
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

        // Basic configuration methods
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
            return AddInterfaceToVlan(interfaceName, vlanId);
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

        public bool SaveConfiguration()
        {
            _device.AddLogEntry("Configuration committed");
            return true;
        }

        public bool ReloadDevice()
        {
            _device.AddLogEntry("Device restarting...");
            return true;
        }
    }
}
