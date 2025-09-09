using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes.Cli;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Handles command processing, CLI state management, and command history for network devices.
/// This service extracts command processing responsibilities from NetworkDevice.
/// </summary>
public class DeviceCommandProcessor : ICommandProcessor
{
    private readonly CommandHistory _commandHistory;
    private readonly ICliHandlerManager? _commandManager;
    private readonly IDeviceIdentity _deviceIdentity;
    
    private DeviceMode _currentMode = DeviceMode.User;
    private string _currentInterface = "";

    public DeviceCommandProcessor(IDeviceIdentity deviceIdentity, ICliHandlerManager? commandManager = null)
    {
        _deviceIdentity = deviceIdentity ?? throw new ArgumentNullException(nameof(deviceIdentity));
        _commandManager = commandManager;
        _commandHistory = new CommandHistory(1000); // Max 1000 commands
    }

    /// <summary>
    /// Asynchronously processes a command and returns the output.
    /// </summary>
    public async Task<string> ProcessCommandAsync(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return "";

        // Add to command history
        _commandHistory.AddCommand(command);

        // Handle history recall commands
        if (command.StartsWith("!") || command.StartsWith("^"))
        {
            var recalledCommand = _commandHistory.ProcessRecallCommand(command);
            if (!string.IsNullOrEmpty(recalledCommand))
            {
                // Process the recalled command
                return await ProcessCommandAsync(recalledCommand);
            }
            return "% Invalid history command";
        }

        // Process command through the handler manager if available
        if (_commandManager != null)
        {
            try
            {
                var result = await _commandManager.ProcessCommandAsync(command);
                return result?.Output ?? "";
            }
            catch (Exception ex)
            {
                return $"% Error processing command: {ex.Message}";
            }
        }

        // Fallback for basic commands when no handler manager is available
        return ProcessBasicCommand(command);
    }

    /// <summary>
    /// Processes basic commands when no handler manager is available.
    /// </summary>
    private string ProcessBasicCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "";

        var cmd = parts[0].ToLowerInvariant();

        return cmd switch
        {
            "enable" => HandleEnableCommand(),
            "disable" => HandleDisableCommand(),
            "configure" when parts.Length > 1 && parts[1] == "terminal" => HandleConfigureTerminal(),
            "exit" => HandleExitCommand(),
            "end" => HandleEndCommand(),
            "history" => _commandHistory.BuildHistoryDisplay(),
            "show" when parts.Length > 1 && parts[1] == "history" => _commandHistory.BuildHistoryDisplay(),
            _ => $"% Invalid input detected at '^' marker."
        };
    }

    /// <summary>
    /// Handles the enable command.
    /// </summary>
    private string HandleEnableCommand()
    {
        _currentMode = DeviceMode.Privileged;
        return "";
    }

    /// <summary>
    /// Handles the disable command.
    /// </summary>
    private string HandleDisableCommand()
    {
        _currentMode = DeviceMode.User;
        _currentInterface = "";
        return "";
    }

    /// <summary>
    /// Handles the configure terminal command.
    /// </summary>
    private string HandleConfigureTerminal()
    {
        if (_currentMode == DeviceMode.Privileged)
        {
            _currentMode = DeviceMode.Config;
            return "Enter configuration commands, one per line. End with CNTL/Z.";
        }
        return "% Invalid input detected at '^' marker.";
    }

    /// <summary>
    /// Handles the exit command.
    /// </summary>
    private string HandleExitCommand()
    {
        switch (_currentMode)
        {
            case DeviceMode.Interface:
            case DeviceMode.Router:
            case DeviceMode.Vlan:
            case DeviceMode.Acl:
                _currentMode = DeviceMode.Config;
                _currentInterface = "";
                break;
            case DeviceMode.Config:
                _currentMode = DeviceMode.Privileged;
                break;
            case DeviceMode.Privileged:
                _currentMode = DeviceMode.User;
                break;
        }
        return "";
    }

    /// <summary>
    /// Handles the end command.
    /// </summary>
    private string HandleEndCommand()
    {
        if (_currentMode != DeviceMode.User)
        {
            _currentMode = DeviceMode.Privileged;
            _currentInterface = "";
            return "";
        }
        return "% Invalid input detected at '^' marker.";
    }

    /// <summary>
    /// Gets the command history for this device.
    /// </summary>
    public CommandHistory GetCommandHistory() => _commandHistory;

    /// <summary>
    /// Clears the command history.
    /// </summary>
    public void ClearCommandHistory()
    {
        _commandHistory.Clear();
    }

    /// <summary>
    /// Gets the current CLI prompt.
    /// </summary>
    public string GetPrompt()
    {
        return GeneratePrompt();
    }

    /// <summary>
    /// Gets the current CLI prompt (alternative method for compatibility).
    /// </summary>
    public string GetCurrentPrompt()
    {
        return GetPrompt();
    }

    /// <summary>
    /// Generates the CLI prompt based on current mode and context.
    /// </summary>
    private string GeneratePrompt()
    {
        var hostname = _deviceIdentity.GetHostname();
        
        return _currentMode switch
        {
            DeviceMode.User => $"{hostname}>",
            DeviceMode.Privileged => $"{hostname}#",
            DeviceMode.Config => $"{hostname}(config)#",
            DeviceMode.Interface => $"{hostname}(config-if)#",
            DeviceMode.Router => $"{hostname}(config-router)#",
            DeviceMode.Vlan => $"{hostname}(config-vlan)#",
            DeviceMode.Acl => $"{hostname}(config-acl)#",
            _ => $"{hostname}>"
        };
    }

    /// <summary>
    /// Gets the current device mode as a string.
    /// </summary>
    public string GetCurrentMode() => _currentMode.ToModeString();

    /// <summary>
    /// Sets the current device mode as a string.
    /// </summary>
    public void SetCurrentMode(string mode)
    {
        _currentMode = DeviceModeExtensions.FromModeString(mode);
    }

    /// <summary>
    /// Gets the current device mode as a strongly typed enum.
    /// </summary>
    public DeviceMode GetCurrentModeEnum() => _currentMode;

    /// <summary>
    /// Sets the current device mode using strongly typed enum.
    /// </summary>
    public void SetCurrentModeEnum(DeviceMode mode)
    {
        _currentMode = mode;
    }

    /// <summary>
    /// Sets the device mode (e.g., configure, interface, etc.).
    /// </summary>
    public void SetMode(string mode)
    {
        SetCurrentMode(mode);
    }

    /// <summary>
    /// Sets the device mode using strongly typed enum.
    /// </summary>
    public void SetModeEnum(DeviceMode mode)
    {
        _currentMode = mode;
    }

    /// <summary>
    /// Gets the current interface being configured.
    /// </summary>
    public string GetCurrentInterface() => _currentInterface;

    /// <summary>
    /// Sets the current interface being configured.
    /// </summary>
    public void SetCurrentInterface(string iface)
    {
        _currentInterface = iface ?? "";
    }
}