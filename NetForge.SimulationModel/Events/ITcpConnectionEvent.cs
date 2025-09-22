using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ITcpConnectionEvent : ILayer4Event
{
    string ConnectionId { get; }
    TcpState OldState { get; }
    TcpState NewState { get; }
    int LocalPort { get; }
    int RemotePort { get; }
}
