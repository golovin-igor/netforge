using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IArpEvent : ILayer3Event
{
    string IpAddress { get; }
    string MacAddress { get; }
    ArpOperation Operation { get; }
}
