using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Juniper-specific vendor context implementation
    /// </summary>
    public class JuniperVendorContext : IVendorContext
    {
        private readonly NetworkDevice _device;
        private readonly JuniperVendorCapabilities _capabilities;

        public JuniperVendorContext(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _capabilities = new JuniperVendorCapabilities(device);
        }

        public string VendorName => "Juniper";
        public IVendorCapabilities Capabilities => _capabilities;
        public ICandidateConfiguration? CandidateConfig => null; // Simplified - JunOS typically uses candidate config

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
                "set" => "Set configuration values",
                "delete" => "Delete configuration values",
                "commit" => "Commit configuration changes",
                "rollback" => "Rollback configuration changes",
                "exit" => "Exit current mode",
                "ping" => "Send ping packets to test connectivity",
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
                "operational" => new[] { "show", "ping", "configure", "exit" },
                "configuration" => new[] { "set", "delete", "show", "commit", "rollback", "exit" },
                _ => Array.Empty<string>()
            };
        }

        public string PreprocessCommand(string command)
        {
            // Handle Juniper-specific command preprocessing
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            // Ensure output ends with proper newline for Juniper format
            if (!string.IsNullOrEmpty(output) && !output.EndsWith('\n'))
                return output + '\n';
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            // Juniper-specific configuration rendering
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            // Get the currently selected interface for configuration
            return _device.GetCurrentInterface();
        }
    }
}
