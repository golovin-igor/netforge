namespace NetForge.SimulationModel.Events;

public interface IHttpRequestEvent : IApplicationEvent
{
    string Method { get; }
    string Uri { get; }
    int ResponseCode { get; }
    TimeSpan ResponseTime { get; }
}
