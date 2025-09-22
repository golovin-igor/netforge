using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Protocols;

public interface ILayer2Protocol : INetworkProtocol
{
    void ProcessFrame(IFrame frame, string ingressInterface);
    void SendFrame(IFrame frame, string egressInterface);
    IMacAddressTable GetMacTable();
    IVlanConfiguration GetVlanConfig();
    void OnMacLearned(string macAddress, string interfaceId);
    void OnVlanChange(int vlanId, VlanOperation operation);
    event EventHandler<IMacLearnedEvent> MacLearned;
    event EventHandler<IVlanChangeEvent> VlanChanged;
}
