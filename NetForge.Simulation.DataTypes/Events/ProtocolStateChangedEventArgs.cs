using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.Protocols.Common.Events
{
    /// <summary>
    /// Event arguments for protocol state changes
    /// Extends the existing NetworkEventArgs pattern
    /// </summary>
    public class ProtocolStateChangedEventArgs : NetworkEventArgs
    {
        /// <summary>
        /// Name of the device that reported the protocol state change
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// The type of protocol that changed state
        /// </summary>
        public NetworkProtocolType NetworkProtocolType { get; }

        /// <summary>
        /// The name of the protocol that changed state
        /// </summary>
        public string ProtocolName { get; }

        /// <summary>
        /// Details about what changed in the protocol state
        /// </summary>
        public string ChangeDetails { get; }

        /// <summary>
        /// The new state data (optional)
        /// </summary>
        public Dictionary<string, object>? NewStateData { get; set; }

        /// <summary>
        /// The previous state data (optional)
        /// </summary>
        public Dictionary<string, object>? PreviousStateData { get; set; }

        /// <summary>
        /// Whether this is a critical state change that requires immediate attention
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// Create protocol state changed event
        /// </summary>
        /// <param name="deviceName">Name of the device</param>
        /// <param name="networkProtocolType">Type of protocol</param>
        /// <param name="protocolName">Name of protocol</param>
        /// <param name="changeDetails">Details of the change</param>
        public ProtocolStateChangedEventArgs(
            string deviceName,
            NetworkProtocolType networkProtocolType,
            string protocolName,
            string changeDetails)
        {
            DeviceName = deviceName;
            NetworkProtocolType = networkProtocolType;
            ProtocolName = protocolName;
            ChangeDetails = changeDetails;
        }

        /// <summary>
        /// Create protocol state changed event with state data
        /// </summary>
        /// <param name="deviceName">Name of the device</param>
        /// <param name="networkProtocolType">Type of protocol</param>
        /// <param name="protocolName">Name of protocol</param>
        /// <param name="changeDetails">Details of the change</param>
        /// <param name="newStateData">New state data</param>
        /// <param name="previousStateData">Previous state data</param>
        public ProtocolStateChangedEventArgs(
            string deviceName,
            NetworkProtocolType networkProtocolType,
            string protocolName,
            string changeDetails,
            Dictionary<string, object>? newStateData,
            Dictionary<string, object>? previousStateData = null) : this(deviceName, networkProtocolType, protocolName, changeDetails)
        {
            NewStateData = newStateData;
            PreviousStateData = previousStateData;
        }

        /// <summary>
        /// String representation of the event
        /// </summary>
        /// <returns>Event description</returns>
        public override string ToString()
        {
            var critical = IsCritical ? " [CRITICAL]" : "";
            return $"{DeviceName}: {ProtocolName} ({NetworkProtocolType}) - {ChangeDetails}{critical}";
        }
    }
}
