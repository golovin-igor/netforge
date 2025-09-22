namespace NetForge.SimulationModel.Management;

public interface IHttpResponse
{
    int StatusCode { get; }
    string StatusMessage { get; }
    string Body { get; }
}
