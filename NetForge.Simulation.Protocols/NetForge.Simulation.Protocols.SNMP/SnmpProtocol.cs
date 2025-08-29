using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpProtocol : BaseProtocol
{
    private SnmpAgent? _snmpAgent;
    private readonly SnmpState _snmpState = new();

    public override NetworkProtocolType Type => NetworkProtocolType.SNMP;
    public override string Name => "Simple Network Management Protocol";

    protected override BaseProtocolState CreateInitialState()
    {
        return _snmpState;
    }

    protected override void OnInitialized()
    {
        var snmpConfig = GetSnmpConfig();
        if (snmpConfig.IsEnabled)
        {
            _ = Task.Run(async () => await StartSnmpAgent(snmpConfig));
        }
    }

    protected override async Task RunProtocolCalculation(INetworkDevice device)
    {
        var snmpConfig = GetSnmpConfig();

        if (!snmpConfig.IsEnabled)
        {
            if (_snmpAgent != null)
            {
                await StopSnmpAgent();
            }
            _snmpState.IsActive = false;
            return;
        }

        // Update MIB database with current device state
        await UpdateMibDatabase(device, snmpConfig);

        // Ensure SNMP agent is running
        if (_snmpAgent == null || !_snmpState.AgentRunning)
        {
            await StartSnmpAgent(snmpConfig);
        }

        _snmpState.IsActive = true;
        _snmpState.LastUpdate = DateTime.Now;
    }

    private async Task StartSnmpAgent(SnmpConfig config)
    {
        try
        {
            _snmpAgent = new SnmpAgent(_device!, config, _snmpState);
            _snmpAgent.RequestReceived += OnSnmpRequestReceived;
            _snmpAgent.ResponseSent += OnSnmpResponseSent;

            // Initialize MIB database
            await InitializeMibDatabase(config);

            await _snmpAgent.StartAsync();

            _device!.AddLogEntry($"SNMP agent started on port {config.Port}");
            _snmpState.IsActive = true;
            _snmpState.MarkStateChanged();
        }
        catch (Exception ex)
        {
            _device!.AddLogEntry($"Failed to start SNMP agent: {ex.Message}");
            _snmpState.IsActive = false;
        }
    }

    private async Task StopSnmpAgent()
    {
        if (_snmpAgent != null)
        {
            try
            {
                await _snmpAgent.StopAsync();
                _snmpAgent.Dispose();
                _snmpAgent = null;

                _device!.AddLogEntry("SNMP agent stopped");
                _snmpState.IsActive = false;
                _snmpState.MarkStateChanged();
            }
            catch (Exception ex)
            {
                _device!.AddLogEntry($"Error stopping SNMP agent: {ex.Message}");
            }
        }
    }

    private async Task InitializeMibDatabase(SnmpConfig config)
    {
        _snmpState.MibDatabase.Clear();

        // System group (1.3.6.1.2.1.1)
        _snmpState.MibDatabase["1.3.6.1.2.1.1.1.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.1.0", "sysDescr", config.SystemDescription);

        _snmpState.MibDatabase["1.3.6.1.2.1.1.2.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.2.0", "sysObjectID", "1.3.6.1.4.1.99999.1");

        _snmpState.MibDatabase["1.3.6.1.2.1.1.3.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.3.0", "sysUpTime", _snmpState.SystemUpTime.TotalMilliseconds, SnmpType.TimeTicks);

        _snmpState.MibDatabase["1.3.6.1.2.1.1.4.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.4.0", "sysContact", config.SystemContact);

        _snmpState.MibDatabase["1.3.6.1.2.1.1.5.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.5.0", "sysName", _device!.GetHostname() ?? _device.Name);

        _snmpState.MibDatabase["1.3.6.1.2.1.1.6.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.6.0", "sysLocation", config.SystemLocation);

        _snmpState.MibDatabase["1.3.6.1.2.1.1.7.0"] = new SnmpVariable(
            "1.3.6.1.2.1.1.7.0", "sysServices", 78, SnmpType.Integer); // Layer 2-4 services

        // Add custom OIDs from configuration
        foreach (var customOid in config.CustomOids)
        {
            _snmpState.MibDatabase[customOid.Key] = new SnmpVariable(
                customOid.Key, $"custom_{customOid.Key}", customOid.Value);
        }

        await Task.CompletedTask;
    }

    private async Task UpdateMibDatabase(INetworkDevice device, SnmpConfig config)
    {
        // Update system uptime
        if (_snmpState.MibDatabase.TryGetValue("1.3.6.1.2.1.1.3.0", out var uptimeVar))
        {
            uptimeVar.Value = _snmpState.SystemUpTime.TotalMilliseconds;
            uptimeVar.LastUpdated = DateTime.Now;
        }

        // Update interface statistics (2.1.2)
        await UpdateInterfaceStatistics(device);

        // Update routing table (2.1.4)
        await UpdateIpRouteTable(device);

        // Update system information if changed
        var currentSysName = _snmpState.MibDatabase.GetValueOrDefault("1.3.6.1.2.1.1.5.0");
        if (currentSysName != null && !currentSysName.Value.Equals(device.GetHostname() ?? device.Name))
        {
            currentSysName.Value = device.GetHostname() ?? device.Name;
            currentSysName.LastUpdated = DateTime.Now;
        }

        _snmpState.MarkStateChanged();
    }

    private async Task UpdateInterfaceStatistics(INetworkDevice device)
    {
        var interfaces = device.GetAllInterfaces();
        var ifIndex = 1;

        foreach (var (interfaceName, interfaceConfig) in interfaces)
        {
            var baseOid = $"1.3.6.1.2.1.2.2.1";

            // ifDescr
            var ifDescrOid = $"{baseOid}.2.{ifIndex}";
            _snmpState.MibDatabase[ifDescrOid] = new SnmpVariable(
                ifDescrOid, $"ifDescr.{ifIndex}", interfaceName);

            // ifType (Ethernet = 6)
            var ifTypeOid = $"{baseOid}.3.{ifIndex}";
            _snmpState.MibDatabase[ifTypeOid] = new SnmpVariable(
                ifTypeOid, $"ifType.{ifIndex}", 6, SnmpType.Integer);

            // ifMtu
            var ifMtuOid = $"{baseOid}.4.{ifIndex}";
            _snmpState.MibDatabase[ifMtuOid] = new SnmpVariable(
                ifMtuOid, $"ifMtu.{ifIndex}", interfaceConfig?.Mtu ?? 1500, SnmpType.Integer);

            // ifSpeed (assume 100Mbps)
            var ifSpeedOid = $"{baseOid}.5.{ifIndex}";
            _snmpState.MibDatabase[ifSpeedOid] = new SnmpVariable(
                ifSpeedOid, $"ifSpeed.{ifIndex}", 100000000, SnmpType.Gauge);

            // ifAdminStatus and ifOperStatus
            var adminStatus = interfaceConfig?.IsShutdown == false ? 1 : 2;
            var operStatus = interfaceConfig?.IsUp == true ? 1 : 2;

            var ifAdminStatusOid = $"{baseOid}.7.{ifIndex}";
            _snmpState.MibDatabase[ifAdminStatusOid] = new SnmpVariable(
                ifAdminStatusOid, $"ifAdminStatus.{ifIndex}", adminStatus, SnmpType.Integer);

            var ifOperStatusOid = $"{baseOid}.8.{ifIndex}";
            _snmpState.MibDatabase[ifOperStatusOid] = new SnmpVariable(
                ifOperStatusOid, $"ifOperStatus.{ifIndex}", operStatus, SnmpType.Integer);

            ifIndex++;
        }

        await Task.CompletedTask;
    }

    private async Task UpdateIpRouteTable(INetworkDevice device)
    {
        var routes = device.GetRoutingTable();
        var routeIndex = 1;

        foreach (var route in routes)
        {
            var baseOid = $"1.3.6.1.2.1.4.21.1";

            // ipRouteDest
            var destOid = $"{baseOid}.1.{route.Network}";
            _snmpState.MibDatabase[destOid] = new SnmpVariable(
                destOid, $"ipRouteDest.{routeIndex}", route.Network, SnmpType.IpAddress);

            // ipRouteNextHop
            var nextHopOid = $"{baseOid}.7.{route.Network}";
            _snmpState.MibDatabase[nextHopOid] = new SnmpVariable(
                nextHopOid, $"ipRouteNextHop.{routeIndex}", route.NextHop, SnmpType.IpAddress);

            // ipRouteMetric1
            var metricOid = $"{baseOid}.3.{route.Network}";
            _snmpState.MibDatabase[metricOid] = new SnmpVariable(
                metricOid, $"ipRouteMetric1.{routeIndex}", route.Metric, SnmpType.Integer);

            routeIndex++;
        }

        await Task.CompletedTask;
    }

    private void OnSnmpRequestReceived(object? sender, SnmpRequestEventArgs e)
    {
        _device!.AddLogEntry($"SNMP {e.Request.RequestType} request from {e.ClientEndpoint}: {e.Request.Oid}");
        _snmpState.UpdateActivity();
    }

    private void OnSnmpResponseSent(object? sender, SnmpResponseEventArgs e)
    {
        _device!.AddLogEntry($"SNMP response sent to {e.ClientEndpoint}: Status={e.Response.ErrorStatus}");
    }

    public async Task SendTrapAsync(string trapOid, Dictionary<string, object> varbinds)
    {
        if (_snmpAgent != null)
        {
            await _snmpAgent.SendTrapAsync(trapOid, varbinds);
        }
    }

    protected override void OnSubscribeToEvents(INetworkEventBus eventBus, INetworkDevice self)
    {
        // TODO: Add event subscriptions for interface status changes and route updates
        // This will require proper event type definitions in the Events namespace
    }

    private SnmpConfig GetSnmpConfig()
    {
        return _device?.GetSnmpConfiguration() as SnmpConfig ?? new SnmpConfig { IsEnabled = false };
    }

    protected override object GetProtocolConfiguration()
    {
        return GetSnmpConfig();
    }

    protected override void OnApplyConfiguration(object configuration)
    {
        if (configuration is SnmpConfig snmpConfig)
        {
            _device?.SetSnmpConfiguration(snmpConfig);

            // Restart SNMP agent if configuration changed
            _ = Task.Run(async () =>
            {
                if (_snmpAgent != null)
                {
                    await StopSnmpAgent();
                }

                if (snmpConfig.IsEnabled)
                {
                    await StartSnmpAgent(snmpConfig);
                }
            });
        }
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Generic", "Cisco", "Juniper", "Arista" }; // SNMP is universal
    }

    public override void Dispose()
    {
        if (_snmpAgent != null)
        {
            Task.Run(async () =>
            {
                await StopSnmpAgent();
            }).Wait(5000);
        }

        base.Dispose();
    }
}
