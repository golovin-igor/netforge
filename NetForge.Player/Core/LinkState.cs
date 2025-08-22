// TODO: Phase 1.2 - Implement Network Management System
// This enum represents network link states

namespace NetForge.Player.Core;

/// <summary>
/// Link state enumeration
/// </summary>
public enum LinkState
{
    // TODO: Expand LinkState with comprehensive state options
    // - Add transitional states (Connecting, Disconnecting)
    // - Add error states (Failed, Degraded, Flapping)
    // - Add maintenance states (Disabled, Testing)
    
    Up,
    Down,
    Unknown
    
    // TODO: Add additional states
    // Connecting,
    // Disconnecting,
    // Failed,
    // Degraded,
    // Flapping,
    // Disabled,
    // Testing
}