namespace NetForge.SimulationModel.Configuration;

public interface IVlanConfiguration
{
    int VlanId { get; }
    string Name { get; }
    string Description { get; }
    string IpAddress { get; }
    string SubnetMask { get; }
    bool IsActive { get; }
    bool IsTagged { get; }
    string AssociatedInterface { get; }

}
