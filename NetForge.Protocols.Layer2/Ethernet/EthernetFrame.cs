namespace NetForge.Protocols.Layer2.Ethernet;

public class EthernetFrame
{
    public string DestinationMac { get; set; } = string.Empty;
    public string SourceMac { get; set; } = string.Empty;
    public ushort EtherType { get; set; }
    public int? Vlan { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public uint Fcs { get; set; } // Frame Check Sequence

    public int Length => 14 + (Vlan.HasValue ? 4 : 0) + Payload.Length + 4; // Header + VLAN + Payload + FCS

    public bool IsUnicast => !DestinationMac.StartsWith("01:") && DestinationMac != "FF:FF:FF:FF:FF:FF";
    public bool IsBroadcast => DestinationMac == "FF:FF:FF:FF:FF:FF";
    public bool IsMulticast => DestinationMac.StartsWith("01:");
}