// TODO: Phase 1.3 - Implement Configuration Management
// This class manages terminal server configuration settings

using System;
using System.Collections.Generic;

namespace NetForge.Player.Configuration;

/// <summary>
/// Terminal server configuration
/// </summary>
public class TerminalConfig
{
    // TODO: Implement terminal server configuration
    // - Multi-protocol server settings (Telnet, SSH, WebSocket)
    // - Authentication and security options
    // - Session limits and timeouts
    // - Terminal emulation settings
    // - Logging and audit configuration
    
    public bool EnableTelnet { get; set; } = true;
    public int TelnetPort { get; set; } = 2323;
    public bool EnableSSH { get; set; } = true;
    public int SSHPort { get; set; } = 2222;
    public bool EnableWebSocket { get; set; } = true;
    public int WebSocketPort { get; set; } = 8080;
    public int SessionTimeoutMinutes { get; set; } = 60;
    
    // TODO: Add authentication settings
    // public AuthenticationConfig Authentication { get; set; } = new();
    // public string DefaultUsername { get; set; } = "admin";
    // public string DefaultPassword { get; set; } = "admin";
    // public bool RequireAuthentication { get; set; } = false;
}