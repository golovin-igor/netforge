using Microsoft.Extensions.Logging;
using NetForge.Player.Interfaces;

namespace NetForge.Player.Services;

/// <summary>
/// Session manager implementation for NetForge.Player
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly Dictionary<string, IDeviceSession> _sessions = new();

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }

    public async Task<IDeviceSession?> CreateSessionAsync(INetworkDevice device, string protocol)
    {
        _logger.LogInformation("Creating {Protocol} session to device {DeviceName}", protocol, device.Name);
        
        var sessionId = Guid.NewGuid().ToString("N")[..8];
        var session = new StubDeviceSession(sessionId, device, protocol);
        
        _sessions[sessionId] = session;
        
        return session;
    }

    public async Task<IDeviceSession?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public async Task<bool> CloseSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            await session.CloseAsync();
            _sessions.Remove(sessionId);
            return true;
        }
        
        return false;
    }

    public async Task<IEnumerable<IDeviceSession>> GetActiveSessionsAsync()
    {
        return _sessions.Values.Where(s => s.IsActive);
    }
}

/// <summary>
/// Stub implementation of IDeviceSession for compilation
/// </summary>
internal class StubDeviceSession : IDeviceSession
{
    public StubDeviceSession(string sessionId, INetworkDevice device, string protocol)
    {
        SessionId = sessionId;
        Device = device;
        Protocol = protocol;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public string SessionId { get; }
    public INetworkDevice Device { get; }
    public string Protocol { get; }
    public DateTime CreatedAt { get; }
    public bool IsActive { get; private set; }

    public async Task StartInteractiveSessionAsync()
    {
        Console.WriteLine($"Starting interactive session to {Device.Name} via {Protocol}...");
        Console.WriteLine("(This is a stub implementation - interactive session not yet implemented)");
        Console.WriteLine("Press any key to return to NetForge console...");
        
        Console.ReadKey(true);
    }

    public async Task<string> SendCommandAsync(string command)
    {
        // TODO: Implement actual command sending
        return $"Command '{command}' sent to {Device.Name} (stub response)";
    }

    public async Task CloseAsync()
    {
        IsActive = false;
    }
}