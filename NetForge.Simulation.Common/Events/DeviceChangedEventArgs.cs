using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Common.Events
{
    public enum DeviceChangeType
    {
        Added,
        Removed
    }

    public class DeviceChangedEventArgs(INetworkDevice device, DeviceChangeType changeType) : NetworkEventArgs
    {
        public string? DeviceName { get; } = device.Name;
        public INetworkDevice Device { get; } = device; // Reference to the device instance
        public DeviceChangeType ChangeType { get; } = changeType;
    }
}
