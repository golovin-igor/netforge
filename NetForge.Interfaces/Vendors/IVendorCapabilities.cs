namespace NetForge.Simulation.Common.CLI.Interfaces
{
    /// <summary>
    /// Interface for vendor-agnostic device capabilities used by CLI handlers
    /// </summary>
    public interface IVendorCapabilities
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
        /// Execute vendor-specific command formatting
        /// </summary>
        string FormatCommandOutput(string command, object? data = null);

        /// <summary>
        /// Get vendor-specific error message
        /// </summary>
        string GetVendorErrorMessage(string errorType, string? context = null);

        /// <summary>
        /// Check if device supports a specific feature
        /// </summary>
        bool SupportsFeature(string feature);

        /// <summary>
        /// Get vendor-specific interface name format
        /// </summary>
        string FormatInterfaceName(string interfaceName);

        /// <summary>
        /// Validate vendor-specific syntax
        /// </summary>
        bool ValidateVendorSyntax(string[] commandParts, string command);

        // Interface configuration methods
        /// <summary>
        /// Configure IP address on an interface
        /// </summary>
        bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask);

        /// <summary>
        /// Remove IP address from an interface
        /// </summary>
        bool RemoveInterfaceIp(string interfaceName);

        /// <summary>
        /// Apply access group to an interface
        /// </summary>
        bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction);

        /// <summary>
        /// Remove access group from an interface
        /// </summary>
        bool RemoveAccessGroup(string interfaceName);

        /// <summary>
        /// Set interface shutdown state
        /// </summary>
        bool SetInterfaceShutdown(string interfaceName, bool shutdown);

        // VLAN management methods
        /// <summary>
        /// Create or select VLAN for configuration
        /// </summary>
        bool CreateOrSelectVlan(int vlanId);

        // Routing protocol initialization methods
        /// <summary>
        /// Initialize OSPF routing protocol
        /// </summary>
        bool InitializeOspf(int processId);

        /// <summary>
        /// Initialize BGP routing protocol
        /// </summary>
        bool InitializeBgp(int asNumber);

        /// <summary>
        /// Initialize RIP routing protocol
        /// </summary>
        bool InitializeRip();

        /// <summary>
        /// Initialize EIGRP routing protocol
        /// </summary>
        bool InitializeEigrp(int asNumber);

        /// <summary>
        /// Set current router protocol context
        /// </summary>
        bool SetCurrentRouterProtocol(string protocol);

        // ACL management methods
        /// <summary>
        /// Add an ACL entry to the specified access list
        /// </summary>
        bool AddAclEntry(int aclNumber, object aclEntry);

        /// <summary>
        /// Set the current ACL number for configuration
        /// </summary>
        bool SetCurrentAclNumber(int aclNumber);

        /// <summary>
        /// Get the current ACL number
        /// </summary>
        int GetCurrentAclNumber();

        // Configuration management methods
        /// <summary>
        /// Append a line to the running configuration
        /// </summary>
        bool AppendToRunningConfig(string configLine);

        // VLAN management methods (additional)
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

        // Spanning Tree Protocol methods
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

        // Port Channel methods
        /// <summary>
        /// Create or update a port channel
        /// </summary>
        bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode);

        // CDP methods
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

        // Hostname and system methods
        /// <summary>
        /// Set device hostname
        /// </summary>
        bool SetHostname(string hostname);

        // Interface state methods
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

        /// <summary>
        /// Save current configuration to startup config
        /// </summary>
        bool SaveConfiguration();

        /// <summary>
        /// Reload/restart the device
        /// </summary>
        bool ReloadDevice();
    }
}
