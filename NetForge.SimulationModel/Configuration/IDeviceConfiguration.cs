using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Configuration;

public interface IDeviceConfiguration
{
    string DeviceId { get; }
    DateTime LastModified { get; }
    IConfigurationSection System { get; }
    IConfigurationSection Interfaces { get; }
    IConfigurationSection Routing { get; }
    IConfigurationSection Security { get; }
    IConfigurationSection Services { get; }

    void Load(string configuration);
    string Export(ConfigurationFormat format);
    void Apply();
    void Rollback();
    IConfigurationDiff GetPendingChanges();
    void Commit();
}
