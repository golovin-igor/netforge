namespace NetForge.Simulation.Common.Events
{
    /// <summary>
    /// Interface for events that have a device name
    /// Used for event filtering by device
    /// </summary>
    public interface IDeviceEvent
    {
        /// <summary>
        /// Name of the device that generated this event
        /// </summary>
        string DeviceName { get; }
    }
}