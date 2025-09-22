namespace NetForge.SimulationModel.Management;

public interface IHttpRequest
{
    string Method { get; }
    string Url { get; }
    string Body { get; }
}
