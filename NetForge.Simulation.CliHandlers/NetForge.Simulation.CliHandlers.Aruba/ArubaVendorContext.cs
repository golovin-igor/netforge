using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Aruba
{
    /// <summary>
    /// Vendor-specific context for Aruba devices
    /// </summary>
    public class ArubaVendorContext : IVendorContext
    {
        private readonly NetworkDevice _device;
        private readonly ArubaVendorCapabilities _capabilities;

        public ArubaVendorContext(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _capabilities = new ArubaVendorCapabilities(device);
        }

        public string VendorName => "Aruba";

        public IVendorCapabilities Capabilities => _capabilities;

        public ICandidateConfiguration? CandidateConfig => null; // Aruba doesn't use candidate config like Juniper

        public bool IsInMode(string mode)
        {
            var currentMode = _device.GetCurrentMode();
            return mode.ToLower() switch
            {
                "user" => currentMode == "user",
                "privileged" => currentMode == "privileged" || currentMode == "enable",
                "config" => currentMode == "config" || currentMode == "configuration",
                "interface" => currentMode == "interface",
                "vlan" => currentMode == "vlan",
                _ => false
            };
        }

        public bool SetMode(string mode)
        {
            try
            {
                _device.SetCurrentMode(mode);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetModePrompt()
        {
            var hostname = _device.Name;
            var mode = _device.GetCurrentMode();
            
            return mode switch
            {
                "config" => $"{hostname}(config)# ",
                "interface" => $"{hostname}(eth0)# ",
                "vlan" => $"{hostname}(vlan)# ", 
                "privileged" => $"{hostname}# ",
                _ => $"{hostname}> "
            };
        }

        public Dictionary<string, string> GetErrorMessages()
        {
            return new Dictionary<string, string>
            {
                ["invalid_command"] = "Invalid input -> {0}",
                ["incomplete_command"] = "% Incomplete command",
                ["invalid_interface"] = "% Invalid interface",
                ["access_denied"] = "% Access denied",
                ["invalid_vlan"] = "% Invalid VLAN",
                ["invalid_ip"] = "% Invalid IP address",
                ["interface_not_found"] = "% Interface not found",
                ["vlan_not_found"] = "% VLAN not found"
            };
        }

        public List<string> GetSupportedFeatures()
        {
            return new List<string>
            {
                "vlan_support",
                "interface_configuration", 
                "ip_routing",
                "access_lists",
                "port_security",
                "spanning_tree",
                "link_aggregation"
            };
        }

        public string GetCommandHelp(string command)
        {
            return command.ToLower() switch
            {
                "ping" => "Send ICMP echo requests",
                "configure" => "Enter global configuration mode", 
                "show" => "Display system information",
                "interface" => "Configure interface parameters",
                "vlan" => "Configure VLAN parameters",
                "ip" => "Configure IP settings",
                "write" => "Save configuration to startup-config",
                "reload" => "Restart the system",
                _ => "No help available for this command"
            };
        }

        public IEnumerable<string> GetCommandCompletions(string[] commandParts)
        {
            if (commandParts.Length == 0)
                return new[] { "ping", "show", "configure", "exit", "write", "reload" };

            var firstCommand = commandParts[0].ToLower();
            return firstCommand switch
            {
                "show" => new[] { "running-config", "startup-config", "version", "interfaces", "vlan", "ip", "arp" },
                "ip" => new[] { "address", "route" },
                "interface" => new[] { "ethernet", "vlan" },
                "vlan" => GetVlanCompletions(),
                _ => Array.Empty<string>()
            };
        }

        private string[] GetVlanCompletions()
        {
            var completions = new List<string>();
            for (int i = 1; i <= 4094; i++)
            {
                completions.Add(i.ToString());
            }
            return completions.ToArray();
        }

        public string PreprocessCommand(string command)
        {
            // Aruba preprocessing - normalize command syntax
            return command.Trim();
        }

        public string PostprocessOutput(string output)
        {
            // Aruba postprocessing - add vendor-specific formatting
            return output;
        }

        public string RenderConfiguration(object configData)
        {
            // Render configuration in Aruba format
            return configData?.ToString() ?? "";
        }

        public string GetCurrentInterface()
        {
            // Get the current interface from device context
            var interfaces = _device.GetAllInterfaces();
            var currentInterface = interfaces?.Values?.FirstOrDefault(i => i.IsUp);
            return currentInterface?.Name ?? "";
        }
    }
} 
