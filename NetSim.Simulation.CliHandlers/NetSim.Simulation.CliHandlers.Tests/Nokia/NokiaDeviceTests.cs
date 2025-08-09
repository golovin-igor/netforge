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
        public void Constructor_SetsVendorToNokia()
        {
            // Arrange & Act
            var device = new NokiaDevice("SR1");
            
            // Assert
            Assert.Equal("Nokia", device.Vendor);
            Assert.Equal("SR1", device.Name);
        }
        
        [Fact]
        public void InitialPrompt_ShowsAdminMode()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = device.ProcessCommand("");
            
            // Assert
            Assert.Equal("SR1>admin# ", output);
        }
        
        [Fact]
        public void Configure_EntersConfigMode()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Contains("SR1[configure]>admin# ", output);
        }
        
        [Fact]
        public void ConfigurePort_EntersPortContext()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("port 1/1/1");
            
            // Assert
            Assert.Contains("SR1[configure][Port 1/1/1]>admin# ", output);
        }
        
        [Fact]
        public void PortShutdown_DisablesInterface()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            device.ProcessCommand("configure");
            device.ProcessCommand("port 1/1/1");
            
            // Act
            var output = device.ProcessCommand("shutdown");
            
            // Assert
            var iface = device.GetAllInterfaces()["1/1/1"];
            Assert.True(iface.IsShutdown);
            Assert.False(iface.IsUp);
        }
        
        [Fact]
        public void PortNoShutdown_EnablesInterface()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            device.ProcessCommand("configure");
            device.ProcessCommand("port 1/1/1");
            device.ProcessCommand("shutdown");
            
            // Act
            var output = device.ProcessCommand("no shutdown");
            
            // Assert
            var iface = device.GetAllInterfaces()["1/1/1"];
            Assert.False(iface.IsShutdown);
            Assert.True(iface.IsUp);
        }
        
        [Fact]
        public void RouterInterface_ConfiguresIpAddress()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            device.ProcessCommand("configure");
            device.ProcessCommand("router interface test1");
            
            // Act
            var output = device.ProcessCommand("address 192.168.1.1/24");
            
            // Assert
            var iface = device.GetAllInterfaces()["router-test1"];
            Assert.Equal("192.168.1.1", iface.IpAddress);
            Assert.Equal("255.255.255.0", iface.SubnetMask);
        }
        
        [Fact]
        public void RouterStaticRoute_AddsRoute()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("router static-route 192.168.1.0/24 next-hop 10.0.0.254");
            
            // Assert
            var showOutput = device.ProcessCommand("show router route-table");
            Assert.Contains("192.168.1.0/24", showOutput);
            Assert.Contains("10.0.0.254", showOutput);
        }
        
        [Fact]
        public void ShowCommand_DisplaysSystemInfo()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("show system");

            // Assert
            Assert.Contains("System Name    : SR1", output);
            Assert.Contains("System Type    : 7750 SR-1", output);
            Assert.Contains("System Version : 20.10.R1", output);
            Assert.Contains("SR1>admin# ", output);
        }

        [Fact]
        public async Task PingCommand_ExecutesPing()
        {
            // Arrange
            var network = new Network();
            var sr1 = new NokiaDevice("SR1");
            var sr2 = new NokiaDevice("SR2");
            await network.AddDeviceAsync(sr1);
            await network.AddDeviceAsync(sr2);
            await network.AddLinkAsync("SR1", "1/1/1", "SR2", "1/1/1");

            // Configure interfaces
            sr1.ProcessCommand("configure");
            sr1.ProcessCommand("router interface test1");
            sr1.ProcessCommand("address 192.168.1.1/24");
            sr1.ProcessCommand("port 1/1/1");
            sr1.ProcessCommand("exit");
            sr1.ProcessCommand("exit");

            sr2.ProcessCommand("configure");
            sr2.ProcessCommand("router interface test2");
            sr2.ProcessCommand("address 192.168.1.2/24");
            sr2.ProcessCommand("port 1/1/1");
            sr2.ProcessCommand("exit");
            sr2.ProcessCommand("exit");

            // Act
            var output = sr1.ProcessCommand("ping 192.168.1.2");

            // Assert
            Assert.Contains("64 bytes from 192.168.1.2", output);
            Assert.Contains("icmp_seq=", output);
            Assert.Contains("ttl=64", output);
            Assert.Contains("time=", output);
        }

        [Fact]
        public void AdminSaveCommand_SavesConfig()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("admin save");

            // Assert
            Assert.Contains("Writing configuration to cf3:\\config.cfg", output);
            Assert.Contains("Saving configuration ... OK", output);
            Assert.Contains("Completed.", output);
        }

        [Fact]
        public void AdminRebootCommand_PromptsForConfirmation()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("admin reboot");

            // Assert
            Assert.Contains("Are you want to reboot (y/n)?", output);
        }

        [Fact]
        public void ClearPortStatistics_ClearsCounters()
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
            var output = device.ProcessCommand($"clear port {portName} statistics");

            // Assert
            Assert.Contains($"Cleared statistics for port {portName}", output);
            Assert.Equal(0, iface.RxPackets);
            Assert.Equal(0, iface.TxPackets);
            Assert.Equal(0, iface.RxBytes);
            Assert.Equal(0, iface.TxBytes);
        }

        [Fact]
        public void ClearPortStatistics_FailsForInvalidPort()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("clear port invalid-port statistics");

            // Assert
            Assert.Contains("MINOR: CLI Invalid parameter.", output);
        }

        [Fact]
        public void ShowVersion_DisplaysVersion()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("show version");

            // Assert
            Assert.Contains("TiMOS-C-20.10.R1", output);
            Assert.Contains("Copyright (c) Nokia 2000-2023", output);
            Assert.Contains("All rights reserved.", output);
        }

        [Fact]
        public void ShowPort_DisplaysPortStatus()
        {
            // Arrange
            var device = new NokiaDevice("SR1");

            // Act
            var output = device.ProcessCommand("show port");

            // Assert
            Assert.Contains("Port        Admin Link Port    Cfg  Oper LAG/ Port Port Port   C/QS/S", output);
            Assert.Contains("Id          State      State   MTU  MTU  Bndl Mode Encp Type   XFP", output);
            Assert.Contains("1/1/1", output);
            Assert.Contains("1/1/2", output);
            Assert.Contains("1/1/3", output);
            Assert.Contains("1/1/4", output);
        }

        [Fact]
        public void InvalidCommand_ReturnsError()
        {
            // Arrange
            var device = new NokiaDevice("SR1");
            
            // Act
            var output = device.ProcessCommand("invalid-command");
            
            // Assert
            Assert.Contains("Error: Invalid command", output);
        }

        [Fact]
        public async Task Nokia_ConfigureInterfaceAndPing_ShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("port 1/1/1");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router interface \"to_R2\"");
            r1.ProcessCommand("address 192.168.1.1/24");
            r1.ProcessCommand("port 1/1/1");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("port 1/1/1");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router interface \"to_R1\"");
            r2.ProcessCommand("address 192.168.1.2/24");
            r2.ProcessCommand("port 1/1/1");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            var pingOutput = r1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 received", pingOutput);
        }

        [Fact]
        public async Task Nokia_ConfigureOspf_ShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure router interface \"system\" address 1.1.1.1/32");
            r1.ProcessCommand("configure router interface \"to_R2\" address 10.0.0.1/30 port 1/1/1");
            r1.ProcessCommand("configure router ospf area 0.0.0.0 interface \"system\"");
            r1.ProcessCommand("configure router ospf area 0.0.0.0 interface \"to_R2\"");

            r2.ProcessCommand("configure router interface \"system\" address 2.2.2.2/32");
            r2.ProcessCommand("configure router interface \"to_R1\" address 10.0.0.2/30 port 1/1/1");
            r2.ProcessCommand("configure router ospf area 0.0.0.0 interface \"system\"");
            r2.ProcessCommand("configure router ospf area 0.0.0.0 interface \"to_R1\"");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = r1.ProcessCommand("show router ospf neighbor");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task Nokia_ConfigureBgp_ShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure router interface \"system\" address 1.1.1.1/32");
            r1.ProcessCommand("configure router interface \"to_R2\" address 10.0.0.1/30 port 1/1/1");
            r1.ProcessCommand("configure router bgp group \"ebgp\" autonomous-system 65001");
            r1.ProcessCommand("configure router bgp group \"ebgp\" neighbor 10.0.0.2 peer-as 65002");

            r2.ProcessCommand("configure router interface \"system\" address 2.2.2.2/32");
            r2.ProcessCommand("configure router interface \"to_R1\" address 10.0.0.2/30 port 1/1/1");
            r2.ProcessCommand("configure router bgp group \"ebgp\" autonomous-system 65002");
            r2.ProcessCommand("configure router bgp group \"ebgp\" neighbor 10.0.0.1 peer-as 65001");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = r1.ProcessCommand("show router bgp summary");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Established", bgpOutput);
        }

        [Fact]
        public async Task Nokia_ConfigureRip_ShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure router interface \"system\" address 1.1.1.1/32");
            r1.ProcessCommand("configure router interface \"to_R2\" address 10.0.0.1/24 port 1/1/1");
            r1.ProcessCommand("configure router rip group \"rip-group\" interface \"to_R2\"");

            r2.ProcessCommand("configure router interface \"system\" address 2.2.2.2/32");
            r2.ProcessCommand("configure router interface \"to_R1\" address 10.0.0.2/24 port 1/1/1");
            r2.ProcessCommand("configure router rip group \"rip-group\" interface \"to_R1\"");

            network.UpdateProtocols();
            await Task.Delay(100); 

            var routeOutput = r1.ProcessCommand("show router route-table");
            Assert.Contains("2.2.2.2/32", routeOutput);
            Assert.Contains("RIP", routeOutput);
        }
    }
} 
