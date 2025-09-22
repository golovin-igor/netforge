namespace NetForge.SimulationModel.Devices;

public interface IDeviceVendor
{
    string Name { get; }
    string Model { get; }
    string Version { get; }
    string SerialNumber { get; }
    IReadOnlyDictionary<string, string> Properties { get; }
}
