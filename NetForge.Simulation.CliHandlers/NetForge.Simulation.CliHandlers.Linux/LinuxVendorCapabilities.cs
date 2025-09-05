using NetForge.Interfaces.Vendors;

namespace NetForge.Simulation.CliHandlers.Linux;

public class LinuxVendorCapabilities : IVendorCapabilities
{
    public string GetRunningConfiguration()
    {
        var config = new List<string>();
        
        // Linux doesn't have a traditional "running-config" - show various system info
        config.Add("! Linux System Configuration");
        config.Add("!");
        
        return string.Join("\n", config);
    }

    public string GetStartupConfiguration()
    {
        // Linux doesn't have a traditional startup config
        return GetRunningConfiguration();
    }

    public void SetDeviceMode(string mode)
    {
        // Linux mode setting would be handled through su/sudo commands
    }

    public string GetDeviceMode()
    {
        return "user"; // Default mode
    }

    public bool SupportsMode(string mode)
    {
        return mode == "user" || mode == "root";
    }

    public IEnumerable<string> GetAvailableModes()
    {
        return new[] { "user", "root" };
    }

    public string FormatCommandOutput(string command, object? data = null)
    {
        return data?.ToString() ?? "";
    }

    public string GetVendorErrorMessage(string errorType, string? context = null)
    {
        return errorType switch
        {
            "invalid_command" => $"bash: {context}: command not found",
            "permission_denied" => $"bash: {context}: Permission denied",
            _ => $"Error: {errorType}"
        };
    }

    public bool SupportsFeature(string feature)
    {
        return feature switch
        {
            "ip_commands" => true,
            "routing" => true,
            "interfaces" => true,
            _ => false
        };
    }

    public string FormatInterfaceName(string interfaceName)
    {
        // Linux interfaces are typically eth0, eth1, etc.
        return interfaceName.ToLower();
    }

    public bool ValidateVendorSyntax(string[] commandParts, string command)
    {
        // Linux has flexible command syntax
        return true;
    }

    // Interface configuration methods
    public bool ConfigureInterfaceIp(string interfaceName, string ipAddress, string subnetMask)
    {
        // Linux interface IP configuration
        return true;
    }

    public bool RemoveInterfaceIp(string interfaceName)
    {
        // Linux interface IP removal
        return true;
    }

    public bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction)
    {
        // Linux access group (iptables equivalent)
        return true;
    }

    public bool RemoveAccessGroup(string interfaceName)
    {
        // Linux access group removal
        return true;
    }

    public bool SetInterfaceShutdown(string interfaceName, bool shutdown)
    {
        // Linux interface shutdown (ip link set down/up)
        return true;
    }

    public bool SetHostname(string hostname)
    {
        // Linux hostname setting
        return true;
    }

    public bool SetInterfaceDescription(string interfaceName, string description)
    {
        // Linux doesn't have native interface descriptions
        return true;
    }

    // VLAN management methods
    public bool CreateOrSelectVlan(int vlanId)
    {
        // Linux VLAN creation would create a new VLAN interface
        return true;
    }

    public bool AddInterfaceToVlan(string interfaceName, int vlanId)
    {
        // Linux VLAN assignment would create a new interface like eth0.100
        return true;
    }

    public bool VlanExists(int vlanId)
    {
        // Check if VLAN interface exists
        return true;
    }

    public bool SetVlanName(int vlanId, string name)
    {
        // Linux doesn't have native VLAN naming
        return true;
    }

    // Routing protocol initialization methods
    public bool InitializeOspf(int processId)
    {
        // Linux OSPF would be handled through FRRouting
        return true;
    }

    public bool InitializeBgp(int asNumber)
    {
        // Linux BGP would be handled through FRRouting
        return true;
    }

    public bool InitializeRip()
    {
        // Linux RIP would be handled through FRRouting
        return true;
    }

    public bool InitializeEigrp(int asNumber)
    {
        // Linux EIGRP would be handled through FRRouting
        return true;
    }

    public bool SetCurrentRouterProtocol(string protocol)
    {
        // Set current routing protocol context
        return true;
    }

    // ACL management methods
    public bool AddAclEntry(int aclNumber, object aclEntry)
    {
        // Linux ACL would be handled through iptables
        return true;
    }

    public bool SetCurrentAclNumber(int aclNumber)
    {
        // Set current ACL number for configuration
        return true;
    }

    public int GetCurrentAclNumber()
    {
        // Get current ACL number
        return 0;
    }

    // Configuration management methods
    public bool AppendToRunningConfig(string configLine)
    {
        // Append to running configuration
        return true;
    }

    // Spanning Tree Protocol methods
    public bool SetStpMode(string mode)
    {
        // Linux STP mode setting
        return true;
    }

    public bool SetStpVlanPriority(int vlanId, int priority)
    {
        // Linux STP VLAN priority
        return true;
    }

    public bool SetStpPriority(int priority)
    {
        // Linux STP global priority
        return true;
    }

    public bool EnablePortfast(string interfaceName)
    {
        // Linux doesn't have native PortFast
        return true;
    }

    public bool DisablePortfast(string interfaceName)
    {
        // Linux doesn't have native PortFast
        return true;
    }

    public bool EnablePortfastDefault()
    {
        // Linux doesn't have native PortFast default
        return true;
    }

    public bool EnableBpduGuard(string interfaceName)
    {
        // Linux doesn't have native BPDU Guard
        return true;
    }

    public bool DisableBpduGuard(string interfaceName)
    {
        // Linux doesn't have native BPDU Guard
        return true;
    }

    public bool EnableBpduGuardDefault()
    {
        // Linux doesn't have native BPDU Guard default
        return true;
    }

    // Port Channel methods
    public bool CreateOrUpdatePortChannel(int channelId, string interfaceName, string mode)
    {
        // Linux bonding/teaming equivalent
        return true;
    }

    // CDP methods
    public bool EnableCdpGlobal()
    {
        // Linux doesn't have native CDP (use LLDP instead)
        return true;
    }

    public bool DisableCdpGlobal()
    {
        // Linux doesn't have native CDP
        return true;
    }

    public bool EnableCdpInterface(string interfaceName)
    {
        // Linux doesn't have native CDP
        return true;
    }

    public bool DisableCdpInterface(string interfaceName)
    {
        // Linux doesn't have native CDP
        return true;
    }

    public bool SetCdpTimer(int seconds)
    {
        // Linux doesn't have native CDP
        return true;
    }

    public bool SetCdpHoldtime(int seconds)
    {
        // Linux doesn't have native CDP
        return true;
    }

    // Interface state methods
    public bool SetSwitchportMode(string interfaceName, string mode)
    {
        // Linux doesn't have native switchport concept
        return true;
    }

    public bool SetInterfaceVlan(string interfaceName, int vlanId)
    {
        // Linux VLAN assignment to interface
        return true;
    }

    // Additional required interface methods
    public bool SetCurrentInterface(string interfaceName)
    {
        // Set current interface for configuration context
        return true;
    }

    public bool SetInterfaceState(string interfaceName, string state)
    {
        // Linux interface state setting (ip link set up/down)
        return true;
    }

    public bool SetInterface(string interfaceName, string property, object value)
    {
        // Generic interface property setter
        return true;
    }

    public bool SaveConfiguration()
    {
        // Linux configuration saving (typically not needed as changes are immediate)
        return true;
    }

    public bool ReloadDevice()
    {
        // Linux system reload/reboot
        return true;
    }

    // ICommandFormatter interface implementation
    public string PreprocessCommand(string command)
    {
        return command; // No preprocessing by default
    }

    public string PostprocessOutput(string output)
    {
        return output; // No postprocessing by default
    }

    public string RenderConfiguration(object configData)
    {
        return configData?.ToString() ?? "";
    }
} 
