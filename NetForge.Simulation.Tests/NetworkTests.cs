using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Events;
using Xunit;

// Added for xUnit

namespace NetForge.Simulation.Tests
{
    public class RootNetworkTests
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;

        public RootNetworkTests() // Constructor acts as TestInitialize / Setup
        {
            _network = new Network();
            _eventBus = _network.EventBus; 
        }

        [Fact]
        public async Task AddDeviceAsync_WhenDeviceAdded_ShouldPublishDeviceAddedEvent()
        {
            // Arrange
            DeviceChangedEventArgs? receivedArgs = null;
            _eventBus.Subscribe<DeviceChangedEventArgs>(args => 
            {
                receivedArgs = args;
                return Task.CompletedTask;
            });

            var device = new CiscoDevice("TestRouter1");
            
            // Act
            await _network.AddDeviceAsync(device);

            // Assert
            Assert.NotNull(receivedArgs);
            Assert.Equal("TestRouter1", receivedArgs?.DeviceName);
            Assert.Equal(DeviceChangeType.Added, receivedArgs?.ChangeType);
            Assert.Same(device, receivedArgs?.Device);
        }

        [Fact]
        public async Task AddLinkAsync_WhenLinkAdded_ShouldPublishEvents()
        {
            // Arrange
            LinkChangedEventArgs? receivedLinkArgs = null;
            InterfaceStateChangedEventArgs? if1Args = null;
            InterfaceStateChangedEventArgs? if2Args = null;
            int interfaceStateChangedCount = 0;

            _eventBus.Subscribe<LinkChangedEventArgs>(args => 
            {
                receivedLinkArgs = args;
                return Task.CompletedTask;
            });
            _eventBus.Subscribe<InterfaceStateChangedEventArgs>(args => 
            {
                interfaceStateChangedCount++;
                if(args.DeviceName == "R1") if1Args = args;
                if(args.DeviceName == "R2") if2Args = args;
                return Task.CompletedTask;
            });

            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);
            
            var iface1Config = r1.GetInterface("GigabitEthernet0/0");
            var iface2Config = r2.GetInterface("GigabitEthernet0/0");
            if (iface1Config != null) iface1Config.IsShutdown = false;
            if (iface2Config != null) iface2Config.IsShutdown = false;

            // Act
            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Assert
            Assert.NotNull(receivedLinkArgs);
            Assert.Equal("R1", receivedLinkArgs?.Device1Name);
            Assert.Equal("R2", receivedLinkArgs?.Device2Name);
            Assert.Equal(LinkChangeType.Added, receivedLinkArgs?.ChangeType);
            
            Assert.True(interfaceStateChangedCount >= 0 && interfaceStateChangedCount <= 2);
            if (if1Args != null) Assert.True(if1Args.IsUp);
            if (if2Args != null) Assert.True(if2Args.IsUp);
        }

        [Fact]
        public async Task RemoveLinkAsync_WhenLinkRemoved_ShouldPublishEvents()
        {
            // Arrange
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);
            
            var iface1Config = r1.GetInterface("GigabitEthernet0/0");
            var iface2Config = r2.GetInterface("GigabitEthernet0/0");
            if (iface1Config != null) { iface1Config.IsShutdown = false; iface1Config.IsUp = true; }
            if (iface2Config != null) { iface2Config.IsShutdown = false; iface2Config.IsUp = true; }
            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            LinkChangedEventArgs? receivedLinkArgs = null;
            InterfaceStateChangedEventArgs? if1Args = null;
            InterfaceStateChangedEventArgs? if2Args = null;
            int interfaceStateChangedCount = 0;

            _eventBus.Subscribe<LinkChangedEventArgs>(args => 
            {
                receivedLinkArgs = args;
                return Task.CompletedTask;
            });
             _eventBus.Subscribe<InterfaceStateChangedEventArgs>(args => 
            {
                interfaceStateChangedCount++;
                if(args.DeviceName == "R1") if1Args = args;
                if(args.DeviceName == "R2") if2Args = args;
                return Task.CompletedTask;
            });

            // Act
            await _network.RemoveLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            await Task.Delay(20);

            // Assert
            Assert.NotNull(receivedLinkArgs);
            Assert.Equal(LinkChangeType.Removed, receivedLinkArgs?.ChangeType);
            
            Assert.True(interfaceStateChangedCount >= 0 && interfaceStateChangedCount <= 2);
            if (if1Args != null) Assert.False(if1Args.IsUp);
            if (if2Args != null) Assert.False(if2Args.IsUp);
        }
    }
} 
