using System.Globalization;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Events;

namespace NetForge.Simulation.Common.Configuration
{
    /// <summary>
    /// Represents the configuration and state of a network interface
    /// </summary>
    public class InterfaceConfig
    {
        private NetworkDevice _parentDevice;

        public string Name { get; set; }
        public string? IpAddress { get; set; }
        public string? SubnetMask { get; set; }
        private bool _isUp = true;
        private bool _isShutdown;
        public int VlanId { get; set; } = 1; // Default VLAN
        public string SwitchportMode { get; set; } = "access";
        public string Description { get; set; } = "";
        public long RxPackets { get; set; }
        public long TxPackets { get; set; }
        public long RxBytes { get; set; }
        public long TxBytes { get; set; }

        // For port-channel/aggregation
        public int? ChannelGroup { get; set; }
        public string? ChannelMode { get; set; }

        public string MacAddress { get; set; }

        public int Mtu { get; set; } = 1500;
        public string Duplex { get; set; } = "auto";
        public string Speed { get; set; } = "auto";

        // OSPF specific settings
        public bool OspfEnabled { get; set; }
        public int OspfProcessId { get; set; } = 1;
        public int OspfArea { get; set; }
        public int OspfCost { get; set; } = 10;
        public string OspfNetworkType { get; set; } = "broadcast"; // broadcast, point-to-point

        // STP specific settings
        public bool StpPortfast { get; set; }
        public bool StpBpduGuard { get; set; }

        // ACL assignments
        public int? IncomingAccessList { get; set; }
        public int? OutgoingAccessList { get; set; }

        public bool IsUp
        {
            get => _isUp;
            set
            {
                if (_isUp != value)
                {
                    _isUp = value;
                    if (_parentDevice?.ParentNetwork?.EventBus != null)
                    {
                        _ = _parentDevice.ParentNetwork.EventBus.PublishAsync(new InterfaceStateChangedEventArgs(_parentDevice.Name, Name, _isUp, _isShutdown));
                    }
                }
            }
        }

        public bool IsShutdown
        {
            get => _isShutdown;
            set
            {
                if (_isShutdown == value)
                {
                    return;
                }

                _isShutdown = value;
                bool oldOperationalUp = _isUp;
                bool newOperationalUp = !_isShutdown && _isUp;
                if (_isShutdown) newOperationalUp = false;

                if (oldOperationalUp != newOperationalUp)
                {
                    _isUp = newOperationalUp;
                    if (_parentDevice?.ParentNetwork?.EventBus != null)
                    {
                        _ = _parentDevice.ParentNetwork.EventBus.PublishAsync(new InterfaceStateChangedEventArgs(_parentDevice.Name, Name, _isUp, _isShutdown));
                    }
                }
                else if (_parentDevice?.ParentNetwork?.EventBus != null)
                {
                    _ = _parentDevice.ParentNetwork.EventBus.PublishAsync(new InterfaceStateChangedEventArgs(_parentDevice.Name, Name, _isUp, _isShutdown));
                }
            }
        }

        public InterfaceConfig(string name, NetworkDevice? parentDevice = null)
        {
            Name = name;
            MacAddress = GenerateMac(name);
            _parentDevice = parentDevice;
        }

        private string GenerateMac(string seed)
        {
            // simple hash-based mac generation
            var hash = Math.Abs(seed.GetHashCode());
            var bytes = BitConverter.GetBytes(hash);
            return string.Format(CultureInfo.InvariantCulture, "02:{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}", bytes[0], bytes[1], bytes[2], bytes[3], new Random(hash).Next(0, 255)).ToLowerInvariant();
        }

        public string GetStatus()
        {
            if (IsShutdown)
                return "administratively down";
            if (IsUp)
                return "up";
            return "down";
        }

        public void SetParentDevice(NetworkDevice device)
        {
            _parentDevice = device;
        }
    }
}
