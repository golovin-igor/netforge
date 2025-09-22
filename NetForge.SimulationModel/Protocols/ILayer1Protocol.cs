using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Protocols;

public interface ILayer1Protocol : INetworkProtocol
{
    void TransmitBits(byte[] data, INetworkInterface outInterface);
    void ReceiveBits(byte[] data, INetworkInterface inInterface);
    LinkSpeed GetLinkSpeed(string interfaceId);
    void SetLinkSpeed(string interfaceId, LinkSpeed speed);
    void OnLinkStateChange(string interfaceId, LinkState newState);
    event EventHandler<ILinkStateChangeEvent> LinkStateChanged;
}
