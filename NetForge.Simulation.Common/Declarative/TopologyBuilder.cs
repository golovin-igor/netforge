using NetForge.Simulation.DataTypes.NetworkPrimitives;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Fluent builder for creating network topology specifications declaratively
    /// Follows the declarative topology approach: create devices -> create physical connections
    /// </summary>
    public class TopologyBuilder
    {
        private string? _name;
        private readonly List<DeviceSpec> _devices = [];
        private readonly List<ConnectionSpec> _connections = [];
        private NetworkSettingsSpec? _settings;

        /// <summary>
        /// Create a new topology builder
        /// </summary>
        /// <param name="name">Network topology name</param>
        public static TopologyBuilder Create(string name) => new TopologyBuilder { _name = name };

        /// <summary>
        /// Step 1: Add a device to the topology (each device must have unique name)
        /// </summary>
        public TopologyBuilder AddDevice(DeviceSpec device)
        {
            if (_devices.Any(d => d.Name.Equals(device.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Device '{device.Name}' already exists in topology");

            _devices.Add(device);
            return this;
        }

        /// <summary>
        /// Add a device using a builder
        /// </summary>
        public TopologyBuilder AddDevice(Func<DeviceBuilder, DeviceSpec> deviceBuilder)
        {
            var device = deviceBuilder(new DeviceBuilder());
            return AddDevice(device);
        }

        /// <summary>
        /// Add multiple devices at once
        /// </summary>
        public TopologyBuilder AddDevices(params DeviceSpec[] devices)
        {
            foreach (var device in devices)
                AddDevice(device);
            return this;
        }

        /// <summary>
        /// Add devices from builders
        /// </summary>
        public TopologyBuilder AddDevices(params Func<DeviceBuilder, DeviceSpec>[] deviceBuilders)
        {
            foreach (var builder in deviceBuilders)
                AddDevice(builder);
            return this;
        }

        /// <summary>
        /// Step 2: Create a physical connection between device interfaces
        /// </summary>
        public TopologyBuilder Connect(string sourceDevice, string sourceInterface, 
            string destDevice, string destInterface, 
            PhysicalConnectionType connectionType = PhysicalConnectionType.Ethernet,
            CableSpec? cable = null)
        {
            var connection = new ConnectionSpec
            {
                Source = new EndpointSpec { DeviceName = sourceDevice, InterfaceName = sourceInterface },
                Destination = new EndpointSpec { DeviceName = destDevice, InterfaceName = destInterface },
                ConnectionType = connectionType,
                Cable = cable
            };

            _connections.Add(connection);
            return this;
        }

        /// <summary>
        /// Create a connection with cable specifications
        /// </summary>
        public TopologyBuilder Connect(string sourceDevice, string sourceInterface,
            string destDevice, string destInterface,
            string cableType, double cableLengthMeters = 1.0,
            PhysicalConnectionType connectionType = PhysicalConnectionType.Ethernet)
        {
            var cable = new CableSpec { Type = cableType, LengthMeters = cableLengthMeters };
            return Connect(sourceDevice, sourceInterface, destDevice, destInterface, connectionType, cable);
        }

        /// <summary>
        /// Create a connection with signal degradation
        /// </summary>
        public TopologyBuilder ConnectWithQuality(string sourceDevice, string sourceInterface,
            string destDevice, string destInterface,
            double packetLossPercent = 0.0, double latencyMs = 0.0, double jitterMs = 0.0,
            PhysicalConnectionType connectionType = PhysicalConnectionType.Ethernet)
        {
            var cable = new CableSpec
            {
                Type = "Cat6",
                LengthMeters = 1.0,
                SignalQuality = new SignalQualitySpec
                {
                    PacketLossPercent = packetLossPercent,
                    LatencyMs = latencyMs,
                    JitterMs = jitterMs
                }
            };

            return Connect(sourceDevice, sourceInterface, destDevice, destInterface, connectionType, cable);
        }

        /// <summary>
        /// Create multiple connections from a single device to multiple destinations
        /// </summary>
        public TopologyBuilder ConnectStar(string hubDevice, string hubInterface,
            params (string device, string iface)[] spokes)
        {
            foreach (var (device, iface) in spokes)
            {
                Connect(hubDevice, hubInterface, device, iface);
            }
            return this;
        }

        /// <summary>
        /// Create a linear chain of connections between devices
        /// </summary>
        public TopologyBuilder ConnectChain(params (string device, string iface)[] chain)
        {
            for (int i = 0; i < chain.Length - 1; i++)
            {
                var current = chain[i];
                var next = chain[i + 1];
                Connect(current.device, current.iface, next.device, next.iface);
            }
            return this;
        }

        /// <summary>
        /// Create a full mesh topology between devices
        /// </summary>
        public TopologyBuilder ConnectFullMesh(params (string device, string iface)[] endpoints)
        {
            for (int i = 0; i < endpoints.Length; i++)
            {
                for (int j = i + 1; j < endpoints.Length; j++)
                {
                    var source = endpoints[i];
                    var dest = endpoints[j];
                    Connect(source.device, source.iface, dest.device, dest.iface);
                }
            }
            return this;
        }

        /// <summary>
        /// Set global network settings
        /// </summary>
        public TopologyBuilder WithSettings(NetworkSettingsSpec settings)
        {
            _settings = settings;
            return this;
        }

        /// <summary>
        /// Set simulation settings
        /// </summary>
        public TopologyBuilder WithSimulation(double timeScale = 1.0, bool realTimeMode = false, string logLevel = "Info")
        {
            _settings = new NetworkSettingsSpec
            {
                Simulation = new SimulationSettingsSpec
                {
                    TimeScale = timeScale,
                    RealTimeMode = realTimeMode,
                    LogLevel = logLevel
                }
            };
            return this;
        }

        /// <summary>
        /// Build the complete topology specification
        /// </summary>
        public TopologySpec Build()
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException("Network name is required");

            if (_devices.Count == 0)
                throw new InvalidOperationException("At least one device is required");

            var spec = new TopologySpec
            {
                Name = _name,
                Devices = _devices,
                Connections = _connections,
                Settings = _settings
            };

            spec.Validate();
            return spec;
        }

        /// <summary>
        /// Get a device by name for further configuration
        /// </summary>
        public DeviceSpec? GetDevice(string name)
        {
            return _devices.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get all connections for a device
        /// </summary>
        public List<ConnectionSpec> GetDeviceConnections(string deviceName)
        {
            return _connections.Where(c =>
                c.Source.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) ||
                c.Destination.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        /// <summary>
        /// Validate the current state without building
        /// </summary>
        public void ValidateCurrentState()
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException("Network name is required");

            // Check for duplicate device names
            var deviceNames = _devices.Select(d => d.Name).ToList();
            if (deviceNames.Count != deviceNames.Distinct().Count())
                throw new InvalidOperationException("Duplicate device names found");

            // Validate each connection references existing devices and interfaces
            foreach (var connection in _connections)
            {
                var sourceDevice = _devices.FirstOrDefault(d => d.Name == connection.Source.DeviceName);
                if (sourceDevice == null)
                    throw new InvalidOperationException($"Connection source device '{connection.Source.DeviceName}' not found");

                if (!sourceDevice.Interfaces.Any(i => i.Name == connection.Source.InterfaceName))
                    throw new InvalidOperationException($"Interface '{connection.Source.InterfaceName}' not found on device '{connection.Source.DeviceName}'");

                var destDevice = _devices.FirstOrDefault(d => d.Name == connection.Destination.DeviceName);
                if (destDevice == null)
                    throw new InvalidOperationException($"Connection destination device '{connection.Destination.DeviceName}' not found");

                if (!destDevice.Interfaces.Any(i => i.Name == connection.Destination.InterfaceName))
                    throw new InvalidOperationException($"Interface '{connection.Destination.InterfaceName}' not found on device '{connection.Destination.DeviceName}'");
            }
        }
    }

    /// <summary>
    /// Helper class for creating common network topologies
    /// </summary>
    public static class CommonTopologies
    {
        /// <summary>
        /// Create a simple two-device point-to-point topology
        /// </summary>
        public static TopologyBuilder PointToPoint(string name, DeviceSpec device1, DeviceSpec device2, 
            string interface1 = "ge-0/0/0", string interface2 = "ge-0/0/0")
        {
            return TopologyBuilder.Create(name)
                .AddDevice(device1)
                .AddDevice(device2)
                .Connect(device1.Name, interface1, device2.Name, interface2);
        }

        /// <summary>
        /// Create a hub-and-spoke topology
        /// </summary>
        public static TopologyBuilder HubAndSpoke(string name, DeviceSpec hubDevice, params DeviceSpec[] spokeDevices)
        {
            var builder = TopologyBuilder.Create(name)
                .AddDevice(hubDevice)
                .AddDevices(spokeDevices);

            // Connect each spoke to the hub
            for (int i = 0; i < spokeDevices.Length; i++)
            {
                var hubInterface = $"GigabitEthernet0/{i}";
                var spokeInterface = "GigabitEthernet0/0";
                
                builder.Connect(hubDevice.Name, hubInterface, spokeDevices[i].Name, spokeInterface);
            }

            return builder;
        }

        /// <summary>
        /// Create a linear topology (daisy chain)
        /// </summary>
        public static TopologyBuilder Linear(string name, params DeviceSpec[] devices)
        {
            var builder = TopologyBuilder.Create(name).AddDevices(devices);

            // Connect devices in sequence
            for (int i = 0; i < devices.Length - 1; i++)
            {
                builder.Connect(devices[i].Name, "GigabitEthernet0/1", 
                              devices[i + 1].Name, "GigabitEthernet0/0");
            }

            return builder;
        }

        /// <summary>
        /// Create a ring topology
        /// </summary>
        public static TopologyBuilder Ring(string name, params DeviceSpec[] devices)
        {
            if (devices.Length < 3)
                throw new ArgumentException("Ring topology requires at least 3 devices");

            var builder = Linear(name, devices);
            
            // Close the ring
            var lastDevice = devices[^1];
            var firstDevice = devices[0];
            builder.Connect(lastDevice.Name, "GigabitEthernet0/2", 
                          firstDevice.Name, "GigabitEthernet0/2");

            return builder;
        }

        /// <summary>
        /// Create a tree topology with core, distribution, and access layers
        /// </summary>
        public static TopologyBuilder ThreeTier(string name, 
            DeviceSpec coreDevice, DeviceSpec[] distributionDevices, DeviceSpec[][] accessDevices)
        {
            var builder = TopologyBuilder.Create(name)
                .AddDevice(coreDevice)
                .AddDevices(distributionDevices.SelectMany(d => new[] { d }).ToArray());

            // Add all access devices
            foreach (var accessGroup in accessDevices)
            {
                builder.AddDevices(accessGroup);
            }

            // Connect distribution to core
            for (int i = 0; i < distributionDevices.Length; i++)
            {
                builder.Connect(coreDevice.Name, $"GigabitEthernet0/{i}", 
                              distributionDevices[i].Name, "GigabitEthernet0/0");
            }

            // Connect access to distribution
            for (int distIndex = 0; distIndex < distributionDevices.Length; distIndex++)
            {
                var distDevice = distributionDevices[distIndex];
                var accessGroup = accessDevices[distIndex];

                for (int accIndex = 0; accIndex < accessGroup.Length; accIndex++)
                {
                    builder.Connect(distDevice.Name, $"GigabitEthernet0/{accIndex + 1}", 
                                  accessGroup[accIndex].Name, "GigabitEthernet0/0");
                }
            }

            return builder;
        }
    }
}