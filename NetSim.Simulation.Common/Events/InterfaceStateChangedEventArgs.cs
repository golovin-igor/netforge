namespace NetSim.Simulation.Events
{
    public class InterfaceStateChangedEventArgs : NetworkEventArgs
    {
        public string DeviceName { get; }
        public string InterfaceName { get; }
        public bool IsUp { get; }
        public bool IsShutdown { get; }

        public InterfaceStateChangedEventArgs(string deviceName, string interfaceName, bool isUp, bool isShutdown)
        {
            DeviceName = deviceName;
            InterfaceName = interfaceName;
            IsUp = isUp;
            IsShutdown = isShutdown; // Reflects admin state
        }
    }
} 
