using System.Text.RegularExpressions;

namespace NetForge.Simulation.Common.Events
{
    /// <summary>
    /// Filter for selective event subscription
    /// </summary>
    public class EventFilter
    {
        /// <summary>
        /// Device name pattern (supports wildcards * and ?)
        /// </summary>
        public string DeviceNamePattern { get; set; } = "*";

        /// <summary>
        /// Event type pattern (supports wildcards * and ?)
        /// </summary>
        public string EventTypePattern { get; set; } = "*";

        /// <summary>
        /// Custom filter function for advanced filtering
        /// </summary>
        public Func<NetworkEventArgs, bool>? CustomFilter { get; set; }

        /// <summary>
        /// Checks if an event matches this filter
        /// </summary>
        /// <param name="eventArgs">Event to check</param>
        /// <returns>True if event matches filter</returns>
        public bool Matches(NetworkEventArgs eventArgs)
        {
            // Check device name pattern if event has device name
            if (eventArgs is IDeviceEvent deviceEvent)
            {
                if (!MatchesPattern(deviceEvent.DeviceName, DeviceNamePattern))
                    return false;
            }

            // Check event type pattern
            if (!MatchesPattern(eventArgs.GetType().Name, EventTypePattern))
                return false;

            // Check custom filter
            return CustomFilter?.Invoke(eventArgs) ?? true;
        }

        /// <summary>
        /// Checks if a value matches a wildcard pattern
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="pattern">Pattern with * and ? wildcards</param>
        /// <returns>True if value matches pattern</returns>
        private static bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
                return false;

            if (pattern == "*")
                return true;

            // Convert wildcard pattern to regex
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Creates a filter for a specific device
        /// </summary>
        public static EventFilter ForDevice(string deviceName)
        {
            return new EventFilter { DeviceNamePattern = deviceName };
        }

        /// <summary>
        /// Creates a filter for a specific event type
        /// </summary>
        public static EventFilter ForEventType<T>() where T : NetworkEventArgs
        {
            return new EventFilter { EventTypePattern = typeof(T).Name };
        }

        /// <summary>
        /// Creates a filter with custom logic
        /// </summary>
        public static EventFilter Custom(Func<NetworkEventArgs, bool> filter)
        {
            return new EventFilter { CustomFilter = filter };
        }
    }

    /// <summary>
    /// Interface for events that have a device name
    /// </summary>
    public interface IDeviceEvent
    {
        string DeviceName { get; }
    }
}