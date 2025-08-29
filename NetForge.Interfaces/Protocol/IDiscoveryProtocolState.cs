using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Common.State
{
    /// <summary>
    /// Extended state interface for discovery protocols (CDP, LLDP)
    /// Provides discovery-specific state information and metrics
    /// </summary>
    public interface IDiscoveryProtocolState : IProtocolState
    {
        /// <summary>
        /// Number of devices discovered by this protocol
        /// </summary>
        int DiscoveredDevices { get; }

        /// <summary>
        /// Interval between discovery advertisements
        /// </summary>
        TimeSpan DiscoveryInterval { get; }

        /// <summary>
        /// Timestamp of the last discovery advertisement sent
        /// </summary>
        DateTime LastDiscoveryAdvertisement { get; }

        /// <summary>
        /// Hold time for discovered neighbors
        /// </summary>
        TimeSpan HoldTime { get; }

        /// <summary>
        /// Whether discovery is currently enabled
        /// </summary>
        bool DiscoveryEnabled { get; }

        /// <summary>
        /// Get detailed information about discovered devices
        /// </summary>
        /// <returns>Dictionary mapping device ID to device details</returns>
        Dictionary<string, object> GetDiscoveredDevices();

        /// <summary>
        /// Get interfaces where discovery is active
        /// </summary>
        /// <returns>Enumerable of interface names</returns>
        IEnumerable<string> GetActiveInterfaces();
    }
}