using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class CdpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly CdpProtocol _cdpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public CdpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceCDP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _cdpProtocol = _testDevice.GetProtocolsForTesting().OfType<CdpProtocol>().FirstOrDefault();
            Assert.NotNull(_cdpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var cfg = new CdpConfig { IsEnabled = true };
            _testDevice.SetCdpConfiguration(cfg);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerCdpUpdateProcessing()
        {
            string expected1 = $"CdpProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating CDP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForCdpOnRelevantDevice_ShouldTriggerCdpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"CdpProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating CDP configuration and state";

            var newCfg = new CdpConfig { IsEnabled = true };
            _testDevice.SetCdpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenCdpDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetCdpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetCdpConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"CdpProtocol on {_testDevice.Name}: CDP configuration missing or not enabled.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating CDP state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
