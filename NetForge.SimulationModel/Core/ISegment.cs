namespace NetForge.SimulationModel.Core;

public interface ISegment
{
    string Name { get; }
    string Description { get; }
    string Type { get; }
    string Subnet { get; }
    string Gateway { get; }
    int VlanId { get; }
    bool IsManagement { get; }
    IDictionary<string, string> CustomProperties { get; }

}
