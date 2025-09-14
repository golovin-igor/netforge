using NetForge.Player.Interfaces;

namespace NetForge.Player.Services;

/// <summary>
/// Interface for managing device console sessions
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Create a new session to a device
    /// </summary>
    /// <param name="device">Target device</param>
    /// <param name="protocol">Connection protocol (console, telnet, ssh)</param>
    /// <returns>Device session</returns>
    Task<IDeviceSession?> CreateSessionAsync(INetworkDevice device, string protocol);
    
    /// <summary>
    /// Get an existing session by ID
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Device session or null if not found</returns>
    Task<IDeviceSession?> GetSessionAsync(string sessionId);
    
    /// <summary>
    /// Close a session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>True if session was closed</returns>
    Task<bool> CloseSessionAsync(string sessionId);
    
    /// <summary>
    /// Get all active sessions
    /// </summary>
    /// <returns>List of active sessions</returns>
    Task<IEnumerable<IDeviceSession>> GetActiveSessionsAsync();
}

/// <summary>
/// Interface for device console sessions
/// </summary>
public interface IDeviceSession
{
    /// <summary>
    /// Session identifier
    /// </summary>
    string SessionId { get; }
    
    /// <summary>
    /// Target device
    /// </summary>
    INetworkDevice Device { get; }
    
    /// <summary>
    /// Connection protocol
    /// </summary>
    string Protocol { get; }
    
    /// <summary>
    /// Session creation time
    /// </summary>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Whether session is active
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Start interactive session
    /// </summary>
    Task StartInteractiveSessionAsync();
    
    /// <summary>
    /// Send command to device
    /// </summary>
    /// <param name="command">Command to send</param>
    /// <returns>Command output</returns>
    Task<string> SendCommandAsync(string command);
    
    /// <summary>
    /// Close the session
    /// </summary>
    Task CloseAsync();
}