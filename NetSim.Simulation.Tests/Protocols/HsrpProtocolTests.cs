using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class HsrpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly HsrpProtocol _hsrpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public HsrpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceHSRP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _hsrpProtocol = _testDevice.GetProtocolsForTesting().OfType<HsrpProtocol>().FirstOrDefault();
            Assert.NotNull(_hsrpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var cfg = new HsrpConfig();
            cfg.AddGroup(1, "10.0.0.1", 110, "GigabitEthernet0/0");
            cfg.IsEnabled = true;
            _testDevice.SetHsrpConfiguration(cfg);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerHsrpUpdateProcessing()
        {
            string expected1 = $"HSRPProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating HSRP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForHsrpOnRelevantDevice_ShouldTriggerHsrpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"HSRPProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating HSRP configuration and state";

            var newCfg = new HsrpConfig();
            newCfg.AddGroup(2, "10.0.0.2", 120, "GigabitEthernet0/0");
            newCfg.IsEnabled = true;
            _testDevice.SetHsrpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenHsrpDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetHsrpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetHsrpConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"HSRPProtocol on {_testDevice.Name}: HSRP configuration missing or not enabled.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating HSRP state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
