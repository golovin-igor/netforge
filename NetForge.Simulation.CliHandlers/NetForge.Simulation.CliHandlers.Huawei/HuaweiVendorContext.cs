using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Huawei
{
    /// <summary>
    /// Huawei-specific vendor context implementation
    /// </summary>
    public class HuaweiVendorContext(INetworkDevice device) : IVendorContext
    {
        private readonly INetworkDevice _device = device ?? throw new ArgumentNullException(nameof(device));
        private readonly HuaweiVendorCapabilities _capabilities = new(device);

        public string VendorName => "Huawei";
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
            if (command.TrimStart().StartsWith("display ", StringComparison.OrdinalIgnoreCase))
            {
                return "show " + command.TrimStart().Substring(8);
            }
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
