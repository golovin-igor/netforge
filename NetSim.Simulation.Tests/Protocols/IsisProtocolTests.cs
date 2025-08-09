using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class IsisProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly IsisProtocol _isisProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public IsisProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceISIS");
            _network.AddDeviceAsync(_testDevice).Wait();

            _isisProtocol = _testDevice.GetProtocolsForTesting().OfType<IsisProtocol>().FirstOrDefault();
            Assert.NotNull(_isisProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var cfg = new IsIsConfig { SystemId = "0000.0000.0001", IsEnabled = true };
            _testDevice.SetIsisConfiguration(cfg);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerIsisUpdateProcessing()
        {
            string expected1 = $"IS-ISProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating IS-IS state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForIsisOnRelevantDevice_ShouldTriggerIsisUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"IS-ISProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating IS-IS configuration and state";

            var newCfg = new IsIsConfig { SystemId = "0000.0000.0002", IsEnabled = true };
            _testDevice.SetIsisConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenIsisDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetIsisConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetIsisConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"IS-ISProtocol on {_testDevice.Name}: IS-IS configuration missing or not enabled.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating IS-IS state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
