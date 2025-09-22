namespace NetForge.SimulationModel.Configuration;

public interface IProtocolConfiguration
{
    string ProtocolName { get; }
    bool Enabled { get; set; }
    IReadOnlyDictionary<string, object> Parameters { get; }

    void SetParameter(string name, object value);
    T GetParameter<T>(string name);
    void Validate();
    void RestoreDefaults();
}
