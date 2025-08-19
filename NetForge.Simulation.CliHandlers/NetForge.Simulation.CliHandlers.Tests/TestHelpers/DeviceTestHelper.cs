using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;

namespace NetForge.Simulation.Tests.CliHandlers.TestHelpers
{
    /// <summary>
    /// Helper class for setting up devices with consistent test environment
    /// </summary>
    public static class DeviceTestHelper
    {
        /// <summary>
        /// Creates a Cisco device with basic network initialization
        /// </summary>
        public static CiscoDevice CreateCiscoDeviceWithNetwork(string name = "TestRouter")
        {
            var device = new CiscoDevice(name);
            InitializeBasicNetwork(device);
            return device;
        }

        /// <summary>
        /// Creates a Dell device with basic network initialization
        /// </summary>
        public static DellDevice CreateDellDeviceWithNetwork(string name = "TestSwitch")
        {
            var device = new DellDevice(name);
            InitializeBasicNetwork(device);
            return device;
        }

        /// <summary>
        /// Creates a Nokia device with basic network initialization
        /// </summary>
        public static NokiaDevice CreateNokiaDeviceWithNetwork(string name = "SR1")
        {
            var device = new NokiaDevice(name);
            InitializeBasicNetwork(device);
            return device;
        }

        /// <summary>
        /// Creates an Arista device with basic network initialization
        /// </summary>
        public static AristaDevice CreateAristaDeviceWithNetwork(string name = "TestSwitch")
        {
            var device = new AristaDevice(name);
            InitializeBasicNetwork(device);
            return device;
        }

        /// <summary>
        /// Initializes basic network setup for a device
        /// </summary>
        private static void InitializeBasicNetwork(NetworkDevice device)
        {
            try
            {
                // Create a minimal network if none exists
                if (device.ParentNetwork == null)
                {
                    var network = new Network();
                    
                    // Set the network reference
                    device.ParentNetwork = network;
                    // Add device to network using async method - simplified approach
                    _ = network.AddDeviceAsync(device);
                }
            }
            catch
            {
                // If network initialization fails, continue without it
                // Tests will handle the "Network not initialized" scenario
            }
        }

        /// <summary>
        /// Sets up a device in privileged mode for testing configuration commands
        /// </summary>
        public static async Task SetupPrivilegedMode(NetworkDevice device)
        {
            await device.ProcessCommandAsync("enable");
        }

        /// <summary>
        /// Sets up a device in configuration mode for testing config commands
        /// </summary>
        public static async Task SetupConfigurationMode(NetworkDevice device)
        {
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
        }

        /// <summary>
        /// Sets up a device in interface configuration mode
        /// </summary>
        public static async Task SetupInterfaceMode(NetworkDevice device, string interfaceName = "GigabitEthernet0/0")
        {
            SetupConfigurationMode(device);
            await device.ProcessCommandAsync($"interface {interfaceName}");
        }

        /// <summary>
        /// Normalizes command output by removing extra whitespace and newlines
        /// </summary>
        public static string NormalizeOutput(string output)
        {
            if (string.IsNullOrEmpty(output))
                return string.Empty;

            return output.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
        }

        /// <summary>
        /// Checks if output contains expected prompt for the given mode
        /// </summary>
        public static bool HasExpectedPrompt(string output, NetworkDevice device, string expectedMode)
        {
            var expectedPrompt = expectedMode.ToLower() switch
            {
                "user" => $"{device.Name}>",
                "privileged" => $"{device.Name}#",
                "config" => $"{device.Name}(config)#",
                "interface" => $"{device.Name}(config-if)#",
                _ => device.GetPrompt()
            };

            return output.Contains(expectedPrompt);
        }

        /// <summary>
        /// Extracts the device name from a complex hostname setup
        /// </summary>
        public static string GetBaseName(string deviceName)
        {
            // Handle cases like "TestSwitch(config)#" -> "TestSwitch"
            var index = deviceName.IndexOfAny(['(', '#', '>']);
            return index > 0 ? deviceName.Substring(0, index) : deviceName;
        }

        /// <summary>
        /// Verifies that a command completed successfully with expected mode
        /// </summary>
        public static async Task VerifyCommandSuccess(NetworkDevice device, string output, string expectedMode)
        {
            var actualMode = device.GetCurrentMode();
            var normalizedOutput = NormalizeOutput(output);
            
            if (actualMode != expectedMode)
            {
                throw new InvalidOperationException($"Expected mode '{expectedMode}' but got '{actualMode}'. Output: {normalizedOutput}");
            }

            if (!HasExpectedPrompt(normalizedOutput, device, expectedMode))
            {
                throw new InvalidOperationException($"Output '{normalizedOutput}' does not contain expected prompt for mode '{expectedMode}'");
            }
        }
    }
} 
