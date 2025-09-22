namespace NetForge.Protocols.Layer2.CDP;

public class CdpPacket
{
    public byte Version { get; set; } = 2;
    public byte Ttl { get; set; } = 180; // seconds
    public ushort Checksum { get; set; }
    public List<CdpTlv> Tlvs { get; set; } = new();

    // Parsed values from TLVs
    public string DeviceId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Capabilities { get; set; } = string.Empty;
    public string InterfaceId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;

    public void AddTlv(CdpTlvType type, string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        AddTlv(type, bytes);
    }

    public void AddTlv(CdpTlvType type, byte[] value)
    {
        Tlvs.Add(new CdpTlv
        {
            Type = type,
            Length = (ushort)(value.Length + 4), // Type (2) + Length (2) + Value
            Value = value
        });
    }

    public byte[] ToBytes()
    {
        var bytes = new List<byte>();

        // CDP Header
        bytes.Add(Version);
        bytes.Add(Ttl);

        // Checksum placeholder
        bytes.Add(0x00);
        bytes.Add(0x00);

        // Add TLVs
        foreach (var tlv in Tlvs)
        {
            bytes.AddRange(tlv.ToBytes());
        }

        // Calculate checksum
        ushort checksum = CalculateChecksum(bytes.ToArray());
        bytes[2] = (byte)(checksum >> 8);
        bytes[3] = (byte)(checksum & 0xFF);

        return bytes.ToArray();
    }

    public static CdpPacket FromBytes(byte[] data)
    {
        if (data.Length < 4)
            throw new ArgumentException("Invalid CDP packet");

        var packet = new CdpPacket
        {
            Version = data[0],
            Ttl = data[1],
            Checksum = (ushort)((data[2] << 8) | data[3])
        };

        // Parse TLVs
        int offset = 4;
        while (offset < data.Length - 4)
        {
            var tlv = CdpTlv.FromBytes(data, offset);
            packet.Tlvs.Add(tlv);

            // Extract common values
            switch (tlv.Type)
            {
                case CdpTlvType.DeviceId:
                    packet.DeviceId = System.Text.Encoding.UTF8.GetString(tlv.Value);
                    break;
                case CdpTlvType.Platform:
                    packet.Platform = System.Text.Encoding.UTF8.GetString(tlv.Value);
                    break;
                case CdpTlvType.PortId:
                    packet.InterfaceId = System.Text.Encoding.UTF8.GetString(tlv.Value);
                    break;
            }

            offset += tlv.Length;
        }

        return packet;
    }

    private static ushort CalculateChecksum(byte[] data)
    {
        // Simplified checksum calculation
        uint sum = 0;
        for (int i = 0; i < data.Length; i += 2)
        {
            if (i == 2) continue; // Skip checksum field

            ushort word = (ushort)(data[i] << 8);
            if (i + 1 < data.Length)
                word |= data[i + 1];
            sum += word;
        }

        while ((sum >> 16) != 0)
        {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }

        return (ushort)~sum;
    }
}

public class CdpTlv
{
    public CdpTlvType Type { get; set; }
    public ushort Length { get; set; }
    public byte[] Value { get; set; } = Array.Empty<byte>();

    public byte[] ToBytes()
    {
        var bytes = new List<byte>();

        // Type
        bytes.Add((byte)((ushort)Type >> 8));
        bytes.Add((byte)((ushort)Type & 0xFF));

        // Length
        bytes.Add((byte)(Length >> 8));
        bytes.Add((byte)(Length & 0xFF));

        // Value
        bytes.AddRange(Value);

        return bytes.ToArray();
    }

    public static CdpTlv FromBytes(byte[] data, int offset)
    {
        if (offset + 4 > data.Length)
            throw new ArgumentException("Invalid TLV");

        var tlv = new CdpTlv
        {
            Type = (CdpTlvType)((data[offset] << 8) | data[offset + 1]),
            Length = (ushort)((data[offset + 2] << 8) | data[offset + 3])
        };

        int valueLength = tlv.Length - 4; // Subtract Type and Length fields
        if (offset + 4 + valueLength > data.Length)
            throw new ArgumentException("Invalid TLV length");

        tlv.Value = new byte[valueLength];
        Array.Copy(data, offset + 4, tlv.Value, 0, valueLength);

        return tlv;
    }
}

public enum CdpTlvType : ushort
{
    DeviceId = 0x0001,
    Address = 0x0002,
    PortId = 0x0003,
    Capabilities = 0x0004,
    Version = 0x0005,
    Platform = 0x0006,
    IpPrefix = 0x0007,
    VtpDomain = 0x0009,
    NativeVlan = 0x000a,
    Duplex = 0x000b,
    Power = 0x0010
}