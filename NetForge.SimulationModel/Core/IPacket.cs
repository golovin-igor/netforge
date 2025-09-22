using NetForge.SimulationModel.Protocols;

namespace NetForge.SimulationModel.Core;

public interface IPacket
{
    byte[] Data { get; }
    string Source { get; }
    string Destination { get; }
    DateTime Timestamp { get; }
    INetworkProtocol Protocol { get; }
}
