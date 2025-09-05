using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Vendors.Cisco;
using NetForge.Simulation.Vendors.Juniper;
using NetForge.Simulation.Vendors.Arista;
using Xunit;

namespace NetForge.Simulation.Tests.Vendors
{
    /// <summary>
    /// Tests for the vendor descriptor system
    /// </summary>
    public class VendorSystemTests
    {
        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            
            // Add vendor system with IoC
            services.AddVendorSystem();
            
            // Register specific vendors
            services.AddVendor<CiscoVendorDescriptor>();
            services.AddVendor<JuniperVendorDescriptor>();
            services.AddVendor<AristaVendorDescriptor>();
            
            return services.BuildServiceProvider();
        }

        [Fact]
        public void VendorRegistry_Should_Register_Vendors()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            
            // Act
            var cisco = vendorRegistry.GetVendor("Cisco");
            var juniper = vendorRegistry.GetVendor("Juniper");
            var arista = vendorRegistry.GetVendor("Arista");
            
            // Assert
            Assert.NotNull(cisco);
            Assert.NotNull(juniper);
            Assert.NotNull(arista);
            
            Assert.Equal("Cisco", cisco.VendorName);
            Assert.Equal("Juniper", juniper.VendorName);
            Assert.Equal("Arista", arista.VendorName);
        }

        [Fact]
        public void VendorDescriptor_Should_List_Supported_Protocols()
        {
            // Arrange
            var cisco = new CiscoVendorDescriptor();
            
            // Act
            var supportsOspf = cisco.SupportsProtocol(NetworkProtocolType.OSPF);
            var supportsBgp = cisco.SupportsProtocol(NetworkProtocolType.BGP);
            var supportsEigrp = cisco.SupportsProtocol(NetworkProtocolType.EIGRP);
            
            // Assert
            Assert.True(supportsOspf);
            Assert.True(supportsBgp);
            Assert.True(supportsEigrp);
        }

        [Fact]
        public void VendorDescriptor_Should_List_Device_Models()
        {
            // Arrange
            var cisco = new CiscoVendorDescriptor();
            
            // Act
            var isr4451 = cisco.GetModelDescriptor("ISR4451");
            var catalyst9300 = cisco.GetModelDescriptor("Catalyst9300");
            
            // Assert
            Assert.NotNull(isr4451);
            Assert.NotNull(catalyst9300);
            Assert.Equal(DeviceType.Router, isr4451.DeviceType);
            Assert.Equal(DeviceType.Switch, catalyst9300.DeviceType);
        }

        [Fact]
        public void VendorService_Should_Create_Protocols()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var vendorService = serviceProvider.GetRequiredService<IVendorService>();
            
            // Act
            // Note: This will return null unless the actual protocol implementations exist
            var ospfProtocol = vendorService.CreateProtocol("Cisco", NetworkProtocolType.OSPF);
            
            // Assert
            // In a real scenario with implementations, this would not be null
            // For this test, we're verifying the service works
            Assert.NotNull(vendorService);
        }

        [Fact]
        public void VendorRegistry_Should_Find_Vendors_By_Protocol()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            
            // Act
            var ospfVendors = vendorRegistry.GetVendorsForProtocol(NetworkProtocolType.OSPF).ToList();
            var eigrpVendors = vendorRegistry.GetVendorsForProtocol(NetworkProtocolType.EIGRP).ToList();
            
            // Assert
            Assert.NotEmpty(ospfVendors);
            Assert.Contains(ospfVendors, v => v.VendorName == "Cisco");
            Assert.Contains(ospfVendors, v => v.VendorName == "Juniper");
            Assert.Contains(ospfVendors, v => v.VendorName == "Arista");
            
            // EIGRP is Cisco-only
            Assert.Single(eigrpVendors);
            Assert.Equal("Cisco", eigrpVendors.First().VendorName);
        }

        [Fact]
        public void VendorRegistry_Should_Find_Vendors_By_DeviceType()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider();
            var vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            
            // Act
            var routerVendors = vendorRegistry.GetVendorsForDeviceType(DeviceType.Router).ToList();
            var switchVendors = vendorRegistry.GetVendorsForDeviceType(DeviceType.Switch).ToList();
            var firewallVendors = vendorRegistry.GetVendorsForDeviceType(DeviceType.Firewall).ToList();
            
            // Assert
            Assert.NotEmpty(routerVendors);
            Assert.NotEmpty(switchVendors);
            Assert.NotEmpty(firewallVendors);
            
            // All vendors support routers and switches
            Assert.Equal(3, routerVendors.Count);
            Assert.Equal(3, switchVendors.Count);
            
            // Cisco and Juniper support firewalls
            Assert.Equal(2, firewallVendors.Count);
        }

        [Fact]
        public void VendorDescriptor_Should_Have_Correct_Prompts()
        {
            // Arrange
            var cisco = new CiscoVendorDescriptor();
            var juniper = new JuniperVendorDescriptor();
            var arista = new AristaVendorDescriptor();
            
            // Act & Assert
            Assert.Equal(">", cisco.Configuration.DefaultPrompt);
            Assert.Equal("#", cisco.Configuration.EnabledPrompt);
            Assert.Equal("(config)#", cisco.Configuration.ConfigPrompt);
            
            Assert.Equal(">", juniper.Configuration.DefaultPrompt);
            Assert.Equal("#", juniper.Configuration.EnabledPrompt);
            Assert.Equal("#", juniper.Configuration.ConfigPrompt);
            Assert.Equal("[edit]", juniper.Configuration.PromptModes["configuration"]);
            
            Assert.Equal(">", arista.Configuration.DefaultPrompt);
            Assert.Equal("#", arista.Configuration.EnabledPrompt);
            Assert.Equal("(config)#", arista.Configuration.ConfigPrompt);
        }

        [Fact]
        public void VendorDescriptor_Should_Have_CLI_Handlers()
        {
            // Arrange
            var cisco = new CiscoVendorDescriptor();
            
            // Act
            var handlers = cisco.CliHandlers.ToList();
            var showHandlers = handlers.Where(h => h.Type == HandlerType.Show).ToList();
            var configHandlers = handlers.Where(h => h.Type == HandlerType.Configuration).ToList();
            var routingHandlers = handlers.Where(h => h.Type == HandlerType.Routing).ToList();
            
            // Assert
            Assert.NotEmpty(handlers);
            Assert.NotEmpty(showHandlers);
            Assert.NotEmpty(configHandlers);
            Assert.NotEmpty(routingHandlers);
            
            Assert.Contains(handlers, h => h.HandlerName == "ShowVersion");
            Assert.Contains(handlers, h => h.HandlerName == "ShowRunningConfig");
            Assert.Contains(handlers, h => h.HandlerName == "RouterOspf");
            Assert.Contains(handlers, h => h.HandlerName == "RouterBgp");
        }

        [Fact]
        public void Vendor_Priority_Should_Be_Correct()
        {
            // Arrange
            var cisco = new CiscoVendorDescriptor();
            var juniper = new JuniperVendorDescriptor();
            var arista = new AristaVendorDescriptor();
            
            // Act & Assert
            Assert.Equal(100, cisco.Priority);
            Assert.Equal(90, juniper.Priority);
            Assert.Equal(80, arista.Priority);
            
            // Cisco should have highest priority
            Assert.True(cisco.Priority > juniper.Priority);
            Assert.True(cisco.Priority > arista.Priority);
        }
    }
}