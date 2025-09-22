using NetForge.SimulationModel.Core;

namespace NetForge.SimulationModel.Builders;

public interface ITopologyBuilder
{
    ITopologyBuilder WithName(string name);
    IDeviceBuilder AddDevice(string deviceId);
    ITopologyBuilder Connect(string device1, string interface1, string device2, string interface2);
    ITopology Build();
}
