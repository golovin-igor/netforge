using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Manages protocol registration, lifecycle, and coordination for network devices.
/// This interface handles all protocol-related operations.
/// </summary>
public interface IProtocolHost
{
    /// <summary>
    /// Gets the enhanced protocol service for this device.
    /// </summary>
    /// <returns>Protocol service instance.</returns>
    IProtocolService GetProtocolService();

    /// <summary>
    /// Registers a network protocol implementation with the device.
    /// </summary>
    /// <param name="protocol">The protocol to register.</param>
    void RegisterProtocol(IDeviceProtocol protocol);

    /// <summary>
    /// Updates the state of all registered network protocols.
    /// </summary>
    Task UpdateAllProtocolStates();

    /// <summary>
    /// Subscribe all registered protocols to events.
    /// This should be called when a device is added to a network.
    /// </summary>
    void SubscribeProtocolsToEvents();

    /// <summary>
    /// Gets all registered network protocols.
    /// </summary>
    /// <returns>Read-only collection of registered protocols.</returns>
    IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols();
}