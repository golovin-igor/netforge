using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.DataTypes.NetworkPrimitives
{
    /// <summary>
    /// Represents the physical connection state between interfaces
    /// </summary>
    public enum PhysicalConnectionState
    {
        Connected,
        Disconnected,
        Failed,
        Degraded
    }

    /// <summary>
    /// Represents the type of physical connection/cable
    /// </summary>
    public enum PhysicalConnectionType
    {
        Ethernet,
        Serial,
        Fiber,
        Wireless,
        Unknown
    }

    /// <summary>
    /// Represents a physical connection between two network device interfaces
    /// This entity simulates the real physical layer connectivity that protocols must respect
    /// </summary>
    public class PhysicalConnection
    {
        public string Id { get; private set; }
        public string Device1Name { get; private set; }
        public string Interface1Name { get; private set; }
        public string Device2Name { get; private set; }
        public string Interface2Name { get; private set; }

        public PhysicalConnectionState State { get; private set; }
        public PhysicalConnectionType ConnectionType { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastStateChange { get; private set; }

        // Physical layer properties
        public int Bandwidth { get; private set; } // Mbps
        public int Latency { get; private set; } // milliseconds
        public double PacketLoss { get; private set; } // percentage
        public int MaxTransmissionUnit { get; private set; } // bytes

        // Cable/connection properties
        public double CableLength { get; private set; } // meters
        public string CableType { get; private set; }
        public bool AutoNegotiation { get; private set; }

        // Connection health metrics
        public int ErrorCount { get; private set; }
        public int RetransmissionCount { get; private set; }
        public DateTime LastErrorTime { get; private set; }

        // Events
        public event Func<PhysicalConnectionStateChangedEventArgs, Task> StateChanged;

        public PhysicalConnection(string device1Name, string interface1Name,
            string device2Name, string interface2Name,
            PhysicalConnectionType connectionType = PhysicalConnectionType.Ethernet)
        {
            Id = GenerateConnectionId(device1Name, interface1Name, device2Name, interface2Name);
            Device1Name = device1Name;
            Interface1Name = interface1Name;
            Device2Name = device2Name;
            Interface2Name = interface2Name;

            ConnectionType = connectionType;
            State = PhysicalConnectionState.Disconnected;
            CreatedAt = DateTime.UtcNow;
            LastStateChange = DateTime.UtcNow;

            // Set default physical properties based on connection type
            SetDefaultPropertiesForType(connectionType);

            AutoNegotiation = true;
            CableType = GetDefaultCableType(connectionType);
            CableLength = 5.0; // Default 5 meters
        }

        /// <summary>
        /// Establish the physical connection
        /// </summary>
        public async Task ConnectAsync()
        {
            if (State != PhysicalConnectionState.Connected)
            {
                var previousState = State;
                State = PhysicalConnectionState.Connected;
                LastStateChange = DateTime.UtcNow;

                await OnStateChangedAsync(previousState, State);
            }
        }

        /// <summary>
        /// Disconnect the physical connection
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (State != PhysicalConnectionState.Disconnected)
            {
                var previousState = State;
                State = PhysicalConnectionState.Disconnected;
                LastStateChange = DateTime.UtcNow;

                await OnStateChangedAsync(previousState, State);
            }
        }

        /// <summary>
        /// Set connection to failed state (simulates cable/hardware failure)
        /// </summary>
        public async Task SetFailedAsync(string reason = "Hardware failure")
        {
            if (State != PhysicalConnectionState.Failed)
            {
                var previousState = State;
                State = PhysicalConnectionState.Failed;
                LastStateChange = DateTime.UtcNow;
                LastErrorTime = DateTime.UtcNow;
                ErrorCount++;

                await OnStateChangedAsync(previousState, State, reason);
            }
        }

        /// <summary>
        /// Set connection to degraded state (simulates poor cable quality)
        /// </summary>
        public async Task SetDegradedAsync(double packetLoss, int additionalLatency, string reason = "Signal degradation")
        {
            var previousState = State;
            State = PhysicalConnectionState.Degraded;
            PacketLoss = Math.Min(packetLoss, 100.0); // Cap at 100%
            Latency += additionalLatency;
            LastStateChange = DateTime.UtcNow;
            ErrorCount++;

            await OnStateChangedAsync(previousState, State, reason);
        }

        /// <summary>
        /// Restore connection from degraded/failed state
        /// </summary>
        public async Task RestoreAsync()
        {
            if (State == PhysicalConnectionState.Failed || State == PhysicalConnectionState.Degraded)
            {
                var previousState = State;
                State = PhysicalConnectionState.Connected;

                // Reset degraded properties
                SetDefaultPropertiesForType(ConnectionType);

                LastStateChange = DateTime.UtcNow;

                await OnStateChangedAsync(previousState, State, "Connection restored");
            }
        }

        /// <summary>
        /// Check if the connection is operational (connected and not failed)
        /// </summary>
        public bool IsOperational => State == PhysicalConnectionState.Connected || State == PhysicalConnectionState.Degraded;

        /// <summary>
        /// Check if the connection affects the specified device interface
        /// </summary>
        public bool InvolvesInterface(string deviceName, string interfaceName)
        {
            return (Device1Name == deviceName && Interface1Name == interfaceName) ||
                   (Device2Name == deviceName && Interface2Name == interfaceName);
        }

        /// <summary>
        /// Get the remote device and interface for a given local device interface
        /// </summary>
        public (string deviceName, string interfaceName)? GetRemoteEnd(string localDeviceName, string localInterfaceName)
        {
            if (Device1Name == localDeviceName && Interface1Name == localInterfaceName)
                return (Device2Name, Interface2Name);

            if (Device2Name == localDeviceName && Interface2Name == localInterfaceName)
                return (Device1Name, Interface1Name);

            return null;
        }

        /// <summary>
        /// Simulate packet transmission with physical layer effects
        /// </summary>
        public PhysicalTransmissionResult SimulateTransmission(int packetSize)
        {
            if (!IsOperational)
                return new PhysicalTransmissionResult { Success = false, Reason = "Connection not operational" };

            // Simulate packet loss
            var random = new Random();
            if (random.NextDouble() * 100 < PacketLoss)
            {
                return new PhysicalTransmissionResult { Success = false, Reason = "Packet lost" };
            }

            // Check MTU
            if (packetSize > MaxTransmissionUnit)
            {
                return new PhysicalTransmissionResult { Success = false, Reason = "Packet exceeds MTU" };
            }

            // Calculate transmission time
            var transmissionTime = CalculateTransmissionTime(packetSize);

            return new PhysicalTransmissionResult
            {
                Success = true,
                TransmissionTime = transmissionTime,
                ActualLatency = Latency + (int)(transmissionTime * 1000) // Convert to milliseconds
            };
        }

        private async Task OnStateChangedAsync(PhysicalConnectionState previousState, PhysicalConnectionState newState, string reason = "")
        {
            var eventArgs = new PhysicalConnectionStateChangedEventArgs(
                this, previousState, newState, reason);

            StateChanged?.Invoke(eventArgs);
        }

        private void SetDefaultPropertiesForType(PhysicalConnectionType type)
        {
            switch (type)
            {
                case PhysicalConnectionType.Ethernet:
                    Bandwidth = 1000; // 1 Gbps
                    Latency = 1; // 1ms
                    PacketLoss = 0.0;
                    MaxTransmissionUnit = 1500;
                    break;

                case PhysicalConnectionType.Serial:
                    Bandwidth = 2; // 2 Mbps
                    Latency = 5; // 5ms
                    PacketLoss = 0.1;
                    MaxTransmissionUnit = 1500;
                    break;

                case PhysicalConnectionType.Fiber:
                    Bandwidth = 10000; // 10 Gbps
                    Latency = 0; // Near 0ms
                    PacketLoss = 0.0;
                    MaxTransmissionUnit = 9000; // Jumbo frames
                    break;

                case PhysicalConnectionType.Wireless:
                    Bandwidth = 54; // 54 Mbps
                    Latency = 10; // 10ms
                    PacketLoss = 1.0; // 1%
                    MaxTransmissionUnit = 1500;
                    break;

                default:
                    Bandwidth = 100; // 100 Mbps
                    Latency = 2; // 2ms
                    PacketLoss = 0.0;
                    MaxTransmissionUnit = 1500;
                    break;
            }
        }

        private string GetDefaultCableType(PhysicalConnectionType type)
        {
            return type switch
            {
                PhysicalConnectionType.Ethernet => "Cat6",
                PhysicalConnectionType.Serial => "V.35",
                PhysicalConnectionType.Fiber => "Single-mode",
                PhysicalConnectionType.Wireless => "802.11ac",
                _ => "Unknown"
            };
        }

        private double CalculateTransmissionTime(int packetSize)
        {
            // Transmission time = packet size (bits) / bandwidth (bits per second)
            return (packetSize * 8.0) / (Bandwidth * 1_000_000.0); // Convert Mbps to bps
        }

        private static string GenerateConnectionId(string device1, string interface1, string device2, string interface2)
        {
            // Create a consistent ID regardless of order
            var parts = new[] { $"{device1}:{interface1}", $"{device2}:{interface2}" };
            Array.Sort(parts);
            return $"CONN_{string.Join("_", parts)}";
        }

        public override string ToString()
        {
            return $"{Device1Name}:{Interface1Name} <-> {Device2Name}:{Interface2Name} ({State})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is PhysicalConnection other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    /// <summary>
    /// Result of a simulated packet transmission over a physical connection
    /// </summary>
    public class PhysicalTransmissionResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; } = "";
        public double TransmissionTime { get; set; } // seconds
        public int ActualLatency { get; set; } // milliseconds
    }
}
