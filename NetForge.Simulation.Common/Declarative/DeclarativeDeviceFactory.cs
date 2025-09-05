using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Common.Declarative
{
    /// <summary>
    /// Factory for creating actual device instances from declarative specifications
    /// Important: Handlers know about protocols, but protocols DO NOT know about handlers
    /// </summary>
    public class DeclarativeDeviceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IVendorRegistry _vendorRegistry;
        private readonly IVendorService _vendorService;

        public DeclarativeDeviceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            _vendorService = serviceProvider.GetRequiredService<IVendorService>();
        }

        /// <summary>
        /// Create a network device from a declarative specification
        /// Follows the separation principle: protocols are independent, handlers know about protocols
        /// </summary>
        public async Task<INetworkDevice> CreateDeviceAsync(DeviceSpec spec)
        {
            spec.Validate();

            // Create the base device using vendor-agnostic approach
            var device = CreateBaseDevice(spec);

            // Step 1: Configure physical characteristics
            ConfigurePhysicalCharacteristics(device, spec.Physical);

            // Step 2: Configure interfaces
            ConfigureInterfaces(device, spec.Interfaces);

            // Step 3: Configure protocols (protocols don't know about handlers)
            await ConfigureProtocolsAsync(device, spec.Protocols);

            // Step 4: Configure CLI handlers (handlers can access protocols)
            ConfigureCliHandlers(device, spec.Management);

            // Step 5: Configure SNMP handlers
            ConfigureSnmpHandlers(device, spec.Snmp);

            // Step 6: Configure HTTP handlers
            ConfigureHttpHandlers(device, spec.Http);

            // Step 7: Load initial NVRAM configuration
            await LoadInitialConfigurationAsync(device, spec.InitialConfig);

            return device;
        }

        private INetworkDevice CreateBaseDevice(DeviceSpec spec)
        {
            // Create a generic NetworkDevice instance
            // We'll use reflection or a factory approach to avoid vendor-specific classes
            var deviceType = GetVendorDeviceType(spec.Vendor);
            
            if (deviceType != null)
            {
                // Create vendor-specific device if available
                return (INetworkDevice)Activator.CreateInstance(deviceType, spec.Name)!;
            }
            else
            {
                // Fall back to generic device
                return new GenericNetworkDevice(spec.Name, spec.Vendor.Vendor, spec.Vendor.Model);
            }
        }

        private Type? GetVendorDeviceType(VendorSpec vendor)
        {
            // Map vendors to device types - this could be configuration-driven
            return vendor.Vendor.ToLowerInvariant() switch
            {
                "cisco" => Type.GetType("NetForge.Simulation.Devices.CiscoDevice, NetForge.Simulation.Topology"),
                "juniper" => Type.GetType("NetForge.Simulation.Devices.JuniperDevice, NetForge.Simulation.Topology"),
                "arista" => Type.GetType("NetForge.Simulation.Devices.AristaDevice, NetForge.Simulation.Topology"),
                _ => null
            };
        }

        private void ConfigurePhysicalCharacteristics(INetworkDevice device, PhysicalSpec physical)
        {
            // Set physical characteristics using reflection or direct interface calls
            // This would typically be stored in device properties or a physical info object
            if (device is NetworkDevice baseDevice)
            {
                baseDevice.SetPhysicalSpecs(physical.MemoryMB, physical.StorageMB, 
                    physical.Cpu, physical.PowerConsumptionWatts);
            }
        }

        private void ConfigureInterfaces(INetworkDevice device, List<InterfaceSpec> interfaces)
        {
            // Clear default interfaces and add specified ones
            if (device is NetworkDevice baseDevice)
            {
                baseDevice.ClearInterfaces();
            }

            foreach (var ifaceSpec in interfaces)
            {
                var iface = new InterfaceConfig(ifaceSpec.Name, device)
                {
                    Description = ifaceSpec.Description,
                    IsShutdown = ifaceSpec.InitiallyShutdown,
                    Speed = ifaceSpec.SpeedMbps
                };

                // Configure VLAN settings for switched interfaces
                if (ifaceSpec.VlanConfig != null)
                {
                    iface.SwitchportMode = ifaceSpec.VlanConfig.Mode.ToString();
                    if (ifaceSpec.VlanConfig.AccessVlan.HasValue)
                        iface.VlanId = ifaceSpec.VlanConfig.AccessVlan.Value;
                }

                // Add interface to device
                if (device is NetworkDevice networkDevice)
                {
                    networkDevice.AddInterface(iface);
                }
            }
        }

        private async Task ConfigureProtocolsAsync(INetworkDevice device, List<ProtocolSpec> protocols)
        {
            foreach (var protocolSpec in protocols)
            {
                if (!protocolSpec.Enabled)
                    continue;

                // Create protocol instance using vendor service
                var protocol = _vendorService.CreateProtocol(device.Vendor, protocolSpec.Protocol);
                
                if (protocol != null)
                {
                    // Configure protocol with specified parameters
                    ConfigureProtocolInstance(protocol, protocolSpec);

                    // Register protocol with device
                    device.RegisterProtocol(protocol as IDeviceProtocol ?? 
                        new ProtocolWrapper(protocol, protocolSpec.Protocol));
                }
                else
                {
                    // Create generic protocol if vendor-specific not available
                    var genericProtocol = CreateGenericProtocol(protocolSpec);
                    if (genericProtocol != null)
                    {
                        device.RegisterProtocol(genericProtocol);
                    }
                }

                // Set protocol configurations on device
                await SetDeviceProtocolConfigurationAsync(device, protocolSpec);
            }
        }

        private void ConfigureProtocolInstance(object protocol, ProtocolSpec spec)
        {
            // Use reflection to set configuration properties
            var protocolType = protocol.GetType();
            
            foreach (var (key, value) in spec.Configuration)
            {
                var property = protocolType.GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(protocol, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        // Log configuration error but continue
                        Console.WriteLine($"Failed to set {key} on {protocol.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        private IDeviceProtocol? CreateGenericProtocol(ProtocolSpec spec)
        {
            // Create generic protocol implementations
            return spec.Protocol switch
            {
                NetworkProtocolType.OSPF => new GenericOspfProtocol(spec.Configuration),
                NetworkProtocolType.BGP => new GenericBgpProtocol(spec.Configuration),
                NetworkProtocolType.RIP => new GenericRipProtocol(spec.Configuration),
                NetworkProtocolType.EIGRP => new GenericEigrpProtocol(spec.Configuration),
                _ => null
            };
        }

        private async Task SetDeviceProtocolConfigurationAsync(INetworkDevice device, ProtocolSpec spec)
        {
            // Set protocol configurations on the device based on protocol type
            switch (spec.Protocol)
            {
                case NetworkProtocolType.OSPF:
                    if (spec.Configuration.TryGetValue("ProcessId", out var processId))
                    {
                        var ospfConfig = new OspfConfig((int)processId)
                        {
                            IsEnabled = spec.Enabled
                        };
                        device.SetOspfConfiguration(ospfConfig);
                    }
                    break;

                case NetworkProtocolType.BGP:
                    if (spec.Configuration.TryGetValue("ASNumber", out var asNumber))
                    {
                        var bgpConfig = new BgpConfig((int)asNumber)
                        {
                            IsEnabled = spec.Enabled
                        };
                        device.SetBgpConfiguration(bgpConfig);
                    }
                    break;

                case NetworkProtocolType.RIP:
                    var ripConfig = new RipConfig
                    {
                        IsEnabled = spec.Enabled
                    };
                    device.SetRipConfiguration(ripConfig);
                    break;

                case NetworkProtocolType.EIGRP:
                    if (spec.Configuration.TryGetValue("ASNumber", out var eigrpAs))
                    {
                        var eigrpConfig = new EigrpConfig((int)eigrpAs)
                        {
                            IsEnabled = spec.Enabled
                        };
                        device.SetEigrpConfiguration(eigrpConfig);
                    }
                    break;
            }

            await Task.CompletedTask;
        }

        private void ConfigureCliHandlers(INetworkDevice device, List<ManagementProtocolSpec> management)
        {
            // Configure CLI handlers using the vendor system
            // Note: Handlers can access and interact with protocols, but protocols don't know about handlers
            
            foreach (var mgmtSpec in management)
            {
                if (!mgmtSpec.Enabled)
                    continue;

                // Create vendor-specific CLI handlers
                var handlers = _vendorService.GetVendorHandlers(device.Vendor);
                
                foreach (var handler in handlers)
                {
                    // CLI handlers can access protocol information from the device
                    if (handler is IProtocolAwareHandler protocolAwareHandler)
                    {
                        protocolAwareHandler.SetDevice(device);
                    }
                }

                // Configure management protocol settings
                ConfigureManagementProtocol(device, mgmtSpec);
            }
        }

        private void ConfigureManagementProtocol(INetworkDevice device, ManagementProtocolSpec spec)
        {
            switch (spec.Protocol)
            {
                case ManagementProtocolType.Telnet:
                    var telnetConfig = new
                    {
                        Enabled = spec.Enabled,
                        Port = spec.Port ?? 23,
                        Configuration = spec.Configuration
                    };
                    device.SetTelnetConfiguration(telnetConfig);
                    break;

                case ManagementProtocolType.SSH:
                    var sshConfig = new
                    {
                        Enabled = spec.Enabled,
                        Port = spec.Port ?? 22,
                        Configuration = spec.Configuration
                    };
                    device.SetSshConfiguration(sshConfig);
                    break;
            }
        }

        private void ConfigureSnmpHandlers(INetworkDevice device, SnmpSpec? snmp)
        {
            if (snmp?.Enabled != true)
                return;

            var snmpConfig = new
            {
                Enabled = snmp.Enabled,
                Version = snmp.Version.ToString(),
                Port = snmp.Port,
                Communities = snmp.Communities
            };

            device.SetSnmpConfiguration(snmpConfig);
        }

        private void ConfigureHttpHandlers(INetworkDevice device, HttpSpec? http)
        {
            if (http?.HttpEnabled != true && http?.HttpsEnabled != true)
                return;

            var httpConfig = new
            {
                HttpEnabled = http?.HttpEnabled ?? false,
                HttpsEnabled = http?.HttpsEnabled ?? true,
                HttpPort = http?.HttpPort ?? 80,
                HttpsPort = http?.HttpsPort ?? 443
            };

            device.SetHttpConfiguration(httpConfig);
        }

        private async Task LoadInitialConfigurationAsync(INetworkDevice device, NvramSpec? config)
        {
            if (config == null)
                return;

            // Parse and apply configuration based on format
            var configParser = CreateConfigurationParser(config.Format);
            if (configParser != null)
            {
                await configParser.ApplyConfigurationAsync(device, config.Content, config.LoadAsStartupConfig);
            }
            else
            {
                // Generic configuration loading
                if (device is NetworkDevice networkDevice)
                {
                    networkDevice.LoadConfiguration(config.Content);
                    device.IsNvramLoaded = true;
                }
            }
        }

        private IConfigurationParser? CreateConfigurationParser(string format)
        {
            return format.ToLowerInvariant() switch
            {
                "ios" => new IosConfigurationParser(),
                "junos" => new JunosConfigurationParser(),
                "eos" => new EosConfigurationParser(),
                _ => null
            };
        }
    }

    /// <summary>
    /// Generic network device for vendor-agnostic device creation
    /// </summary>
    public class GenericNetworkDevice : NetworkDevice
    {
        public GenericNetworkDevice(string name, string vendor, string model) : base(name)
        {
            Vendor = vendor;
            Model = model;
        }

        public string Model { get; }

        protected override void InitializeDefaultInterfaces()
        {
            // No default interfaces - will be configured declaratively
        }
    }

    /// <summary>
    /// Interface for handlers that need access to protocol information
    /// </summary>
    public interface IProtocolAwareHandler
    {
        void SetDevice(INetworkDevice device);
    }

    /// <summary>
    /// Interface for configuration parsers
    /// </summary>
    public interface IConfigurationParser
    {
        Task ApplyConfigurationAsync(INetworkDevice device, string content, bool loadAsStartup);
    }

    // Placeholder configuration parsers
    public class IosConfigurationParser : IConfigurationParser
    {
        public Task ApplyConfigurationAsync(INetworkDevice device, string content, bool loadAsStartup)
        {
            // Parse IOS-style configuration
            return Task.CompletedTask;
        }
    }

    public class JunosConfigurationParser : IConfigurationParser
    {
        public Task ApplyConfigurationAsync(INetworkDevice device, string content, bool loadAsStartup)
        {
            // Parse Junos-style configuration
            return Task.CompletedTask;
        }
    }

    public class EosConfigurationParser : IConfigurationParser
    {
        public Task ApplyConfigurationAsync(INetworkDevice device, string content, bool loadAsStartup)
        {
            // Parse EOS-style configuration
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Wrapper for generic protocol objects to implement IDeviceProtocol
    /// </summary>
    public class ProtocolWrapper : IDeviceProtocol
    {
        private readonly object _protocol;
        private readonly NetworkProtocolType _protocolType;

        public ProtocolWrapper(object protocol, NetworkProtocolType protocolType)
        {
            _protocol = protocol;
            _protocolType = protocolType;
        }

        public NetworkProtocolType ProtocolType => _protocolType;
        public bool IsEnabled { get; set; } = true;
        public INetworkDevice? Device { get; set; }

        public Task InitializeAsync()
        {
            // Call Initialize method on wrapped protocol if it exists
            var initMethod = _protocol.GetType().GetMethod("Initialize") ?? 
                           _protocol.GetType().GetMethod("InitializeAsync");
            
            if (initMethod != null)
            {
                var result = initMethod.Invoke(_protocol, initMethod.GetParameters().Length > 0 ? new object[] { Device } : null);
                if (result is Task task)
                    return task;
            }

            return Task.CompletedTask;
        }

        public Task UpdateStateAsync()
        {
            // Call update method on wrapped protocol if it exists
            var updateMethod = _protocol.GetType().GetMethod("UpdateState") ??
                             _protocol.GetType().GetMethod("UpdateStateAsync");

            if (updateMethod != null)
            {
                var result = updateMethod.Invoke(_protocol, null);
                if (result is Task task)
                    return task;
            }

            return Task.CompletedTask;
        }

        public void SubscribeToEvents()
        {
            // Call subscribe method on wrapped protocol if it exists
            var subscribeMethod = _protocol.GetType().GetMethod("SubscribeToEvents");
            subscribeMethod?.Invoke(_protocol, null);
        }
    }

    // Generic protocol implementations
    public class GenericOspfProtocol : IDeviceProtocol
    {
        public GenericOspfProtocol(Dictionary<string, object> config)
        {
            Configuration = config;
        }

        public NetworkProtocolType ProtocolType => NetworkProtocolType.OSPF;
        public bool IsEnabled { get; set; } = true;
        public INetworkDevice? Device { get; set; }
        public Dictionary<string, object> Configuration { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task UpdateStateAsync() => Task.CompletedTask;
        public void SubscribeToEvents() { }
    }

    public class GenericBgpProtocol : IDeviceProtocol
    {
        public GenericBgpProtocol(Dictionary<string, object> config)
        {
            Configuration = config;
        }

        public NetworkProtocolType ProtocolType => NetworkProtocolType.BGP;
        public bool IsEnabled { get; set; } = true;
        public INetworkDevice? Device { get; set; }
        public Dictionary<string, object> Configuration { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task UpdateStateAsync() => Task.CompletedTask;
        public void SubscribeToEvents() { }
    }

    public class GenericRipProtocol : IDeviceProtocol
    {
        public GenericRipProtocol(Dictionary<string, object> config)
        {
            Configuration = config;
        }

        public NetworkProtocolType ProtocolType => NetworkProtocolType.RIP;
        public bool IsEnabled { get; set; } = true;
        public INetworkDevice? Device { get; set; }
        public Dictionary<string, object> Configuration { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task UpdateStateAsync() => Task.CompletedTask;
        public void SubscribeToEvents() { }
    }

    public class GenericEigrpProtocol : IDeviceProtocol
    {
        public GenericEigrpProtocol(Dictionary<string, object> config)
        {
            Configuration = config;
        }

        public NetworkProtocolType ProtocolType => NetworkProtocolType.EIGRP;
        public bool IsEnabled { get; set; } = true;
        public INetworkDevice? Device { get; set; }
        public Dictionary<string, object> Configuration { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task UpdateStateAsync() => Task.CompletedTask;
        public void SubscribeToEvents() { }
    }
}