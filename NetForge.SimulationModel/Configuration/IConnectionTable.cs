namespace NetForge.SimulationModel.Configuration;

public interface IConnectionTable
{
    string Name { get; }
    string Description { get; }
    IEnumerable<IConnectionTableEntry> Entries { get; }

}

public interface IConnectionTableEntry
{
    string Source { get; }
    string Destination { get; }
    string ConnectionType { get; }
    IDictionary<string, string> Parameters { get; }
}
