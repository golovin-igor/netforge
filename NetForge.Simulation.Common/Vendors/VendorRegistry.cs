using System.Reflection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Central registry for all vendor descriptors
    /// </summary>
    public class VendorRegistry : IVendorRegistry
    {
        private readonly Dictionary<string, IVendorDescriptor> _vendors = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        public void RegisterVendor(IVendorDescriptor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            lock (_lock)
            {
                _vendors[vendor.VendorName] = vendor;
            }
        }

        public IVendorDescriptor? GetVendor(string vendorName)
        {
            if (string.IsNullOrEmpty(vendorName))
                return null;

            lock (_lock)
            {
                return _vendors.GetValueOrDefault(vendorName);
            }
        }

        public IEnumerable<IVendorDescriptor> GetAllVendors()
        {
            lock (_lock)
            {
                return _vendors.Values.ToList();
            }
        }

        public bool IsVendorRegistered(string vendorName)
        {
            if (string.IsNullOrEmpty(vendorName))
                return false;

            lock (_lock)
            {
                return _vendors.ContainsKey(vendorName);
            }
        }

        public IEnumerable<IVendorDescriptor> GetVendorsForProtocol(NetworkProtocolType protocolType)
        {
            lock (_lock)
            {
                return _vendors.Values
                    .Where(v => v.SupportsProtocol(protocolType))
                    .OrderByDescending(v => v.Priority)
                    .ToList();
            }
        }

        public IEnumerable<IVendorDescriptor> GetVendorsForDeviceType(DeviceType deviceType)
        {
            lock (_lock)
            {
                return _vendors.Values
                    .Where(v => v.SupportedModels.Any(m => m.DeviceType == deviceType))
                    .OrderByDescending(v => v.Priority)
                    .ToList();
            }
        }

        public int DiscoverAndRegisterVendors()
        {
            var count = 0;
            var vendorType = typeof(IVendorDescriptor);

            // Get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.GetName().Name?.StartsWith("NetForge") == true);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => !t.IsAbstract && 
                                   !t.IsInterface && 
                                   vendorType.IsAssignableFrom(t));

                    foreach (var type in types)
                    {
                        try
                        {
                            var vendor = Activator.CreateInstance(type) as IVendorDescriptor;
                            if (vendor != null)
                            {
                                RegisterVendor(vendor);
                                count++;
                            }
                        }
                        catch
                        {
                            // Skip vendors that fail to instantiate
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that fail to load types
                }
            }

            return count;
        }
    }
}