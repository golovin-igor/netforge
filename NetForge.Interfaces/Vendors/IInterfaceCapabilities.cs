namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Interface configuration capabilities
    /// </summary>
    public interface IInterfaceCapabilities
    {
        /// <summary>
        /// Get vendor-specific interface name format
        /// </summary>
        string FormatInterfaceName(string interfaceName);

        /// <summary>
        /// Configure IP address on an interface
        /// </summary>
        bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask);

        /// <summary>
        /// Remove IP address from an interface
        /// </summary>
        bool RemoveInterfaceIp(string interfaceName);

        /// <summary>
        /// Set interface shutdown state
        /// </summary>
        bool SetInterfaceShutdown(string interfaceName, bool shutdown);

        /// <summary>
        /// Set interface description
        /// </summary>
        bool SetInterfaceDescription(string interfaceName, string description);

        /// <summary>
        /// Set interface switchport mode
        /// </summary>
        bool SetSwitchportMode(string interfaceName, string mode);

        /// <summary>
        /// Set interface VLAN ID
        /// </summary>
        bool SetInterfaceVlan(string interfaceName, int vlanId);

        /// <summary>
        /// Set current interface for configuration context
        /// </summary>
        bool SetCurrentInterface(string interfaceName);

        /// <summary>
        /// Set interface state (up/down, enabled/disabled)
        /// </summary>
        bool SetInterfaceState(string interfaceName, string state);

        /// <summary>
        /// Set interface configuration (generic setter)
        /// </summary>
        bool SetInterface(string interfaceName, string property, object value);
    }
}