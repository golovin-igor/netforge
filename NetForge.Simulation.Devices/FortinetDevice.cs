using NetForge.Simulation.CliHandlers.Fortinet;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.Devices
{
    /// <summary>
    /// Fortinet FortiOS device implementation
    /// </summary>
    public sealed class FortinetDevice : NetworkDevice
    {
        private int _currentVlanId = 0;
        private string _currentNeighborIp = "";
        private string _currentBgpNetwork = "";
        private string _currentStaticRoute = "";
        private int _currentStaticRouteId = 0;
        private Dictionary<int, (string Network, string Mask, string Gateway, string Device)> _pendingStaticRoutes = new Dictionary<int, (string, string, string, string)>();

        public FortinetDevice(string name) : base(name)
        {
            Vendor = "Fortinet";

            // Fortinet devices should start in Global mode, not User mode
            CurrentMode = DeviceMode.Global;

            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // FortiGate uses VLAN 1 by default
            Vlans[1] = new VlanConfig(1, "default");

            // Auto-register protocols using the new plugin-based discovery service
            // This will discover and register protocols that support the "Fortinet" vendor
            AutoRegisterProtocols();
        }

        protected override void InitializeDefaultInterfaces()
        {
            Interfaces["port1"] = new InterfaceConfig("port1", this);
            Interfaces["port2"] = new InterfaceConfig("port2", this);
            Interfaces["port3"] = new InterfaceConfig("port3", this);
            Interfaces["port4"] = new InterfaceConfig("port4", this);
            Interfaces["internal"] = new InterfaceConfig("internal", this);
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Fortinet handlers to ensure they are available for tests
            var registry = new FortinetHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            return base.CurrentMode switch
            {
                DeviceMode.GlobalConfig => $"{Hostname} (global) # ",
                DeviceMode.SystemInterface => $"{Hostname} (interface) # ",
                DeviceMode.RouterOspfFortinet => $"{Hostname} (ospf) # ",
                DeviceMode.RouterBgpFortinet => $"{Hostname} (bgp) # ",
                DeviceMode.Interface => $"{Hostname} ({base.CurrentInterface}) # ",
                DeviceMode.Vlan => $"{Hostname} (vlan) # ",
                DeviceMode.Global => $"{Hostname} # ",
                _ => $"{Hostname} # "
            };
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            // Use the command handler manager for all command processing
            if (CommandManager != null)
            {
                var result = await CommandManager.ProcessCommandAsync(command);

                // If command was handled, return the result
                if (result != null)
                {
                    // Check if result already ends with prompt
                    var prompt = GetPrompt();
                    if (result.Output.EndsWith(prompt))
                    {
                        return result.Output;
                    }
                    else
                    {
                        return result.Output + prompt;
                    }

                }
            }

            // If no handler found, return FortiOS error
            return "Invalid command" + GetPrompt();
        }

        // Helper methods for command handlers
        public new string GetCurrentInterface() => base.CurrentInterface;
        public new void SetCurrentInterface(string iface) => base.CurrentInterface = iface;

        public int GetCurrentVlanId() => _currentVlanId;
        public void SetCurrentVlanId(int vlanId) => _currentVlanId = vlanId;

        public string GetMode() => base.CurrentMode.ToModeString();
        public new void SetCurrentMode(string mode) => base.CurrentMode = DeviceModeExtensions.FromModeString(mode);

        // Add missing methods for command handlers
        public void AppendToRunningConfig(string line)
        {
            RunningConfig.AppendLine(line);
        }

        public void AddInterface(string interfaceName)
        {
            if (!Interfaces.ContainsKey(interfaceName))
            {
                Interfaces[interfaceName] = new InterfaceConfig(interfaceName, this);
            }
        }

        public void CreateOrSelectVlan(int vlanId)
        {
            if (!Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId] = new VlanConfig(vlanId);
            }
            _currentVlanId = vlanId;
        }

        public void AddInterfaceToVlan(string interfaceName, int vlanId)
        {
            if (Vlans.ContainsKey(vlanId))
            {
                Vlans[vlanId].Interfaces.Add(interfaceName);
            }
        }

        public void SetCurrentVlanName(string name)
        {
            if (_currentVlanId > 0 && Vlans.ContainsKey(_currentVlanId))
            {
                Vlans[_currentVlanId].Name = name;
            }
        }

        public void UpdateProtocols()
        {
            // Update routing protocols if needed
            ParentNetwork?.UpdateProtocols();
        }

        public void UpdateConnectedRoutesPublic()
        {
            UpdateConnectedRoutes();
        }

        public new Dictionary<string, InterfaceConfig> GetAllInterfaces() => Interfaces;
        public new OspfConfig? GetOspfConfiguration() => OspfConfig;
        public new BgpConfig? GetBgpConfiguration() => BgpConfig;
        public new RipConfig? GetRipConfiguration() => RipConfig;

        public string GetRunningConfig()
        {
            return RunningConfig.ToString();
        }

        public void InitializeOspf(int processId)
        {
            if (OspfConfig == null)
            {
                OspfConfig = new OspfConfig(processId);
            }
        }

        public void InitializeBgp(int asNumber)
        {
            if (BgpConfig == null)
            {
                BgpConfig = new BgpConfig(asNumber);
            }
        }

        public void InitializeRip()
        {
            if (RipConfig == null)
            {
                RipConfig = new RipConfig();
            }
        }

        public void SetCurrentBgpNeighbor(string neighborIp)
        {
            _currentNeighborIp = neighborIp;
        }

        public string GetCurrentBgpNeighbor()
        {
            return _currentNeighborIp;
        }

        public void SetCurrentBgpNetwork(string networkId)
        {
            _currentBgpNetwork = networkId;
        }

        public string GetCurrentBgpNetwork()
        {
            return _currentBgpNetwork;
        }

        public void SetCurrentStaticRoute(string routeId)
        {
            _currentStaticRoute = routeId;
            if (int.TryParse(routeId, out int id))
            {
                _currentStaticRouteId = id;
                if (!_pendingStaticRoutes.ContainsKey(id))
                {
                    _pendingStaticRoutes[id] = ("", "", "", "");
                }
            }
        }

        public string GetCurrentStaticRoute()
        {
            return _currentStaticRoute;
        }

        public void SetStaticRouteDst(int routeId, string network, string mask)
        {
            if (_pendingStaticRoutes.ContainsKey(routeId))
            {
                var current = _pendingStaticRoutes[routeId];
                _pendingStaticRoutes[routeId] = (network, mask, current.Gateway, current.Device);
            }
        }

        public void SetStaticRouteGateway(int routeId, string gateway)
        {
            if (_pendingStaticRoutes.ContainsKey(routeId))
            {
                var current = _pendingStaticRoutes[routeId];
                _pendingStaticRoutes[routeId] = (current.Network, current.Mask, gateway, current.Device);
            }
        }

        public void SetStaticRouteDevice(int routeId, string device)
        {
            if (_pendingStaticRoutes.ContainsKey(routeId))
            {
                var current = _pendingStaticRoutes[routeId];
                _pendingStaticRoutes[routeId] = (current.Network, current.Mask, current.Gateway, device);
            }
        }

        public void AddStaticRouteToTable()
        {
            if (_currentStaticRouteId > 0 && _pendingStaticRoutes.ContainsKey(_currentStaticRouteId))
            {
                var route = _pendingStaticRoutes[_currentStaticRouteId];
                if (!string.IsNullOrEmpty(route.Network) && !string.IsNullOrEmpty(route.Gateway))
                {
                    var routeEntry = new Route(route.Network, route.Mask, route.Gateway, route.Device, "Static")
                    {
                        AdminDistance = 1,
                        Metric = 0
                    };

                    RoutingTable.Add(routeEntry);
                }
            }
        }
    }
}
