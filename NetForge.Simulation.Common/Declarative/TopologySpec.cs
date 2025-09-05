using NetForge.Simulation.DataTypes.NetworkPrimitives;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Declarative specification for a complete network topology
    /// Defines devices and their physical interconnections
    /// </summary>
    public class TopologySpec
    {
        /// <summary>
        /// Network name/identifier
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// All devices in the topology
        /// Each device must have a unique name
        /// </summary>
        public required List<DeviceSpec> Devices { get; init; } = [];

        /// <summary>
        /// Physical connections between device interfaces
        /// </summary>
        public required List<ConnectionSpec> Connections { get; init; } = [];

        /// <summary>
        /// Global network settings
        /// </summary>
        public NetworkSettingsSpec? Settings { get; init; }

        /// <summary>
        /// Validate the complete topology specification
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Network name is required");

            if (Devices.Count == 0)
                throw new InvalidOperationException("At least one device must be specified");

            // Validate unique device names
            var deviceNames = Devices.Select(d => d.Name).ToList();
            if (deviceNames.Count != deviceNames.Distinct().Count())
                throw new InvalidOperationException("Device names must be unique within the topology");

            // Validate each device
            foreach (var device in Devices)
                device.Validate();

            // Validate each connection
            foreach (var connection in Connections)
                connection.Validate(deviceNames, Devices);

            Settings?.Validate();
        }

        /// <summary>
        /// Get a device by name
        /// </summary>
        public DeviceSpec? GetDevice(string deviceName)
        {
            return Devices.FirstOrDefault(d => d.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get all connections for a specific device
        /// </summary>
        public List<ConnectionSpec> GetDeviceConnections(string deviceName)
        {
            return Connections.Where(c => 
                c.Source.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase) ||
                c.Destination.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }

    /// <summary>
    /// Physical connection specification between two device interfaces
    /// </summary>
    public class ConnectionSpec
    {
        /// <summary>
        /// Source endpoint (device + interface)
        /// </summary>
        public required EndpointSpec Source { get; init; }

        /// <summary>
        /// Destination endpoint (device + interface)
        /// </summary>
        public required EndpointSpec Destination { get; init; }

        /// <summary>
        /// Type of physical connection
        /// </summary>
        public PhysicalConnectionType ConnectionType { get; init; } = PhysicalConnectionType.Ethernet;

        /// <summary>
        /// Cable specifications (optional)
        /// </summary>
        public CableSpec? Cable { get; init; }

        /// <summary>
        /// Connection-specific settings
        /// </summary>
        public Dictionary<string, object> Properties { get; init; } = [];

        /// <summary>
        /// Connection identifier (auto-generated if not specified)
        /// </summary>
        public string? Id => $"{Source.DeviceName}:{Source.InterfaceName}<->{Destination.DeviceName}:{Destination.InterfaceName}";

        public void Validate(List<string> availableDevices, List<DeviceSpec> devices)
        {
            Source.Validate();
            Destination.Validate();

            // Validate that referenced devices exist
            if (!availableDevices.Contains(Source.DeviceName))
                throw new InvalidOperationException($"Source device '{Source.DeviceName}' not found in topology");

            if (!availableDevices.Contains(Destination.DeviceName))
                throw new InvalidOperationException($"Destination device '{Destination.DeviceName}' not found in topology");

            // Validate that referenced interfaces exist on their respective devices
            var sourceDevice = devices.First(d => d.Name == Source.DeviceName);
            if (!sourceDevice.Interfaces.Any(i => i.Name == Source.InterfaceName))
                throw new InvalidOperationException($"Interface '{Source.InterfaceName}' not found on device '{Source.DeviceName}'");

            var destDevice = devices.First(d => d.Name == Destination.DeviceName);
            if (!destDevice.Interfaces.Any(i => i.Name == Destination.InterfaceName))
                throw new InvalidOperationException($"Interface '{Destination.InterfaceName}' not found on device '{Destination.DeviceName}'");

            // Validate that we're not connecting a device to itself on the same interface
            if (Source.DeviceName == Destination.DeviceName && Source.InterfaceName == Destination.InterfaceName)
                throw new InvalidOperationException("Cannot connect an interface to itself");

            Cable?.Validate();
        }
    }

    /// <summary>
    /// Connection endpoint (device + interface)
    /// </summary>
    public class EndpointSpec
    {
        /// <summary>
        /// Device name
        /// </summary>
        public required string DeviceName { get; init; }

        /// <summary>
        /// Interface name (must exist on the specified device)
        /// </summary>
        public required string InterfaceName { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DeviceName))
                throw new InvalidOperationException("Device name is required for endpoint");

            if (string.IsNullOrWhiteSpace(InterfaceName))
                throw new InvalidOperationException("Interface name is required for endpoint");
        }

        public override string ToString() => $"{DeviceName}:{InterfaceName}";
    }

    /// <summary>
    /// Cable specification for physical connections
    /// </summary>
    public class CableSpec
    {
        /// <summary>
        /// Cable type (e.g., "Cat6", "Fiber", "Serial")
        /// </summary>
        public required string Type { get; init; }

        /// <summary>
        /// Cable length in meters
        /// </summary>
        public double LengthMeters { get; init; } = 1.0;

        /// <summary>
        /// Signal quality/degradation settings
        /// </summary>
        public SignalQualitySpec? SignalQuality { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Type))
                throw new InvalidOperationException("Cable type is required");

            if (LengthMeters <= 0)
                throw new InvalidOperationException("Cable length must be positive");

            SignalQuality?.Validate();
        }
    }

    /// <summary>
    /// Signal quality and degradation settings
    /// </summary>
    public class SignalQualitySpec
    {
        /// <summary>
        /// Packet loss percentage (0.0 to 100.0)
        /// </summary>
        public double PacketLossPercent { get; init; } = 0.0;

        /// <summary>
        /// Additional latency in milliseconds
        /// </summary>
        public double LatencyMs { get; init; } = 0.0;

        /// <summary>
        /// Jitter in milliseconds
        /// </summary>
        public double JitterMs { get; init; } = 0.0;

        /// <summary>
        /// Bandwidth limitation in Mbps (0 = no limitation)
        /// </summary>
        public int BandwidthLimitMbps { get; init; } = 0;

        public void Validate()
        {
            if (PacketLossPercent < 0.0 || PacketLossPercent > 100.0)
                throw new InvalidOperationException("Packet loss must be between 0.0 and 100.0 percent");

            if (LatencyMs < 0.0)
                throw new InvalidOperationException("Latency cannot be negative");

            if (JitterMs < 0.0)
                throw new InvalidOperationException("Jitter cannot be negative");

            if (BandwidthLimitMbps < 0)
                throw new InvalidOperationException("Bandwidth limit cannot be negative");
        }
    }

    /// <summary>
    /// Global network settings
    /// </summary>
    public class NetworkSettingsSpec
    {
        /// <summary>
        /// Global simulation settings
        /// </summary>
        public SimulationSettingsSpec? Simulation { get; init; }

        /// <summary>
        /// Default protocol settings
        /// </summary>
        public Dictionary<string, object> DefaultProtocolSettings { get; init; } = [];

        public void Validate()
        {
            Simulation?.Validate();
        }
    }

    /// <summary>
    /// Simulation-specific settings
    /// </summary>
    public class SimulationSettingsSpec
    {
        /// <summary>
        /// Simulation time scale factor
        /// </summary>
        public double TimeScale { get; init; } = 1.0;

        /// <summary>
        /// Whether to enable real-time mode
        /// </summary>
        public bool RealTimeMode { get; init; } = false;

        /// <summary>
        /// Logging level
        /// </summary>
        public string LogLevel { get; init; } = "Info";

        public void Validate()
        {
            if (TimeScale <= 0.0)
                throw new InvalidOperationException("Time scale must be positive");
        }
    }
}