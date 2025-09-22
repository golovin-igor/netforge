using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IVlanChangeEvent : ILayer2Event
{
    int VlanId { get; }
    VlanOperation Operation { get; }
    IReadOnlyCollection<string> AffectedPorts { get; }
}
