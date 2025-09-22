namespace NetForge.SimulationModel.Types;

public enum Layer3EventType
{
    RouteAdded,
    RouteDeleted,
    RouteChanged,
    ArpRequest,
    ArpReply,
    NeighborUp,
    NeighborDown,
    NetworkUnreachable
}
