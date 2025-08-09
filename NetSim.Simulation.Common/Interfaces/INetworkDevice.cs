namespace NetSim.Simulation.Interfaces;

public interface INetworkDevice
{
    string GetCurrentMode();
    void AddLogEntry(string entry);
    
    string Name { get; }
    string Vendor { get; }
}
