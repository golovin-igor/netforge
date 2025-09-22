using System.Net;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Management;

public interface ICliHandler
{
    bool IsEnabled { get; }
    int Port { get; }
    CliProtocol Protocol { get; } // Telnet, SSH

    void Enable(int port, CliProtocol protocol);
    void Disable();
    ICliSession CreateSession(ICredentials credentials);
    void ExecuteCommand(string command, ICliSession session);
    string GetPrompt(ICliSession session);
    void RegisterCommandHandler(string command, ICommandHandler handler);
}
