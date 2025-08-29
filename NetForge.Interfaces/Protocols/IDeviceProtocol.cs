
using System.Net.Sockets;

namespace NetForge.Interfaces
{
    /// <summary>
    /// Unified interface for all device protocols in NetForge
    /// Defines the complete contract for protocol lifecycle, state management, and vendor support
    /// </summary>
    public interface IDeviceProtocol
    {
        /// <summary>
        /// Protocol type enumeration
        /// </summary>
        ProtocolType Type { get; }

        /// <summary>
        /// Human-readable name of the protocol
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the protocol implementation
        /// </summary>
        string Version { get; }

        /// <summary>
        /// List of vendors supported by this protocol implementation
        /// </summary>
        ICollection<string> SupportedVendors { get; }

        /// <summary>
        /// Get the current state of this protocol
        /// </summary>
        /// <returns>Protocol state information or null if not available</returns>
        IProtocolState GetState();

        /// <summary>
        /// Get the current configuration of this protocol
        /// </summary>
        /// <returns>Configuration object or null if not configured</returns>
        object GetConfiguration();

        /// <summary>
        /// Get performance metrics for this protocol
        /// </summary>
        /// <returns>Metrics object or null if not available</returns>
        object GetMetrics();

        /// <summary>
        /// Start the protocol
        /// </summary>
        /// <returns>Task representing the start operation</returns>
        Task Start();

        /// <summary>
        /// Stop the protocol
        /// </summary>
        /// <returns>Task representing the stop operation</returns>
        Task Stop();

        /// <summary>
        /// Configure the protocol with the provided configuration
        /// </summary>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>Task with success status</returns>
        Task<bool> Configure(object configuration);
    }
}
