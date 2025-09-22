using Microsoft.Extensions.DependencyInjection;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Examples demonstrating the declarative device and topology creation APIs
    /// Shows the complete workflow from device specification to topology creation
    /// </summary>
    public static class DeclarativeApiExamples
    {
        /// <summary>
        /// Example 1: Create a simple point-to-point network using the declarative API
        /// </summary>
        public static TopologySpec CreatePointToPointExample()
        {
            // Create first device using fluent builder API
            var router1 = DeviceBuilder.Create("Router1")
                .WithVendor("Cisco", "ISR4451", "15.7")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192,
                    cpu: new CpuSpec { Architecture = "ARM", FrequencyMHz = 1800, Cores = 4 })
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000, "WAN Interface")
                .AddInterface("GigabitEthernet0/1", InterfaceType.GigabitEthernet, 1000, "LAN Interface")
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(communities: ["public", "private"])
                .WithInitialConfig("ios", """
                    hostname Router1
                    interface GigabitEthernet0/0
                     description WAN Interface
                     ip address 192.168.1.1 255.255.255.252
                     no shutdown
                    interface GigabitEthernet0/1
                     description LAN Interface  
                     ip address 10.0.1.1 255.255.255.0
                     no shutdown
                    router ospf 1
                     network 192.168.1.0 0.0.0.3 area 0
                     network 10.0.1.0 0.0.0.255 area 0
                    """)
                .Build();

            // Create second device
            var router2 = DeviceBuilder.Create("Router2")
                .WithVendor("Cisco", "ISR4451", "15.7")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192,
                    cpu: new CpuSpec { Architecture = "ARM", FrequencyMHz = 1800, Cores = 4 })
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000, "WAN Interface")
                .AddInterface("GigabitEthernet0/1", InterfaceType.GigabitEthernet, 1000, "LAN Interface")
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(communities: ["public", "private"])
                .WithInitialConfig("ios", """
                    hostname Router2
                    interface GigabitEthernet0/0
                     description WAN Interface
                     ip address 192.168.1.2 255.255.255.252
                     no shutdown
                    interface GigabitEthernet0/1
                     description LAN Interface
                     ip address 10.0.2.1 255.255.255.0
                     no shutdown
                    router ospf 1
                     network 192.168.1.0 0.0.0.3 area 0
                     network 10.0.2.0 0.0.0.255 area 0
                    """)
                .Build();

            // Create topology with connection
            return TopologyBuilder.Create("Point-to-Point Network")
                .AddDevice(router1)
                .AddDevice(router2)
                .Connect("Router1", "GigabitEthernet0/0", "Router2", "GigabitEthernet0/0", "Cat6", 5.0)
                .WithSimulation(timeScale: 1.0, realTimeMode: false, logLevel: "Info")
                .Build();
        }

        /// <summary>
        /// Example 2: Create a three-tier network topology
        /// </summary>
        public static TopologySpec CreateThreeTierExample()
        {
            // Core layer device
            var coreSwitch = DeviceBuilder.Create("Core01")
                .WithVendor("Cisco", "Catalyst9600", "16.12")
                .WithPhysicalSpecs(memoryMB: 8192, storageMB: 16384)
                .AddInterface("TenGigabitEthernet1/0/1", InterfaceType.TenGigabitEthernet, 10000, "Uplink to Dist01")
                .AddInterface("TenGigabitEthernet1/0/2", InterfaceType.TenGigabitEthernet, 10000, "Uplink to Dist02")
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(communities: ["public"])
                .Build();

            // Distribution layer devices
            var dist01 = DeviceBuilder.Create("Dist01")
                .WithVendor("Cisco", "Catalyst9400", "16.12")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192)
                .AddInterface("TenGigabitEthernet1/0/1", InterfaceType.TenGigabitEthernet, 10000, "Uplink to Core")
                .AddInterface("GigabitEthernet1/0/1", InterfaceType.GigabitEthernet, 1000, "Access01 Connection")
                .AddInterface("GigabitEthernet1/0/2", InterfaceType.GigabitEthernet, 1000, "Access02 Connection")
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(communities: ["public"])
                .Build();

            var dist02 = DeviceBuilder.Create("Dist02")
                .WithVendor("Cisco", "Catalyst9400", "16.12")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192)
                .AddInterface("TenGigabitEthernet1/0/1", InterfaceType.TenGigabitEthernet, 10000, "Uplink to Core")
                .AddInterface("GigabitEthernet1/0/1", InterfaceType.GigabitEthernet, 1000, "Access03 Connection")
                .AddInterface("GigabitEthernet1/0/2", InterfaceType.GigabitEthernet, 1000, "Access04 Connection")
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(communities: ["public"])
                .Build();

            // Access layer devices
            var access01 = CreateCiscoSwitch("Access01", 24);
            var access02 = CreateCiscoSwitch("Access02", 24);
            var access03 = CreateCiscoSwitch("Access03", 48);
            var access04 = CreateCiscoSwitch("Access04", 48);

            // Build topology using builder pattern
            return TopologyBuilder.Create("Enterprise Network")
                .AddDevice(coreSwitch)
                .AddDevice(dist01)
                .AddDevice(dist02)
                .AddDevice(access01)
                .AddDevice(access02)
                .AddDevice(access03)
                .AddDevice(access04)
                .Connect("Core01", "TenGigabitEthernet1/0/1", "Dist01", "TenGigabitEthernet1/0/1")
                .Connect("Core01", "TenGigabitEthernet1/0/2", "Dist02", "TenGigabitEthernet1/0/1")
                .Connect("Dist01", "GigabitEthernet1/0/1", "Access01", "GigabitEthernet1/0/1")
                .Connect("Dist01", "GigabitEthernet1/0/2", "Access02", "GigabitEthernet1/0/1")
                .Connect("Dist02", "GigabitEthernet1/0/1", "Access03", "GigabitEthernet1/0/1")
                .Connect("Dist02", "GigabitEthernet1/0/2", "Access04", "GigabitEthernet1/0/1")
                .Build();
        }

        /// <summary>
        /// Example 3: Multi-vendor network with different protocols
        /// </summary>
        public static TopologySpec CreateMultiVendorExample()
        {
            // Cisco BGP router
            var ciscoBgp = DeviceBuilder.Create("Cisco-BGP")
                .WithVendor("Cisco", "ASR1001-X", "16.9")
                .WithPhysicalSpecs(memoryMB: 8192, storageMB: 16384)
                .AddInterface("GigabitEthernet0/0/0", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("GigabitEthernet0/0/1", InterfaceType.GigabitEthernet, 1000)
                .AddBgp(asNumber: 65001)
                .AddOspf(processId: 1)
                .AddSsh()
                .WithSnmp(version: SnmpVersion.V3, communities: ["snmp-ro"])
                .Build();

            // Juniper OSPF router
            var juniperOspf = DeviceBuilder.Create("Juniper-OSPF")
                .WithVendor("Juniper", "MX204", "20.4R3")
                .WithPhysicalSpecs(memoryMB: 8192, storageMB: 16384)
                .AddInterface("ge-0/0/0", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("ge-0/0/1", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("ge-0/0/2", InterfaceType.GigabitEthernet, 1000)
                .AddOspf(processId: 100)
                .AddSsh()
                .WithSnmp(version: SnmpVersion.V3, communities: ["public"])
                .Build();

            // Arista switch with EIGRP
            var aristaEigrp = DeviceBuilder.Create("Arista-EIGRP")
                .WithVendor("Arista", "DCS-7050SX3-48YC8", "4.27.3F")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192)
                .AddInterface("Ethernet1", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("Ethernet2", InterfaceType.GigabitEthernet, 1000)
                .AddInterface("Ethernet49", InterfaceType.TenGigabitEthernet, 10000)
                .AddEigrp(asNumber: 1)
                .AddSsh()
                .WithHttp(httpEnabled: true, httpsEnabled: true)
                .Build();

            return TopologyBuilder.Create("Multi-Vendor Network")
                .AddDevice(ciscoBgp)
                .AddDevice(juniperOspf)
                .AddDevice(aristaEigrp)
                .Connect("Cisco-BGP", "GigabitEthernet0/0/0", "Juniper-OSPF", "ge-0/0/0")
                .Connect("Juniper-OSPF", "ge-0/0/1", "Arista-EIGRP", "Ethernet1")
                .ConnectWithQuality("Cisco-BGP", "GigabitEthernet0/0/1", "Arista-EIGRP", "Ethernet2",
                    packetLossPercent: 0.1, latencyMs: 5.0, jitterMs: 1.0)
                .WithSimulation(timeScale: 2.0, realTimeMode: true, logLevel: "Debug")
                .Build();
        }

        /// <summary>
        /// Example 4: Ring topology with redundancy
        /// </summary>
        public static TopologySpec CreateRingTopologyExample()
        {
            // Create 5 routers for the ring
            var routers = new List<DeviceSpec>();
            for (int i = 1; i <= 5; i++)
            {
                var router = DeviceBuilder.Create($"Ring-Router{i:D2}")
                    .WithVendor("Cisco", "ISR4451", "15.7")
                    .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192)
                    .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000, "Ring Interface 1")
                    .AddInterface("GigabitEthernet0/1", InterfaceType.GigabitEthernet, 1000, "Ring Interface 2")
                    .AddInterface("GigabitEthernet0/2", InterfaceType.GigabitEthernet, 1000, "Ring Interface 3")
                    .AddOspf(processId: 1)
                    .AddSsh()
                    .WithSnmp(communities: ["public"])
                    .Build();
                routers.Add(router);
            }

            var builder = TopologyBuilder.Create("Ring Network");
            foreach (var router in routers)
            {
                builder.AddDevice(router);
            }

            // Connect devices in a ring
            for (int i = 0; i < routers.Count; i++)
            {
                var current = routers[i];
                var next = routers[(i + 1) % routers.Count];
                builder.Connect(current.Name, "GigabitEthernet0/1", next.Name, "GigabitEthernet0/0");
            }

            return builder.Build();
        }

        /// <summary>
        /// Example 5: Hub and spoke topology with QoS
        /// </summary>
        public static TopologySpec CreateHubSpokeQosExample()
        {
            // Hub device with advanced features
            var hub = DeviceBuilder.Create("HQ-Hub")
                .WithVendor("Cisco", "ISR4431", "16.9")
                .WithPhysicalSpecs(memoryMB: 4096, storageMB: 8192,
                    cpu: new CpuSpec { Architecture = "ARM", FrequencyMHz = 1600, Cores = 2 })
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000, "Spoke1")
                .AddInterface("GigabitEthernet0/1", InterfaceType.GigabitEthernet, 1000, "Spoke2")
                .AddInterface("GigabitEthernet0/2", InterfaceType.GigabitEthernet, 1000, "Spoke3")
                .AddInterface("GigabitEthernet0/3", InterfaceType.GigabitEthernet, 1000, "Internet")
                .AddOspf(processId: 1)
                .AddBgp(asNumber: 65000)
                .AddSsh()
                .AddTelnet(enabled: false) // Disable telnet for security
                .WithSnmp(version: SnmpVersion.V3, communities: ["monitor"])
                .WithHttp(httpEnabled: false, httpsEnabled: true) // HTTPS only
                .Build();

            // Spoke devices with limited resources
            var spoke1 = DeviceBuilder.Create("Branch-East")
                .WithVendor("Cisco", "ISR921", "15.5")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000)
                .AddOspf(processId: 1)
                .AddSsh()
                .Build();

            var spoke2 = DeviceBuilder.Create("Branch-West")
                .WithVendor("Cisco", "ISR921", "15.5")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000)
                .AddOspf(processId: 1)
                .AddSsh()
                .Build();

            var spoke3 = DeviceBuilder.Create("Branch-South")
                .WithVendor("Cisco", "ISR921", "15.5")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddInterface("GigabitEthernet0/0", InterfaceType.GigabitEthernet, 1000)
                .AddOspf(processId: 1)
                .AddSsh()
                .Build();

            return TopologyBuilder.Create("Hub-Spoke Network")
                .AddDevice(hub)
                .AddDevice(spoke1)
                .AddDevice(spoke2)
                .AddDevice(spoke3)
                .ConnectWithQuality("HQ-Hub", "GigabitEthernet0/0", "Branch-East", "GigabitEthernet0/0",
                    packetLossPercent: 0.05, latencyMs: 10.0) // Simulate WAN conditions
                .ConnectWithQuality("HQ-Hub", "GigabitEthernet0/1", "Branch-West", "GigabitEthernet0/0",
                    packetLossPercent: 0.1, latencyMs: 15.0)
                .ConnectWithQuality("HQ-Hub", "GigabitEthernet0/2", "Branch-South", "GigabitEthernet0/0",
                    packetLossPercent: 0.2, latencyMs: 25.0)
                .Build();
        }

        /// <summary>
        /// Example usage of the complete declarative API workflow
        /// </summary>
        public static async Task<INetworkTopology> CreateCompleteNetworkExample(IServiceProvider serviceProvider)
        {
            // Step 1: Create topology specification using declarative builders
            var topologySpec = CreatePointToPointExample();

            // Step 2: Create the actual network topology from the specification
            var factory = new DeclarativeTopologyFactory(serviceProvider);
            var topology = await factory.CreateTopologyAsync(topologySpec);

            // The topology is now ready for simulation
            return topology;
        }

        /// <summary>
        /// Helper method to create a Cisco switch with specified port count
        /// </summary>
        private static DeviceSpec CreateCiscoSwitch(string name, int portCount)
        {
            var builder = DeviceBuilder.Create(name)
                .WithVendor("Cisco", "Catalyst9300", "16.12")
                .WithPhysicalSpecs(memoryMB: 2048, storageMB: 4096)
                .AddSsh()
                .WithSnmp(communities: ["public"]);

            // Add uplink interfaces
            builder.AddInterface("GigabitEthernet1/0/1", InterfaceType.GigabitEthernet, 1000, "Uplink");

            // Add access ports
            for (int i = 1; i <= portCount; i++)
            {
                builder.AddInterface($"GigabitEthernet1/0/{i + 1}", InterfaceType.GigabitEthernet, 1000,
                    vlanConfig: new VlanInterfaceSpec { Mode = SwitchportMode.Access, AccessVlan = 1 });
            }

            return builder.Build();
        }
    }
}