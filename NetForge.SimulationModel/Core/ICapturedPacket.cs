namespace NetForge.SimulationModel.Core;

public interface ICapturedPacket
{
    DateTime Timestamp { get; }
    byte[] Data { get; }
    string Source { get; }
    string Destination { get; }
    string Protocol { get; }
    int Length { get; }
}
