using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Events;

namespace NetForge.SimulationModel.Protocols;

public interface ILayer4Protocol : INetworkProtocol
{
    void ProcessSegment(ISegment segment);
    void SendSegment(ISegment segment, string destinationIp, int destinationPort);
    IConnectionTable GetConnectionTable();
    void EstablishConnection(string remoteIp, int remotePort, int localPort);
    void CloseConnection(string connectionId);
    void OnConnectionStateChange(string connectionId, object newState);
    event EventHandler<ITcpConnectionEvent> ConnectionStateChanged;
}
