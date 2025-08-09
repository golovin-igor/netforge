using NetSim.Simulation.Interfaces;

// For ProtocolType

namespace NetSim.Simulation.Events
{
    public class ProtocolConfigChangedEventArgs : NetworkEventArgs
    {
        public string DeviceName { get; }
        public ProtocolType ProtocolType { get; }
        public string ChangeDetails { get; } // Generic details, e.g., "OSPF process started", "BGP neighbor added: 1.1.1.1"

        public ProtocolConfigChangedEventArgs(string deviceName, ProtocolType protocolType, string changeDetails = "Configuration updated")
        {
            DeviceName = deviceName;
            ProtocolType = protocolType;
            ChangeDetails = changeDetails;
        }
    }
} 
