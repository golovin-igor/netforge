namespace NetForge.SimulationModel.Management;

public interface ICommandHandler
{
    string ExecuteCommand(string command);

    string GetStatus();

    void Reset();
}
