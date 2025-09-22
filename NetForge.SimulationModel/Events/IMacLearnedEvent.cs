namespace NetForge.SimulationModel.Events;

public interface IMacLearnedEvent : ILayer2Event
{
    string MacAddress { get; }
    string LearnedInterface { get; }
    int VlanId { get; }
}
