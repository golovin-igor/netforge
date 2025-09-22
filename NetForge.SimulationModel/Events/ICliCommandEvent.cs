namespace NetForge.SimulationModel.Events;

public interface ICliCommandEvent : IApplicationEvent
{
    string Command { get; }
    string User { get; }
    string SessionId { get; }
    bool Success { get; }
    string Output { get; }
}
