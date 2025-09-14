using Microsoft.Extensions.Logging;

namespace NetForge.Player.Services;

/// <summary>
/// Terminal server manager implementation for NetForge.Player
/// </summary>
public class TerminalServerManager : ITerminalServerManager
{
    private readonly ILogger<TerminalServerManager> _logger;
    private readonly TerminalServerStatus _status = new();

    public TerminalServerManager(ILogger<TerminalServerManager> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting terminal servers");
        
        // TODO: Implement actual terminal server startup
        _status.StartTime = DateTime.UtcNow;
        
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping terminal servers");
        
        // TODO: Implement actual terminal server shutdown
        _status.TelnetServerRunning = false;
        _status.SshServerRunning = false;
        _status.WebSocketServerRunning = false;
        
        await Task.CompletedTask;
    }

    public async Task<TerminalServerStatus> GetStatusAsync()
    {
        return _status;
    }

    public async Task StartServerAsync(string serverType, int port)
    {
        _logger.LogInformation("Starting {ServerType} server on port {Port}", serverType, port);
        
        // TODO: Implement actual server startup
        switch (serverType.ToLowerInvariant())
        {
            case "telnet":
                _status.TelnetServerRunning = true;
                _status.TelnetPort = port;
                break;
            case "ssh":
                _status.SshServerRunning = true;
                _status.SshPort = port;
                break;
            case "websocket":
                _status.WebSocketServerRunning = true;
                _status.WebSocketPort = port;
                break;
        }
        
        await Task.CompletedTask;
    }

    public async Task StopServerAsync(string serverType)
    {
        _logger.LogInformation("Stopping {ServerType} server", serverType);
        
        // TODO: Implement actual server shutdown
        switch (serverType.ToLowerInvariant())
        {
            case "telnet":
                _status.TelnetServerRunning = false;
                break;
            case "ssh":
                _status.SshServerRunning = false;
                break;
            case "websocket":
                _status.WebSocketServerRunning = false;
                break;
        }
        
        await Task.CompletedTask;
    }
}