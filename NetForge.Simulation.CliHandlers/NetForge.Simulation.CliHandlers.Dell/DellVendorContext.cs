using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Dell
{
    /// <summary>
    /// Dell-specific vendor context implementation
    /// </summary>
    public class DellVendorContext : IVendorContext
    {
        private readonly NetworkDevice _device;
        private readonly DellVendorCapabilities _capabilities;

        public DellVendorContext(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _capabilities = new DellVendorCapabilities(device);
        }

        public string VendorName => "Dell";
        public IVendorCapabilities Capabilities => _capabilities;
        public ICandidateConfiguration? CandidateConfig => null;

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
                "enable" => "Enter privileged mode",
                "exit" => "Exit current mode",
                "ping" => "Send ping packets to test connectivity",
                _ => $"No help available for \"{command}\""
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
                "privileged" => new[] { "configure", "show", "ping", "exit" },
                "config" => new[] { "interface", "hostname", "ip", "exit" },
                "interface" => new[] { "ip", "shutdown", "no", "description", "exit" },
                _ => Array.Empty<string>()
            };
        }

        public string PreprocessCommand(string command)
        {
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            if (!string.IsNullOrEmpty(output) && !output.EndsWith("\n"))
                return output + "\n";
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            return _device.GetCurrentInterface();
        }
    }
}
