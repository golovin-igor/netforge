namespace NetForge.Player.Services;

/// <summary>
/// Interface for managing terminal servers (Telnet, SSH, WebSocket)
/// </summary>
public interface ITerminalServerManager
{
    /// <summary>
    /// Start all configured terminal servers
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stop all terminal servers
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Get status of all terminal servers
    /// </summary>
    /// <returns>Server status information</returns>
    Task<TerminalServerStatus> GetStatusAsync();
    
    /// <summary>
    /// Start a specific terminal server
    /// </summary>
    /// <param name="serverType">Server type (telnet, ssh, websocket)</param>
    /// <param name="port">Port number</param>
    Task StartServerAsync(string serverType, int port);
    
    /// <summary>
    /// Stop a specific terminal server
    /// </summary>
    /// <param name="serverType">Server type</param>
    Task StopServerAsync(string serverType);
}

/// <summary>
/// Terminal server status information
/// </summary>
public class TerminalServerStatus
{
    public bool TelnetServerRunning { get; set; }
    public int TelnetPort { get; set; }
    
    public bool SshServerRunning { get; set; }
    public int SshPort { get; set; }
    
    public bool WebSocketServerRunning { get; set; }
    public int WebSocketPort { get; set; }
    
    public int ActiveConnections { get; set; }
    public DateTime StartTime { get; set; }
}