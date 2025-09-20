using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Protocols.Common.Events;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Manages all device configuration including protocol settings, system settings, and configuration state.
/// This service extracts configuration management responsibilities from NetworkDevice.
/// </summary>
public class DeviceConfigurationManager : IConfigurationProvider
{
    private readonly Dictionary<string, string> _systemSettings = new();
    private readonly DeviceConfiguration _runningConfig = new();
    private readonly INetworkEventBus? _eventBus;
    private readonly string _deviceName;
    
    private OspfConfig _ospfConfig = new(1);
    private BgpConfig _bgpConfig = new(65000);
    private RipConfig _ripConfig = new();
    private EigrpConfig _eigrpConfig = new(1);
    private StpConfig _stpConfig = new();
    private IgrpConfig _igrpConfig = new(1);
    private VrrpConfig _vrrpConfig = new();
    private HsrpConfig _hsrpConfig = new();
    private CdpConfig _cdpConfig = new();
    private LldpConfig _lldpConfig = new();
    private object _telnetConfig = new();
    private object _sshConfig = new();
    private object _snmpConfig = new();
    private object _httpConfig = new();

    /// <summary>
    /// Initializes a new instance of DeviceConfigurationManager.
    /// </summary>
    /// <param name="deviceName">Name of the device for event publishing</param>
    /// <param name="eventBus">Optional event bus for publishing configuration changes</param>
    public DeviceConfigurationManager(string deviceName, INetworkEventBus? eventBus = null)
    {
        _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
        _eventBus = eventBus;
    }

    /// <summary>
    /// Gets or sets whether the NVRAM configuration has been loaded.
    /// </summary>
    public bool IsNvramLoaded { get; set; }

    /// <summary>
    /// Gets all system settings for the device.
    /// </summary>
    public Dictionary<string, string> GetSystemSettings() => _systemSettings;

    /// <summary>
    /// Gets a specific system setting by name.
    /// </summary>
    public string? GetSystemSetting(string name)
    {
        return _systemSettings.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Sets a system setting.
    /// </summary>
    public void SetSystemSetting(string name, string value)
    {
        _systemSettings[name] = value;
    }

    /// <summary>
    /// Sets the running configuration directly without processing commands.
    /// </summary>
    public void SetRunningConfig(string config)
    {
        // Clear existing configuration by resetting to a new instance
        var lines = config.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            _runningConfig.AppendLine(line.Trim());
        }
    }

    /// <summary>
    /// Gets the running configuration.
    /// </summary>
    public DeviceConfiguration GetRunningConfig() => _runningConfig;

    #region Protocol Configuration Methods

    public OspfConfig? GetOspfConfiguration() => _ospfConfig;
    
    public void SetOspfConfiguration(OspfConfig config)
    {
        bool wasNull = _ospfConfig == null;
        _ospfConfig = config ?? throw new ArgumentNullException(nameof(config));

        // Publish configuration change event
        _eventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(_deviceName, NetworkProtocolType.OSPF,
            wasNull ? "OSPF configuration initialized" : "OSPF configuration updated"));
    }

    public BgpConfig? GetBgpConfiguration() => _bgpConfig;
    
    public void SetBgpConfiguration(BgpConfig config)
    {
        bool wasNull = _bgpConfig == null;
        _bgpConfig = config ?? throw new ArgumentNullException(nameof(config));

        // Publish configuration change event
        _eventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(_deviceName, NetworkProtocolType.BGP,
            wasNull ? "BGP configuration initialized" : "BGP configuration updated"));
    }

    public RipConfig? GetRipConfiguration() => _ripConfig;
    
    public void SetRipConfiguration(RipConfig config)
    {
        _ripConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public EigrpConfig? GetEigrpConfiguration() => _eigrpConfig;
    
    public void SetEigrpConfiguration(EigrpConfig config)
    {
        _eigrpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public StpConfig GetStpConfiguration() => _stpConfig;
    
    public void SetStpConfiguration(StpConfig config)
    {
        _stpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public IgrpConfig? GetIgrpConfiguration() => _igrpConfig;
    
    public void SetIgrpConfiguration(IgrpConfig config)
    {
        _igrpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public VrrpConfig? GetVrrpConfiguration() => _vrrpConfig;
    
    public void SetVrrpConfiguration(VrrpConfig config)
    {
        _vrrpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public HsrpConfig? GetHsrpConfiguration() => _hsrpConfig;
    
    public void SetHsrpConfiguration(HsrpConfig config)
    {
        _hsrpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public CdpConfig? GetCdpConfiguration() => _cdpConfig;
    
    public void SetCdpConfiguration(CdpConfig config)
    {
        _cdpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public LldpConfig? GetLldpConfiguration() => _lldpConfig;
    
    public void SetLldpConfiguration(LldpConfig config)
    {
        _lldpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public object GetTelnetConfiguration() => _telnetConfig;
    
    public void SetTelnetConfiguration(object config)
    {
        _telnetConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public object GetSshConfiguration() => _sshConfig;
    
    public void SetSshConfiguration(object config)
    {
        _sshConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public object GetSnmpConfiguration() => _snmpConfig;
    
    public void SetSnmpConfiguration(object config)
    {
        _snmpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    public object GetHttpConfiguration() => _httpConfig;
    
    public void SetHttpConfiguration(object config)
    {
        _httpConfig = config ?? throw new ArgumentNullException(nameof(config));
    }

    #endregion
}