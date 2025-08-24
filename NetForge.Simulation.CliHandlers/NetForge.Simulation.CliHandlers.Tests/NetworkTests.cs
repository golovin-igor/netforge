using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers
{
    public class CommandHandlerNetworkTests
    {
        private Network _network;

        public CommandHandlerNetworkTests()
        {
            _network = new Network();
        }

        [Fact]
        public async Task AddDeviceAsyncDeviceAddedEventPublished()
        {
            var device = new CiscoDevice("R1");
            bool eventFired = false;
            _network.EventBus.Subscribe<DeviceChangedEventArgs>(args =>
            {
                if (args.DeviceName == "R1" && args.ChangeType == DeviceChangeType.Added)
                    eventFired = true;
                return Task.CompletedTask;
            });

            await _network.AddDeviceAsync(device);

            Assert.True(eventFired, "DeviceAddedEvent was not fired.");
            Assert.Same(device, _network.GetDevice("R1"));
        }

        [Fact]
        public async Task AddLinkAsyncLinkAddedEventPublishedAndInterfacesUp()
        {
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);

            bool linkEventFired = false;
            bool r1IfaceEventFired = false;
            bool r2IfaceEventFired = false;

            _network.EventBus.Subscribe<LinkChangedEventArgs>(args =>
            {
                if (args.Device1Name == "R1" && args.Device2Name == "R2" && args.ChangeType == LinkChangeType.Added)
                    linkEventFired = true;
                return Task.CompletedTask;
            });
            _network.EventBus.Subscribe<InterfaceStateChangedEventArgs>(args =>
            {
                if (args.DeviceName == "R1" && args.InterfaceName == "GigabitEthernet0/0" && args.IsUp)
                    r1IfaceEventFired = true;
                if (args.DeviceName == "R2" && args.InterfaceName == "GigabitEthernet0/0" && args.IsUp)
                    r2IfaceEventFired = true;
                return Task.CompletedTask;
            });

            // Ensure interfaces are initially shutdown for a clear test of them coming up
            var r1Iface = r1.GetInterface("GigabitEthernet0/0"); // Default interface name might vary by device type
            if(r1Iface != null) {
                r1Iface.IsShutdown = true;
                r1Iface.IsShutdown = false;
            }
            var r2Iface = r2.GetInterface("GigabitEthernet0/0");
            if(r2Iface != null) {
                r2Iface.IsShutdown = true;
                r2Iface.IsShutdown = false;
            }

            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            await Task.Delay(50); // Allow events to propagate

            Assert.True(linkEventFired, "LinkAddedEvent was not fired.");
            Assert.True(r1IfaceEventFired, "R1 InterfaceStateChangeEvent (Up) not fired or interface not Up.");
            Assert.True(r2IfaceEventFired, "R2 InterfaceStateChangeEvent (Up) not fired or interface not Up.");
        }

        [Fact]
        public async Task RemoveLinkAsyncLinkRemovedEventPublishedAndInterfacesDown()
        {
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);
            var r1Iface = r1.GetInterface("GigabitEthernet0/0");
            if(r1Iface != null) r1Iface.IsShutdown = false; // Ensure up before link
            var r2Iface = r2.GetInterface("GigabitEthernet0/0");
            if(r2Iface != null) r2Iface.IsShutdown = false; // Ensure up before link
            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            await Task.Delay(50); // Allow link add events

            bool linkEventFired = false;
            bool r1IfaceEventFired = false;
            bool r2IfaceEventFired = false;

            _network.EventBus.Subscribe<LinkChangedEventArgs>(args =>
            {
                if (args.Device1Name == "R1" && args.Device2Name == "R2" && args.ChangeType == LinkChangeType.Removed)
                    linkEventFired = true;
                return Task.CompletedTask;
            });
             _network.EventBus.Subscribe<InterfaceStateChangedEventArgs>(args =>
            {
                if (args.DeviceName == "R1" && args.InterfaceName == "GigabitEthernet0/0" && !args.IsUp)
                    r1IfaceEventFired = true;
                if (args.DeviceName == "R2" && args.InterfaceName == "GigabitEthernet0/0" && !args.IsUp)
                    r2IfaceEventFired = true;
                return Task.CompletedTask;
            });

            await _network.RemoveLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            await Task.Delay(50); // Allow events to propagate

            Assert.True(linkEventFired, "LinkRemovedEvent was not fired.");
            Assert.True(r1IfaceEventFired, "R1 InterfaceStateChangeEvent (Down) not fired or interface not Down.");
            Assert.True(r2IfaceEventFired, "R2 InterfaceStateChangeEvent (Down) not fired or interface not Down.");
        }

        [Fact]
        public void GetDeviceNonExistentDeviceReturnsNull()
        {
            Assert.Null(_network.GetDevice("NonExistent"));
        }

        [Fact]
        public async Task FindDeviceByIpDeviceExistsReturnsDevice()
        {
            var device = new CiscoDevice("R1");
            await _network.AddDeviceAsync(device);
            var ifaceConfig = device.GetInterface("GigabitEthernet0/0");
            if (ifaceConfig != null) ifaceConfig.IpAddress = "192.168.1.1";

            Assert.Same(device, _network.FindDeviceByIp("192.168.1.1"));
        }

        [Fact]
        public async Task GetConnectedDevicesNoLinkReturnsEmpty()
        {
            var device = new CiscoDevice("R1");
            await _network.AddDeviceAsync(device);
            Assert.Empty(_network.GetConnectedDevices("R1", "GigabitEthernet0/0"));
        }

        [Fact]
        public async Task GetConnectedDevicesWithLinkReturnsConnectedDevice()
        {
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);
            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/1");

            var connected = _network.GetConnectedDevices("R1", "GigabitEthernet0/0");
            Assert.Single(connected);
            Assert.Same(r2, connected[0].device);
            Assert.Equal("GigabitEthernet0/1", connected[0].interfaceName);
        }

        [Fact]
        public async Task UpdateProtocolsCallsUpdateOnAllDevices()
        {
            var device1 = new TestDevice("D1");
            var device2 = new TestDevice("D2");
            await _network.AddDeviceAsync(device1);
            await _network.AddDeviceAsync(device2);

            _network.UpdateProtocols();
            await Task.Delay(50); // Allow time for async operations within UpdateAllProtocolStates to complete if any.

            Assert.True(device1.UpdateAllProtocolStatesCalled, "Device1.UpdateAllProtocolStates was not called.");
            Assert.True(device2.UpdateAllProtocolStatesCalled, "Device2.UpdateAllProtocolStates was not called.");
        }

        [Fact]
        public async Task AddDeviceNullDeviceDoesNotThrowAndDoesNotAdd()
        {
            int initialCount = _network.GetAllDevices().Count();
            await _network.AddDeviceAsync(null);
            Assert.Equal(initialCount, _network.GetAllDevices().Count());
        }

        [Fact]
        public async Task AddLinkDeviceNotExistsDoesNotAddLink()
        {
            var r1 = new CiscoDevice("R1");
            await _network.AddDeviceAsync(r1);

            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "NonExistentR2", "GigabitEthernet0/0");
            Assert.False(_network.AreConnected("R1", "GigabitEthernet0/0", "NonExistentR2", "GigabitEthernet0/0"));

            await _network.AddLinkAsync("NonExistentR1", "GigabitEthernet0/0", "R1", "GigabitEthernet0/0");
            Assert.False(_network.AreConnected("NonExistentR1", "GigabitEthernet0/0", "R1", "GigabitEthernet0/0"));
        }

        [Fact]
        public async Task RemoveLinkNonExistentLinkDoesNotThrow()
        {
             var r1 = new CiscoDevice("R1");
             var r2 = new CiscoDevice("R2");
             await _network.AddDeviceAsync(r1);
             await _network.AddDeviceAsync(r2);
            await _network.RemoveLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            Assert.True(true);
        }

        [Fact]
        public async Task MultipleLinksBetweenSameDevicesAreAllowed()
        {
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            await _network.AddDeviceAsync(r1);
            await _network.AddDeviceAsync(r2);
            await _network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");
            await _network.AddLinkAsync("R1", "GigabitEthernet0/1", "R2", "GigabitEthernet0/1");

            Assert.True(_network.AreConnected("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0"));
            Assert.True(_network.AreConnected("R1", "GigabitEthernet0/1", "R2", "GigabitEthernet0/1"));
        }

        private class TestDevice : NetworkDevice
        {
            public bool UpdateAllProtocolStatesCalled { get; private set; }
            public TestDevice(string name) : base(name) { Vendor = "Test"; }
            protected override void InitializeDefaultInterfaces() { Interfaces.Add("Default0", new InterfaceConfig("Default0", this)); }
            public override string GetPrompt() => $"{Name}>";
            protected override void RegisterDeviceSpecificHandlers() { }
            public override async Task UpdateAllProtocolStates()
            {
                UpdateAllProtocolStatesCalled = true;
                await base.UpdateAllProtocolStates();
            }
        }
    }
}
