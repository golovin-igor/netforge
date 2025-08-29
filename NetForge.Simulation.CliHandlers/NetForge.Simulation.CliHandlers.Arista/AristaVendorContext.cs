using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Arista
{
    /// <summary>
    /// Arista-specific vendor context implementation
    /// </summary>
    public class AristaVendorContext(INetworkDevice device) : IVendorContext
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));
        private readonly AristaVendorCapabilities _capabilities = new(device);

        public string VendorName => "Arista";
        public IVendorCapabilities Capabilities => _capabilities;
        public ICandidateConfiguration? CandidateConfig => null; // Arista doesn't use candidate config in this implementation

        public bool IsInMode(string mode)
        {
            var currentMode = _device.GetCurrentMode();
            return currentMode.Equals(mode, StringComparison.OrdinalIgnoreCase);
        }

        public string GetModePrompt()
        {
            return _device.GetPrompt();
        }

        public string GetCommandHelp(string command)
        {
            return command.ToLower() switch
            {
                "show" => "Display device information",
                "configure" => "Enter configuration mode",
                "interface" => "Configure an interface",
                "router" => "Configure routing protocol",
                "enable" => "Enter privileged mode",
                "exit" => "Exit current mode",
                "ping" => "Send ping packets to test connectivity",
                "write" => "Save configuration",
                "reload" => "Restart the system",
                _ => $"No help available for '{command}'"
            };
        }

        public IEnumerable<string> GetCommandCompletions(string[] commandParts)
        {
            if (commandParts.Length == 0)
                return Array.Empty<string>();

            var currentMode = _device.GetCurrentMode();
            return currentMode.ToLower() switch
            {
                "user" => new[] { "enable", "ping", "show", "exit" },
                "privileged" => new[] { "configure", "show", "ping", "write", "reload", "exit" },
                "config" => new[] { "interface", "router", "hostname", "ip", "vlan", "exit" },
                "interface" => new[] { "ip", "shutdown", "no", "description", "switchport", "exit" },
                "router" => new[] { "network", "neighbor", "router-id", "version", "exit" },
                _ => Array.Empty<string>()
            };
        }

        public string PreprocessCommand(string command)
        {
            // Handle Arista-specific command preprocessing
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            // Ensure output ends with proper newline for Arista format
            if (!string.IsNullOrEmpty(output) && !output.EndsWith('\n'))
                return output + '\n';
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            // Arista-specific configuration rendering
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            // Get the currently selected interface for configuration
            return _device.GetCurrentInterface();
        }
    }
}
