using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.DataTypes.ValueObjects;
using System.Text;

namespace NetForge.Simulation.Protocols.CDP;

public class CdpState : BaseProtocolState
{
    public Dictionary<string, CdpNeighbor> Neighbors { get; set; } = new();
    public string DeviceId { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Version { get; set; } = "";
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, CdpInterfaceSettings> InterfaceSettings { get; set; } = new();
    public new bool IsActive { get; set; } = false;
    public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
    public long PacketsSent { get; set; } = 0;
    public long PacketsReceived { get; set; } = 0;

    public override void MarkStateChanged()
    {
        base.MarkStateChanged();
    }

    public CdpNeighbor GetOrCreateCdpNeighbor(string key, Func<CdpNeighbor> factory)
    {
        if (!Neighbors.ContainsKey(key))
        {
            Neighbors[key] = factory();
            MarkStateChanged();
        }
        UpdateNeighborActivity(key);
        return Neighbors[key];
    }

    public override void UpdateNeighborActivity(string neighborKey)
    {
        if (Neighbors.TryGetValue(neighborKey, out var neighbor))
        {
            neighbor.LastSeen = DateTime.Now;
            MarkStateChanged();
        }
    }

    public bool ShouldSendAdvertisement(int timerInterval)
    {
        return (DateTime.Now - LastAdvertisementSent).TotalSeconds >= timerInterval;
    }

    public void RecordAdvertisement()
    {
        LastAdvertisementSent = DateTime.Now;
        PacketsSent++;
        MarkStateChanged();
    }

    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["Neighbors"] = Neighbors;
        baseData["DeviceId"] = DeviceId;
        baseData["Platform"] = Platform;
        baseData["IsActive"] = IsActive;
        baseData["PacketsSent"] = PacketsSent;
        baseData["PacketsReceived"] = PacketsReceived;
        return baseData;
    }
}

public class CdpNeighbor
{
    public string DeviceId { get; set; } = "";
    public string LocalInterface { get; set; } = "";
    public string RemoteInterface { get; set; } = "";
    public string Platform { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Version { get; set; } = "";
    public List<string> Capabilities { get; set; } = new();
    public int HoldTime { get; set; } = 180;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public Dictionary<CdpTlvType, CdpTlv> Tlvs { get; set; } = new();

    public CdpNeighbor(string deviceId, string localInterface, string remoteInterface)
    {
        DeviceId = deviceId;
        LocalInterface = localInterface;
        RemoteInterface = remoteInterface;
    }

    public bool IsExpired => (DateTime.Now - LastSeen).TotalSeconds > HoldTime;

    public void UpdateLastSeen()
    {
        LastSeen = DateTime.Now;
    }

    public string GetCapabilityString()
    {
        return string.Join(" ", Capabilities);
    }

    public void ProcessTlvs(List<CdpTlv> tlvs)
    {
        foreach (var tlv in tlvs)
        {
            Tlvs[tlv.Type] = tlv;

            switch (tlv.Type)
            {
                case CdpTlvType.DeviceId:
                    DeviceId = Encoding.ASCII.GetString(tlv.Value);
                    break;

                case CdpTlvType.Address:
                    IpAddress = ParseAddressTlv(tlv.Value);
                    break;

                case CdpTlvType.PortId:
                    RemoteInterface = Encoding.ASCII.GetString(tlv.Value);
                    break;

                case CdpTlvType.Capabilities:
                    var capValue = BitConverter.ToInt32(tlv.Value, 0);
                    Capabilities = ParseCapabilities(capValue);
                    break;

                case CdpTlvType.Version:
                    Version = Encoding.ASCII.GetString(tlv.Value);
                    break;

                case CdpTlvType.Platform:
                    Platform = Encoding.ASCII.GetString(tlv.Value);
                    break;
            }
        }
    }

    private string ParseAddressTlv(byte[] addressData)
    {
        if (addressData.Length >= 4)
        {
            return $"{addressData[0]}.{addressData[1]}.{addressData[2]}.{addressData[3]}";
        }
        return "0.0.0.0";
    }

    private List<string> ParseCapabilities(int capabilities)
    {
        var caps = new List<string>();
        if ((capabilities & 0x01) != 0) caps.Add("Router");
        if ((capabilities & 0x02) != 0) caps.Add("Trans-Bridge");
        if ((capabilities & 0x04) != 0) caps.Add("Source-Route-Bridge");
        if ((capabilities & 0x08) != 0) caps.Add("Switch");
        if ((capabilities & 0x10) != 0) caps.Add("Host");
        if ((capabilities & 0x20) != 0) caps.Add("IGMP");
        if ((capabilities & 0x40) != 0) caps.Add("Repeater");
        return caps;
    }
}

public class CdpPacket
{
    public byte Version { get; set; } = 2;
    public byte Ttl { get; set; } = 180;
    public ushort Checksum { get; set; } = 0;
    public List<CdpTlv> Tlvs { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // CDP Header
        writer.Write(Version);
        writer.Write(Ttl);
        writer.Write(Checksum);

        // TLVs
        foreach (var tlv in Tlvs)
        {
            writer.Write((ushort)tlv.Type);
            writer.Write(tlv.Length);
            writer.Write(tlv.Value);
        }

        return stream.ToArray();
    }

    public static CdpPacket? Deserialize(byte[] data)
    {
        if (data.Length < 4) return null;

        try
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var packet = new CdpPacket
            {
                Version = reader.ReadByte(),
                Ttl = reader.ReadByte(),
                Checksum = reader.ReadUInt16()
            };

            // Parse TLVs
            while (stream.Position < stream.Length)
            {
                if (stream.Length - stream.Position < 4) break;

                var type = (CdpTlvType)reader.ReadUInt16();
                var length = reader.ReadUInt16();

                if (length < 4 || stream.Position + length - 4 > stream.Length) break;

                var value = reader.ReadBytes(length - 4);

                packet.Tlvs.Add(new CdpTlv
                {
                    Type = type,
                    Length = length,
                    Value = value
                });
            }

            return packet;
        }
        catch
        {
            return null;
        }
    }
}

public class CdpTlv
{
    public CdpTlvType Type { get; set; }
    public ushort Length { get; set; }
    public byte[] Value { get; set; } = Array.Empty<byte>();
}

public enum CdpTlvType : ushort
{
    DeviceId = 0x0001,
    Address = 0x0002,
    PortId = 0x0003,
    Capabilities = 0x0004,
    Version = 0x0005,
    Platform = 0x0006,
    IpNetworkPrefix = 0x0007,
    VtpManagementDomain = 0x0009,
    NativeVlan = 0x000A,
    Duplex = 0x000B,
    VoipVlanReply = 0x000E,
    VoipVlanQuery = 0x000F,
    Power = 0x0010,
    Mtu = 0x0011,
    ExtendedTrust = 0x0012,
    UntrustedPortCos = 0x0013,
    SystemName = 0x0014,
    SystemObjectId = 0x0015,
    ManagementAddress = 0x0016,
    Location = 0x0017,
    ExternalPortId = 0x0018,
    PowerRequested = 0x0019,
    PowerAvailable = 0x001A,
    PortUnidirectional = 0x001B,
    FourWirePoePowerRequested = 0x001C,
    FourWirePoePowerAvailable = 0x001D,
    Unknown = 0xFFFF
}

public class CdpConfig : ConfigurationBase
{
    public bool IsEnabled { get; set; } = false;
    public string DeviceId { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Version { get; set; } = "";
    public List<string> Capabilities { get; set; } = new();
    public int Timer { get; set; } = 60; // Advertisement timer in seconds
    public int HoldTime { get; set; } = 180; // Hold time in seconds
    public Dictionary<string, CdpInterfaceSettings> InterfaceSettings { get; set; } = new();

    public override ValidationResult Validate()
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrEmpty(DeviceId))
            results.Add(ValidationResult.Error("CDP Device ID is required"));

        if (Timer < 5 || Timer > 254)
            results.Add(ValidationResult.Error("CDP timer must be between 5 and 254 seconds"));

        if (HoldTime <= Timer)
            results.Add(ValidationResult.Error("CDP hold time must be greater than timer"));

        if (HoldTime < 10 || HoldTime > 255)
            results.Add(ValidationResult.Error("CDP hold time must be between 10 and 255 seconds"));

        return CombineResults(results.ToArray());
    }

    public CdpConfig Clone()
    {
        return new CdpConfig
        {
            IsEnabled = IsEnabled,
            DeviceId = DeviceId,
            Platform = Platform,
            Version = Version,
            Capabilities = new List<string>(Capabilities),
            Timer = Timer,
            HoldTime = HoldTime,
            InterfaceSettings = new Dictionary<string, CdpInterfaceSettings>(
                InterfaceSettings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()))
        };
    }
}

public class CdpInterfaceSettings
{
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 64;
    public string Description { get; set; } = "";

    public CdpInterfaceSettings Clone()
    {
        return new CdpInterfaceSettings
        {
            IsEnabled = IsEnabled,
            Priority = Priority,
            Description = Description
        };
    }
}