using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Nokia
{
    public class NokiaDeviceTests
    {
        private async Task<(Network, NokiaDevice, NokiaDevice)> SetupNetworkWithTwoDevicesAsync(string r1Name = "R1", string r2Name = "R2", string iface1 = "1/1/1", string iface2 = "1/1/1")
        {
            var network = new Network();
            var r1 = new NokiaDevice(r1Name);
            var r2 = new NokiaDevice(r2Name);
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync(r1Name, iface1, r2Name, iface2);
            return (network, r1, r2);
        }

        [Fact]
        public async Task ConstructorSetsVendorToNokia()
        {
            // Arrange & Act
            var device = new NokiaDevice("SR1");
            
            // Assert
            Assert.Equal("Nokia", device.Vendor);
            Assert.Equal("SR1", device.Name);
        }
        
        [Fact]
        public async Task InitialPromptShowsAdminMode()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = await device.ProcessCommandAsync("");
            
            // Assert
            Assert.Equal("SR1>admin# ", output);
        }
        
        [Fact]
        public async Task ConfigureEntersConfigMode()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = await device.ProcessCommandAsync("configure");
            
            // Assert
            Assert.Contains("SR1[configure]>admin# ", output);
        }
        
        [Fact]
        public async Task ConfigurePortEntersPortContext()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output = await device.ProcessCommandAsync("port 1/1/1");
            
            // Assert
            Assert.Contains("SR1[configure][Port 1/1/1]>admin# ", output);
        }
        
        [Fact]
        public async Task PortShutdownDisablesInterface()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("port 1/1/1");
            
            // Act
            var output = await device.ProcessCommandAsync("shutdown");
            
            // Assert
            var iface = device.GetAllInterfaces()["1/1/1"];
            Assert.True(iface.IsShutdown);
            Assert.False(iface.IsUp);
        }
        
        [Fact]
        public async Task PortNoShutdownEnablesInterface()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("port 1/1/1");
            await device.ProcessCommandAsync("shutdown");
            
            // Act
            var output = await device.ProcessCommandAsync("no shutdown");
            
            // Assert
            var iface = device.GetAllInterfaces()["1/1/1"];
            Assert.False(iface.IsShutdown);
            Assert.True(iface.IsUp);
        }
        
        [Fact]
        public async Task RouterInterfaceConfiguresIpAddress()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router interface test1");
            
            // Act
            var output = await device.ProcessCommandAsync("address 192.168.1.1/24");
            
            // Assert
            var iface = device.GetAllInterfaces()["router-test1"];
            Assert.Equal("192.168.1.1", iface.IpAddress);
            Assert.Equal("255.255.255.0", iface.SubnetMask);
        }
        
        [Fact]
        public async Task RouterStaticRouteAddsRoute()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output = await device.ProcessCommandAsync("router static-route 192.168.1.0/24 next-hop 10.0.0.254");
            
            // Assert
            var showOutput = await device.ProcessCommandAsync("show router route-table");
            Assert.Contains("192.168.1.0/24", showOutput);
            Assert.Contains("10.0.0.254", showOutput);
        }
        
        [Fact]
        public async Task ShowCommandDisplaysSystemInfo()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("show system");

            // Assert
            Assert.Contains("System Name    : SR1", output);
            Assert.Contains("System Type    : 7750 SR-1", output);
            Assert.Contains("System Version : 20.10.R1", output);
            Assert.Contains("SR1>admin# ", output);
        }

        [Fact]
        public async Task PingCommandExecutesPing()
        {
            // Arrange
            var network = new Network();
            var sr1 = new NokiaDevice("SR1");
            var sr2 = new NokiaDevice("SR2");
            await network.AddDeviceAsync(sr1);
            await network.AddDeviceAsync(sr2);
            await network.AddLinkAsync("SR1", "1/1/1", "SR2", "1/1/1");

            // Configure interfaces
            await sr1.ProcessCommandAsync("configure");
            await sr1.ProcessCommandAsync("router interface test1");
            await sr1.ProcessCommandAsync("address 192.168.1.1/24");
            await sr1.ProcessCommandAsync("port 1/1/1");
            await sr1.ProcessCommandAsync("exit");
            await sr1.ProcessCommandAsync("exit");

            await sr2.ProcessCommandAsync("configure");
            await sr2.ProcessCommandAsync("router interface test2");
            await sr2.ProcessCommandAsync("address 192.168.1.2/24");
            await sr2.ProcessCommandAsync("port 1/1/1");
            await sr2.ProcessCommandAsync("exit");
            await sr2.ProcessCommandAsync("exit");

            // Act
            var output = await sr1.ProcessCommandAsync("ping 192.168.1.2");

            // Assert
            Assert.Contains("64 bytes from 192.168.1.2", output);
            Assert.Contains("icmp_seq=", output);
            Assert.Contains("ttl=64", output);
            Assert.Contains("time=", output);
        }

        [Fact]
        public async Task AdminSaveCommandSavesConfig()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("admin save");

            // Assert
            Assert.Contains("Writing configuration to cf3:\\config.cfg", output);
            Assert.Contains("Saving configuration ... OK", output);
            Assert.Contains("Completed.", output);
        }

        [Fact]
        public async Task AdminRebootCommandPromptsForConfirmation()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("admin reboot");

            // Assert
            Assert.Contains("Are you want to reboot (y/n)?", output);
        }

        [Fact]
        public async Task ClearPortStatisticsClearsCounters()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            var interfaces = device.GetAllInterfaces();
            var portName = "1/1/1";
            var iface = interfaces[portName];
            iface.RxPackets = 100;
            iface.TxPackets = 100;
            iface.RxBytes = 1000;
            iface.TxBytes = 1000;

            // Act
            var output = await device.ProcessCommandAsync($"clear port {portName} statistics");

            // Assert
            Assert.Contains($"Cleared statistics for port {portName}", output);
            Assert.Equal(0, iface.RxPackets);
            Assert.Equal(0, iface.TxPackets);
            Assert.Equal(0, iface.RxBytes);
            Assert.Equal(0, iface.TxBytes);
        }

        [Fact]
        public async Task ClearPortStatisticsFailsForInvalidPort()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("clear port invalid-port statistics");

            // Assert
            Assert.Contains("MINOR: CLI Invalid parameter.", output);
        }

        [Fact]
        public async Task ShowVersionDisplaysVersion()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("show version");

            // Assert
            Assert.Contains("TiMOS-C-20.10.R1", output);
            Assert.Contains("Copyright (c) Nokia 2000-2023", output);
            Assert.Contains("All rights reserved.", output);
        }

        [Fact]
        public async Task ShowPortDisplaysPortStatus()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = await device.ProcessCommandAsync("show port");

            // Assert
            Assert.Contains("Port        Admin Link Port    Cfg  Oper LAG/ Port Port Port   C/QS/S", output);
            Assert.Contains("Id          State      State   MTU  MTU  Bndl Mode Encp Type   XFP", output);
            Assert.Contains("1/1/1", output);
            Assert.Contains("1/1/2", output);
            Assert.Contains("1/1/3", output);
            Assert.Contains("1/1/4", output);
        }

        [Fact]
        public async Task InvalidCommandReturnsError()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid-command");
            
            // Assert
            Assert.Contains("Error: Invalid command", output);
        }

        [Fact]
        public async Task NokiaConfigureInterfaceAndPingShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("port 1/1/1");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router interface \"to_R2\"");
            await r1.ProcessCommandAsync("address 192.168.1.1/24");
            await r1.ProcessCommandAsync("port 1/1/1");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("port 1/1/1");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router interface \"to_R1\"");
            await r2.ProcessCommandAsync("address 192.168.1.2/24");
            await r2.ProcessCommandAsync("port 1/1/1");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            var pingOutput = await r1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 received", pingOutput);
        }

        [Fact]
        public async Task NokiaConfigureOspfShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure router interface \"system\" address 1.1.1.1/32");
            await r1.ProcessCommandAsync("configure router interface \"to_R2\" address 10.0.0.1/30 port 1/1/1");
            await r1.ProcessCommandAsync("configure router ospf area 0.0.0.0 interface \"system\"");
            await r1.ProcessCommandAsync("configure router ospf area 0.0.0.0 interface \"to_R2\"");

            await r2.ProcessCommandAsync("configure router interface \"system\" address 2.2.2.2/32");
            await r2.ProcessCommandAsync("configure router interface \"to_R1\" address 10.0.0.2/30 port 1/1/1");
            await r2.ProcessCommandAsync("configure router ospf area 0.0.0.0 interface \"system\"");
            await r2.ProcessCommandAsync("configure router ospf area 0.0.0.0 interface \"to_R1\"");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = await r1.ProcessCommandAsync("show router ospf neighbor");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task NokiaConfigureBgpShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure router interface \"system\" address 1.1.1.1/32");
            await r1.ProcessCommandAsync("configure router interface \"to_R2\" address 10.0.0.1/30 port 1/1/1");
            await r1.ProcessCommandAsync("configure router bgp group \"ebgp\" autonomous-system 65001");
            await r1.ProcessCommandAsync("configure router bgp group \"ebgp\" neighbor 10.0.0.2 peer-as 65002");

            await r2.ProcessCommandAsync("configure router interface \"system\" address 2.2.2.2/32");
            await r2.ProcessCommandAsync("configure router interface \"to_R1\" address 10.0.0.2/30 port 1/1/1");
            await r2.ProcessCommandAsync("configure router bgp group \"ebgp\" autonomous-system 65002");
            await r2.ProcessCommandAsync("configure router bgp group \"ebgp\" neighbor 10.0.0.1 peer-as 65001");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = await r1.ProcessCommandAsync("show router bgp summary");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Established", bgpOutput);
        }

        [Fact]
        public async Task NokiaConfigureRipShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure router interface \"system\" address 1.1.1.1/32");
            await r1.ProcessCommandAsync("configure router interface \"to_R2\" address 10.0.0.1/24 port 1/1/1");
            await r1.ProcessCommandAsync("configure router rip group \"rip-group\" interface \"to_R2\"");

            await r2.ProcessCommandAsync("configure router interface \"system\" address 2.2.2.2/32");
            await r2.ProcessCommandAsync("configure router interface \"to_R1\" address 10.0.0.2/24 port 1/1/1");
            await r2.ProcessCommandAsync("configure router rip group \"rip-group\" interface \"to_R1\"");

            network.UpdateProtocols();
            await Task.Delay(100); 

            var routeOutput = await r1.ProcessCommandAsync("show router route-table");
            Assert.Contains("2.2.2.2/32", routeOutput);
            Assert.Contains("RIP", routeOutput);
        }
    }
} 
