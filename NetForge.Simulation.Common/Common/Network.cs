using NetForge.Simulation.Common.Events;

namespace NetForge.Simulation.Common.Common
{
    /// <summary>
    /// Manages network topology and device interconnections with physical connection simulation
    /// </summary>
    public class Network
    {
        private Dictionary<string, NetworkDevice> devices = new Dictionary<string, NetworkDevice>();
        private Dictionary<string, NetworkDevice> devicesByDeviceId = new Dictionary<string, NetworkDevice>();
        private Dictionary<string, PhysicalConnection> physicalConnections = new Dictionary<string, PhysicalConnection>();
        private readonly NetworkEventBus _eventBus;

        public NetworkEventBus EventBus => _eventBus;

        public Network()
        {
            _eventBus = new NetworkEventBus();
        }

        /// <summary>
        /// Add a device to the network
        /// </summary>
        public async Task AddDeviceAsync(NetworkDevice device)
        {
            if (device == null)
                return;
            if (!devices.ContainsKey(device.Name))
            {
                devices[device.Name] = device;

                // Also add to devicesByDeviceId dictionary if DeviceId is provided
                if (!string.IsNullOrEmpty(device.DeviceId))
                {
                    devicesByDeviceId[device.DeviceId] = device;
                }

                device.ParentNetwork = this;

                // Subscribe protocols to events now that ParentNetwork is set
                device.SubscribeProtocolsToEvents();

                await _eventBus.PublishAsync(new DeviceChangedEventArgs(device, DeviceChangeType.Added));
            }
        }

        /// <summary>
        /// Add a physical connection between two device interfaces
        /// </summary>
        public async Task AddPhysicalConnectionAsync(string device1Name, string interface1, string device2Name, string interface2,
            PhysicalConnectionType connectionType = PhysicalConnectionType.Ethernet)
        {
            if (!devices.ContainsKey(device1Name) || !devices.ContainsKey(device2Name))
                throw new ArgumentException("One or both devices do not exist in the network");

            var connection = new PhysicalConnection(device1Name, interface1, device2Name, interface2, connectionType);

            if (physicalConnections.ContainsKey(connection.Id))
                throw new InvalidOperationException($"Physical connection {connection.Id} already exists");

            physicalConnections[connection.Id] = connection;

            // Subscribe to connection state changes
            connection.StateChanged += async (args) =>
            {
                await _eventBus.PublishAsync(args);
                await HandlePhysicalConnectionStateChangeAsync(args);
            };

            // Establish the connection (this will trigger state change events)
            await connection.ConnectAsync();
        }

        /// <summary>
        /// Add a link between two device interfaces (legacy method - creates physical connection)
        /// </summary>
        public async Task AddLinkAsync(string device1Name, string interface1, string device2Name, string interface2)
        {
            await AddPhysicalConnectionAsync(device1Name, interface1, device2Name, interface2);
        }

        /// <summary>
        /// Remove a physical connection between devices
        /// </summary>
        public async Task RemovePhysicalConnectionAsync(string device1Name, string interface1, string device2Name, string interface2)
        {
            var connectionId = GenerateConnectionId(device1Name, interface1, device2Name, interface2);

            if (physicalConnections.TryGetValue(connectionId, out var connection))
            {
                await connection.DisconnectAsync();
                physicalConnections.Remove(connectionId);
            }
        }

        /// <summary>
        /// Remove a link between devices (legacy method)
        /// </summary>
        public async Task RemoveLinkAsync(string device1Name, string interface1, string device2Name, string interface2)
        {
            await RemovePhysicalConnectionAsync(device1Name, interface1, device2Name, interface2);
        }

        /// <summary>
        /// Get a physical connection by device and interface names
        /// </summary>
        public PhysicalConnection? GetPhysicalConnection(string device1Name, string interface1, string device2Name, string interface2)
        {
            var connectionId = GenerateConnectionId(device1Name, interface1, device2Name, interface2);
            return physicalConnections.TryGetValue(connectionId, out var connection) ? connection : null;
        }

        /// <summary>
        /// Get all physical connections for a specific device interface
        /// </summary>
        public List<PhysicalConnection> GetPhysicalConnectionsForInterface(string deviceName, string interfaceName)
        {
            return physicalConnections.Values
                .Where(conn => conn.InvolvesInterface(deviceName, interfaceName))
                .ToList();
        }

        /// <summary>
        /// Get all physical connections in the network
        /// </summary>
        public IEnumerable<PhysicalConnection> GetAllPhysicalConnections()
        {
            return physicalConnections.Values;
        }

        /// <summary>
        /// Simulate cable failure on a specific connection
        /// </summary>
        public async Task SimulateCableFailureAsync(string device1Name, string interface1, string device2Name, string interface2, string reason = "Cable failure")
        {
            var connection = GetPhysicalConnection(device1Name, interface1, device2Name, interface2);
            if (connection != null)
            {
                await connection.SetFailedAsync(reason);
            }
        }

        /// <summary>
        /// Simulate connection degradation
        /// </summary>
        public async Task SimulateConnectionDegradationAsync(string device1Name, string interface1, string device2Name, string interface2,
            double packetLoss, int additionalLatency, string reason = "Signal degradation")
        {
            var connection = GetPhysicalConnection(device1Name, interface1, device2Name, interface2);
            if (connection != null)
            {
                await connection.SetDegradedAsync(packetLoss, additionalLatency, reason);
            }
        }

        /// <summary>
        /// Restore a failed or degraded connection
        /// </summary>
        public async Task RestoreConnectionAsync(string device1Name, string interface1, string device2Name, string interface2)
        {
            var connection = GetPhysicalConnection(device1Name, interface1, device2Name, interface2);
            if (connection != null)
            {
                await connection.RestoreAsync();
            }
        }

        /// <summary>
        /// Get device by name
        /// </summary>
        public NetworkDevice GetDevice(string name)
        {
            return devices.ContainsKey(name) ? devices[name] : null;
        }

        /// <summary>
        /// Remove a device from the network
        /// </summary>
        public async Task RemoveDeviceAsync(string deviceName)
        {
            if (devices.TryGetValue(deviceName, out var device))
            {
                devices.Remove(deviceName);

                // Also remove from devicesByDeviceId dictionary if DeviceId exists
                if (!string.IsNullOrEmpty(device.DeviceId))
                {
                    devicesByDeviceId.Remove(device.DeviceId);
                }

                device.ParentNetwork = null;
                await _eventBus.PublishAsync(new DeviceChangedEventArgs(device, DeviceChangeType.Removed));
            }
        }

        /// <summary>
        /// Remove a device from the network by device ID
        /// </summary>
        public async Task RemoveDeviceByIdAsync(string deviceId)
        {
            var device = GetDeviceById(deviceId);
            if (device != null)
            {
                await RemoveDeviceAsync(device.Name);
            }
        }

        /// <summary>
        /// Update device ID mapping (useful when a device's DeviceId changes after being added)
        /// </summary>
        public void UpdateDeviceIdMapping(NetworkDevice device, string? oldDeviceId = null)
        {
            // Remove old mapping if provided
            if (!string.IsNullOrEmpty(oldDeviceId))
            {
                devicesByDeviceId.Remove(oldDeviceId);
            }

            // Add new mapping if DeviceId is provided
            if (!string.IsNullOrEmpty(device.DeviceId))
            {
                devicesByDeviceId[device.DeviceId] = device;
            }
        }

        /// <summary>
        /// Check if a device with the specified ID exists in the network
        /// </summary>
        public bool ContainsDeviceId(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return false;

            return devicesByDeviceId.ContainsKey(deviceId);
        }

        /// <summary>
        /// Get device by device ID
        /// </summary>
        public NetworkDevice? GetDeviceById(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            return devicesByDeviceId.TryGetValue(deviceId, out var device) ? device : null;
        }

        /// <summary>
        /// Find device by IP address
        /// </summary>
        public NetworkDevice FindDeviceByIp(string ip)
        {
            foreach (var device in devices.Values)
            {
                foreach (var iface in device.GetAllInterfaces().Values)
                {
                    if (iface.IpAddress == ip)
                        return device;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all devices in the network
        /// </summary>
        public IEnumerable<NetworkDevice> GetAllDevices()
        {
            return devices.Values;
        }

        /// <summary>
        /// Get devices connected to a specific device interface
        /// </summary>
        public List<(NetworkDevice device, string interfaceName)> GetConnectedDevices(string deviceName, string interfaceName)
        {
            var result = new List<(NetworkDevice device, string interfaceName)>();

            var connections = GetPhysicalConnectionsForInterface(deviceName, interfaceName);
            foreach (var connection in connections.Where(c => c.IsOperational))
            {
                var remoteEnd = connection.GetRemoteEnd(deviceName, interfaceName);
                if (remoteEnd.HasValue && devices.ContainsKey(remoteEnd.Value.deviceName))
                {
                    result.Add((devices[remoteEnd.Value.deviceName], remoteEnd.Value.interfaceName));
                }
            }

            return result;
        }

        /// <summary>
        /// Check if two interfaces are connected via operational physical connection
        /// </summary>
        public bool AreConnected(string device1Name, string interface1, string device2Name, string interface2)
        {
            var connection = GetPhysicalConnection(device1Name, interface1, device2Name, interface2);
            return connection?.IsOperational ?? false;
        }

        /// <summary>
        /// Check if an interface has any operational physical connections
        /// </summary>
        public bool IsInterfaceConnected(string deviceName, string interfaceName)
        {
            return GetPhysicalConnectionsForInterface(deviceName, interfaceName)
                .Any(conn => conn.IsOperational);
        }

        /// <summary>
        /// Get network statistics for monitoring
        /// </summary>
        public NetworkStatistics GetNetworkStatistics()
        {
            var stats = new NetworkStatistics
            {
                TotalDevices = devices.Count,
                TotalConnections = physicalConnections.Count,
                OperationalConnections = physicalConnections.Values.Count(c => c.IsOperational),
                FailedConnections = physicalConnections.Values.Count(c => c.State == PhysicalConnectionState.Failed),
                DegradedConnections = physicalConnections.Values.Count(c => c.State == PhysicalConnectionState.Degraded)
            };

            return stats;
        }

        /// <summary>
        /// Update all protocol states (legacy method - protocols now update via events)
        /// </summary>
        public void UpdateProtocols()
        {
            Console.WriteLine("Network.UpdateProtocols() called - protocols now update automatically via events.");
            foreach (var device in devices.Values)
            {
                device.UpdateAllProtocolStates();
            }
        }

        /// <summary>
        /// Handle physical connection state changes and update interface states accordingly
        /// </summary>
        private async Task HandlePhysicalConnectionStateChangeAsync(PhysicalConnectionStateChangedEventArgs args)
        {
            var connection = args.Connection;
            var dev1 = GetDevice(connection.Device1Name);
            var dev2 = GetDevice(connection.Device2Name);

            if (dev1 != null && dev2 != null)
            {
                var iface1 = dev1.GetInterface(connection.Interface1Name);
                var iface2 = dev2.GetInterface(connection.Interface2Name);

                // Update interface operational status based on physical connection state
                bool shouldBeUp = connection.IsOperational;

                if (iface1 != null)
                {
                    bool wasUp = iface1.IsUp;
                    iface1.IsUp = shouldBeUp && !iface1.IsShutdown;

                    if (wasUp != iface1.IsUp)
                    {
                        await _eventBus.PublishAsync(new InterfaceStateChangedEventArgs(
                            dev1.Name, iface1.Name, iface1.IsUp, iface1.IsShutdown));
                    }
                }

                if (iface2 != null)
                {
                    bool wasUp = iface2.IsUp;
                    iface2.IsUp = shouldBeUp && !iface2.IsShutdown;

                    if (wasUp != iface2.IsUp)
                    {
                        await _eventBus.PublishAsync(new InterfaceStateChangedEventArgs(
                            dev2.Name, iface2.Name, iface2.IsUp, iface2.IsShutdown));
                    }
                }

                // Publish legacy LinkChangedEvent for backward compatibility
                var changeType = args.NewState == PhysicalConnectionState.Connected
                    ? LinkChangeType.Added
                    : LinkChangeType.Removed;

                await _eventBus.PublishAsync(new LinkChangedEventArgs(
                    connection.Device1Name, connection.Interface1Name,
                    connection.Device2Name, connection.Interface2Name, changeType));
            }
        }

        private static string GenerateConnectionId(string device1, string interface1, string device2, string interface2)
        {
            // Create a consistent ID regardless of order
            var parts = new[] { $"{device1}:{interface1}", $"{device2}:{interface2}" };
            Array.Sort(parts);
            return $"CONN_{string.Join("_", parts)}";
        }
    }

    /// <summary>
    /// Network statistics for monitoring
    /// </summary>
    public class NetworkStatistics
    {
        public int TotalDevices { get; set; }
        public int TotalConnections { get; set; }
        public int OperationalConnections { get; set; }
        public int FailedConnections { get; set; }
        public int DegradedConnections { get; set; }

        public double ConnectionReliability => TotalConnections > 0
            ? (double)OperationalConnections / TotalConnections * 100
            : 0;
    }
}
