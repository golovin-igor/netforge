using NetForge.Simulation.DataTypes.Cli;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes.NetworkPrimitives;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Handles command processing, CLI state management, and command history for network devices.
/// This interface manages all command-line interface operations.
/// </summary>
public interface ICommandProcessor
{
    /// <summary>
    /// Asynchronously processes a command and returns the output.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <returns>The command output.</returns>
    Task<string> ProcessCommandAsync(string command);

    /// <summary>
    /// Gets the command history for this device.
    /// </summary>
    CommandHistory GetCommandHistory();

    /// <summary>
    /// Clears the command history.
    /// </summary>
    void ClearCommandHistory();

    /// <summary>
    /// Gets the current CLI prompt.
    /// </summary>
    string GetPrompt();

    /// <summary>
    /// Gets the current CLI prompt (alternative method for compatibility).
    /// </summary>
    string GetCurrentPrompt();

    /// <summary>
    /// Gets the current device mode as a string.
    /// </summary>
    string GetCurrentMode();

    /// <summary>
    /// Sets the current device mode as a string.
    /// </summary>
    /// <param name="mode">The mode to set.</param>
    void SetCurrentMode(string mode);

    /// <summary>
    /// Gets the current device mode as a strongly typed enum.
    /// </summary>
    DeviceMode GetCurrentModeEnum();

    /// <summary>
    /// Sets the current device mode using strongly typed enum.
    /// </summary>
    /// <param name="mode">The device mode to set.</param>
    void SetCurrentModeEnum(DeviceMode mode);

    /// <summary>
    /// Sets the device mode (e.g., configure, interface, etc.).
    /// </summary>
    /// <param name="mode">The mode to set.</param>
    void SetMode(string mode);

    /// <summary>
    /// Sets the device mode using strongly typed enum.
    /// </summary>
    /// <param name="mode">The device mode to set.</param>
    void SetModeEnum(DeviceMode mode);

    /// <summary>
    /// Gets the current interface being configured.
    /// </summary>
    string GetCurrentInterface();

    /// <summary>
    /// Sets the current interface being configured.
    /// </summary>
    /// <param name="iface">The interface name.</param>
    void SetCurrentInterface(string iface);
}