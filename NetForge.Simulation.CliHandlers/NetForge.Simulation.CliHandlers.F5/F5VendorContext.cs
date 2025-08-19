using NetForge.Simulation.Interfaces;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.F5
{
    /// <summary>
    /// F5 BIG-IP vendor context implementation
    /// </summary>
    public class F5VendorContext : IVendorContext
    {
        private readonly NetworkDevice _device;
        private readonly F5VendorCapabilities _capabilities;

        public F5VendorContext(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _capabilities = new F5VendorCapabilities(device);
        }

        public string VendorName => "F5";
        public IVendorCapabilities Capabilities => _capabilities;
        public ICandidateConfiguration? CandidateConfig => null; // F5 doesn't use candidate config

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
                "show" => "Display F5 BIG-IP system information",
                "configure" => "Enter configuration mode",
                "tmsh" => "Enter TMSH (Traffic Management Shell) mode",
                "bash" => "Enter bash shell mode",
                "enable" => "Enter privileged mode",
                "disable" => "Exit privileged mode",
                "exit" => "Exit current mode",
                "ping" => "Send ping packets to test connectivity",
                "list" => "List objects in current context",
                "create" => "Create a new object",
                "modify" => "Modify an existing object",
                "delete" => "Delete an object",
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
                "user" => new[] { "enable", "ping", "show", "exit", "tmsh", "bash" },
                "privileged" => new[] { "configure", "show", "ping", "disable", "exit", "tmsh", "bash" },
                "config" => new[] { "interface", "hostname", "ip", "exit" },
                "tmsh" => new[] { "list", "create", "modify", "delete", "show", "exit" },
                "bash" => new[] { "ls", "cd", "pwd", "exit" },
                _ => Array.Empty<string>()
            };
        }

        public string PreprocessCommand(string command)
        {
            // Handle F5-specific command preprocessing
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            // Ensure output ends with proper newline for F5 format
            if (!string.IsNullOrEmpty(output) && !output.EndsWith('\n'))
                return output + '\n';
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            // F5-specific configuration rendering
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            // Get the currently selected interface for configuration
            return _device.GetCurrentInterface();
        }
    }
} 
