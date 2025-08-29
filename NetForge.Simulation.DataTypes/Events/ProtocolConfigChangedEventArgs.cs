using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Events
{
    /// <summary>
    /// Enhanced event arguments for protocol configuration changes
    /// Provides more detailed information about configuration changes
    /// </summary>
    public class ProtocolConfigChangedEventArgs : NetworkEventArgs
    {
        /// <summary>
        /// Name of the device that had the configuration change
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// The type of protocol that had its configuration changed
        /// </summary>
        public NetworkProtocolType NetworkProtocolType { get; }

        /// <summary>
        /// The name of the protocol that had its configuration changed
        /// </summary>
        public string ProtocolName { get; }

        /// <summary>
        /// Details about what changed in the configuration
        /// </summary>
        public string ChangeDetails { get; }

        /// <summary>
        /// Type of configuration change
        /// </summary>
        public ConfigChangeType ChangeType { get; }

        /// <summary>
        /// The new configuration data (optional)
        /// </summary>
        public Dictionary<string, object>? NewConfigurationData { get; set; }

        /// <summary>
        /// The previous configuration data (optional)
        /// </summary>
        public Dictionary<string, object>? PreviousConfigurationData { get; set; }

        /// <summary>
        /// Whether this configuration change requires protocol restart
        /// </summary>
        public bool RequiresRestart { get; set; } = false;

        /// <summary>
        /// Whether this is a critical configuration change
        /// </summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// Configuration section that changed (e.g., "RouterId", "Areas", "Neighbors")
        /// </summary>
        public string? ConfigurationSection { get; set; }

        /// <summary>
        /// Configuration validation status
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Validation errors if configuration is invalid
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Create protocol configuration changed event
        /// </summary>
        /// <param name="deviceName">Name of the device</param>
        /// <param name="networkProtocolType">Type of protocol</param>
        /// <param name="protocolName">Name of protocol</param>
        /// <param name="changeDetails">Details of the change</param>
        /// <param name="changeType">Type of configuration change</param>
        public ProtocolConfigChangedEventArgs(
            string deviceName,
            NetworkProtocolType networkProtocolType,
            string protocolName,
            string changeDetails,
            ConfigChangeType changeType = ConfigChangeType.Modified)
        {
            DeviceName = deviceName;
            NetworkProtocolType = networkProtocolType;
            ProtocolName = protocolName;
            ChangeDetails = changeDetails;
            ChangeType = changeType;
        }

        /// <summary>
        /// Create protocol configuration changed event with configuration data
        /// </summary>
        /// <param name="deviceName">Name of the device</param>
        /// <param name="networkProtocolType">Type of protocol</param>
        /// <param name="protocolName">Name of protocol</param>
        /// <param name="changeDetails">Details of the change</param>
        /// <param name="changeType">Type of configuration change</param>
        /// <param name="newConfigData">New configuration data</param>
        /// <param name="previousConfigData">Previous configuration data</param>
        public ProtocolConfigChangedEventArgs(
            string deviceName,
            NetworkProtocolType networkProtocolType,
            string protocolName,
            string changeDetails,
            ConfigChangeType changeType,
            Dictionary<string, object>? newConfigData,
            Dictionary<string, object>? previousConfigData = null) : this(deviceName, networkProtocolType, protocolName, changeDetails, changeType)
        {
            NewConfigurationData = newConfigData;
            PreviousConfigurationData = previousConfigData;
        }

        public ProtocolConfigChangedEventArgs(string deviceName, NetworkProtocolType networkProtocolType, string changes)
            : this(deviceName, networkProtocolType, networkProtocolType.ToString(), changes)
        {


        }

        /// <summary>
        /// String representation of the event
        /// </summary>
        /// <returns>Event description</returns>
        public override string ToString()
        {
            var critical = IsCritical ? " [CRITICAL]" : "";
            var restart = RequiresRestart ? " [RESTART REQUIRED]" : "";
            var section = !string.IsNullOrEmpty(ConfigurationSection) ? $" ({ConfigurationSection})" : "";

            return $"{DeviceName}: {ProtocolName} ({NetworkProtocolType}) configuration {ChangeType}{section} - {ChangeDetails}{critical}{restart}";
        }
    }

    /// <summary>
    /// Types of configuration changes
    /// </summary>
    public enum ConfigChangeType
    {
        /// <summary>
        /// Configuration was created/initialized for the first time
        /// </summary>
        Created,

        /// <summary>
        /// Existing configuration was modified
        /// </summary>
        Modified,

        /// <summary>
        /// Configuration was deleted/removed
        /// </summary>
        Deleted,

        /// <summary>
        /// Configuration was reset to defaults
        /// </summary>
        Reset,

        /// <summary>
        /// Configuration was restored from backup
        /// </summary>
        Restored,

        /// <summary>
        /// Configuration was imported from external source
        /// </summary>
        Imported,

        /// <summary>
        /// Configuration was exported
        /// </summary>
        Exported,

        /// <summary>
        /// Configuration validation was performed
        /// </summary>
        Validated
    }
}
