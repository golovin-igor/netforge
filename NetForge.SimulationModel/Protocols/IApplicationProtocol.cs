using NetForge.SimulationModel.Events;

namespace NetForge.SimulationModel.Protocols;

public interface IApplicationProtocol : INetworkProtocol
{
    int Port { get; }
    void ProcessData(byte[] data, IConnection connection);
    void SendData(byte[] data, IConnection connection);
    bool IsListening { get; }
    void StartListening(int port);
    void StopListening();
    void OnApplicationEvent(IApplicationEvent appEvent);
    event EventHandler<IApplicationEvent> ApplicationEventOccurred;
}
