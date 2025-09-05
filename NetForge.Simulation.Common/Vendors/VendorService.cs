using System.Reflection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Service for creating vendor-specific protocols and handlers
    /// </summary>
    public class VendorService : IVendorService
    {
        private readonly IVendorRegistry _vendorRegistry;
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
        private readonly object _lock = new();

        public VendorService(IVendorRegistry vendorRegistry)
        {
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        public object? CreateProtocol(string vendorName, NetworkProtocolType protocolType)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return null;

            var protocolDesc = vendor.GetProtocolDescriptor(protocolType);
            if (protocolDesc == null || !protocolDesc.IsEnabled)
                return null;

            return CreateInstance<object>(protocolDesc.ImplementationClass, protocolDesc.AssemblyName);
        }

        public object? CreateHandler(string vendorName, string handlerName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return null;

            var handlerDesc = vendor.CliHandlers.FirstOrDefault(h => 
                h.HandlerName.Equals(handlerName, StringComparison.OrdinalIgnoreCase));
            
            if (handlerDesc == null || !handlerDesc.IsEnabled)
                return null;

            return CreateInstance<object>(handlerDesc.ImplementationClass, handlerDesc.AssemblyName);
        }

        public IEnumerable<object> GetVendorProtocols(string vendorName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return Enumerable.Empty<object>();

            var protocols = new List<object>();
            
            foreach (var protocolDesc in vendor.SupportedProtocols.Where(p => p.IsEnabled))
            {
                var protocol = CreateInstance<object>(
                    protocolDesc.ImplementationClass, 
                    protocolDesc.AssemblyName);
                
                if (protocol != null)
                    protocols.Add(protocol);
            }

            return protocols;
        }

        public IEnumerable<object> GetVendorHandlers(string vendorName)
        {
            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return Enumerable.Empty<object>();

            var handlers = new List<object>();
            
            foreach (var handlerDesc in vendor.CliHandlers.Where(h => h.IsEnabled))
            {
                var handler = CreateInstance<object>(
                    handlerDesc.ImplementationClass, 
                    handlerDesc.AssemblyName);
                
                if (handler != null)
                    handlers.Add(handler);
            }

            return handlers;
        }

        public void RegisterDeviceHandlers(object device, object handlerManager)
        {
            if (device == null || handlerManager == null)
                return;

            // Get vendor name via reflection
            var vendorProperty = device.GetType().GetProperty("Vendor");
            var vendorName = vendorProperty?.GetValue(device) as string;
            if (string.IsNullOrEmpty(vendorName))
                return;

            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return;

            foreach (var handlerDesc in vendor.CliHandlers.Where(h => h.IsEnabled))
            {
                try
                {
                    var handler = CreateInstance<object>(
                        handlerDesc.ImplementationClass, 
                        handlerDesc.AssemblyName);
                    
                    if (handler != null)
                    {
                        // Call RegisterHandler via reflection
                        var registerMethod = handlerManager.GetType().GetMethod("RegisterHandler");
                        registerMethod?.Invoke(handlerManager, new[] { handler });
                    }
                }
                catch
                {
                    // Skip handlers that fail to create
                }
            }
        }

        public void InitializeDeviceProtocols(object device)
        {
            if (device == null)
                return;

            // Get vendor name via reflection
            var vendorProperty = device.GetType().GetProperty("Vendor");
            var vendorName = vendorProperty?.GetValue(device) as string;
            if (string.IsNullOrEmpty(vendorName))
                return;

            var vendor = _vendorRegistry.GetVendor(vendorName);
            if (vendor == null)
                return;

            foreach (var protocolDesc in vendor.SupportedProtocols.Where(p => p.IsEnabled))
            {
                try
                {
                    var protocol = CreateInstance<object>(
                        protocolDesc.ImplementationClass, 
                        protocolDesc.AssemblyName);
                    
                    if (protocol != null)
                    {
                        // Initialize protocol via reflection
                        var initMethod = protocol.GetType().GetMethod("Initialize");
                        initMethod?.Invoke(protocol, new[] { device });
                    }
                }
                catch
                {
                    // Skip protocols that fail to initialize
                }
            }
        }

        private T? CreateInstance<T>(string typeName, string assemblyName) where T : class
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            try
            {
                Type? type = null;

                // First try to find the type in already loaded assemblies
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    Assembly assembly = GetOrLoadAssembly(assemblyName);
                    if (assembly != null)
                    {
                        type = assembly.GetType(typeName);
                    }
                }

                // If not found, search all loaded assemblies
                if (type == null)
                {
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType(typeName))
                        .FirstOrDefault(t => t != null);
                }

                if (type == null)
                    return null;

                // Create instance using parameterless constructor
                var instance = Activator.CreateInstance(type);
                return instance as T;
            }
            catch
            {
                return null;
            }
        }

        private Assembly? GetOrLoadAssembly(string assemblyName)
        {
            lock (_lock)
            {
                if (_loadedAssemblies.TryGetValue(assemblyName, out var assembly))
                    return assembly;

                try
                {
                    // Try to load the assembly
                    assembly = Assembly.Load(assemblyName);
                    _loadedAssemblies[assemblyName] = assembly;
                    return assembly;
                }
                catch
                {
                    try
                    {
                        // Try loading from current domain
                        assembly = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == assemblyName);
                        
                        if (assembly != null)
                            _loadedAssemblies[assemblyName] = assembly;
                        
                        return assembly;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}