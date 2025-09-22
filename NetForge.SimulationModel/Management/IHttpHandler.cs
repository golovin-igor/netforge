namespace NetForge.SimulationModel.Management;

public interface IHttpHandler
{
    bool IsEnabled { get; }
    int Port { get; }
    bool UseHttps { get; }

    void Enable(int port, bool useHttps);
    void Disable();
    void RegisterRoute(string path, HttpMethod method, IHttpRequestHandler handler);
    void UnregisterRoute(string path, HttpMethod method);
    IHttpResponse ProcessRequest(IHttpRequest request);
}
