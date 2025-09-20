using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.DataTypes.Events
{
    public class InterfaceStateChangedEventArgs(string deviceName, string interfaceName, bool isUp, bool isShutdown)
        : NetworkEventArgs
    {
        public string DeviceName { get; } = deviceName;
        public string InterfaceName { get; } = interfaceName;
        public bool IsUp { get; } = isUp;
        public bool IsShutdown { get; } = isShutdown; // Reflects admin state
    }
}
