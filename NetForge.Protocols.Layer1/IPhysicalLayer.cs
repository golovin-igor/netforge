using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer1;

public interface IPhysicalLayer
{
    void TransmitBits(byte[] data, string interfaceId);
    void ReceiveBits(byte[] data, string interfaceId);
    void SetInterfaceState(string interfaceId, InterfaceState status);
    InterfaceState GetInterfaceState(string interfaceId);
    void AddInterface(string interfaceId, InterfaceType type);
    void RemoveInterface(string interfaceId);
}