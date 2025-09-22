using NetForge.SimulationModel.Configuration;

namespace NetForge.Protocols.Layer2.Ethernet;

public class EthernetConfiguration : IProtocolConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsEnabled { get; set; } = true;
    public int Mtu { get; set; } = 1500;
    public bool JumboFramesEnabled { get; set; } = false;
    public int JumboFrameSize { get; set; } = 9000;
    public bool VlanEnabled { get; set; } = true;
    public List<int> AllowedVlans { get; set; } = new();
    public int NativeVlan { get; set; } = 1;
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
