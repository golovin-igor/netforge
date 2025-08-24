using NetForge.Simulation.Common;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
using System.Net;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.ARP
{
    /// <summary>
    /// ARP (Address Resolution Protocol) implementation
    /// Following the state management pattern from PROTOCOL_STATE_MANAGEMENT.md
    /// </summary>
    public class ArpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.ARP;
        public override string Name => "Address Resolution Protocol";
        public override string Version => "1.0.0";

        protected override BaseProtocolState CreateInitialState()
        {
            return new ArpState();
        }

        protected override void OnInitialized()
        {
            var arpState = (ArpState)_state;
            arpState.IsActive = true;

            // Initialize ARP table with existing entries from NetworkDevice
            ImportExistingArpTable();

            LogProtocolEvent("ARP protocol initialized");
        }

        protected override async Task UpdateNeighbors(NetworkDevice device)
        {
            var arpState = (ArpState)_state;

            // ARP doesn't have traditional neighbors, but we process ARP table maintenance
            await ProcessArpTableMaintenance(device, arpState);
        }

        protected override async Task RunProtocolCalculation(NetworkDevice device)
        {
            var arpState = (ArpState)_state;

            LogProtocolEvent("Processing ARP table cleanup...");

            // Clean up expired ARP entries
            var expiredEntries = arpState.ArpTable.Values
                .Where(entry => entry.Type == ArpEntryType.Dynamic && entry.IsExpired())
                .ToList();

            foreach (var expiredEntry in expiredEntries)
            {
                LogProtocolEvent($"ARP entry for {expiredEntry.IpAddress} expired, removing");
                arpState.ArpTable.Remove(expiredEntry.IpAddress);

                // Also remove from NetworkDevice's ARP table
                device.GetArpTable().Remove(expiredEntry.IpAddress);

                arpState.MarkStateChanged();
            }

            if (expiredEntries.Count > 0)
            {
                LogProtocolEvent($"Removed {expiredEntries.Count} expired ARP entries");
            }

            await Task.CompletedTask;
        }

        private async Task ProcessArpTableMaintenance(NetworkDevice device, ArpState state)
        {
            // Sync with NetworkDevice's ARP table
            var deviceArpTable = device.GetArpTable();

            // Add new entries from device ARP table
            foreach (var (ip, mac) in deviceArpTable)
            {
                if (!state.ArpTable.ContainsKey(ip))
                {
                    var outInterface = FindOutgoingInterface(device, ip);
                    var arpEntry = new ArpEntry(ip, mac, outInterface)
                    {
                        Type = ArpEntryType.Dynamic
                    };

                    state.ArpTable[ip] = arpEntry;
                    state.MarkStateChanged();
                    LogProtocolEvent($"Added ARP entry: {ip} -> {mac} via {outInterface}");
                }
                else
                {
                    // Update existing entry timestamp
                    state.ArpTable[ip].UpdateTimestamp();
                }
            }

            await Task.CompletedTask;
        }

        private void ImportExistingArpTable()
        {
            var arpState = (ArpState)_state;
            var deviceArpTable = _device.GetArpTable();

            foreach (var (ip, mac) in deviceArpTable)
            {
                var outInterface = FindOutgoingInterface(_device, ip);
                var arpEntry = new ArpEntry(ip, mac, outInterface)
                {
                    Type = ArpEntryType.Dynamic
                };

                arpState.ArpTable[ip] = arpEntry;
                LogProtocolEvent($"Imported ARP entry: {ip} -> {mac}");
            }
        }

        private string FindOutgoingInterface(NetworkDevice device, string destIp)
        {
            // Use the device's existing logic for finding outgoing interface
            try
            {
                // Simple logic: find interface in same subnet
                foreach (var (interfaceName, interfaceConfig) in device.GetAllInterfaces())
                {
                    if (interfaceConfig?.IpAddress == null || interfaceConfig.SubnetMask == null)
                        continue;

                    if (IsInSameSubnet(destIp, interfaceConfig.IpAddress, interfaceConfig.SubnetMask))
                    {
                        return interfaceName;
                    }
                }

                // Fall back to route table lookup
                var routes = device.GetRoutingTable();
                foreach (var route in routes.OrderByDescending(r => MaskToCidr(r.Mask)))
                {
                    if (IsInNetwork(destIp, route.Network, route.Mask))
                    {
                        return route.Interface;
                    }
                }

                return ""; // No interface found
            }
            catch
            {
                return ""; // Fallback
            }
        }

        private bool IsInSameSubnet(string ip1, string ip2, string subnetMask)
        {
            try
            {
                var addr1 = IPAddress.Parse(ip1);
                var addr2 = IPAddress.Parse(ip2);
                var mask = IPAddress.Parse(subnetMask);

                var network1 = GetNetworkAddress(addr1, mask);
                var network2 = GetNetworkAddress(addr2, mask);

                return network1.Equals(network2);
            }
            catch
            {
                return false;
            }
        }

        private bool IsInNetwork(string ip, string network, string mask)
        {
            try
            {
                var ipAddr = IPAddress.Parse(ip);
                var netAddr = IPAddress.Parse(network);
                var maskAddr = IPAddress.Parse(mask);

                var networkAddress = GetNetworkAddress(ipAddr, maskAddr);
                return networkAddress.Equals(netAddr);
            }
            catch
            {
                return false;
            }
        }

        private IPAddress GetNetworkAddress(IPAddress ip, IPAddress mask)
        {
            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var networkBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            return new IPAddress(networkBytes);
        }

        private int MaskToCidr(string mask)
        {
            try
            {
                var maskAddr = IPAddress.Parse(mask);
                var maskBytes = maskAddr.GetAddressBytes();
                int cidr = 0;

                foreach (var b in maskBytes)
                {
                    cidr += CountBits(b);
                }

                return cidr;
            }
            catch
            {
                return 0;
            }
        }

        private int CountBits(byte b)
        {
            int count = 0;
            while (b != 0)
            {
                count++;
                b &= (byte)(b - 1);
            }
            return count;
        }

        /// <summary>
        /// Perform ARP resolution for an IP address
        /// </summary>
        public async Task<string?> ResolveArpAsync(string ipAddress)
        {
            var arpState = (ArpState)_state;

            // Check if we already have the entry
            if (arpState.ArpTable.TryGetValue(ipAddress, out var existingEntry))
            {
                if (!existingEntry.IsExpired())
                {
                    arpState.RecordCacheHit();
                    LogProtocolEvent($"ARP cache hit for {ipAddress} -> {existingEntry.MacAddress}");
                    return existingEntry.MacAddress;
                }
                else
                {
                    // Remove expired entry
                    arpState.ArpTable.Remove(ipAddress);
                    _device.GetArpTable().Remove(ipAddress);
                }
            }

            arpState.RecordCacheMiss();

            // Perform ARP resolution
            var macAddress = await PerformArpResolution(ipAddress);
            if (macAddress != null)
            {
                // Add to ARP table
                var outInterface = FindOutgoingInterface(_device, ipAddress);
                var arpEntry = new ArpEntry(ipAddress, macAddress, outInterface)
                {
                    Type = ArpEntryType.Dynamic
                };

                arpState.ArpTable[ipAddress] = arpEntry;
                _device.GetArpTable()[ipAddress] = macAddress;

                arpState.RecordArpResponse();
                LogProtocolEvent($"ARP resolved: {ipAddress} -> {macAddress}");
                return macAddress;
            }

            LogProtocolEvent($"ARP resolution failed for {ipAddress}");
            return null;
        }

        private async Task<string?> PerformArpResolution(string ipAddress)
        {
            var arpState = (ArpState)_state;

            // Find the destination device in the network
            if (_device.ParentNetwork == null)
                return null;

            var destDevice = _device.ParentNetwork.FindDeviceByIp(ipAddress);
            if (destDevice == null)
                return null;

            // Find the interface with this IP
            var destInterface = destDevice.GetAllInterfaces().Values
                .FirstOrDefault(i => i.IpAddress == ipAddress);

            if (destInterface == null)
                return null;

            // Simulate ARP request/response
            arpState.RecordArpRequest();
            LogProtocolEvent($"Sending ARP request for {ipAddress}");

            // Add delay to simulate network latency
            await Task.Delay(10);

            // Update both sides of the ARP tables (bidirectional)
            var outInterface = FindOutgoingInterface(_device, ipAddress);
            if (!string.IsNullOrEmpty(outInterface))
            {
                var localInterface = _device.GetInterface(outInterface);
                if (localInterface?.IpAddress != null)
                {
                    // Add our entry to destination device's ARP table
                    destDevice.GetArpTable()[localInterface.IpAddress] = localInterface.MacAddress;
                }
            }

            return destInterface.MacAddress;
        }

        /// <summary>
        /// Add static ARP entry
        /// </summary>
        public void AddStaticArpEntry(string ipAddress, string macAddress, string interfaceName)
        {
            var arpState = (ArpState)_state;

            var arpEntry = new ArpEntry(ipAddress, macAddress, interfaceName)
            {
                Type = ArpEntryType.Static
            };

            arpState.ArpTable[ipAddress] = arpEntry;
            _device.GetArpTable()[ipAddress] = macAddress;

            arpState.MarkStateChanged();
            LogProtocolEvent($"Added static ARP entry: {ipAddress} -> {macAddress}");
        }

        /// <summary>
        /// Remove ARP entry
        /// </summary>
        public void RemoveArpEntry(string ipAddress)
        {
            var arpState = (ArpState)_state;

            if (arpState.ArpTable.Remove(ipAddress))
            {
                _device.GetArpTable().Remove(ipAddress);
                arpState.MarkStateChanged();
                LogProtocolEvent($"Removed ARP entry for {ipAddress}");
            }
        }

        /// <summary>
        /// Clear all dynamic ARP entries
        /// </summary>
        public void ClearDynamicArpEntries()
        {
            var arpState = (ArpState)_state;

            var dynamicEntries = arpState.ArpTable.Values
                .Where(entry => entry.Type == ArpEntryType.Dynamic)
                .ToList();

            foreach (var entry in dynamicEntries)
            {
                arpState.ArpTable.Remove(entry.IpAddress);
                _device.GetArpTable().Remove(entry.IpAddress);
            }

            if (dynamicEntries.Count > 0)
            {
                arpState.MarkStateChanged();
                LogProtocolEvent($"Cleared {dynamicEntries.Count} dynamic ARP entries");
            }
        }

        protected override object GetProtocolConfiguration()
        {
            // ARP doesn't have specific configuration, return state information
            return ((ArpState)_state).GetStateData();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            // ARP protocol doesn't have specific configuration to apply
            LogProtocolEvent("ARP configuration updated");
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // ARP is a fundamental protocol supported by all vendors
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "F5", "Fortinet" };
        }

        protected override int GetNeighborTimeoutSeconds()
        {
            // ARP entries timeout in 20 minutes by default
            return 1200;
        }

        protected override void OnNeighborRemoved(string neighborId)
        {
            var arpState = (ArpState)_state;
            if (arpState.ArpTable.ContainsKey(neighborId))
            {
                LogProtocolEvent($"ARP entry for {neighborId} removed due to timeout");
                arpState.ArpTable.Remove(neighborId);
                _device.GetArpTable().Remove(neighborId);
                arpState.MarkStateChanged();
            }
        }

        /// <summary>
        /// Get ARP statistics
        /// </summary>
        public Dictionary<string, object> GetArpStatistics()
        {
            var arpState = (ArpState)_state;

            return new Dictionary<string, object>
            {
                ["ProtocolState"] = arpState.GetStateData(),
                ["TotalEntries"] = arpState.ArpTable.Count,
                ["DynamicEntries"] = arpState.ArpTable.Values.Count(e => e.Type == ArpEntryType.Dynamic),
                ["StaticEntries"] = arpState.ArpTable.Values.Count(e => e.Type == ArpEntryType.Static),
                ["CacheHitRate"] = arpState.ArpCacheHits + arpState.ArpCacheMisses > 0
                    ? (double)arpState.ArpCacheHits / (arpState.ArpCacheHits + arpState.ArpCacheMisses) * 100
                    : 0.0
            };
        }
    }
}
