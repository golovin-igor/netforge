namespace NetForge.SimulationModel.Management;

public interface INetconfHandler
{
    bool IsEnabled { get; }
    int Port { get; }

    void Enable(int port);
    void Disable();
    INetconfSession CreateSession(ICredentials credentials);
    INetconfResponse ExecuteRpc(string rpcXml, INetconfSession session);
    void RegisterCapability(string capabilityUri);
}
