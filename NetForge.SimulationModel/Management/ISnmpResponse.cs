namespace NetForge.SimulationModel.Management;

public interface ISnmpResponse
{
    int RequestId { get; }
    int ErrorStatus { get; }
    int ErrorIndex { get; }
    IDictionary<string, string> VariableBindings { get; }
}
