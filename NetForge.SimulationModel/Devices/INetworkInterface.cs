using System.Net;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Devices;

public interface INetworkInterface
{
    string Id { get; }
    string Name { get; }
    InterfaceType Type { get; }
    string MacAddress { get; set; }
    InterfaceState State { get; }
    IInterfaceStatistics Statistics { get; }

    void BringUp();
    void BringDown();
    void AssignIpAddress(string ipAddress, int subnetMask);
    void RemoveIpAddress(string ipAddress);
    IReadOnlyCollection<IPAddress> IpAddresses { get; }

    void TransmitFrame(IFrame frame);
    void ReceiveFrame(IFrame frame);
}
