using System.Threading.Tasks;
using NetSim.Simulation.Common;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.Implementations
{
    /// <summary>
    /// Minimal ARP protocol implementation used to populate and display
    /// the device ARP table. For now this protocol does not actively
    /// generate traffic and simply serves as a marker that ARP is
    /// supported by the device.
    /// </summary>
    public class ArpProtocol : INetworkProtocol
    {
        private NetworkDevice _device;
        public ProtocolType Type => ProtocolType.ARP;

        public void Initialize(NetworkDevice device)
        {
            _device = device;
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            // No event subscriptions required for the basic ARP simulation.
        }

        public Task UpdateState(NetworkDevice device)
        {
            // ARP state updates are event driven in this simplified model.
            return Task.CompletedTask;
        }
    }
}
