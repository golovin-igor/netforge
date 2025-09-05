namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Core device capabilities for basic operations
    /// </summary>
    public interface IDeviceCapabilities
    {
        /// <summary>
        /// Get the current device configuration as a string
        /// </summary>
        string GetRunningConfiguration();

        /// <summary>
        /// Get the startup configuration as a string
        /// </summary>
        string GetStartupConfiguration();

        /// <summary>
        /// Set device mode using vendor-specific mode names
        /// </summary>
        void SetDeviceMode(string mode);

        /// <summary>
        /// Get current device mode as vendor-specific string
        /// </summary>
        string GetDeviceMode();

        /// <summary>
        /// Check if device supports a specific mode
        /// </summary>
        bool SupportsMode(string mode);

        /// <summary>
        /// Get available modes for this device
        /// </summary>
        IEnumerable<string> GetAvailableModes();

        /// <summary>
        /// Check if device supports a specific feature
        /// </summary>
        bool SupportsFeature(string feature);

        /// <summary>
        /// Save current configuration to startup config
        /// </summary>
        bool SaveConfiguration();

        /// <summary>
        /// Reload/restart the device
        /// </summary>
        bool ReloadDevice();

        /// <summary>
        /// Set device hostname
        /// </summary>
        bool SetHostname(string hostname);
    }
}