using NetForge.SimulationModel.Devices;

namespace NetForge.SimulationModel.Protocols;

public interface IConnection
{
    Guid Id { get; }
    INetworkDevice NodeA { get; }
    INetworkDevice NodeB { get; }
    INetworkInterface InterfaceA { get; }
    INetworkInterface InterfaceB { get; }
    bool IsUp { get; }
    void SetUp();
    void SetDown();

}
