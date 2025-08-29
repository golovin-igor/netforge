namespace NetForge.Simulation.DataTypes.NetworkPrimitives;

/// <summary>
/// Network statistics for monitoring
/// </summary>
public class NetworkStatistics
{
    public int TotalDevices { get; set; }
    public int TotalConnections { get; set; }
    public int OperationalConnections { get; set; }
    public int FailedConnections { get; set; }
    public int DegradedConnections { get; set; }

    public double ConnectionReliability => TotalConnections > 0
        ? (double)OperationalConnections / TotalConnections * 100
        : 0;
}
