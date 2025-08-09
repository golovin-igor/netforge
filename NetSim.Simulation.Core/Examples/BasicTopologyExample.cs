using NetSim.Simulation.Common;
using NetSim.Simulation.Core;

namespace NetSim.Simulation.Examples
{
    class BasicTopologyExample
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== NetSim Network Device Simulator ===\n");
            
            // Create a network topology
            var network = new Network();
            
            // Create devices using the factory pattern
            var cisco1 = DeviceFactory.CreateDevice("cisco", "Router1");
            var juniper1 = DeviceFactory.CreateDevice("juniper", "Router2");
            var arista1 = DeviceFactory.CreateDevice("arista", "Switch1");
            var nokia1 = DeviceFactory.CreateDevice("nokia", "Router3");
            
            // Add devices to network
            await network.AddDeviceAsync(cisco1);
            await network.AddDeviceAsync(juniper1);
            await network.AddDeviceAsync(arista1);
            await network.AddDeviceAsync(nokia1);
            
            // Create a ring topology
            await network.AddLinkAsync("Router1", "GigabitEthernet0/0", "Router2", "ge-0/0/0");
            await network.AddLinkAsync("Router2", "ge-0/0/1", "Switch1", "Ethernet1");
            await network.AddLinkAsync("Switch1", "Ethernet2", "Router3", "1/1/1");
            await network.AddLinkAsync("Router3", "1/1/2", "Router1", "GigabitEthernet0/1");
            
            Console.WriteLine("Network topology created with 4 devices in a ring.\n");
            
            // Configure Cisco device
            Console.WriteLine("=== Configuring Cisco Router ===");
            await ExecuteCommandsAsync(cisco1, new[]
            {
                "enable",
                "configure terminal",
                "interface GigabitEthernet0/0",
                "ip address 10.0.0.1 255.255.255.0",
                "no shutdown",
                "exit",
                "interface GigabitEthernet0/1",
                "ip address 10.0.3.2 255.255.255.0",
                "no shutdown",
                "exit",
                "router ospf 1",
                "network 10.0.0.0 0.0.0.255 area 0",
                "network 10.0.3.0 0.0.0.255 area 0",
                "exit",
                "exit"
            });
            
            // Configure Juniper device
            Console.WriteLine("\n=== Configuring Juniper Router ===");
            await ExecuteCommandsAsync(juniper1, new[]
            {
                "configure",
                "set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/24",
                "set interfaces ge-0/0/1 unit 0 family inet address 10.0.1.1/24",
                "set protocols ospf area 0.0.0.0 interface ge-0/0/0",
                "set protocols ospf area 0.0.0.0 interface ge-0/0/1",
                "commit",
                "exit"
            });
            
            // Configure Arista switch
            Console.WriteLine("\n=== Configuring Arista Switch ===");
            await ExecuteCommandsAsync(arista1, new[]
            {
                "enable",
                "configure",
                "interface Ethernet1",
                "no switchport",
                "ip address 10.0.1.2/24",
                "no shutdown",
                "exit",
                "interface Ethernet2",
                "no switchport",
                "ip address 10.0.2.1/24",
                "no shutdown",
                "exit",
                "router ospf 1",
                "network 10.0.1.0/24 area 0",
                "network 10.0.2.0/24 area 0",
                "exit",
                "exit"
            });
            
            // Configure Nokia router
            Console.WriteLine("\n=== Configuring Nokia Router ===");
            await ExecuteCommandsAsync(nokia1, new[]
            {
                "configure",
                "port 1/1/1",
                "no shutdown",
                "exit",
                "port 1/1/2",
                "no shutdown",
                "exit",
                "router",
                "interface \"system\"",
                "address 10.0.2.2/24",
                "port 1/1/1",
                "exit",
                "interface \"to-router1\"",
                "address 10.0.3.1/24",
                "port 1/1/2",
                "exit",
                "ospf",
                "area 0 interface \"system\"",
                "area 0 interface \"to-router1\"",
                "exit",
                "exit",
                "exit"
            });
            
            // Update network protocols
            Console.WriteLine("\n=== Updating Network Protocols ===");
            network.UpdateProtocols();
            
            // Test connectivity
            Console.WriteLine("\n=== Testing Connectivity ===");
            Console.WriteLine("Ping from Cisco to Juniper:");
            Console.WriteLine(await cisco1.ProcessCommandAsync("ping 10.0.0.2"));
            
            Console.WriteLine("\nPing from Arista to Nokia:");
            Console.WriteLine(await arista1.ProcessCommandAsync("ping 10.0.2.2"));
            
            // Show routing tables
            Console.WriteLine("\n=== Routing Tables ===");
            Console.WriteLine("Cisco routing table:");
            Console.WriteLine(await cisco1.ProcessCommandAsync("show ip route"));
            
            Console.WriteLine("\nJuniper routing table:");
            Console.WriteLine(await juniper1.ProcessCommandAsync("show route"));
            
            // Demonstrate vendor diversity
            Console.WriteLine("\n=== Supported Vendors ===");
            foreach (var vendor in DeviceFactory.GetSupportedVendors())
            {
                Console.WriteLine($"- {vendor}");
            }
            
            Console.WriteLine("\n=== Simulation Complete ===");
        }
        
        static async Task ExecuteCommandsAsync(NetworkDevice device, string[] commands)
        {
            foreach (var cmd in commands)
            {
                Console.WriteLine($"{device.GetCurrentPrompt()}{cmd}");
                var output = await device.ProcessCommandAsync(cmd);
                if (!string.IsNullOrWhiteSpace(output) && !output.Equals(device.GetCurrentPrompt()))
                {
                    Console.Write(output);
                }
            }
        }
    }
} 
