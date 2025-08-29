using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Routing;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.CDP
{
    /// <summary>
    /// CDP (Cisco Discovery Protocol) implementation
    /// Following the state management pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
    /// </summary>
    public class CdpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.CDP;
        public override string Name => "Cisco Discovery Protocol";
        public override string Version => "2.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new CdpState();
        }

        protected override void OnInitialized()
        {
            var cdpConfig = GetCdpConfig();
            if (cdpConfig != null)
            {
                var cdpState = (CdpState)_state;
                cdpState.DeviceId = cdpConfig.DeviceId;
                cdpState.Platform = cdpConfig.Platform;
                cdpState.Version = cdpConfig.Version;
                cdpState.Capabilities = cdpConfig.Capabilities;
                cdpState.InterfaceSettings = cdpConfig.InterfaceSettings;
                cdpState.IsActive = cdpConfig.IsEnabled;

                LogProtocolEvent($"CDP initialized - Device ID: {cdpConfig.DeviceId}");
            }
        }

        protected override async Task UpdateNeighbors(INetworkDevice device)
        {
            var cdpState = (CdpState)_state;
            var cdpConfig = GetCdpConfig();

            if (cdpConfig == null || !cdpConfig.IsEnabled)
            {
                cdpState.IsActive = false;
                return;
            }

            // Discover CDP neighbors
            await DiscoverCdpNeighbors(device, cdpConfig, cdpState);
        }

        protected override async Task ProcessTimers(INetworkDevice device)
        {
            var cdpState = (CdpState)_state;
            var cdpConfig = GetCdpConfig();

            if (cdpConfig == null || !cdpConfig.IsEnabled)
                return;

            // Send CDP advertisements if timer expired
            if (cdpState.ShouldSendAdvertisement(cdpConfig.Timer))
            {
                await SendCdpAdvertisements(device, cdpConfig, cdpState);
            }
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var cdpState = (CdpState)_state;

            LogProtocolEvent("Processing CDP neighbor database...");

            // Clean up expired neighbors
            var expiredNeighbors = cdpState.Neighbors.Values
                .Where(n => n.IsExpired)
                .ToList();

            foreach (var expiredNeighbor in expiredNeighbors)
            {
                LogProtocolEvent($"CDP neighbor {expiredNeighbor.DeviceId} on {expiredNeighbor.LocalInterface} expired");
                cdpState.Neighbors.Remove($"{expiredNeighbor.DeviceId}:{expiredNeighbor.LocalInterface}");
                cdpState.MarkStateChanged();
            }

            if (expiredNeighbors.Any())
            {
                LogProtocolEvent($"Removed {expiredNeighbors.Count} expired CDP neighbors");
            }

            await Task.CompletedTask;
        }

        private async Task DiscoverCdpNeighbors(INetworkDevice device, CdpConfig config, CdpState state)
        {
            // CDP discovery based on physical connections
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if CDP is enabled on this interface
                if (!IsCdpEnabledOnInterface(interfaceName, config))
                    continue;

                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;
                    var neighborInterface = connectedDevice.Value.interfaceName;

                    if (!IsNeighborReachable(device, interfaceName, neighborDevice))
                        continue;

                    // Check if neighbor has CDP enabled
                    var neighborCdpConfig = GetNeighborCdpConfig(neighborDevice);
                    if (neighborCdpConfig?.IsEnabled == true)
                    {
                        await ProcessCdpNeighbor(device, state, config, interfaceName, neighborDevice, neighborInterface);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task ProcessCdpNeighbor(INetworkDevice device, CdpState state, CdpConfig config,
            string localInterface, INetworkDevice neighborDevice, string neighborInterface)
        {
            var neighborConfig = GetNeighborCdpConfig(neighborDevice);
            if (neighborConfig == null) return;

            var neighborKey = $"{neighborConfig.DeviceId}:{localInterface}";
            var neighbor = state.GetOrCreateCdpNeighbor(neighborKey, () => new CdpNeighbor(
                neighborConfig.DeviceId,
                localInterface,
                neighborInterface)
            {
                Platform = neighborConfig.Platform,
                Version = neighborConfig.Version,
                Capabilities = neighborConfig.Capabilities,
                IpAddress = neighborDevice.GetInterface(neighborInterface)?.IpAddress ?? "0.0.0.0",
                HoldTime = neighborConfig.HoldTime
            });

            // Update neighbor information
            neighbor.UpdateLastSeen();
            neighbor.Platform = neighborConfig.Platform;
            neighbor.Version = neighborConfig.Version;
            neighbor.Capabilities = neighborConfig.Capabilities;
            neighbor.IpAddress = neighborDevice.GetInterface(neighborInterface)?.IpAddress ?? "0.0.0.0";

            state.UpdateNeighborActivity(neighborKey);
            state.Neighbors[neighborKey] = neighbor;

            LogProtocolEvent($"CDP neighbor {neighbor.DeviceId} discovered on {localInterface} (Platform: {neighbor.Platform})");

            await Task.CompletedTask;
        }

        private async Task SendCdpAdvertisements(INetworkDevice device, CdpConfig config, CdpState state)
        {
            var sentCount = 0;

            // Send advertisements on all CDP-enabled interfaces
            foreach (var interfaceName in device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp)
                    continue;

                // Check if CDP is enabled on this interface
                if (!IsCdpEnabledOnInterface(interfaceName, config))
                    continue;

                // Check if there's a connected device
                var connectedDevice = device.GetConnectedDevice(interfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborDevice = connectedDevice.Value.device;

                    // Check if neighbor has CDP enabled
                    var neighborCdpConfig = GetNeighborCdpConfig(neighborDevice);
                    if (neighborCdpConfig?.IsEnabled == true)
                    {
                        // Simulate sending CDP advertisement
                        LogProtocolEvent($"Sending CDP advertisement on {interfaceName} to {neighborDevice.Name}");
                        sentCount++;
                    }
                }
            }

            if (sentCount > 0)
            {
                state.RecordAdvertisement();
                LogProtocolEvent($"Sent {sentCount} CDP advertisements");
            }

            await Task.CompletedTask;
        }

        // Helper methods

        private CdpConfig? GetCdpConfig()
        {
            return _device?.GetCdpConfiguration() as CdpConfig;
        }

        private CdpConfig? GetNeighborCdpConfig(INetworkDevice neighbor)
        {
            return neighbor?.GetCdpConfiguration() as CdpConfig;
        }

        private bool IsCdpEnabledOnInterface(string interfaceName, CdpConfig config)
        {
            // Check global CDP setting
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
            return GetCdpConfig();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is CdpConfig cdpConfig)
            {
                _device.SetCdpConfiguration(cdpConfig);

                var cdpState = (CdpState)_state;
                cdpState.DeviceId = cdpConfig.DeviceId;
                cdpState.Platform = cdpConfig.Platform;
                cdpState.Version = cdpConfig.Version;
                cdpState.Capabilities = cdpConfig.Capabilities;
                cdpState.InterfaceSettings = cdpConfig.InterfaceSettings;
                cdpState.IsActive = cdpConfig.IsEnabled;
                cdpState.MarkStateChanged();

                LogProtocolEvent($"CDP configuration updated - Device ID: {cdpConfig.DeviceId}");
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // CDP is Cisco proprietary but simulated on other vendors
            return new[] { "Cisco", "Generic" };
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            var cdpConfig = GetCdpConfig();
            return cdpConfig?.HoldTime ?? 180;
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            var cdpState = (CdpState)_state;
            if (cdpState.Neighbors.ContainsKey(neighborId))
            {
                var neighbor = cdpState.Neighbors[neighborId];
                LogProtocolEvent($"CDP neighbor {neighbor.DeviceId} on {neighbor.LocalInterface} removed due to timeout");
                cdpState.Neighbors.Remove(neighborId);
                cdpState.MarkStateChanged();
            }
        }

        /// <summary>
        /// Get CDP-specific statistics
        /// </summary>
        public Dictionary<string, object> GetCdpStatistics()
        {
            var cdpState = (CdpState)_state;

            return new Dictionary<string, object>
            {
                ["ProtocolState"] = cdpState.GetStateData(),
                ["NeighborCount"] = cdpState.Neighbors.Count,
                ["ActiveInterfaces"] = GetActiveCdpInterfaces().Count(),
                ["Configuration"] = GetCdpConfig()
            };
        }

        /// <summary>
        /// Get list of interfaces with CDP enabled
        /// </summary>
        public IEnumerable<string> GetActiveCdpInterfaces()
        {
            var cdpConfig = GetCdpConfig();
            if (cdpConfig == null || !cdpConfig.IsEnabled)
                return Enumerable.Empty<string>();

            var activeInterfaces = new List<string>();

            foreach (var interfaceName in _device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = _device.GetInterface(interfaceName);
                if (interfaceConfig?.IsUp == true && !interfaceConfig.IsShutdown)
                {
                    if (IsCdpEnabledOnInterface(interfaceName, cdpConfig))
                    {
                        activeInterfaces.Add(interfaceName);
                    }
                }
            }

            return activeInterfaces;
        }
    }
}
