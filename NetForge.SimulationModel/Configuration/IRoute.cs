namespace NetForge.SimulationModel.Configuration;

public interface IRoute
{
    string DestinationNetwork { get; }
    string SubnetMask { get; }
    string NextHop { get; }
    int Metric { get; }
}
