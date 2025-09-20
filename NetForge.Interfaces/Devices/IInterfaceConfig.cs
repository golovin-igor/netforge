using NetForge.Interfaces.Devices;

namespace NetForge.Interfaces.Devices
{

public interface IInterfaceConfig
{
    string Name { get; set; }
    string? IpAddress { get; set; }
    string? SubnetMask { get; set; }
    int VlanId { get; set; } // Default VLAN
    string SwitchportMode { get; set; }
    string Description { get; set; }
    long RxPackets { get; set; }
    long TxPackets { get; set; }
    long RxBytes { get; set; }
    long TxBytes { get; set; }
    int? ChannelGroup { get; set; }
    string? ChannelMode { get; set; }
    string MacAddress { get; set; }
    int Mtu { get; set; }
    string Duplex { get; set; }
    string Speed { get; set; }
    bool OspfEnabled { get; set; }
    int OspfProcessId { get; set; }
    int OspfArea { get; set; }
    int OspfCost { get; set; }
    string OspfNetworkType { get; set; } // broadcast, point-to-point
    bool StpPortfast { get; set; }
    bool StpBpduGuard { get; set; }
    int? IncomingAccessList { get; set; }
    int? OutgoingAccessList { get; set; }
    bool IsUp { get; set; }
    bool IsShutdown { get; set; }
    string GetStatus();
    void SetParentDevice(INetworkDevice device);
}
}
