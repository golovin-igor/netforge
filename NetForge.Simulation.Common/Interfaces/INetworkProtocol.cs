using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;

namespace NetForge.Simulation.Common.Interfaces
{
    /// <summary>
    /// Legacy interface maintained for backward compatibility
    /// New implementations should use IDeviceProtocol directly
    /// </summary>
    [Obsolete("Use IDeviceProtocol instead. This interface is maintained for backward compatibility.")]
    public interface INetworkProtocol
    {
        /// <summary>
        /// The type of protocol (OSPF, BGP, etc.)
        /// </summary>
        ProtocolType Type { get; }
        
        /// <summary>
        /// Initialize the protocol with device context
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        void Initialize(NetworkDevice device);
        
        /// <summary>
        /// Update the protocol state (called periodically by simulation engine)
        /// </summary>
        /// <param name="device">The network device this protocol runs on</param>
        Task UpdateState(NetworkDevice device);
        
        /// <summary>
        /// Subscribe to network events for protocol operation
        /// </summary>
        /// <param name="eventBus">The network event bus</param>
        /// <param name="self">The device this protocol is running on</param>
        void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self);
    }
}
