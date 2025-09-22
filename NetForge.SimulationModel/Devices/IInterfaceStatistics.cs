namespace NetForge.SimulationModel.Devices;

public interface IInterfaceStatistics
{
    long BytesSent { get; }
    long BytesReceived { get; }
    long PacketsSent { get; }
    long PacketsReceived { get; }
    long ErrorsSent { get; }
    long ErrorsReceived { get; }
    long DropsSent { get; }
    long DropsReceived { get; }
    TimeSpan Uptime { get; }
}
