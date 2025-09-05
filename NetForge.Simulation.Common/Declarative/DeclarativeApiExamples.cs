using NetForge.Simulation.DataTypes;
using NetForge.Simulation.DataTypes.NetworkPrimitives;

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
            var access01 = CommonDeviceSpecs.CiscoSwitch("Access01", portCount: 24).Build();
            var access02 = CommonDeviceSpecs.CiscoSwitch("Access02", portCount: 24).Build();
            var access03 = CommonDeviceSpecs.CiscoSwitch("Access03", portCount: 48).Build();
            var access04 = CommonDeviceSpecs.CiscoSwitch("Access04", portCount: 48).Build();

            // Build topology using common topology pattern
            return CommonTopologies.ThreeTier("Enterprise Network",
                coreDevice: coreSwitch,
                distributionDevices: [dist01, dist02],
                accessDevices: [[access01, access02], [access03, access04]]
            ).Build();
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
            var juniperOspf = CommonDeviceSpecs.JuniperRouter("Juniper-OSPF", "MX204")
                .AddOspf(processId: 100)
                .AddInterface("ge-0/0/2", InterfaceType.GigabitEthernet, 1000)
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
                var router = CommonDeviceSpecs.CiscoRouter($"Ring-Router{i:D2}")
                    .AddInterface("GigabitEthernet0/2", InterfaceType.GigabitEthernet, 1000, "Ring Interface 2")
                    .AddOspf(processId: 1)
                    .Build();
                routers.Add(router);
            }

            return CommonTopologies.Ring("Ring Network", routers.ToArray()).Build();
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
            var spoke1 = CommonDeviceSpecs.CiscoRouter("Branch-East", "ISR921")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddOspf(processId: 1)
                .Build();

            var spoke2 = CommonDeviceSpecs.CiscoRouter("Branch-West", "ISR921")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddOspf(processId: 1)
                .Build();

            var spoke3 = CommonDeviceSpecs.CiscoRouter("Branch-South", "ISR921")
                .WithPhysicalSpecs(memoryMB: 512, storageMB: 256)
                .AddOspf(processId: 1)
                .Build();

            return CommonTopologies.HubAndSpoke("Hub-Spoke Network", hub, spoke1, spoke2, spoke3)
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
    }
}