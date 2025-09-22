using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Factory for creating actual network topologies from declarative specifications
    /// Orchestrates device creation and physical connection establishment
    /// </summary>
    public class DeclarativeTopologyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DeclarativeDeviceFactory _deviceFactory;

        public DeclarativeTopologyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _deviceFactory = new DeclarativeDeviceFactory(serviceProvider);
        }

        /// <summary>
        /// Create a complete network topology from a declarative specification
        /// Step 1: Create all devices
        /// Step 2: Establish physical connections between device interfaces
        /// </summary>
        public async Task<INetworkTopology> CreateTopologyAsync(TopologySpec spec)
        {
            spec.Validate();

            // Create the topology container
            var topology = new NetworkTopology(spec.Name);

            // Step 1: Create all devices using the device factory
            var deviceMap = new Dictionary<string, INetworkDevice>();
            foreach (var deviceSpec in spec.Devices)
            {
                var device = await _deviceFactory.CreateDeviceAsync(deviceSpec);
                deviceMap[deviceSpec.Name] = device;
                topology.AddDevice(device);
            }

            // Step 2: Create physical connections between device interfaces
            foreach (var connectionSpec in spec.Connections)
            {
                await EstablishPhysicalConnectionAsync(topology, deviceMap, connectionSpec);
            }

            // Apply global network settings if specified
            if (spec.Settings != null)
            {
                ApplyNetworkSettings(topology, spec.Settings);
            }

            return topology;
        }

        /// <summary>
        /// Establish a physical connection between two device interfaces
        /// </summary>
        private async Task EstablishPhysicalConnectionAsync(
            INetworkTopology topology, 
            Dictionary<string, INetworkDevice> deviceMap, 
            ConnectionSpec connectionSpec)
        {
            // Get source and destination devices
            var sourceDevice = deviceMap[connectionSpec.Source.DeviceName];
            var destDevice = deviceMap[connectionSpec.Destination.DeviceName];

            // Get source and destination interfaces
            var sourceInterface = GetDeviceInterface(sourceDevice, connectionSpec.Source.InterfaceName);
            var destInterface = GetDeviceInterface(destDevice, connectionSpec.Destination.InterfaceName);

            if (sourceInterface == null)
                throw new InvalidOperationException($"Interface '{connectionSpec.Source.InterfaceName}' not found on device '{connectionSpec.Source.DeviceName}'");

            if (destInterface == null)
                throw new InvalidOperationException($"Interface '{connectionSpec.Destination.InterfaceName}' not found on device '{connectionSpec.Destination.DeviceName}'");

            // Create the physical connection
            var connection = CreatePhysicalConnection(connectionSpec, sourceInterface, destInterface);
            
            // Establish the connection in the topology
            topology.AddConnection(connection);

            // Configure interface states based on connection
            ConfigureInterfaceConnection(sourceInterface, destInterface, connectionSpec);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get an interface from a device by name
        /// </summary>
        private IInterfaceConfig? GetDeviceInterface(INetworkDevice device, string interfaceName)
        {
            if (device is NetworkDevice networkDevice)
            {
                return networkDevice.GetInterface(interfaceName);
            }

            // For other implementations, try reflection
            var interfacesProperty = device.GetType().GetProperty("Interfaces");
            if (interfacesProperty?.GetValue(device) is IEnumerable<IInterfaceConfig> interfaces)
            {
                return interfaces.FirstOrDefault(i => i.Name == interfaceName);
            }

            return null;
        }

        /// <summary>
        /// Create a physical connection object from the connection specification
        /// </summary>
        private IPhysicalConnection CreatePhysicalConnection(
            ConnectionSpec spec, 
            IInterfaceConfig sourceInterface, 
            IInterfaceConfig destInterface)
        {
            var connection = new PhysicalConnection
            {
                Id = spec.Id ?? Guid.NewGuid().ToString(),
                SourceInterface = sourceInterface,
                DestinationInterface = destInterface,
                ConnectionType = spec.ConnectionType,
                Properties = spec.Properties
            };

            // Apply cable specifications if provided
            if (spec.Cable != null)
            {
                ApplyCableSpecifications(connection, spec.Cable);
            }

            return connection;
        }

        /// <summary>
        /// Apply cable specifications to the physical connection
        /// </summary>
        private void ApplyCableSpecifications(IPhysicalConnection connection, CableSpec cable)
        {
            connection.Properties["CableType"] = cable.Type;
            connection.Properties["CableLengthMeters"] = cable.LengthMeters;

            // Apply signal quality settings if specified
            if (cable.SignalQuality != null)
            {
                connection.Properties["PacketLossPercent"] = cable.SignalQuality.PacketLossPercent;
                connection.Properties["LatencyMs"] = cable.SignalQuality.LatencyMs;
                connection.Properties["JitterMs"] = cable.SignalQuality.JitterMs;
                
                if (cable.SignalQuality.BandwidthLimitMbps > 0)
                {
                    connection.Properties["BandwidthLimitMbps"] = cable.SignalQuality.BandwidthLimitMbps;
                }
            }
        }

        /// <summary>
        /// Configure interface connection states
        /// </summary>
        private void ConfigureInterfaceConnection(
            IInterfaceConfig sourceInterface, 
            IInterfaceConfig destInterface, 
            ConnectionSpec connectionSpec)
        {
            // Mark interfaces as connected
            sourceInterface.IsConnected = true;
            destInterface.IsConnected = true;

            // Set connection properties on interfaces
            sourceInterface.ConnectedTo = destInterface;
            destInterface.ConnectedTo = sourceInterface;

            // Apply any interface-specific connection properties
            foreach (var (key, value) in connectionSpec.Properties)
            {
                if (key.StartsWith("Source."))
                {
                    var interfaceProperty = key.Substring(7);
                    SetInterfaceProperty(sourceInterface, interfaceProperty, value);
                }
                else if (key.StartsWith("Destination."))
                {
                    var interfaceProperty = key.Substring(12);
                    SetInterfaceProperty(destInterface, interfaceProperty, value);
                }
            }
        }

        /// <summary>
        /// Set a property on an interface using reflection
        /// </summary>
        private void SetInterfaceProperty(IInterfaceConfig iface, string propertyName, object value)
        {
            try
            {
                var property = iface.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(iface, convertedValue);
                }
            }
            catch (Exception ex)
            {
                // Log property setting error but continue
                Console.WriteLine($"Failed to set {propertyName} on interface {iface.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply global network settings to the topology
        /// </summary>
        private void ApplyNetworkSettings(INetworkTopology topology, NetworkSettingsSpec settings)
        {
            // Apply simulation settings
            if (settings.Simulation != null)
            {
                topology.SetProperty("TimeScale", settings.Simulation.TimeScale);
                topology.SetProperty("RealTimeMode", settings.Simulation.RealTimeMode);
                topology.SetProperty("LogLevel", settings.Simulation.LogLevel);
            }

            // Apply default protocol settings to all devices
            if (settings.DefaultProtocolSettings.Count > 0)
            {
                foreach (var device in topology.GetDevices())
                {
                    ApplyDefaultProtocolSettings(device, settings.DefaultProtocolSettings);
                }
            }
        }

        /// <summary>
        /// Apply default protocol settings to a device
        /// </summary>
        private void ApplyDefaultProtocolSettings(INetworkDevice device, Dictionary<string, object> defaultSettings)
        {
            foreach (var (key, value) in defaultSettings)
            {
                try
                {
                    // Try to set the property directly on the device
                    var property = device.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanWrite)
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(device, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    // Log setting error but continue
                    Console.WriteLine($"Failed to set default setting {key} on device {device.Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Interface for network topology
    /// </summary>
    public interface INetworkTopology
    {
        string Name { get; }
        void AddDevice(INetworkDevice device);
        void AddConnection(IPhysicalConnection connection);
        IEnumerable<INetworkDevice> GetDevices();
        IEnumerable<IPhysicalConnection> GetConnections();
        void SetProperty(string name, object value);
    }

    /// <summary>
    /// Interface for physical connections between device interfaces
    /// </summary>
    public interface IPhysicalConnection
    {
        string Id { get; }
        IInterfaceConfig SourceInterface { get; }
        IInterfaceConfig DestinationInterface { get; }
        PhysicalConnectionType ConnectionType { get; }
        Dictionary<string, object> Properties { get; }
    }

    /// <summary>
    /// Concrete implementation of network topology
    /// </summary>
    public class NetworkTopology : INetworkTopology
    {
        private readonly List<INetworkDevice> _devices = [];
        private readonly List<IPhysicalConnection> _connections = [];
        private readonly Dictionary<string, object> _properties = [];

        public NetworkTopology(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public void AddDevice(INetworkDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            
            if (_devices.Any(d => d.Name == device.Name))
                throw new InvalidOperationException($"Device with name '{device.Name}' already exists in topology");

            _devices.Add(device);
        }

        public void AddConnection(IPhysicalConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            
            if (_connections.Any(c => c.Id == connection.Id))
                throw new InvalidOperationException($"Connection with ID '{connection.Id}' already exists in topology");

            _connections.Add(connection);
        }

        public IEnumerable<INetworkDevice> GetDevices() => _devices.AsReadOnly();
        public IEnumerable<IPhysicalConnection> GetConnections() => _connections.AsReadOnly();

        public void SetProperty(string name, object value)
        {
            _properties[name] = value;
        }

        public T? GetProperty<T>(string name)
        {
            return _properties.TryGetValue(name, out var value) && value is T typed ? typed : default;
        }
    }

    /// <summary>
    /// Concrete implementation of physical connection
    /// </summary>
    public class PhysicalConnection : IPhysicalConnection
    {
        public required string Id { get; init; }
        public required IInterfaceConfig SourceInterface { get; init; }
        public required IInterfaceConfig DestinationInterface { get; init; }
        public PhysicalConnectionType ConnectionType { get; init; } = PhysicalConnectionType.Ethernet;
        public Dictionary<string, object> Properties { get; init; } = [];
    }

    /// <summary>
    /// Extension to IInterfaceConfig for connection tracking
    /// </summary>
    public static class InterfaceConfigExtensions
    {
        public static void SetConnectedInterface(this IInterfaceConfig iface, IInterfaceConfig connectedInterface)
        {
            // Try to set ConnectedTo property if it exists
            var connectedToProperty = iface.GetType().GetProperty("ConnectedTo");
            if (connectedToProperty != null && connectedToProperty.CanWrite)
            {
                connectedToProperty.SetValue(iface, connectedInterface);
            }

            // Try to set IsConnected property if it exists
            var isConnectedProperty = iface.GetType().GetProperty("IsConnected");
            if (isConnectedProperty != null && isConnectedProperty.CanWrite && isConnectedProperty.PropertyType == typeof(bool))
            {
                isConnectedProperty.SetValue(iface, true);
            }
        }
    }
}