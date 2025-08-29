

namespace NetForge.Simulation.Common.Events
{
    public enum DeviceChangeType
    {
        Added,
        Removed
    }

    public class DeviceChangedEventArgs(string deviceName, object device, DeviceChangeType changeType) : NetworkEventArgs
    {
        public string? DeviceName { get; } = deviceName;
        public object Device { get; } = device; // Reference to the device instance
        public DeviceChangeType ChangeType { get; } = changeType;
    }
}
