using NetForge.SimulationModel.Devices;

namespace NetForge.SimulationModel.Configuration;

public interface IMacAddressTable
{
    void AddEntry(string macAddress, string port);

    void RemoveEntry(string macAddress);

    string? GetPort(string macAddress);

    IDictionary<string, string> GetAllEntries();
    void Clear();
    string? GetInterface(string frameDestinationMac);
}
