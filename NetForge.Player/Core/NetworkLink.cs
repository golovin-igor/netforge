// TODO: Phase 1.2 - Implement Network Management System
// This class represents network link information

using System;

namespace NetForge.Player.Core;

/// <summary>
/// Network link information
/// </summary>
public class NetworkLink
{
    // TODO: Enhance NetworkLink with comprehensive link properties
    // - Link performance metrics (bandwidth, latency, packet loss)
    // - Link state and health information
    // - Traffic statistics and monitoring
    // - Link configuration and properties
    // - Security and access control settings
    
    public string Device1 { get; set; } = string.Empty;
    public string Interface1 { get; set; } = string.Empty;
    public string Device2 { get; set; } = string.Empty;
    public string Interface2 { get; set; } = string.Empty;
    public LinkState State { get; set; } = LinkState.Up;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // TODO: Add link metrics and properties
    // public long Bandwidth { get; set; } = 1_000_000_000; // 1 Gbps default
    // public TimeSpan Latency { get; set; } = TimeSpan.FromMilliseconds(1);
    // public double PacketLoss { get; set; } = 0.0;
    // public LinkMetrics Metrics { get; set; } = new();
}