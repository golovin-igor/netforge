using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IProtocolEvent
{
    string Protocol { get; }

    string Device { get; }

    DateTime Timestamp { get; }

    string Description { get; }

    EventScope Scope { get; }

    string? Interface { get; }

    string? AdditionalInfo { get; }

}
