using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.LLDP
{
    /// <summary>
    /// LLDP (Link Layer Discovery Protocol) implementation - IEEE 802.1AB standard
    /// Following the state management pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
    /// </summary>
    public class LldpProtocol : BaseProtocol
    {
        public override NetworkProtocolType Type => NetworkProtocolType.LLDP;
        public override string Name => "Link Layer Discovery Protocol";
        public override string Version => "1.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new LldpState();
        }

        protected override void OnInitialized()
        {
            var lldpConfig = GetLldpConfig();
            if (lldpConfig != null)
            {
                var lldpState = (LldpState)_state;
                lldpState.ChassisId = lldpConfig.ChassisId;
                lldpState.ChassisIdType = "mac"; // Default type
                lldpState.SystemName = lldpConfig.SystemName;
                lldpState.SystemDescription = lldpConfig.SystemDescription;
                lldpState.SystemCapabilities = lldpConfig.SystemCapabilities;
                lldpState.ManagementAddress = GetDeviceManagementIp();
                lldpState.IsActive = lldpConfig.IsEnabled;

                LogProtocolEvent($"LLDP initialized - Chassis ID: {lldpConfig.ChassisId}");
            }
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var lldpState = (LldpState)_state;
            var lldpConfig = GetLldpConfig();

            if (lldpConfig == null || !lldpConfig.IsEnabled)
            {
                lldpState.IsActive = false;
                return;
            }

            // Discover LLDP neighbors
            await DiscoverLldpNeighbors(device, lldpConfig, lldpState);
        }

        protected override async Task ProcessTimers(INetworkDevice device)
        {
            var lldpState = (LldpState)_state;
            var lldpConfig = GetLldpConfig();

            if (lldpConfig == null || !lldpConfig.IsEnabled)
                return;

            // Send LLDP advertisements if timer expired
            if (lldpState.ShouldSendAdvertisement(lldpConfig.TransmitInterval))
            {
                await SendLldpAdvertisements(device, lldpConfig, lldpState);
            }
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var lldpState = (LldpState)_state;

            LogProtocolEvent("Processing LLDP neighbor database...");

            // Clean up expired neighbors
            var expiredNeighbors = lldpState.Neighbors.Values
                .Where(n => n.IsExpired)
                .ToList();

            foreach (var expiredNeighbor in expiredNeighbors)
            {
                LogProtocolEvent($"LLDP neighbor {expiredNeighbor.ChassisId}:{expiredNeighbor.PortId} on {expiredNeighbor.LocalPortId} expired");
                lldpState.Neighbors.Remove($"{expiredNeighbor.ChassisId}:{expiredNeighbor.LocalPortId}");
                lldpState.MarkStateChanged();
            }

            if (expiredNeighbors.Any())
            {
                LogProtocolEvent($"Removed {expiredNeighbors.Count} expired LLDP neighbors");
            }

            await Task.CompletedTask;
        }

        private async Task DiscoverLldpNeighbors(INetworkDevice device, LldpConfig config, LldpState state)
        {
            // LLDP discovery based on physical connections
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if LLDP is enabled on this interface
                if (!IsLldpEnabledOnInterface(interfaceName, config))
                    continue;

                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;

                    // Check if neighbor has LLDP enabled
                    var neighborLldpConfig = GetNeighborLldpConfig(neighborDevice);
                    if (neighborLldpConfig?.IsEnabled == true)
                    {
                        await ProcessLldpNeighbor(device, state, config, interfaceName, neighborDevice, neighborInterface);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task ProcessLldpNeighbor(INetworkDevice device, LldpState state, LldpConfig config,
            string localInterface, INetworkDevice neighborDevice, string neighborInterface)
        {
            var neighborConfig = GetNeighborLldpConfig(neighborDevice);
            if (neighborConfig == null) return;

            var neighborKey = $"{neighborConfig.ChassisId}:{localInterface}";
            var neighbor = state.GetOrCreateLldpNeighbor(neighborKey, () => new LldpNeighbor(
                neighborConfig.ChassisId,
                "mac", // Default chassis ID type
                neighborInterface, // Port ID is the remote interface
                "ifName", // Default port ID type
                localInterface)    // Local port is our interface
            {
                SystemName = neighborConfig.SystemName,
                SystemDescription = neighborConfig.SystemDescription,
                SystemCapabilities = neighborConfig.SystemCapabilities,
                ManagementAddress = GetDeviceManagementIp(neighborDevice),
                TimeToLive = neighborConfig.GetTimeToLive()
            });

            // Update neighbor information
            neighbor.UpdateLastSeen();
            neighbor.SystemName = neighborConfig.SystemName;
            neighbor.SystemDescription = neighborConfig.SystemDescription;
            neighbor.SystemCapabilities = neighborConfig.SystemCapabilities;
            neighbor.ManagementAddress = GetDeviceManagementIp(neighborDevice);

            state.UpdateNeighborActivity(neighborKey);
            state.Neighbors[neighborKey] = neighbor;

            LogProtocolEvent($"LLDP neighbor {neighbor.ChassisId} discovered on {localInterface} " +
                           $"(System: {neighbor.SystemName}, Capabilities: {string.Join(",", neighbor.SystemCapabilities)})");

            await Task.CompletedTask;
        }

        private async Task SendLldpAdvertisements(INetworkDevice device, LldpConfig config, LldpState state)
        {
            var sentCount = 0;

            // Send advertisements on all LLDP-enabled interfaces
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if LLDP is enabled on this interface
                if (!IsLldpEnabledOnInterface(interfaceName, config))
                    continue;

                // Check if there's a connected device
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;

                    // Check if neighbor has LLDP enabled
                    var neighborLldpConfig = GetNeighborLldpConfig(neighborDevice);
                    if (neighborLldpConfig?.IsEnabled == true)
                    {
                        // Simulate sending LLDP advertisement
                        LogProtocolEvent($"Sending LLDP advertisement on {interfaceName} to {neighborDevice.Name}");
                        sentCount++;
                    }
                }
            }

            if (sentCount > 0)
            {
                state.RecordAdvertisement();
                LogProtocolEvent($"Sent {sentCount} LLDP advertisements");
            }

            await Task.CompletedTask;
        }

        // Helper methods

        private LldpConfig? GetLldpConfig()
        {
            return _device?.GetLldpConfiguration() as LldpConfig;
        }

        private LldpConfig? GetNeighborLldpConfig(INetworkDevice neighbor)
        {
            return neighbor?.GetLldpConfiguration() as LldpConfig;
        }

        private bool IsLldpEnabledOnInterface(string interfaceName, LldpConfig config)
        {
            // Check global LLDP setting
            if (!config.IsEnabled)
                return false;

            // Check interface-specific setting
            if (config.InterfaceSettings.TryGetValue(interfaceName, out var interfaceSettings))
            {
                return interfaceSettings.IsEnabled;
            }

            // Default to enabled if not explicitly configured
            return true;
        }

        protected override object GetProtocolConfiguration()
        {
            return GetLldpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is LldpConfig lldpConfig)
            {
                _device.SetLldpConfiguration(lldpConfig);

                var lldpState = (LldpState)_state;
                lldpState.ChassisId = lldpConfig.ChassisId;
                lldpState.SystemName = lldpConfig.SystemName;
                lldpState.SystemDescription = lldpConfig.SystemDescription;
                lldpState.SystemCapabilities = lldpConfig.SystemCapabilities;
                lldpState.ManagementAddress = GetDeviceManagementIp();
                lldpState.IsActive = lldpConfig.IsEnabled;
                lldpState.MarkStateChanged();

                LogProtocolEvent($"LLDP configuration updated - Chassis ID: {lldpConfig.ChassisId}");
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // LLDP is IEEE standard supported by all major vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Generic" };
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            var lldpConfig = GetLldpConfig();
            return lldpConfig?.GetTimeToLive() ?? 120;
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            var lldpState = (LldpState)_state;
            if (lldpState.Neighbors.ContainsKey(neighborId))
            {
                var neighbor = lldpState.Neighbors[neighborId];
                LogProtocolEvent($"LLDP neighbor {neighbor.ChassisId}:{neighbor.PortId} on {neighbor.LocalPortId} removed due to timeout");
                lldpState.Neighbors.Remove(neighborId);
                lldpState.MarkStateChanged();
            }
        }

        /// <summary>
        /// Get LLDP-specific statistics
        /// </summary>
        public Dictionary<string, object> GetLldpStatistics()
        {
            var lldpState = (LldpState)_state;

            return new Dictionary<string, object>
            {
                ["ProtocolState"] = lldpState.GetStateData(),
                ["NeighborCount"] = lldpState.Neighbors.Count,
                ["ActiveInterfaces"] = GetActiveLldpInterfaces().Count(),
                ["Configuration"] = GetLldpConfig(),
                ["TotalAdvertisements"] = lldpState.AdvertisementCount,
                ["LastAdvertisement"] = lldpState.LastAdvertisement
            };
        }

        /// <summary>
        /// Get list of interfaces with LLDP enabled
        /// </summary>
        public IEnumerable<string> GetActiveLldpInterfaces()
        {
            var lldpConfig = GetLldpConfig();
            if (lldpConfig == null || !lldpConfig.IsEnabled)
                return Enumerable.Empty<string>();

            var activeInterfaces = new List<string>();

            foreach (var interfaceName in _device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = _device.GetInterface(interfaceName);
                if (interfaceConfig?.IsUp == true && !interfaceConfig.IsShutdown)
                {
                    if (IsLldpEnabledOnInterface(interfaceName, lldpConfig))
                    {
                        activeInterfaces.Add(interfaceName);
                    }
                }
            }

            return activeInterfaces;
        }

        /// <summary>
        /// Get detailed neighbor information for a specific interface
        /// </summary>
        public LldpNeighbor? GetNeighborOnInterface(string interfaceName)
        {
            var lldpState = (LldpState)_state;
            return lldpState.Neighbors.Values.FirstOrDefault(n => n.LocalPortId == interfaceName);
        }

        /// <summary>
        /// Helper method to get device management IP address
        /// </summary>
        private string GetDeviceManagementIp(INetworkDevice? device = null)
        {
            var targetDevice = device ?? _device;
            if (targetDevice == null) return "";

            // Try to find the first active interface with an IP address
            foreach (var interfaceName in targetDevice.GetAllInterfaces().Keys)
            {
                var interfaceConfig = targetDevice.GetInterface(interfaceName);
                if (interfaceConfig?.IsUp == true && !interfaceConfig.IsShutdown && !string.IsNullOrEmpty(interfaceConfig.IpAddress))
                {
                    return interfaceConfig.IpAddress;
                }
            }

            return "";
        }
    }
}
