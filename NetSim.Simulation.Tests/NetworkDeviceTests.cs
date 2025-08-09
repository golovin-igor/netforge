using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.Protocols.Routing;
using Xunit;
// For ProtocolType
// Added for OspfConfig, BgpConfig

namespace NetSim.Simulation.Tests
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
                if (args.DeviceName == _testDevice.Name && args.ProtocolType == ProtocolType.OSPF)
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
            Assert.Equal(ProtocolType.OSPF, receivedArgs.ProtocolType);
            Assert.Contains("OSPF configuration", receivedArgs.ChangeDetails);
        }

        [Fact]
        public async Task SetBgpConfiguration_WhenCalled_ShouldPublishProtocolConfigChangedEvent()
        {
            // Arrange
            ProtocolConfigChangedEventArgs? receivedArgs = null;
            _eventBus.Subscribe<ProtocolConfigChangedEventArgs>(args => 
            {
                if (args.DeviceName == _testDevice.Name && args.ProtocolType == ProtocolType.BGP)
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
            Assert.Equal(ProtocolType.BGP, receivedArgs.ProtocolType);
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
    public class TestEventSubscribingProtocol : INetworkProtocol
    {
        public ProtocolType Type => (ProtocolType)999; // Dummy type
        public bool InitializeCalled { get; private set; } = false;
        public bool UpdateStateCalled { get; private set; } = false;
        public bool SubscribeToEventsCalled { get; private set; } = false;
    public NetworkEventBus? SubscribedEventBus { get; private set; }
    public NetworkDevice? SubscribedDevice { get; private set; }

        public void Initialize(NetworkDevice device)
        {
            InitializeCalled = true;
        }

        public Task UpdateState(NetworkDevice device)
        {
            UpdateStateCalled = true;
            return Task.CompletedTask;
        }

        public void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self)
        {
            SubscribeToEventsCalled = true;
            SubscribedEventBus = eventBus;
            SubscribedDevice = self;
        }
    }
} 