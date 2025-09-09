using System.Text;
using NetForge.Simulation.CliHandlers.Dell;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Dell EMC OS10 device implementation
    /// </summary>
    public sealed class DellDevice : NetworkDevice
    {
        public override string DeviceType => "Switch";
        
        // Device state is now managed by command handlers

        public DellDevice(string name) : base(name, "Dell")
        {
            // Add default VLAN 1
            AddVlan(1, new VlanConfig(1, "default"));
            InitializeDefaultInterfaces();
            RegisterDeviceSpecificHandlers();

            // Protocol registration is now handled by the vendor registry system
        }

        protected override void InitializeDefaultInterfaces()
        {
            // Add default interfaces for a Dell switch (OS10 style)
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 24; j++)
                {
                    AddInterface($"ethernet 1/{i}/{j}", new InterfaceConfig($"ethernet 1/{i}/{j}", this));
                }
            }
            AddInterface("mgmt 1/1/1", new InterfaceConfig("mgmt 1/1/1", this));
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // Explicitly register Dell handlers to ensure they are available for tests
            var registry = new DellHandlerRegistry();
            registry.RegisterHandlers(CommandManager);
        }

        public override string GetPrompt()
        {
            var mode = GetCurrentModeEnum();
            var hostname = GetHostname();

            return mode switch
            {
                DeviceMode.User => $"{hostname}>",
                DeviceMode.Privileged => $"{hostname}#",
                DeviceMode.Config => $"{hostname}(config)#",
                DeviceMode.Interface => $"{hostname}(conf-if-{GetCurrentInterfaceName()?.Replace(" ", "-").Replace("/", "-")})#",
                DeviceMode.Vlan => $"{hostname}(conf-vlan-{GetVlans().LastOrDefault()?.Id})#",
                DeviceMode.Router => GetRouterPrompt(),
                DeviceMode.Acl => $"{hostname}(config-ipv4-acl)#",
                _ => $"{hostname}>"
            };
        }

        private string GetRouterPrompt()
        {
            var protocol = "ospf";
            var ospfConfig = GetOspfConfiguration();
            var bgpConfig = GetBgpConfiguration();
            if (bgpConfig != null) protocol = "bgp";
            return $"{GetHostname()}(config-router-{protocol})# ";
        }

        public override async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            // Use the base class implementation for actual command processing
            // This will use the vendor discovery system to find appropriate handlers
            return await base.ProcessCommandAsync(command);
        }

        // Helper methods for command handlers
        public string GetMode() => GetCurrentModeEnum().ToModeString();
        public new void SetCurrentMode(string mode) => SetModeEnum(DeviceModeExtensions.FromModeString(mode));
        public new string GetCurrentInterface() => GetCurrentInterfaceName();
        public new void SetCurrentInterface(string iface) => SetCurrentInterfaceName(iface);

        // Dell-specific helper methods
        public void AppendToRunningConfig(string line)
        {
            GetRunningConfigBuilder().AppendLine(line);
        }

        public void UpdateProtocols()
        {
            GetParentNetwork()?.UpdateProtocols();
        }

        public void UpdateConnectedRoutesPublic()
        {
            UpdateConnectedRoutes();
        }

        /// <summary>
        /// Convert CIDR to subnet mask
        /// </summary>
        private string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        /// <summary>
        /// Convert subnet mask to CIDR
        /// </summary>
        private int MaskToCidr(string mask)
        {
            if (string.IsNullOrEmpty(mask))
                return 0;

            var parts = mask.Split('.').Select(int.Parse).ToArray();
            int cidr = 0;

            foreach (var part in parts)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if ((part & (1 << i)) != 0)
                        cidr++;
                }
            }

            return cidr;
        }

        // Legacy ProcessShowCommand removed - now handled by ShowCommandHandler

        public string SimulatePingDell(string destIp)
        {
            var output = new StringBuilder();
            output.AppendLine($"PING {destIp} ({destIp}): 56 data bytes");

            bool reachable = IsReachable(destIp);

            for (int i = 0; i < 5; i++)
            {
                if (reachable)
                {
                    output.AppendLine($"64 bytes from {destIp}: icmp_seq={i} ttl=64 time=0.{100 + i % 50} ms");
                }
                else
                {
                    // Timeout - no output for that sequence
                }
            }

            output.AppendLine("");
            output.AppendLine($"--- {destIp} ping statistics ---");
            output.AppendLine($"5 packets transmitted, {(reachable ? 5 : 0)} packets received, {(reachable ? 0 : 100)}% packet loss");
            if (reachable)
            {
                output.AppendLine("round-trip min/avg/max/stddev = 0.100/0.120/0.150/0.020 ms");
            }

            return output.ToString();
        }

        private bool IsReachable(string destIp)
        {
            // Check if IP is in any connected network
            foreach (var route in GetRoutingTable().Where(r => r.Protocol == "Connected"))
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    return true;
                }
            }

            // Check if there's a route to the destination
            foreach (var route in GetRoutingTable())
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIpInNetwork(string ip, string network, string mask)
        {
            try
            {
                var ipBytes = ip.Split('.').Select(byte.Parse).ToArray();
                var networkBytes = network.Split('.').Select(byte.Parse).ToArray();
                var maskBytes = mask.Split('.').Select(byte.Parse).ToArray();

                for (int i = 0; i < 4; i++)
                {
                    if ((ipBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetBridgeId()
        {
            return $"{GetStpConfiguration().GetPriority(1):x4}.aabb.cc00.0100";
        }

        /// <summary>
        /// Get interface with alias expansion support
        /// </summary>
        /// <param name="interfaceName">Interface name or alias (e.g., "eth 1/1/1" or "ethernet 1/1/1")</param>
        /// <returns>Interface configuration or null if not found</returns>
        public override IInterfaceConfig? GetInterface(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return null;

            // Try direct lookup first
            var interfaces = GetAllInterfaces();
            if (interfaces.TryGetValue(interfaceName, out var directMatch))
                return directMatch;

            // Try basic alias expansion - interface alias handling is now managed by the vendor registry system
            // For now, just do a case-insensitive search and common Dell interface format variations
            foreach (var kvp in interfaces)
            {
                if (string.Equals(kvp.Key, interfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }

                // Try simple format matching (e.g., "eth 1/1/1" matches "ethernet 1/1/1")
                if (kvp.Key.StartsWith("ethernet", StringComparison.OrdinalIgnoreCase) &&
                    interfaceName.StartsWith("eth", StringComparison.OrdinalIgnoreCase))
                {
                    var fullName = kvp.Key.Replace("ethernet", "").Trim();
                    var shortName = interfaceName.Replace("eth", "").Trim();
                    if (string.Equals(fullName, shortName, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Add interface to the device (exposed for command handlers)
        /// </summary>
        /// <param name="interfaceName">Interface name</param>
        /// <param name="interfaceConfig">Interface configuration (optional)</param>
        public void AddNewInterface(string interfaceName, InterfaceConfig? interfaceConfig = null)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return;

            // Use canonical interface name for storage - simplified for now
            var canonicalName = interfaceName.ToLower().Replace("eth ", "ethernet ");

            if (GetInterface(canonicalName) == null)
            {
                AddInterface(canonicalName, interfaceConfig ?? new InterfaceConfig(canonicalName, this));
            }
        }
    }
}

