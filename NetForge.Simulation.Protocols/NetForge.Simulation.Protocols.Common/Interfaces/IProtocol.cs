using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Common.Interfaces
{
    /// <summary>
    /// Base interface for all protocols in the NetForge system
    /// Provides core protocol identification and versioning
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// The type of protocol (OSPF, BGP, etc.)
        /// </summary>
        ProtocolType Type { get; }

        /// <summary>
        /// Human-readable name of the protocol
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version string of the protocol implementation
        /// </summary>
        string Version { get; }
    }
}