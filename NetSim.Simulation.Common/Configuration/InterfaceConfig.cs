using NetSim.Simulation.Common;
using NetSim.Simulation.Events;

namespace NetSim.Simulation.Configuration
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
        private bool _isShutdown = false;
        public int VlanId { get; set; } = 1; // Default VLAN
        public string SwitchportMode { get; set; } = "access";
        public string Description { get; set; } = "";
        public long RxPackets { get; set; } = 0;
        public long TxPackets { get; set; } = 0;
        public long RxBytes { get; set; } = 0;
        public long TxBytes { get; set; } = 0;
        
        // For port-channel/aggregation
        public int? ChannelGroup { get; set; }
        public string? ChannelMode { get; set; }
        
        public string MacAddress { get; set; }
        
        public int Mtu { get; set; } = 1500;
        public string Duplex { get; set; } = "auto";
        public string Speed { get; set; } = "auto";
        
        // OSPF specific settings
        public bool OspfEnabled { get; set; } = false;
        public int OspfProcessId { get; set; } = 1;
        public int OspfArea { get; set; } = 0;
        public int OspfCost { get; set; } = 10;
        public string OspfNetworkType { get; set; } = "broadcast"; // broadcast, point-to-point

        // STP specific settings
        public bool StpPortfast { get; set; } = false;
        public bool StpBpduGuard { get; set; } = false;

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
                if (_isShutdown != value)
                {
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
        }

        public InterfaceConfig(string name, NetworkDevice parentDevice = null)
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
            return $"02:{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}:{new Random(hash).Next(0,255):X2}".ToLower();
        }
        
        public string GetStatus()
        {
            if (IsShutdown)
                return "administratively down";
            else if (IsUp)
                return "up";
            else
                return "down";
        }

        public void SetParentDevice(NetworkDevice device)
        {
            _parentDevice = device;
        }
    }
} 
