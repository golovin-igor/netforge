using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaCommandHandlerComprehensiveTests
    {
        [Fact]
        public void ArubaHandler_ShowRunningConfig_ShouldDisplayConfig()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowVersion_ShouldDisplayVersion()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("ArubaOS", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowInterfaces_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show interfaces");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowIpRoute_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show ip route");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show arp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_InterfaceGigabitEthernet_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestSwitch(eth-0/0)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_IpAddress_ShouldConfigureInterface()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act
            var output = device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            
            // Assert
            Assert.Equal("TestSwitch(eth-0/0)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_Enable_ShouldEnableInterface()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act
            var output = device.ProcessCommand("enable");
            
            // Assert
            Assert.Equal("TestSwitch(eth-0/0)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_IpRoute_ShouldConfigureRoute()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("ip route 10.0.0.0 255.0.0.0 192.168.1.1");
            
            // Assert
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("TestSwitch(vlan-100)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_VlanName_ShouldSetVlanName()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("vlan 100");
            
            // Act
            var output = device.ProcessCommand("name TestVLAN");
            
            // Assert
            Assert.Equal("TestSwitch(vlan-100)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowSpanningTree_ShouldDisplayStp()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show spanning-tree");
            
            // Assert
            Assert.Contains("Spanning Tree", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowLog_ShouldDisplayLogs()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show log");
            
            // Assert
            Assert.Contains("Log", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowTech_ShouldDisplayTechInfo()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show tech");
            
            // Assert
            Assert.Contains("Technical", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowSystem_ShouldDisplaySystemInfo()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show system");
            
            // Assert
            Assert.Contains("System", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ShowPower_ShouldDisplayPowerInfo()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show power");
            
            // Assert
            Assert.Contains("Power", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show running-config")]
        [InlineData("show version")]
        [InlineData("show interfaces")]
        [InlineData("show ip route")]
        [InlineData("show arp")]
        [InlineData("show vlan")]
        [InlineData("show spanning-tree")]
        [InlineData("show log")]
        [InlineData("show tech")]
        [InlineData("show system")]
        [InlineData("show power")]
        [InlineData("show mac-address")]
        [InlineData("show lldp info remote-device")]
        [InlineData("show time")]
        [InlineData("show uptime")]
        [InlineData("show cpu")]
        [InlineData("show memory")]
        [InlineData("show flash")]
        [InlineData("show modules")]
        [InlineData("show environment")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public void ArubaHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("enable")]
        [InlineData("configure")]
        [InlineData("hostname NewName")]
        [InlineData("interface GigabitEthernet0/0")]
        [InlineData("ip address 192.168.1.1 255.255.255.0")]
        [InlineData("enable")]
        [InlineData("ip route 10.0.0.0 255.0.0.0 192.168.1.1")]
        [InlineData("vlan 100")]
        [InlineData("name TestVLAN")]
        [InlineData("snmp-server community public")]
        [InlineData("time daylight-time-rule")]
        [InlineData("time timezone")]
        public void ArubaHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            if (!command.StartsWith("enable") && !command.StartsWith("configure"))
            {
                device.ProcessCommand("enable");
                if (!command.StartsWith("hostname") && !command.StartsWith("snmp") && !command.StartsWith("time"))
                {
                    device.ProcessCommand("configure");
                }
            }
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("show interfaces brief")]
        [InlineData("show interfaces detail")]
        [InlineData("show ip route detail")]
        [InlineData("show arp statistics")]
        [InlineData("show vlan brief")]
        [InlineData("show spanning-tree detail")]
        [InlineData("show log tail")]
        [InlineData("show system information")]
        [InlineData("show power brief")]
        [InlineData("show mac-address table")]
        [InlineData("show lldp info remote-device detail")]
        [InlineData("show modules detail")]
        [InlineData("show environment detail")]
        public void ArubaHandler_DetailedShowCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("snmp-server community public")]
        [InlineData("snmp-server host 192.168.1.100 public")]
        [InlineData("snmp-server enable traps")]
        [InlineData("time daylight-time-rule continental-us-and-canada")]
        [InlineData("time timezone -5")]
        [InlineData("time sync sntp")]
        [InlineData("sntp server 192.168.1.1")]
        [InlineData("logging 192.168.1.100")]
        [InlineData("no spanning-tree")]
        public void ArubaHandler_SystemConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("tagged 1-10")]
        [InlineData("untagged 11-20")]
        [InlineData("ip helper-address 192.168.1.1")]
        [InlineData("ip directed-broadcast")]
        [InlineData("ip igmp")]
        [InlineData("name TestVLAN")]
        public void ArubaHandler_VlanConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("vlan 100");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch(vlan-100)#", device.GetPrompt());
        }

        [Theory]
        [InlineData("description Test Interface")]
        [InlineData("speed-duplex 1000-full")]
        [InlineData("flow-control")]
        [InlineData("broadcast-limit 10")]
        [InlineData("multicast-limit 10")]
        [InlineData("unknown-vlans-limit 10")]
        [InlineData("rate-limit all in kbps 1000")]
        [InlineData("rate-limit all out kbps 1000")]
        public void ArubaHandler_InterfaceConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch(eth-0/0)#", device.GetPrompt());
        }

        [Fact]
        public void ArubaHandler_ComplexVlanConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act & Assert for complex VLAN configuration
            var commands = new[]
            {
                "vlan 100",
                "name Production",
                "tagged 1-10",
                "untagged 11-20",
                "ip helper-address 192.168.1.1",
                "ip directed-broadcast",
                "exit",
                "vlan 200",
                "name Development",
                "tagged 21-30",
                "untagged 31-40"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public void ArubaHandler_SpanningTreeConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act & Assert for spanning tree configuration
            var commands = new[]
            {
                "spanning-tree priority 4096",
                "spanning-tree force-version rstp-operation",
                "spanning-tree 1 priority 8192",
                "spanning-tree 1 root primary",
                "spanning-tree mode mstp",
                "spanning-tree config-name REGION1",
                "spanning-tree config-revision 1"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch(config)#", device.GetPrompt());
            }
        }

        [Fact]
        public void ArubaHandler_SnmpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act & Assert for SNMP configuration
            var commands = new[]
            {
                "snmp-server community public",
                "snmp-server community private rw",
                "snmp-server host 192.168.1.100 public",
                "snmp-server host 192.168.1.101 private",
                "snmp-server enable traps",
                "snmp-server contact \"Network Admin\"",
                "snmp-server location \"Data Center\""
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ArubaHandler_TimeConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act & Assert for time configuration
            var commands = new[]
            {
                "time daylight-time-rule continental-us-and-canada",
                "time timezone -5",
                "time sync sntp",
                "sntp server 192.168.1.1",
                "sntp server 192.168.1.2",
                "sntp 60"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ArubaHandler_LoggingConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            
            // Act & Assert for logging configuration
            var commands = new[]
            {
                "logging 192.168.1.100",
                "logging 192.168.1.101",
                "logging facility local7",
                "logging severity info",
                "no logging console",
                "logging buffer 1000"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ArubaHandler_AdvancedInterfaceConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act & Assert for advanced interface configuration
            var commands = new[]
            {
                "description Production Interface",
                "ip address 192.168.1.1 255.255.255.0",
                "speed-duplex 1000-full",
                "flow-control",
                "broadcast-limit 10",
                "multicast-limit 10",
                "unknown-vlans-limit 10",
                "rate-limit all in kbps 10000",
                "rate-limit all out kbps 10000",
                "enable"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch(eth-0/0)#", device.GetPrompt());
            }
        }

        [Fact]
        public void ArubaHandler_TrunkConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act & Assert for trunk configuration
            var commands = new[]
            {
                "trunk 1-4 trk1 lacp",
                "interface trk1",
                "ip address 192.168.1.1 255.255.255.0",
                "tagged vlan 100,200,300",
                "exit",
                "trunk 5-8 trk2 100full",
                "interface trk2",
                "untagged vlan 1"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public void ArubaHandler_MacAddressTableConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ArubaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            
            // Act & Assert for MAC address table configuration
            var commands = new[]
            {
                "mac-age-time 600",
                "vlan 100",
                "name Production",
                "exit",
                "interface GigabitEthernet0/0",
                "broadcast-limit 10",
                "unknown-vlans-limit 5"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }
    }
}
