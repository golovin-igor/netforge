namespace NetForge.Protocols.Layer1;

public class EthernetFrame
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string InterfaceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Length => Data.Length;
}