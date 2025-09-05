namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Layer 2 switching capabilities
    /// </summary>
    public interface ISwitchingCapabilities
    {
        /// <summary>
        /// Create or select VLAN for configuration
        /// </summary>
        bool CreateOrSelectVlan(int vlanId);

        /// <summary>
        /// Add an interface to a VLAN
        /// </summary>
        bool AddInterfaceToVlan(string interfaceName, int vlanId);

        /// <summary>
        /// Check if a VLAN exists
        /// </summary>
        bool VlanExists(int vlanId);

        /// <summary>
        /// Set VLAN name
        /// </summary>
        bool SetVlanName(int vlanId, string name);

        /// <summary>
        /// Set spanning tree mode
        /// </summary>
        bool SetStpMode(string mode);

        /// <summary>
        /// Set spanning tree priority for a specific VLAN
        /// </summary>
        bool SetStpVlanPriority(int vlanId, int priority);

        /// <summary>
        /// Set global spanning tree priority
        /// </summary>
        bool SetStpPriority(int priority);

        /// <summary>
        /// Enable PortFast on a specific interface
        /// </summary>
        bool EnablePortfast(string interfaceName);

        /// <summary>
        /// Disable PortFast on a specific interface
        /// </summary>
        bool DisablePortfast(string interfaceName);

        /// <summary>
        /// Enable PortFast by default on all access ports
        /// </summary>
        bool EnablePortfastDefault();

        /// <summary>
        /// Enable BPDU Guard on a specific interface
        /// </summary>
        bool EnableBpduGuard(string interfaceName);

        /// <summary>
        /// Disable BPDU Guard on a specific interface
        /// </summary>
        bool DisableBpduGuard(string interfaceName);

        /// <summary>
        /// Enable BPDU Guard by default
        /// </summary>
        bool EnableBpduGuardDefault();

        /// <summary>
        /// Create or update a port channel
        /// </summary>
        bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode);
    }
}