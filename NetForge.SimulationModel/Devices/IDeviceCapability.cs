using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Devices;

public interface IDeviceCapability
{
    DeviceCapabilityType Type { get; }

    string Description { get; }

    IReadOnlyDictionary<string, object> Parameters { get; }

    bool IsEnabled { get; set; }
}
