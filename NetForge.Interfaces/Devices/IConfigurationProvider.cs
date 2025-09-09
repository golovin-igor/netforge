using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Provides access to device configuration including protocol settings, system settings, and configuration state.
/// This interface handles all configuration-related operations for a network device.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Gets or sets whether the NVRAM configuration has been loaded.
    /// </summary>
    bool IsNvramLoaded { get; set; }

    /// <summary>
    /// Gets all system settings for the device.
    /// </summary>
    Dictionary<string, string> GetSystemSettings();

    /// <summary>
    /// Gets a specific system setting by name.
    /// </summary>
    /// <param name="name">The name of the setting to retrieve.</param>
    /// <returns>The setting value, or null if not found.</returns>
    string? GetSystemSetting(string name);

    /// <summary>
    /// Sets a system setting.
    /// </summary>
    /// <param name="name">The name of the setting.</param>
    /// <param name="value">The value to set.</param>
    void SetSystemSetting(string name, string value);

    /// <summary>
    /// Sets the running configuration directly without processing commands.
    /// </summary>
    /// <param name="config">The configuration to set.</param>
    void SetRunningConfig(string config);

    /// <summary>
    /// Gets the OSPF configuration.
    /// </summary>
    OspfConfig? GetOspfConfiguration();

    /// <summary>
    /// Sets the OSPF configuration.
    /// </summary>
    void SetOspfConfiguration(OspfConfig config);

    /// <summary>
    /// Gets the BGP configuration.
    /// </summary>
    BgpConfig? GetBgpConfiguration();

    /// <summary>
    /// Sets the BGP configuration.
    /// </summary>
    void SetBgpConfiguration(BgpConfig config);

    /// <summary>
    /// Gets the RIP configuration.
    /// </summary>
    RipConfig? GetRipConfiguration();

    /// <summary>
    /// Sets the RIP configuration.
    /// </summary>
    void SetRipConfiguration(RipConfig config);

    /// <summary>
    /// Gets the EIGRP configuration.
    /// </summary>
    EigrpConfig? GetEigrpConfiguration();

    /// <summary>
    /// Sets the EIGRP configuration.
    /// </summary>
    void SetEigrpConfiguration(EigrpConfig config);

    /// <summary>
    /// Gets the STP configuration.
    /// </summary>
    StpConfig GetStpConfiguration();

    /// <summary>
    /// Sets the STP configuration.
    /// </summary>
    void SetStpConfiguration(StpConfig config);

    /// <summary>
    /// Gets the IGRP configuration.
    /// </summary>
    IgrpConfig? GetIgrpConfiguration();

    /// <summary>
    /// Sets the IGRP configuration.
    /// </summary>
    void SetIgrpConfiguration(IgrpConfig config);

    /// <summary>
    /// Gets the VRRP configuration.
    /// </summary>
    VrrpConfig? GetVrrpConfiguration();

    /// <summary>
    /// Sets the VRRP configuration.
    /// </summary>
    void SetVrrpConfiguration(VrrpConfig config);

    /// <summary>
    /// Gets the HSRP configuration.
    /// </summary>
    HsrpConfig? GetHsrpConfiguration();

    /// <summary>
    /// Sets the HSRP configuration.
    /// </summary>
    void SetHsrpConfiguration(HsrpConfig config);

    /// <summary>
    /// Gets the CDP configuration.
    /// </summary>
    CdpConfig? GetCdpConfiguration();

    /// <summary>
    /// Sets the CDP configuration.
    /// </summary>
    void SetCdpConfiguration(CdpConfig config);

    /// <summary>
    /// Gets the LLDP configuration.
    /// </summary>
    LldpConfig? GetLldpConfiguration();

    /// <summary>
    /// Sets the LLDP configuration.
    /// </summary>
    void SetLldpConfiguration(LldpConfig config);

    /// <summary>
    /// Gets the Telnet configuration.
    /// </summary>
    object GetTelnetConfiguration();

    /// <summary>
    /// Sets the Telnet configuration.
    /// </summary>
    void SetTelnetConfiguration(object config);

    /// <summary>
    /// Gets the SSH configuration.
    /// </summary>
    object GetSshConfiguration();

    /// <summary>
    /// Sets the SSH configuration.
    /// </summary>
    void SetSshConfiguration(object config);

    /// <summary>
    /// Gets the SNMP configuration.
    /// </summary>
    object GetSnmpConfiguration();

    /// <summary>
    /// Sets the SNMP configuration.
    /// </summary>
    void SetSnmpConfiguration(object config);

    /// <summary>
    /// Gets the HTTP configuration.
    /// </summary>
    object GetHttpConfiguration();

    /// <summary>
    /// Sets the HTTP configuration.
    /// </summary>
    void SetHttpConfiguration(object config);
}