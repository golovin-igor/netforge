using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Builders;

public interface IInterfaceBuilder
{
    IInterfaceBuilder WithMacAddress(string macAddress);
    IInterfaceBuilder WithIpAddress(string ipAddress, int subnetMask);
    IInterfaceBuilder WithDescription(string description);
    IInterfaceBuilder WithVlan(int vlanId);
    IInterfaceBuilder WithSpeed(LinkSpeed speed);
}
