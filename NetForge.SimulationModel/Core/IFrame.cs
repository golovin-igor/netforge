namespace NetForge.SimulationModel.Core;

public interface IFrame
{
    byte[] Payload { get; }
    string SourceMac { get; }
    string DestinationMac { get; }
    int EtherType { get; }
}
