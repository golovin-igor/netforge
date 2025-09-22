using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Core;

public interface ITopology
{
    string Id { get; }
    string Name { get; }
    IReadOnlyCollection<INetworkDevice> Devices { get; }
    IReadOnlyCollection<IPhysicalConnection> Connections { get; }
    IEventBus GlobalEventBus { get; }

    void AddDevice(INetworkDevice device);
    void RemoveDevice(string deviceId);
    INetworkDevice GetDevice(string deviceId);

    void ConnectDevices(string deviceId1, string interfaceId1,
        string deviceId2, string interfaceId2);

    void DisconnectDevices(string connectionId);

    IEnumerable<INetworkDevice> FindDevicesByCapability(DeviceCapabilityType capability);
    IEnumerable<IPhysicalConnection> GetDeviceConnections(string deviceId);

    void Start();
    void Stop();
    void Reset();

    void BroadcastGlobalEvent(INetworkEvent networkEvent);
    void SubscribeToGlobalEvents<TEvent>(Action<TEvent> handler) where TEvent : INetworkEvent;
}
