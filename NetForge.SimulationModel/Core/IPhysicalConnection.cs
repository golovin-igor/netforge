using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Core;

public interface IPhysicalConnection
{
    string Id { get; }
    INetworkInterface Endpoint1 { get; }
    INetworkInterface Endpoint2 { get; }
    ConnectionType Type { get; }
    int BandwidthMbps { get; }
    int DelayMs { get; }
    double PacketLossRate { get; }
    ConnectionState State { get; }

    void Connect();
    void Disconnect();
    void SimulateFailure();
    void RestoreConnection();
}
