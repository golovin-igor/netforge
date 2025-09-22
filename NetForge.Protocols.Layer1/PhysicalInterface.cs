using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer1;

public class PhysicalInterface
{
    public string Id { get; set; } = string.Empty;
    public InterfaceType Type { get; set; }
    public InterfaceState Status { get; set; }
    public long Speed { get; set; }
    public DuplexMode Duplex { get; set; }
    public long BytesReceived { get; private set; }
    public long BytesTransmitted { get; private set; }
    public long PacketsReceived { get; private set; }
    public long PacketsTransmitted { get; private set; }
    public long ErrorsReceived { get; private set; }
    public long ErrorsTransmitted { get; private set; }
    public DateTime LastStateChange { get; set; }

    public void UpdateStatistics(int bytes, bool isTransmit = false)
    {
        if (isTransmit)
        {
            BytesTransmitted += bytes;
            PacketsTransmitted++;
        }
        else
        {
            BytesReceived += bytes;
            PacketsReceived++;
        }
    }

    public void IncrementErrors(bool isTransmit = false)
    {
        if (isTransmit)
        {
            ErrorsTransmitted++;
        }
        else
        {
            ErrorsReceived++;
        }
    }

    public void ResetCounters()
    {
        BytesReceived = 0;
        BytesTransmitted = 0;
        PacketsReceived = 0;
        PacketsTransmitted = 0;
        ErrorsReceived = 0;
        ErrorsTransmitted = 0;
    }
}