using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Examples
{
    /// <summary>
    /// Demonstrates the PhysicalConnection entity and how protocols respect physical connectivity
    /// </summary>
    public class PhysicalConnectionExample
    {
        public static async Task RunExampleAsync()
        {
            Console.WriteLine("=== Physical Connection Entity Demonstration ===\n");

            // Create a network
            var network = new Network();

            // Create devices
            var router1 = new CiscoDevice("R1");
            var router2 = new CiscoDevice("R2");
            var router3 = new CiscoDevice("R3");

            // Configure IP addresses on interfaces
            ConfigureDevice(router1, "192.168.1.1", "255.255.255.0", "GigabitEthernet0/0");
            ConfigureDevice(router2, "192.168.1.2", "255.255.255.0", "GigabitEthernet0/0");
            ConfigureDevice(router3, "192.168.2.1", "255.255.255.0", "GigabitEthernet0/0");

            // Add devices to network
            await network.AddDeviceAsync(router1);
            await network.AddDeviceAsync(router2);
            await network.AddDeviceAsync(router3);

            // Configure OSPF on devices
            ConfigureOspf(router1, "GigabitEthernet0/0", 0);
            ConfigureOspf(router2, "GigabitEthernet0/0", 0);
            ConfigureOspf(router3, "GigabitEthernet0/0", 0);

            // Protocols are auto-registered based on device vendor compatibility

            Console.WriteLine("1. Creating Physical Connections\n");

            // Create physical connections between devices
            await network.AddPhysicalConnectionAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0", 
                PhysicalConnectionType.Ethernet);
            
            await network.AddPhysicalConnectionAsync("R2", "GigabitEthernet0/1", "R3", "GigabitEthernet0/0", 
                PhysicalConnectionType.Fiber);

            // Display network statistics
            var stats = network.GetNetworkStatistics();
            Console.WriteLine($"Network Statistics:");
            Console.WriteLine($"- Total Devices: {stats.TotalDevices}");
            Console.WriteLine($"- Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"- Operational Connections: {stats.OperationalConnections}");
            Console.WriteLine($"- Connection Reliability: {stats.ConnectionReliability:F1}%\n");

            Console.WriteLine("2. Testing Physical Connection Quality\n");

            // Test connection quality
            var connection1 = network.GetPhysicalConnection("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            if (connection1 != null)
            {
                Console.WriteLine($"Connection R1-R2: {connection1}");
                Console.WriteLine($"- Bandwidth: {connection1.Bandwidth} Mbps");
                Console.WriteLine($"- Latency: {connection1.Latency} ms");
                Console.WriteLine($"- Packet Loss: {connection1.PacketLoss}%");
                Console.WriteLine($"- MTU: {connection1.MaxTransmissionUnit} bytes\n");

                // Test packet transmission
                var transmissionResult = connection1.SimulateTransmission(1500);
                Console.WriteLine($"Packet transmission test: {(transmissionResult.Success ? "SUCCESS" : "FAILED")}");
                if (transmissionResult.Success)
                {
                    Console.WriteLine($"- Transmission time: {transmissionResult.TransmissionTime * 1000:F3} ms");
                    Console.WriteLine($"- Actual latency: {transmissionResult.ActualLatency} ms\n");
                }
                else
                {
                    Console.WriteLine($"- Reason: {transmissionResult.Reason}\n");
                }
            }

            Console.WriteLine("3. Demonstrating Protocol Behavior with Physical Connectivity\n");

            // Show how protocols respect physical connectivity
            Console.WriteLine("OSPF Protocol State Updates:");
            await router1.UpdateAllProtocolStates();
            await router2.UpdateAllProtocolStates();

            Console.WriteLine("\n4. Simulating Cable Failure\n");

            // Simulate cable failure
            await network.SimulateCableFailureAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0", 
                "Fiber optic cable damaged");

            // Check connection state after failure
            Console.WriteLine($"Connection state after failure: {connection1.State}");
            
            // Protocols should automatically update their state
            Console.WriteLine("Protocol updates after cable failure:");
            await router1.UpdateAllProtocolStates();
            await router2.UpdateAllProtocolStates();

            Console.WriteLine("\n5. Simulating Connection Degradation\n");

            // First restore the connection
            await network.RestoreConnectionAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            Console.WriteLine($"Connection restored: {connection1.State}");

            // Then simulate degradation
            await network.SimulateConnectionDegradationAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0", 
                5.0, 20, "Poor weather conditions affecting wireless link");

            Console.WriteLine($"Connection after degradation: {connection1.State}");
            Console.WriteLine($"- New packet loss: {connection1.PacketLoss}%");
            Console.WriteLine($"- New latency: {connection1.Latency} ms");

            // Test transmission with degraded connection
            var degradedResult = connection1.SimulateTransmission(1500);
            Console.WriteLine($"Degraded transmission test: {(degradedResult.Success ? "SUCCESS" : "FAILED")}");

            Console.WriteLine("\n6. Protocol Decision Making Based on Physical Connection Quality\n");

            // Show how protocols use physical connection metrics
            var metrics = router1.GetPhysicalConnectionMetrics("GigabitEthernet0/0");
            if (metrics != null)
            {
                Console.WriteLine($"Physical Connection Metrics for R1 GigabitEthernet0/0:");
                Console.WriteLine($"- Quality Score: {metrics.QualityScore:F1}%");
                Console.WriteLine($"- Suitable for Real-time: {metrics.IsSuitableForRealTime}");
                Console.WriteLine($"- Suitable for Routing: {metrics.IsSuitableForRouting}");
            }

            // Show interface participation decision
            bool shouldParticipate = router1.ShouldInterfaceParticipateInProtocols("GigabitEthernet0/0");
            Console.WriteLine($"Should interface participate in protocols: {shouldParticipate}");

            Console.WriteLine("\n7. Testing Ping with Physical Layer Effects\n");

            // Configure router2 interface for ping test
            var r2Interface = router2.GetInterface("GigabitEthernet0/0");
            if (r2Interface != null)
            {
                r2Interface.IpAddress = "192.168.1.2";
                r2Interface.SubnetMask = "255.255.255.0";
            }

            // Test ping that considers physical connectivity
            string pingResult = router1.ExecutePing("192.168.1.2");
            Console.WriteLine("Ping result with physical layer simulation:");
            Console.WriteLine(pingResult);

            Console.WriteLine("\n=== Physical Connection Demonstration Complete ===");
        }

        private static void ConfigureDevice(NetworkDevice device, string ipAddress, string subnetMask, string interfaceName)
        {
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig != null)
            {
                interfaceConfig.IpAddress = ipAddress;
                interfaceConfig.SubnetMask = subnetMask;
                interfaceConfig.IsShutdown = false;
            }
        }

        private static void ConfigureOspf(NetworkDevice device, string interfaceName, int area)
        {
            var ospfConfig = new OspfConfig(1)
            {
                RouterId = "1.1.1.1", // In reality, this would be unique per router
                IsEnabled = true
            };

            var ospfInterface = new OspfInterface(interfaceName, area)
            {
                Cost = 10,
                Priority = 1,
                NetworkType = "broadcast"
            };

            ospfConfig.Interfaces[interfaceName] = ospfInterface;
            device.SetOspfConfiguration(ospfConfig);

            // Also configure the interface for OSPF in the interface config
            var interfaceConfig = device.GetInterface(interfaceName);
            if (interfaceConfig != null)
            {
                interfaceConfig.OspfEnabled = true;
                interfaceConfig.OspfProcessId = 1;
            }
        }
    }
} 
