
using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.DataTypes.Events
{
    public enum DeviceChangeType
    {
        Added,
        Removed
    }

    public class DeviceChangedEventArgs(string deviceName, object device, DeviceChangeType changeType) : NetworkEventArgs, IDeviceEvent
    {
        public string DeviceName { get; } = deviceName;
        public object Device { get; } = device; // Reference to the device instance
        public DeviceChangeType ChangeType { get; } = changeType;
    }
}
