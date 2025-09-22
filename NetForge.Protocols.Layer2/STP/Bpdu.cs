namespace NetForge.Protocols.Layer2.STP;

public class Bpdu
{
    public ushort ProtocolId { get; set; } = 0x0000; // Always 0 for STP
    public byte Version { get; set; } = 0; // 0 for STP, 2 for RSTP
    public byte Type { get; set; } = 0x00; // Configuration BPDU

    public byte Flags { get; set; }
    public string RootBridgeId { get; set; } = string.Empty;
    public int RootPathCost { get; set; }
    public string BridgeId { get; set; } = string.Empty;
    public string PortId { get; set; } = string.Empty;

    public ushort MessageAge { get; set; }
    public ushort MaxAge { get; set; } = 20;
    public ushort HelloTime { get; set; } = 2;
    public ushort ForwardDelay { get; set; } = 15;

    public bool TopologyChange => (Flags & 0x01) != 0;
    public bool TopologyChangeAck => (Flags & 0x80) != 0;

    public byte[] ToBytes()
    {
        // Simplified serialization
        var bytes = new List<byte>();

        // Protocol Identifier
        bytes.Add((byte)(ProtocolId >> 8));
        bytes.Add((byte)(ProtocolId & 0xFF));

        // Version, Type, Flags
        bytes.Add(Version);
        bytes.Add(Type);
        bytes.Add(Flags);

        // Add bridge IDs, costs, timers (simplified)
        // In real implementation, these would be properly serialized

        return bytes.ToArray();
    }

    public static Bpdu FromBytes(byte[] data)
    {
        // Simplified deserialization
        if (data.Length < 35) // Minimum BPDU size
            throw new ArgumentException("Invalid BPDU data");

        return new Bpdu
        {
            ProtocolId = (ushort)((data[0] << 8) | data[1]),
            Version = data[2],
            Type = data[3],
            Flags = data[4]
            // Parse remaining fields in real implementation
        };
    }
}