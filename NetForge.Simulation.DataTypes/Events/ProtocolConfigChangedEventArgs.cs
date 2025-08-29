
// For ProtocolType

using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Events
{
    public class ProtocolConfigChangedEventArgs(string deviceName, ProtocolType protocolType, string changeDetails = "Configuration updated")
        : NetworkEventArgs
    {
        public string DeviceName { get; } = deviceName;
        public ProtocolType ProtocolType { get; } = protocolType;
        public string ChangeDetails { get; } = changeDetails; // Generic details, e.g., "OSPF process started", "BGP neighbor added: 1.1.1.1"
    }
}
