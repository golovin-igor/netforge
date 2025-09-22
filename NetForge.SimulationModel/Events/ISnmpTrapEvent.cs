using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ISnmpTrapEvent : IApplicationEvent
{
    string TrapOid { get; }
    SnmpTrapType TrapType { get; }
    IDictionary<string, object> Variables { get; }
}
