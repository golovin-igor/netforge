namespace NetForge.SimulationModel.Configuration;

public interface IConfigurationDiff
{
    string GetDiffAsString();

    bool IsEmpty();

    IConfigurationDiff Reverse();

    IConfigurationDiff Merge(IConfigurationDiff other);

    IConfigurationDiff Clone();

    string GetSummary();
}
