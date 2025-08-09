using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class BgpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly BgpProtocol _bgpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public BgpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceBGP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _bgpProtocol = _testDevice.GetProtocolsForTesting().OfType<BgpProtocol>().FirstOrDefault();
            Assert.NotNull(_bgpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var bgpConfig = new BgpConfig(65000) { IsEnabled = true };
            _testDevice.SetBgpConfiguration(bgpConfig);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerBgpUpdateProcessing()
        {
            string expected1 = $"BGPProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating BGP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            // Clear logs before triggering the event
            _deviceLogs.Clear();
            
            // Trigger interface state change which should fire the event
            iface.IsUp = !iface.IsUp;
            
            // Wait longer to ensure async event processing completes
            await Task.Delay(200);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForBgpOnRelevantDevice_ShouldTriggerBgpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"BGPProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating BGP configuration and state";

            var newBgp = new BgpConfig(65001) { IsEnabled = true };

            // Setting BGP configuration should trigger ProtocolConfigChangedEventArgs
            _testDevice.SetBgpConfiguration(newBgp);
            
            // Wait longer to ensure async event processing completes
            await Task.Delay(200);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenBgpDisabled_ShouldClearBgpRoutesAndNotProcessFurther()
        {
            var cfg = _testDevice.GetBgpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetBgpConfiguration(cfg);
            
            // Wait for BGP disable to be processed
            await Task.Delay(100);
            _deviceLogs.Clear();

            _testDevice.AddRoute(new Route("10.0.0.0", "255.0.0.0", "", "", "BGP"));
            Assert.Contains(_testDevice.GetRoutingTable(), r => r.Protocol == "BGP");

            string expected = $"BGPProtocol on {_testDevice.Name}: BGP configuration missing or not enabled. Clearing BGP routes.";
            string unexpected = "Simulating peer discovery";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            
            // Wait longer to ensure async event processing completes
            await Task.Delay(200);

            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_testDevice.GetRoutingTable(), r => r.Protocol == "BGP");
            Assert.DoesNotContain(_deviceLogs, l => l.Contains(unexpected));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
