using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Interfaces.Devices;

namespace NetForge.Simulation.Protocols.CDP
{
    /// <summary>
    /// CDP (Cisco Discovery Protocol) implementation with comprehensive TLV processing
    /// Following the state management pattern from COMPREHENSIVE_PROTOCOL_DOCUMENTATION.md
    /// </summary>
    public class CdpProtocol : BaseProtocol
    {
        private readonly CdpTlvProcessor _tlvProcessor;

        public override NetworkProtocolType Type => NetworkProtocolType.CDP;
        public override string Name => "Cisco Discovery Protocol";
        public override string Version => "2.0.0";

        public CdpProtocol()
        {
            _tlvProcessor = new CdpTlvProcessor();
        }

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

            // Simulate receiving CDP packet from neighbor with TLV processing
            var receivedTlvs = _tlvProcessor.BuildTlvs(neighborDevice, neighborInterface);
            _tlvProcessor.ProcessReceivedTlvs(neighbor, receivedTlvs);

            // Update neighbor information from processed TLVs
            neighbor.UpdateLastSeen();
            state.UpdateNeighborActivity(neighborKey);
            state.Neighbors[neighborKey] = neighbor;
            state.PacketsReceived++;

            LogProtocolEvent($"CDP neighbor {neighbor.DeviceId} discovered on {localInterface} with {receivedTlvs.Count} TLVs (Platform: {neighbor.Platform})");

            await Task.CompletedTask;
        }

        private async Task SendCdpPacket(CdpPacket packet, INetworkDevice sourceDevice, INetworkDevice targetDevice, string interfaceName)
        {
            // Simulate packet transmission with serialization
            var packetData = packet.Serialize();

            // In a real implementation, this would send the packet over the network
            // For simulation, we just log the packet details
            LogProtocolEvent($"CDP packet sent: {packetData.Length} bytes with {packet.Tlvs.Count} TLVs");

            // Simulate packet reception on target device (if it has CDP enabled)
            var targetCdpProtocol = targetDevice.GetProtocol<CdpProtocol>();
            if (targetCdpProtocol != null)
            {
                await targetCdpProtocol.ReceiveCdpPacket(packet, sourceDevice, interfaceName);
            }

            await Task.CompletedTask;
        }

        public async Task ReceiveCdpPacket(CdpPacket packet, INetworkDevice sourceDevice, string receivingInterface)
        {
            var cdpState = (CdpState)_state;
            var cdpConfig = GetCdpConfig();

            if (cdpConfig == null || !cdpConfig.IsEnabled)
                return;

            // Process received CDP packet
            var neighborKey = $"{sourceDevice.Name}:{receivingInterface}";
            var neighbor = cdpState.GetOrCreateCdpNeighbor(neighborKey, () => new CdpNeighbor(
                sourceDevice.Name,
                receivingInterface,
                "Unknown")
            {
                HoldTime = packet.Ttl
            });

            // Process TLVs from received packet
            _tlvProcessor.ProcessReceivedTlvs(neighbor, packet.Tlvs);
            neighbor.UpdateLastSeen();
            cdpState.UpdateNeighborActivity(neighborKey);
            cdpState.PacketsReceived++;

            LogProtocolEvent($"Received CDP packet from {sourceDevice.Name} on {receivingInterface} with {packet.Tlvs.Count} TLVs");

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
                        // Build CDP packet with comprehensive TLVs
                        var tlvs = _tlvProcessor.BuildTlvs(device, interfaceName);
                        var packet = new CdpPacket
                        {
                            Version = 2,
                            Ttl = (byte)config.HoldTime,
                            Tlvs = tlvs
                        };

                        // Simulate sending CDP advertisement with TLV processing
                        await SendCdpPacket(packet, device, neighborDevice, interfaceName);
                        LogProtocolEvent($"Sending CDP advertisement with {tlvs.Count} TLVs on {interfaceName} to {neighborDevice.Name}");
                        sentCount++;
                    }
                }
            }

            if (sentCount > 0)
            {
                state.RecordAdvertisement();
                LogProtocolEvent($"Sent {sentCount} CDP advertisements with comprehensive TLV processing");
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

        /// <summary>
        /// Get detailed neighbor information including TLV data
        /// </summary>
        public Dictionary<string, object> GetNeighborDetails(string neighborKey)
        {
            var cdpState = (CdpState)_state;

            if (!cdpState.Neighbors.TryGetValue(neighborKey, out var neighbor))
                return new Dictionary<string, object>();

            var details = new Dictionary<string, object>
            {
                ["DeviceId"] = neighbor.DeviceId,
                ["LocalInterface"] = neighbor.LocalInterface,
                ["RemoteInterface"] = neighbor.RemoteInterface,
                ["Platform"] = neighbor.Platform,
                ["IpAddress"] = neighbor.IpAddress,
                ["Version"] = neighbor.Version,
                ["Capabilities"] = neighbor.GetCapabilityString(),
                ["HoldTime"] = neighbor.HoldTime,
                ["LastSeen"] = neighbor.LastSeen,
                ["IsExpired"] = neighbor.IsExpired,
                ["TlvCount"] = neighbor.Tlvs.Count,
                ["TlvTypes"] = neighbor.Tlvs.Keys.Select(k => k.ToString()).ToList()
            };

            // Add specific TLV information
            foreach (var tlv in neighbor.Tlvs.Values)
            {
                switch (tlv.Type)
                {
                    case CdpTlvType.NativeVlan:
                        if (tlv.Value.Length >= 2)
                            details["NativeVlan"] = BitConverter.ToUInt16(tlv.Value, 0);
                        break;

                    case CdpTlvType.Mtu:
                        if (tlv.Value.Length >= 4)
                            details["Mtu"] = BitConverter.ToUInt32(tlv.Value, 0);
                        break;

                    case CdpTlvType.Duplex:
                        if (tlv.Value.Length >= 1)
                            details["Duplex"] = tlv.Value[0] == 0x01 ? "Full" : "Half";
                        break;
                }
            }

            return details;
        }

        /// <summary>
        /// Get CDP traffic statistics
        /// </summary>
        public Dictionary<string, object> GetTrafficStatistics()
        {
            var cdpState = (CdpState)_state;
            return new Dictionary<string, object>
            {
                ["PacketsSent"] = cdpState.PacketsSent,
                ["PacketsReceived"] = cdpState.PacketsReceived,
                ["LastAdvertisementSent"] = cdpState.LastAdvertisementSent,
                ["ActiveNeighbors"] = cdpState.Neighbors.Count(kvp => !kvp.Value.IsExpired),
                ["ExpiredNeighbors"] = cdpState.Neighbors.Count(kvp => kvp.Value.IsExpired),
                ["TotalNeighbors"] = cdpState.Neighbors.Count
            };
        }

        /// <summary>
        /// Validate CDP packet structure and TLVs
        /// </summary>
        public bool ValidateCdpPacket(byte[] packetData, out string validationMessage)
        {
            validationMessage = "";

            var packet = CdpPacket.Deserialize(packetData);
            if (packet == null)
            {
                validationMessage = "Invalid CDP packet structure";
                return false;
            }

            // Check required TLVs
            var requiredTlvs = new[] { CdpTlvType.DeviceId, CdpTlvType.PortId, CdpTlvType.Capabilities };
            var missingTlvs = requiredTlvs.Where(required => !packet.Tlvs.Any(tlv => tlv.Type == required)).ToList();

            if (missingTlvs.Any())
            {
                validationMessage = $"Missing required TLVs: {string.Join(", ", missingTlvs)}";
                return false;
            }

            validationMessage = "CDP packet is valid";
            return true;
        }
    }
}
