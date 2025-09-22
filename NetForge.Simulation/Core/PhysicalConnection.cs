using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Types;

namespace NetForge.Simulation.Core;

public class PhysicalConnection : IPhysicalConnection
{
    public string Id { get; }
    public INetworkInterface Endpoint1 { get; }
    public INetworkInterface Endpoint2 { get; }
    public ConnectionType Type { get; set; } = ConnectionType.Ethernet;
    public int BandwidthMbps { get; set; } = 1000;
    public int DelayMs { get; set; } = 1;
    public double PacketLossRate { get; set; } = 0.0;
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public PhysicalConnection(string id, INetworkInterface endpoint1, INetworkInterface endpoint2)
    {
        Id = id;
        Endpoint1 = endpoint1;
        Endpoint2 = endpoint2;
    }

    public void Connect()
    {
        if (State == ConnectionState.Connected) return;

        State = ConnectionState.Negotiating;

        // Simulate connection negotiation
        Task.Delay(100).ContinueWith(_ =>
        {
            State = ConnectionState.Connected;
            Endpoint1.BringUp();
            Endpoint2.BringUp();
        });
    }

    public void Disconnect()
    {
        State = ConnectionState.Disconnected;
        Endpoint1.BringDown();
        Endpoint2.BringDown();
    }

    public void SimulateFailure()
    {
        State = ConnectionState.Failed;
        Endpoint1.BringDown();
        Endpoint2.BringDown();
    }

    public void RestoreConnection()
    {
        if (State == ConnectionState.Failed)
        {
            Connect();
        }
    }
}
