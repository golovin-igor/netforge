namespace NetForge.SimulationModel.Configuration;

public interface IRoutingTable
{
    IEnumerable<IRoute> Routes { get; }

    void AddRoute(IRoute route);

    void RemoveRoute(IRoute route);

    IRoute? FindRoute(string destinationIp);

    void ClearRoutes();
}
