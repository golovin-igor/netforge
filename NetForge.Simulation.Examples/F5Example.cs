using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core;
using NetForge.Simulation.Core.Devices;

namespace NetForge.Simulation.Core.Examples
{
    /// <summary>
    /// Example demonstrating F5 BIG-IP load balancer functionality
    /// </summary>
    public static class F5Example
    {
        public static async Task RunExample()
        {
            Console.WriteLine("=== F5 BIG-IP Load Balancer Example ===\n");

            // Create a network
            var network = new Network();

            // Create F5 BIG-IP device
            var f5Device = new F5Device("F5-BIGIP-1");
            await network.AddDeviceAsync(f5Device);

            Console.WriteLine($"Created F5 device: {f5Device.GetHostname()}");
            Console.WriteLine($"Vendor: {f5Device.Vendor}");
            Console.WriteLine($"Current mode: {f5Device.GetCurrentMode()}");

            // Demonstrate F5 CLI commands
            Console.WriteLine("\n=== F5 CLI Commands ===");

            // Basic commands
            Console.WriteLine("Testing basic commands:");
            Console.WriteLine(await f5Device.ProcessCommandAsync("help"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("show version"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("show running-config"));

            // TMSH commands
            Console.WriteLine("\nTesting TMSH commands:");
            Console.WriteLine(await f5Device.ProcessCommandAsync("tmsh"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("list ltm pool"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("list ltm virtual"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("list ltm node"));

            // LTM configuration
            Console.WriteLine("\nTesting LTM configuration:");
            Console.WriteLine(await f5Device.ProcessCommandAsync("create ltm pool web_pool"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("create ltm node web_server1 192.168.1.10"));
            Console.WriteLine(await f5Device.ProcessCommandAsync("create ltm virtual web_vs 192.168.1.100:80"));

            // Show interfaces
            Console.WriteLine("\n=== F5 Interfaces ===");
            foreach (var iface in f5Device.GetAllInterfaces())
            {
                Console.WriteLine($"Interface: {iface.Value.Name}, IP: {iface.Value.IpAddress}, Status: {iface.Value.GetStatus()}");
            }

            // Test connectivity
            Console.WriteLine("\n=== Testing Connectivity ===");
            Console.WriteLine(await f5Device.ProcessCommandAsync("ping 192.168.1.1"));

            // Show device capabilities
            Console.WriteLine("\n=== Device Capabilities ===");
            Console.WriteLine($"Vendor: {f5Device.Vendor}");
            Console.WriteLine($"Device Type: Load Balancer");
            Console.WriteLine($"Supports TMSH: Yes");
            Console.WriteLine($"Supports LTM: Yes");
            Console.WriteLine($"Supports GTM: Yes");

            Console.WriteLine("\n=== F5 Example Complete ===");
        }
    }
}
