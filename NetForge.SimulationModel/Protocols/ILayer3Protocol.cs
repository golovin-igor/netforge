using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Protocols;

public interface ILayer3Protocol : INetworkProtocol
{
    void ProcessPacket(IPacket packet, string ingressInterface);
    void SendPacket(IPacket packet, string destinationIp);
    IRoutingTable GetRoutingTable();
    void AddRoute(IRoute route);
    void RemoveRoute(string routeId);
    IArpTable GetArpTable();
    void OnRouteChange(IRoute route, RouteOperation operation);
    void OnArpUpdate(string ipAddress, string macAddress);
    event EventHandler<IRouteChangeEvent> RouteChanged;
    event EventHandler<IArpEvent> ArpUpdated;
}
