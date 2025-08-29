using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Events
{
    /// <summary>
    /// Event arguments for protocol neighbor changes (discovery, loss, state changes)
    /// </summary>
    public class ProtocolNeighborChangedEventArgs : NetworkEventArgs
    {
        /// <summary>
        /// Name of the local device reporting the neighbor change
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// The type of protocol reporting the neighbor change
        /// </summary>
        public ProtocolType ProtocolType { get; }

        /// <summary>
        /// The name of the protocol reporting the neighbor change
        /// </summary>
        public string ProtocolName { get; }

        /// <summary>
        /// Identifier of the neighbor that changed
        /// </summary>
        public string NeighborId { get; }

        /// <summary>
        /// Name of the neighbor device
        /// </summary>
        public string NeighborDeviceName { get; set; } = "";

        /// <summary>
        /// Interface through which the neighbor is reachable
        /// </summary>
        public string InterfaceName { get; }

        /// <summary>
        /// Type of neighbor change
        /// </summary>
        public NeighborChangeType ChangeType { get; }

        /// <summary>
        /// Previous neighbor state (if applicable)
        /// </summary>
        public string? PreviousState { get; set; }

        /// <summary>
        /// New neighbor state (if applicable)
        /// </summary>
        public string? NewState { get; set; }

        /// <summary>
        /// Additional details about the change
        /// </summary>
        public string ChangeDetails { get; set; } = "";

        /// <summary>
        /// Neighbor-specific data (protocol dependent)
        /// </summary>
        public Dictionary<string, object>? NeighborData { get; set; }

        /// <summary>
        /// Create protocol neighbor changed event
        /// </summary>
        /// <param name="deviceName">Name of the local device</param>
        /// <param name="protocolType">Type of protocol</param>
        /// <param name="protocolName">Name of protocol</param>
        /// <param name="neighborId">Neighbor identifier</param>
        /// <param name="changeType">Type of change</param>
        /// <param name="interfaceName">Interface name</param>
        public ProtocolNeighborChangedEventArgs(
            string deviceName,
            ProtocolType protocolType,
            string protocolName,
            string neighborId,
            NeighborChangeType changeType,
            string interfaceName)
        {
            DeviceName = deviceName;
            ProtocolType = protocolType;
            ProtocolName = protocolName;
            NeighborId = neighborId;
            ChangeType = changeType;
            InterfaceName = interfaceName;
        }

        /// <summary>
        /// String representation of the event
        /// </summary>
        /// <returns>Event description</returns>
        public override string ToString()
        {
            var stateInfo = "";
            if (!string.IsNullOrEmpty(PreviousState) && !string.IsNullOrEmpty(NewState))
            {
                stateInfo = $" ({PreviousState} -> {NewState})";
            }
            else if (!string.IsNullOrEmpty(NewState))
            {
                stateInfo = $" ({NewState})";
            }

            return $"{DeviceName}: {ProtocolName} neighbor {NeighborId} {ChangeType} on {InterfaceName}{stateInfo}";
        }
    }

    /// <summary>
    /// Types of neighbor changes
    /// </summary>
    public enum NeighborChangeType
    {
        /// <summary>
        /// New neighbor discovered
        /// </summary>
        Discovered,

        /// <summary>
        /// Neighbor lost (timeout, interface down, etc.)
        /// </summary>
        Lost,

        /// <summary>
        /// Neighbor state changed (e.g., OSPF Down -> Init -> 2-Way, etc.)
        /// </summary>
        StateChanged,

        /// <summary>
        /// Neighbor configuration or properties changed
        /// </summary>
        Updated,

        /// <summary>
        /// Neighbor adjacency established
        /// </summary>
        AdjacencyUp,

        /// <summary>
        /// Neighbor adjacency lost
        /// </summary>
        AdjacencyDown
    }
}
