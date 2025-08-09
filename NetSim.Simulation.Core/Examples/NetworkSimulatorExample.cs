using NetSim.Simulation.Common;
using NetSim.Simulation.Core;

namespace NetSim.Simulation.Examples
{
    /// <summary>
    /// Example program demonstrating the network device simulator
    /// </summary>
    public class NetworkSimulatorExample
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Network Device Simulator Example ===\n");
            
            // Create a network
            var network = new Network();
            
            // Create devices from different vendors using the DeviceFactory
            var ciscoRouter = DeviceFactory.CreateDevice("Cisco", "CiscoR1");
            var juniperRouter = DeviceFactory.CreateDevice("Juniper", "JuniperR1");
            var aristaSwitch = DeviceFactory.CreateDevice("Arista", "AristaSW1");
            var nokiaRouter = DeviceFactory.CreateDevice("Nokia", "NokiaSR1");
            
            // Add devices to the network
            await network.AddDeviceAsync(ciscoRouter);
            await network.AddDeviceAsync(juniperRouter);
            await network.AddDeviceAsync(aristaSwitch);
            await network.AddDeviceAsync(nokiaRouter);
            
            // Connect devices in a ring topology
            await network.AddLinkAsync("CiscoR1", "GigabitEthernet0/0", "JuniperR1", "ge-0/0/0");
            await network.AddLinkAsync("JuniperR1", "ge-0/0/1", "AristaSW1", "Ethernet1");
            await network.AddLinkAsync("AristaSW1", "Ethernet2", "NokiaSR1", "1/1/1");
            await network.AddLinkAsync("NokiaSR1", "1/1/2", "CiscoR1", "GigabitEthernet0/1");
            
            Console.WriteLine("Network topology created with 4 devices in a ring.\n");
            
            // Configure Cisco Router
            Console.WriteLine("=== Configuring Cisco Router ===");
            ConfigureCiscoRouter(ciscoRouter);
            
            // Configure Juniper Router
            Console.WriteLine("\n=== Configuring Juniper Router ===");
            ConfigureJuniperRouter(juniperRouter);
            
            // Configure Arista Switch
            Console.WriteLine("\n=== Configuring Arista Switch ===");
            ConfigureAristaSwitch(aristaSwitch);
            
            // Configure Nokia Router
            Console.WriteLine("\n=== Configuring Nokia Router ===");
            ConfigureNokiaRouter(nokiaRouter);
            
            // Update network protocols
            network.UpdateProtocols();
            
            // Show routing tables
            Console.WriteLine("\n=== Routing Tables ===");
            ShowRoutingTables(ciscoRouter, juniperRouter, aristaSwitch, nokiaRouter);
            
            // Test connectivity
            Console.WriteLine("\n=== Connectivity Tests ===");
            TestConnectivity(ciscoRouter, juniperRouter, aristaSwitch, nokiaRouter);
        }
        
        private static void ConfigureCiscoRouter(NetworkDevice device)
        {
            // Enter privileged mode
            ExecuteCommand(device, "enable");
            ExecuteCommand(device, "configure terminal");
            
            // Configure interfaces
            ExecuteCommand(device, "interface GigabitEthernet0/0");
            ExecuteCommand(device, "ip address 10.0.0.1 255.255.255.252");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            ExecuteCommand(device, "interface GigabitEthernet0/1");
            ExecuteCommand(device, "ip address 10.0.3.2 255.255.255.252");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            // Configure OSPF
            ExecuteCommand(device, "router ospf 1");
            ExecuteCommand(device, "network 10.0.0.0 0.0.0.3 area 0");
            ExecuteCommand(device, "network 10.0.3.0 0.0.0.3 area 0");
            ExecuteCommand(device, "exit");
            
            // Configure BGP
            ExecuteCommand(device, "router bgp 65001");
            ExecuteCommand(device, "neighbor 10.0.0.2 remote-as 65002");
            ExecuteCommand(device, "network 192.168.1.0 mask 255.255.255.0");
            ExecuteCommand(device, "exit");
            
            // Add static route
            ExecuteCommand(device, "ip route 192.168.1.0 255.255.255.0 Null0");
            ExecuteCommand(device, "exit");
        }
        
        private static void ConfigureJuniperRouter(NetworkDevice device)
        {
            // Enter configuration mode
            ExecuteCommand(device, "configure");
            
            // Configure interfaces
            ExecuteCommand(device, "set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            ExecuteCommand(device, "set interfaces ge-0/0/1 unit 0 family inet address 10.0.1.1/30");
            
            // Configure OSPF
            ExecuteCommand(device, "set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            ExecuteCommand(device, "set protocols ospf area 0.0.0.0 interface ge-0/0/1.0");
            
            // Configure BGP
            ExecuteCommand(device, "set routing-options autonomous-system 65002");
            ExecuteCommand(device, "set protocols bgp group EBGP type external");
            ExecuteCommand(device, "set protocols bgp group EBGP peer-as 65001");
            ExecuteCommand(device, "set protocols bgp group EBGP neighbor 10.0.0.1");
            
            // Add static route
            ExecuteCommand(device, "set routing-options static route 192.168.2.0/24 discard");
            
            // Commit configuration
            ExecuteCommand(device, "commit");
            ExecuteCommand(device, "exit");
        }
        
        private static void ConfigureAristaSwitch(NetworkDevice device)
        {
            // Enter privileged mode
            ExecuteCommand(device, "enable");
            ExecuteCommand(device, "configure terminal");
            
            // Configure interfaces
            ExecuteCommand(device, "interface Ethernet1");
            ExecuteCommand(device, "no switchport");
            ExecuteCommand(device, "ip address 10.0.1.2/30");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            ExecuteCommand(device, "interface Ethernet2");
            ExecuteCommand(device, "no switchport");
            ExecuteCommand(device, "ip address 10.0.2.1/30");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            // Configure OSPF
            ExecuteCommand(device, "router ospf 1");
            ExecuteCommand(device, "network 10.0.1.0/30 area 0");
            ExecuteCommand(device, "network 10.0.2.0/30 area 0");
            ExecuteCommand(device, "exit");
            
            // Configure VLANs
            ExecuteCommand(device, "vlan 100");
            ExecuteCommand(device, "name Management");
            ExecuteCommand(device, "exit");
            
            ExecuteCommand(device, "vlan 200");
            ExecuteCommand(device, "name Production");
            ExecuteCommand(device, "exit");
            
            // Add static route
            ExecuteCommand(device, "ip route 192.168.3.0/24 Null0");
            ExecuteCommand(device, "exit");
        }
        
        private static void ConfigureNokiaRouter(NetworkDevice device)
        {
            // Enter configuration mode
            ExecuteCommand(device, "configure");
            
            // Configure system
            ExecuteCommand(device, "system");
            ExecuteCommand(device, "name NokiaSR1-Core");
            ExecuteCommand(device, "exit");
            
            // Configure ports
            ExecuteCommand(device, "port 1/1/1");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            ExecuteCommand(device, "port 1/1/2");
            ExecuteCommand(device, "no shutdown");
            ExecuteCommand(device, "exit");
            
            // Configure router interfaces
            ExecuteCommand(device, "router");
            ExecuteCommand(device, "interface \"to-arista\" address 10.0.2.2/30");
            ExecuteCommand(device, "interface \"to-cisco\" address 10.0.3.1/30");
            
            // Configure OSPF
            ExecuteCommand(device, "ospf 1");
            ExecuteCommand(device, "area 0.0.0.0 interface \"to-arista\"");
            ExecuteCommand(device, "area 0.0.0.0 interface \"to-cisco\"");
            ExecuteCommand(device, "exit");
            
            // Add static route
            ExecuteCommand(device, "static-route 192.168.4.0/24 next-hop 10.0.3.2");
            ExecuteCommand(device, "exit");
            
            ExecuteCommand(device, "exit");
        }
        
        private static void ExecuteCommand(NetworkDevice device, string command)
        {
            var output = device.ProcessCommand(command);
            Console.WriteLine($"{device.Name}> {command}");
            if (output.Contains("Error"))
            {
                Console.WriteLine($"  ERROR: {output.Trim()}");
            }
        }
        
        private static void ShowRoutingTables(params NetworkDevice[] devices)
        {
            foreach (var device in devices)
            {
                Console.WriteLine($"\n--- {device.Name} ({device.Vendor}) Routing Table ---");
                
                string showCommand = "";
                switch (device.Vendor)
                {
                    case "Cisco":
                    case "Arista":
                        showCommand = "show ip route";
                        break;
                    case "Juniper":
                        showCommand = "show route";
                        break;
                    case "Nokia":
                        showCommand = "show router route-table";
                        break;
                }
                
                var output = device.ProcessCommand(showCommand);
                Console.WriteLine(output);
            }
        }
        
        private static void TestConnectivity(params NetworkDevice[] devices)
        {
            // Test ping between devices
            Console.WriteLine("\n--- Ping Tests ---");
            
            // Cisco to Juniper
            Console.WriteLine("\nCisco to Juniper (10.0.0.2):");
            var pingOutput = devices[0].ProcessCommand("ping 10.0.0.2");
            Console.WriteLine(pingOutput);
            
            // Juniper to Arista
            Console.WriteLine("\nJuniper to Arista (10.0.1.2):");
            pingOutput = devices[1].ProcessCommand("ping 10.0.1.2");
            Console.WriteLine(pingOutput);
            
            // Arista to Nokia
            Console.WriteLine("\nArista to Nokia (10.0.2.2):");
            pingOutput = devices[2].ProcessCommand("ping 10.0.2.2");
            Console.WriteLine(pingOutput);
            
            // Nokia to Cisco
            Console.WriteLine("\nNokia to Cisco (10.0.3.2):");
            pingOutput = devices[3].ProcessCommand("ping 10.0.3.2");
            Console.WriteLine(pingOutput);
        }
    }
} 
