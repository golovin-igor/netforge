using NetForge.SimulationModel.Configuration;

namespace NetForge.Protocols.Layer2.CDP;

public class CdpConfiguration : IProtocolConfiguration
{
    Dictionary<string, object> parameters = new();

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsEnabled { get; set; } = true;
    public byte Version { get; set; } = 2;
    public int Timer { get; set; } = 60; // seconds between advertisements
    public int HoldTime { get; set; } = 180; // seconds before neighbor expires
    public bool Advertise { get; set; } = true;
    public List<string> EnabledInterfaces { get; set; } = new();
    public string ProtocolName { get; }
    public bool Enabled { get; set; }
    public IReadOnlyDictionary<string, object> Parameters => parameters;

    public void SetParameter(string name, object value)
    {
        parameters[name] = value;
    }

    public T GetParameter<T>(string name)
    {
        if (parameters.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default!;
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
