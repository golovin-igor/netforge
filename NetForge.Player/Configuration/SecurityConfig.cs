// TODO: Phase 1.3 - Implement Configuration Management
// This class manages security configuration settings

using System;
using System.Collections.Generic;

namespace NetForge.Player.Configuration;

/// <summary>
/// Security configuration
/// </summary>
public class SecurityConfig
{
    // TODO: Implement comprehensive security settings
    // - Network isolation and firewall rules
    // - Access control and authentication
    // - Audit logging and monitoring
    // - Encryption and secure communication
    // - Threat detection and prevention
    
    public bool IsolateSimulatedNetwork { get; set; } = true;
    public List<string> AllowedExternalHosts { get; set; } = new() { "192.168.100.0/24" };
    public bool EnableFirewall { get; set; } = true;
    public bool EnableAuditLogging { get; set; } = true;
    
    // TODO: Add security policies
    // public List<FirewallRule> FirewallRules { get; set; } = new();
    // public AccessControlConfig AccessControl { get; set; } = new();
    // public EncryptionConfig Encryption { get; set; } = new();
    // public List<string> BlockedCommands { get; set; } = new();
}