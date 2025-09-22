namespace NetForge.SimulationModel.Management;

public interface ICliSession
{
    string SessionId { get; }
    string UserName { get; }
    DateTime StartTime { get; }
    DateTime? EndTime { get; }
    bool IsActive { get; }
}
