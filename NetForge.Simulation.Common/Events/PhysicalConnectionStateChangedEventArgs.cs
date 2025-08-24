using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Events
{
    /// <summary>
    /// Event arguments for physical connection state changes
    /// </summary>
    public class PhysicalConnectionStateChangedEventArgs : NetworkEventArgs
    {
        public PhysicalConnection Connection { get; }
        public PhysicalConnectionState PreviousState { get; }
        public PhysicalConnectionState NewState { get; }
        public string Reason { get; }
        public DateTime Timestamp { get; }

        public PhysicalConnectionStateChangedEventArgs(
            PhysicalConnection connection,
            PhysicalConnectionState previousState,
            PhysicalConnectionState newState,
            string reason = "")
        {
            Connection = connection;
            PreviousState = previousState;
            NewState = newState;
            Reason = reason;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"PhysicalConnection {Connection.Id}: {PreviousState} -> {NewState}" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
}
