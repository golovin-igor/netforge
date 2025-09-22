namespace NetForge.Protocols.Layer2.CDP;

public class CdpNeighbor
{
    public string DeviceId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Capabilities { get; set; } = string.Empty;
    public string LocalInterface { get; set; } = string.Empty;
    public string RemoteInterface { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastHeard { get; set; }
    public int HoldTime { get; set; }
    public string Version { get; set; } = string.Empty;
    public int NativeVlan { get; set; }
    public string Duplex { get; set; } = string.Empty;

    public void Update(CdpPacket packet)
    {
        Platform = packet.Platform;
        Capabilities = packet.Capabilities;
        RemoteInterface = packet.InterfaceId;
        IpAddress = packet.IpAddress;
        LastHeard = DateTime.UtcNow;
        HoldTime = packet.Ttl;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow - LastHeard > TimeSpan.FromSeconds(HoldTime);
    }

    public override string ToString()
    {
        return $"{DeviceId} ({Platform}) on {LocalInterface} -> {RemoteInterface}";
    }
}