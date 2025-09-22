using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Management;

public interface ISnmpHandler
{
    bool IsEnabled { get; }
    SnmpVersion Version { get; }
    string Community { get; set; }

    void Enable(SnmpVersion version);
    void Disable();
    ISnmpResponse Get(string oid);
    ISnmpResponse GetNext(string oid);
    ISnmpResponse Set(string oid, object value);
    ISnmpResponse Walk(string baseOid);
    void RegisterMib(IMibModule mib);
    void SendTrap(ISnmpTrap trap);
}
