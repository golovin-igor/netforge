using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IRouteChangeEvent : ILayer3Event
{
    IRoute Route { get; }
    RouteOperation Operation { get; }
    string Protocol { get; }
    int Metric { get; }
}
