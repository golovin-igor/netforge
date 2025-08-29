
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Events
{
    /// <summary>
    /// Event arguments for physical connection state changes
    /// </summary>
    public class PhysicalConnectionStateChangedEventArgs(
        PhysicalConnection connection,
        PhysicalConnectionState previousState,
        PhysicalConnectionState newState,
        string reason = "")
        : NetworkEventArgs
    {
        public PhysicalConnection Connection { get; } = connection;
        public PhysicalConnectionState PreviousState { get; } = previousState;
        public PhysicalConnectionState NewState { get; } = newState;
        public string Reason { get; } = reason;
        public new DateTime Timestamp { get; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $"PhysicalConnection {Connection.Id}: {PreviousState} -> {NewState}" +
                   (!string.IsNullOrEmpty(Reason) ? $" ({Reason})" : "");
        }
    }
}
