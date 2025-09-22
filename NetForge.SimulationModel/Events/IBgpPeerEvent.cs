using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IBgpPeerEvent : ILayer3Event
{
    string PeerAddress { get; }
    int PeerAs { get; }
    BgpState OldState { get; }
    BgpState NewState { get; }
}
