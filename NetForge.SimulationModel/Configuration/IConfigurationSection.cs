namespace NetForge.SimulationModel.Configuration;

public interface IConfigurationSection
{
    string SectionName { get; }

    string ToConfigString();

    void FromConfigString(string config, string sectionName);

    IConfigurationSection Clone();
}
