namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Network discovery protocol capabilities (CDP, LLDP)
    /// </summary>
    public interface IDiscoveryCapabilities
    {
        /// <summary>
        /// Enable CDP globally
        /// </summary>
        bool EnableCdpGlobal();

        /// <summary>
        /// Disable CDP globally
        /// </summary>
        bool DisableCdpGlobal();

        /// <summary>
        /// Enable CDP on an interface
        /// </summary>
        bool EnableCdpInterface(string interfaceName);

        /// <summary>
        /// Disable CDP on an interface
        /// </summary>
        bool DisableCdpInterface(string interfaceName);

        /// <summary>
        /// Set CDP timer
        /// </summary>
        bool SetCdpTimer(int seconds);

        /// <summary>
        /// Set CDP holdtime
        /// </summary>
        bool SetCdpHoldtime(int seconds);
    }
}