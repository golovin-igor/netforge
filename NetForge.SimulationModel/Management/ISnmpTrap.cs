namespace NetForge.SimulationModel.Management;

public interface ISnmpTrap
{
    string Oid { get; }
    string Message { get; }
    DateTime Timestamp { get; }
    IDictionary<string, string> Variables { get; }
}
