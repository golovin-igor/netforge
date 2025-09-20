using NetForge.Simulation.Protocols.HTTP;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Provides HTTP-specific configuration for network devices.
/// </summary>
public interface IHttpConfigurationProvider
{
    /// <summary>
    /// Gets the HTTP configuration.
    /// </summary>
    HttpConfig GetHttpConfiguration();

    /// <summary>
    /// Sets the HTTP configuration.
    /// </summary>
    void SetHttpConfiguration(HttpConfig config);
}