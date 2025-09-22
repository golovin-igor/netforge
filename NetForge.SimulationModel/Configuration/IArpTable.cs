namespace NetForge.SimulationModel.Configuration;

public interface IArpTable
{
    void AddEntry(string ipAddress, string macAddress);
    void RemoveEntry(string ipAddress);
    string? GetMacAddress(string ipAddress);
    IDictionary<string, string> GetAllEntries();
    void Clear();

}
