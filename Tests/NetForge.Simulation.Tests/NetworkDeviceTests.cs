using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Topology.Common;
using NetForge.Simulation.Topology.Devices;
using Xunit;
// For ProtocolType
// Added for OspfConfig, BgpConfig

namespace NetForge.Simulation.Tests
{
    public class NetworkDeviceTests
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice; // Using a concrete device type

        public NetworkDeviceTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestRTR");
            _network.AddDeviceAsync(_testDevice).Wait(); // Ensure device is part of network for ParentNetwork.EventBus access
        }

        [Fact]
        public async Task SetOspfConfiguration_WhenCalled_ShouldPublishProtocolConfigChangedEvent()
        {
            // Arrange
            ProtocolConfigChangedEventArgs? receivedArgs = null;
            _eventBus.Subscribe<ProtocolConfigChangedEventArgs>(args =>
            {
                if (args.DeviceName == _testDevice.Name && args.ProtocolType == NetworkProtocolType.OSPF)
                {
                    receivedArgs = args;
                }
                return Task.CompletedTask;
            });

            var ospfConfig = new OspfConfig(1);
            ospfConfig.IsEnabled = true;

            // Act
            _testDevice.SetOspfConfiguration(ospfConfig);
            await Task.Delay(10); // Allow time for event processing

            // Assert
            Assert.NotNull(receivedArgs);
            Assert.Equal(_testDevice.Name, receivedArgs.DeviceName);
            Assert.Equal(NetworkProtocolType.OSPF, receivedArgs.ProtocolType);
            Assert.Contains("OSPF configuration", receivedArgs.ChangeDetails);
        }

        [Fact]
        public async Task SetBgpConfiguration_WhenCalled_ShouldPublishProtocolConfigChangedEvent()
        {
            // Arrange
            ProtocolConfigChangedEventArgs? receivedArgs = null;
            _eventBus.Subscribe<ProtocolConfigChangedEventArgs>(args =>
            {
                if (args.DeviceName == _testDevice.Name && args.ProtocolType == NetworkProtocolType.BGP)
                {
                    receivedArgs = args;
                }
                return Task.CompletedTask;
            });

            var bgpConfig = new BgpConfig(65000);
            bgpConfig.IsEnabled = true;

            // Act
            _testDevice.SetBgpConfiguration(bgpConfig);
            await Task.Delay(10);

            // Assert
            Assert.NotNull(receivedArgs);
            Assert.Equal(_testDevice.Name, receivedArgs.DeviceName);
            Assert.Equal(NetworkProtocolType.BGP, receivedArgs.ProtocolType);
            Assert.Contains("BGP configuration", receivedArgs.ChangeDetails);
        }

        // Add similar tests for SetRipConfiguration, SetEigrpConfiguration, SetStpConfiguration, SetIsisConfiguration

        [Fact]
        public async Task RegisterProtocol_WhenNewProtocolRegistered_ShouldCallSubscribeToEvents()
        {
            // Arrange
            var testProtocol = new TestEventSubscribingProtocol();

            // Act
            _testDevice.RegisterProtocol(testProtocol);
            await Task.Delay(10); // Allow for any async operations in registration if any

            // Assert
            Assert.True(testProtocol.SubscribeToEventsCalled, "SubscribeToEvents should have been called on the protocol.");
            Assert.Same(_eventBus, testProtocol.SubscribedEventBus); // Check if the correct event bus was passed
            Assert.Same(_testDevice, testProtocol.SubscribedDevice); // Check if the correct device was passed
        }
    }

    // Helper protocol class for testing RegisterProtocol subscription call
    public class TestEventSubscribingProtocol : IDeviceProtocol
    {
        public NetworkProtocolType Type { get; set; } = (NetworkProtocolType)999;
        public string Name => "TestEventSubscribingProtocol";
        public string Version => "1.0";
        public IEnumerable<string> SupportedVendors => new[] { "Test" };
        
        public bool InitializeCalled { get; private set; } = false;
        public bool UpdateStateCalled { get; private set; } = false;
        public bool SubscribeToEventsCalled { get; private set; } = false;
        public NetworkEventBus? SubscribedEventBus { get; private set; }
        public NetworkDevice? SubscribedDevice { get; private set; }

        public void Initialize(INetworkDevice device) => InitializeCalled = true;
        public Task UpdateState(INetworkDevice device) { UpdateStateCalled = true; return Task.CompletedTask; }
        public Task<bool> Start() => Task.FromResult(true);
        public Task<bool> Stop() => Task.FromResult(true);
        public Task<bool> Configure(object config) => Task.FromResult(true);
        public object GetConfiguration() => new object();
        public void ApplyConfiguration(object config) { }
        public IProtocolState GetState() => throw new NotImplementedException();
        public T GetTypedState<T>() where T : class => throw new NotImplementedException();
        public IEnumerable<string> GetSupportedVendors() => SupportedVendors;
        public bool SupportsVendor(string vendor) => vendor == "Test";
        public IEnumerable<NetworkProtocolType> GetDependencies() => Enumerable.Empty<NetworkProtocolType>();
        public IEnumerable<NetworkProtocolType> GetConflicts() => Enumerable.Empty<NetworkProtocolType>();
        public bool CanCoexistWith(NetworkProtocolType protocolType) => true;
        public object GetMetrics() => new object();
        public void SubscribeToEvents(INetworkEventBus eventBus, INetworkDevice device) { SubscribeToEventsCalled = true; }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            SubscribeToEventsCalled = true;
            SubscribedEventBus = eventBus;
            SubscribedDevice = self;
        }
    }
}
