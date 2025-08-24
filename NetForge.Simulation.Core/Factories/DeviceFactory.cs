using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;

namespace NetForge.Simulation.Core
{
    /// <summary>
    /// Factory for creating network device instances
    /// </summary>
    public static class DeviceFactory
    {
        private static readonly Dictionary<string, Func<string, NetworkDevice>> _vendors =
            new(StringComparer.OrdinalIgnoreCase);

        static DeviceFactory()
        {
            // Register default vendors
            RegisterVendor("cisco", name => new CiscoDevice(name));
            RegisterVendor("juniper", name => new JuniperDevice(name));
            RegisterVendor("arista", name => new AristaDevice(name));
            RegisterVendor("nokia", name => new NokiaDevice(name));
            RegisterVendor("alcatel", name => new AlcatelDevice(name));
            RegisterVendor("huawei", name => new HuaweiDevice(name));
            RegisterVendor("fortinet", name => new FortinetDevice(name));
            RegisterVendor("mikrotik", name => new MikroTikDevice(name));
            RegisterVendor("aruba", name => new ArubaDevice(name));
            RegisterVendor("extreme", name => new ExtremeDevice(name));
            RegisterVendor("dell", name => new DellDevice(name));
            RegisterVendor("broadcom", name => new BroadcomDevice(name));
            RegisterVendor("anira", name => new AniraDevice(name));
            RegisterVendor("linux", name => new LinuxDevice(name));
            RegisterVendor("f5", name => new F5Device(name));
        }

        /// <summary>
        /// Register a new vendor constructor
        /// </summary>
        public static void RegisterVendor(string name, Func<string, NetworkDevice> ctor)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Vendor name cannot be null or empty", nameof(name));
            if (ctor == null)
                throw new ArgumentNullException(nameof(ctor));

            _vendors[name] = ctor;
        }

        /// <summary>
        /// Create a network device instance based on vendor
        /// </summary>
        public static NetworkDevice CreateDevice(string vendor, string name)
        {
            if (vendor == null)
                throw new NotSupportedException("Vendor 'null' is not supported");

            if (_vendors.TryGetValue(vendor, out var ctor))
            {
                return ctor(name);
            }

            throw new NotSupportedException($"Vendor '{vendor}' is not supported");
        }
        
        /// <summary>
        /// Get list of supported vendors
        /// </summary>
        public static string[] GetSupportedVendors()
        {
            return _vendors.Keys.ToArray();
        }
    }
} 
