using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Interfaces.CLI;

public interface ICliContext
{
    /// <summary>
    /// The device executing the command
    /// </summary>
    INetworkDevice Device { get; }

    /// <summary>
    /// The command split into parts
    /// </summary>
    string[] CommandParts { get; }

    /// <summary>
    /// The original, full command string
    /// </summary>
    string FullCommand { get; }

    /// <summary>
    /// The current device mode
    /// </summary>
    string CurrentMode { get; }

    /// <summary>
    /// Whether this is a help request (command ends with ?)
    /// </summary>
    bool IsHelpRequest { get; init; }

    /// <summary>
    /// Additional parameters for command execution
    /// </summary>
    Dictionary<string, string> Parameters { get; init; }

    IVendorContext? VendorContext { get; set; }
    T GetService<T>() where T : class;
}
