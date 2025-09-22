using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Protocols;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Devices;

public interface INetworkDevice
{
    string Id { get; }
    string Hostname { get; set; }
    IDeviceVendor Vendor { get; }
    IDeviceConfiguration Configuration { get; }
    IReadOnlyCollection<INetworkInterface> Interfaces { get; }
    IReadOnlyCollection<IDeviceCapability> Capabilities { get; }
    IProtocolStack ProtocolStack { get; }
    DeviceState State { get; }
    IEventBus LocalEventBus { get; }

    void PowerOn();
    void PowerOff();
    void Restart();

    INetworkInterface GetInterface(string interfaceId);
    void AddInterface(INetworkInterface networkInterface);
    void RemoveInterface(string interfaceId);

    void ProcessPacket(IPacket packet, string ingressInterface);
    void SendPacket(IPacket packet, string egressInterface);

    T GetProtocol<T>() where T : INetworkProtocol;
    void RegisterProtocol(INetworkProtocol protocol);

    void SubscribeToEvent<TEvent>(Action<TEvent> handler) where TEvent : INetworkEvent;
    void PublishEvent(INetworkEvent networkEvent);
    void OnRemoteEvent(INetworkEvent networkEvent);
}
