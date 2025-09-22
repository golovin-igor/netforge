namespace NetForge.SimulationModel.Management;

public interface INetconfSession
{
    string SessionId { get; }
    string Username { get; }
    string ClientHost { get; }
    DateTime StartTime { get; }
    DateTime? EndTime { get; }
    bool IsActive { get; }

}
