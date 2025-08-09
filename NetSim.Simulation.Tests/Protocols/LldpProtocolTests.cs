using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class LldpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly LldpProtocol _lldpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public LldpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceLLDP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _lldpProtocol = _testDevice.GetProtocolsForTesting().OfType<LldpProtocol>().FirstOrDefault();
            Assert.NotNull(_lldpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var lldp = new LldpConfig { IsEnabled = true };
            _testDevice.SetLldpConfiguration(lldp);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerLldpUpdateProcessing()
        {
            string expected1 = $"LldpProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating LLDP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForLldpOnRelevantDevice_ShouldTriggerLldpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"LldpProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating LLDP configuration and state";

            var newCfg = new LldpConfig { IsEnabled = true };
            _testDevice.SetLldpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenLldpDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetLldpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetLldpConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"LldpProtocol on {_testDevice.Name}: LLDP configuration missing or not enabled.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating LLDP state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
