using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Protocols;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Builders;

public interface IDeviceBuilder
{
    IDeviceBuilder WithHostname(string hostname);
    IDeviceBuilder WithVendor(string vendor, string model, string version);
    IDeviceBuilder WithCapability(DeviceCapabilityType capability);
    IDeviceBuilder AddInterface(string name, InterfaceType type);
    IDeviceBuilder ConfigureInterface(string name, Action<IInterfaceBuilder> configure);
    IDeviceBuilder EnableProtocol<T>(Action<T> configure) where T : INetworkProtocol;
    IDeviceBuilder EnableCli(int port, CliProtocol protocol);
    IDeviceBuilder EnableSnmp(SnmpVersion version, string community);
    IDeviceBuilder EnableHttp(int port, bool useHttps);
    IDeviceBuilder WithConfiguration(Action<IDeviceConfiguration> configure);
    ITopologyBuilder EndDevice();
}
