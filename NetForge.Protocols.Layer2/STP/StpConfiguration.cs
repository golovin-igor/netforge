using NetForge.SimulationModel.Configuration;

namespace NetForge.Protocols.Layer2.STP;

public class StpConfiguration : IProtocolConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsEnabled { get; set; } = true;
    public int BridgePriority { get; set; } = 32768;
    public StpMode Mode { get; set; } = StpMode.Pvst;
    public int HelloTime { get; set; } = 2;
    public int MaxAge { get; set; } = 20;
    public int ForwardDelay { get; set; } = 15;
    public Dictionary<string, int> PortCosts { get; set; } = new();
    public List<int> Vlans { get; set; } = new() { 1 };
    public string ProtocolName { get; }
    public bool Enabled { get; set; }
    public IReadOnlyDictionary<string, object> Parameters { get; }
    public void SetParameter(string name, object value)
    {
        throw new NotImplementedException();
    }

    public T GetParameter<T>(string name)
    {
        throw new NotImplementedException();
    }

    public void Validate()
    {
        throw new NotImplementedException();
    }

    public void RestoreDefaults()
    {
        throw new NotImplementedException();
    }
}

public enum StpMode
{
    Stp,      // IEEE 802.1D
    Pvst,     // Per-VLAN Spanning Tree (Cisco)
    Rstp,     // Rapid Spanning Tree Protocol
    Mstp      // Multiple Spanning Tree Protocol
}
