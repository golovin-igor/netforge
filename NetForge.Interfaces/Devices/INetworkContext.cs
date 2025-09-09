using NetForge.Simulation.Common.Common;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Provides context about the device's parent network.
/// This interface allows devices to be aware of their network topology context.
/// </summary>
public interface INetworkContext
{
    /// <summary>
    /// Gets or sets the parent network that contains this device.
    /// </summary>
    INetwork? ParentNetwork { get; set; }
}