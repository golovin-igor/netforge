namespace NetForge.SimulationModel.Protocols;

public interface IApplicationProtocolCollection
{

    IApplicationProtocol? GetProtocolByPort(int port);

    IApplicationProtocol? GetProtocolByName(string name);

    IEnumerable<IApplicationProtocol> GetAllProtocols();
}
