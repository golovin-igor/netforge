namespace NetForge.Interfaces.Devices;

/// <summary>
/// Manages device logging and event notifications.
/// This interface handles all logging-related operations for a device.
/// </summary>
public interface IDeviceLogging
{
    /// <summary>
    /// Event raised when a log entry is added.
    /// </summary>
    event Action<string> LogEntryAdded;

    /// <summary>
    /// Gets all log entries for the device.
    /// </summary>
    List<string> GetLogEntries();

    /// <summary>
    /// Adds a log entry.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    void AddLogEntry(string entry);

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    void ClearLog();
}