using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.Protocols.Common.Configuration;
using NetForge.Simulation.Protocols.Common.Dependencies;
using NetForge.Simulation.Protocols.Common.Services;
using NetForge.Simulation.Protocols.Common.State;
using System.ComponentModel.DataAnnotations;

namespace NetForge.Simulation.Protocols.Examples
{
    /// <summary>
    /// Example OSPF protocol implementation using the enhanced architecture
    /// Demonstrates Layer 3 routing protocol best practices
    /// </summary>
    public class EnhancedOspfProtocol : BaseRoutingProtocol
    {
        public override ProtocolType Type => ProtocolType.OSPF;
        public override string Name => "Enhanced OSPF";
        public override string Version => "2.1.0";
        public override int AdministrativeDistance => 110;
        public override bool SupportsECMP => true;
        public override int MaxECMPPaths => 4;

        // Protocol-specific dependencies
        public override IEnumerable<ProtocolType> GetDependencies()
        {
            return new[]
            {
                ProtocolType.ARP  // Required for neighbor MAC resolution
            };
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" };
        }

        protected override object GetProtocolConfiguration()
        {
            // Note: Type mismatch commented out - OspfConfig vs OspfConfiguration
            // return _device?.GetOspfConfiguration() ?? new OspfConfiguration();
            return new OspfConfiguration();
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is OspfConfiguration ospfConfig)
            {
                // Apply OSPF-specific configuration
                LogProtocolEvent($"Applied OSPF configuration: Router ID {ospfConfig.RouterId}");
            }
        }

        protected override async Task<Dictionary<string, object>> CollectRoutingInformation(NetworkDevice device)
        {
            var routingInfo = new Dictionary<string, object>();

            // Collect LSAs from neighbors
            // Note: GetNeighborIds method commented out - not implemented in base class
            /*
            foreach (var neighborId in GetNeighborIds())
            {
                var neighbor = _state.GetNeighbor<OspfNeighbor>(neighborId);
                if (neighbor?.State == "Full")
                {
                    routingInfo[$"LSA_{neighborId}"] = neighbor.LinkStateAdvertisements;
                }
            }
            */

            LogProtocolEvent($"Collected routing information from {routingInfo.Count} neighbors");
            return routingInfo;
        }

        protected override async Task<List<object>> ComputeRoutes(NetworkDevice device, Dictionary<string, object> routingInformation)
        {
            var routes = new List<object>();

            // Simplified SPF calculation
            foreach (var lsaEntry in routingInformation)
            {
                // In real implementation, this would run Dijkstra's algorithm
                var route = new OspfRoute
                {
                    Destination = $"192.168.{routes.Count + 1}.0/24",
                    NextHop = "192.168.1.1",
                    Cost = 10 + routes.Count,
                    Area = "0.0.0.0"
                };
                routes.Add(route);
            }

            LogProtocolEvent($"Computed {routes.Count} routes using SPF algorithm");
            return routes;
        }

        protected override async Task AdvertiseRoutes(NetworkDevice device, List<object> routes)
        {
            // Send LSAs to neighbors
            // Note: GetNeighborIds method commented out - not implemented in base class
            /*
            foreach (var neighborId in GetNeighborIds())
            {
                var neighbor = _state.GetNeighbor<OspfNeighbor>(neighborId);
                if (neighbor?.State == "Full")
                {
                    // Send route advertisements
                    _metrics.RecordPacketSent();
                }
            }
            */

            LogProtocolEvent($"Advertised {routes.Count} routes to neighbors");
        }

        protected override string GetRouteKey(object route)
        {
            if (route is OspfRoute ospfRoute)
            {
                return $"{ospfRoute.Destination}_{ospfRoute.Area}";
            }
            return route.ToString();
        }

        protected override async Task ProcessRoutingTimers(NetworkDevice device)
        {
            // OSPF Hello timer (10 seconds)
            // OSPF Dead interval (40 seconds)
            // LSA refresh timer (30 minutes)
            
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Example OSPF configuration using the enhanced configuration system
    /// </summary>
    public class OspfConfiguration : BaseProtocolConfiguration
    {
        public override string ProtocolName => "OSPF";

        [Required]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Invalid Router ID format")]
        public string RouterId { get; set; } = "1.1.1.1";

        [Range(1, 65535, ErrorMessage = "Process ID must be between 1 and 65535")]
        public int ProcessId { get; set; } = 1;

        public Dictionary<string, OspfArea> Areas { get; set; } = new();

        [Range(1, 3600, ErrorMessage = "Hello interval must be between 1 and 3600 seconds")]
        public int HelloInterval { get; set; } = 10;

        [Range(1, 3600, ErrorMessage = "Dead interval must be between 1 and 3600 seconds")]
        public int DeadInterval { get; set; } = 40;

        public bool EnableECMP { get; set; } = true;

        [Range(1, 32, ErrorMessage = "Max ECMP paths must be between 1 and 32")]
        public int MaxECMPPaths { get; set; } = 4;

        protected override IEnumerable<string> ValidateCustomRules()
        {
            var errors = new List<string>();

            // Custom validation: Dead interval should be at least 4x hello interval
            if (DeadInterval < HelloInterval * 4)
            {
                errors.Add("Dead interval should be at least 4 times the hello interval");
            }

            // Validate areas
            if (!Areas.ContainsKey("0.0.0.0"))
            {
                errors.Add("OSPF configuration must include backbone area 0.0.0.0");
            }

            // Validate router ID is not multicast or reserved
            if (RouterId.StartsWith("224.") || RouterId.StartsWith("239."))
            {
                errors.Add("Router ID cannot be a multicast address");
            }

            return errors;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            dict["RouterId"] = RouterId;
            dict["ProcessId"] = ProcessId;
            dict["Areas"] = Areas;
            dict["HelloInterval"] = HelloInterval;
            dict["DeadInterval"] = DeadInterval;
            dict["EnableECMP"] = EnableECMP;
            dict["MaxECMPPaths"] = MaxECMPPaths;
            return dict;
        }

        public override void FromDictionary(Dictionary<string, object> data)
        {
            base.FromDictionary(data);

            if (data.TryGetValue("RouterId", out var routerId))
                RouterId = routerId?.ToString() ?? "1.1.1.1";

            if (data.TryGetValue("ProcessId", out var processId))
                ProcessId = Convert.ToInt32(processId);

            if (data.TryGetValue("HelloInterval", out var helloInterval))
                HelloInterval = Convert.ToInt32(helloInterval);

            if (data.TryGetValue("DeadInterval", out var deadInterval))
                DeadInterval = Convert.ToInt32(deadInterval);

            if (data.TryGetValue("EnableECMP", out var enableECMP))
                EnableECMP = Convert.ToBoolean(enableECMP);

            if (data.TryGetValue("MaxECMPPaths", out var maxECMPPaths))
                MaxECMPPaths = Convert.ToInt32(maxECMPPaths);
        }
    }

    /// <summary>
    /// OSPF-specific data structures
    /// </summary>
    public class OspfArea
    {
        public string AreaId { get; set; }
        public string AreaType { get; set; } = "standard"; // standard, stub, nssa, totally-stub
        public List<string> Networks { get; set; } = new();
    }

    public class OspfNeighbor
    {
        public string NeighborId { get; set; }
        public string IpAddress { get; set; }
        public string State { get; set; } = "Down"; // Down, Init, 2-Way, ExStart, Exchange, Loading, Full
        public string Interface { get; set; }
        public int Priority { get; set; } = 1;
        public DateTime StateTime { get; set; } = DateTime.Now;
        public List<object> LinkStateAdvertisements { get; set; } = new();
    }

    public class OspfRoute
    {
        public string Destination { get; set; }
        public string NextHop { get; set; }
        public int Cost { get; set; }
        public string Area { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Example usage demonstrating the enhanced protocol architecture
    /// </summary>
    public class ProtocolArchitectureDemo
    {
        private readonly NetForge.Simulation.Protocols.Common.Services.IProtocolService _protocolService;
        private readonly IProtocolConfigurationManager _configManager;
        private readonly IProtocolDependencyManager _dependencyManager;

        public ProtocolArchitectureDemo()
        {
            _configManager = new ProtocolConfigurationManager();
            _dependencyManager = new ProtocolDependencyManager();
            
            // In real implementation, this would be injected
            _protocolService = null; // Would be implemented in Phase 1 completion
        }

        /// <summary>
        /// Demonstrate protocol configuration with validation
        /// </summary>
        public async Task DemonstrateConfigurationManagement()
        {
            // Create OSPF configuration
            var ospfConfig = new OspfConfiguration
            {
                RouterId = "10.1.1.1",
                ProcessId = 100,
                HelloInterval = 10,
                DeadInterval = 40,
                EnableECMP = true,
                MaxECMPPaths = 4
            };

            // Add backbone area
            ospfConfig.Areas["0.0.0.0"] = new OspfArea
            {
                AreaId = "0.0.0.0",
                AreaType = "standard",
                Networks = new List<string> { "10.1.1.0/24", "10.1.2.0/24" }
            };

            // Validate configuration
            if (_configManager.ValidateConfiguration(ospfConfig))
            {
                Console.WriteLine("✓ OSPF configuration is valid");
                
                // Apply configuration
                var success = await _configManager.ApplyConfiguration(ProtocolType.OSPF, ospfConfig);
                if (success)
                {
                    Console.WriteLine("✓ OSPF configuration applied successfully");
                }
            }
            else
            {
                var errors = _configManager.GetConfigurationErrors(ospfConfig);
                Console.WriteLine("✗ OSPF configuration errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            // Create and save template
            await _configManager.SaveConfigurationTemplate(
                ProtocolType.OSPF, "BasicBackboneArea", ospfConfig);
            
            // Backup configuration
            var backup = await _configManager.BackupConfiguration(ProtocolType.OSPF);
            Console.WriteLine($"Configuration backed up: {backup?.Length} characters");
        }

        /// <summary>
        /// Demonstrate protocol dependency management
        /// </summary>
        public void DemonstrateDependencyManagement()
        {
            // Check dependencies for OSPF
            var ospfDependencies = _dependencyManager.GetDependencies(ProtocolType.OSPF);
            Console.WriteLine("OSPF Dependencies:");
            foreach (var dep in ospfDependencies)
            {
                Console.WriteLine($"  {dep.Type}: {dep.RequiredProtocol} - {dep.Reason}");
            }

            // Validate adding BGP to a network with OSPF and ARP
            var activeProtocols = new[] { ProtocolType.OSPF, ProtocolType.ARP };
            var validationResult = _dependencyManager.ValidateProtocolAddition(activeProtocols, ProtocolType.BGP);
            
            Console.WriteLine($"Adding BGP: {(validationResult.IsValid ? "✓ Valid" : "✗ Invalid")}");
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"  Error: {error}");
            }
            foreach (var warning in validationResult.Warnings)
            {
                Console.WriteLine($"  Warning: {warning}");
            }

            // Get optimal protocol set for a routing scenario
            var requestedProtocols = new[] { ProtocolType.OSPF, ProtocolType.BGP };
            var optimalSet = _dependencyManager.GetOptimalProtocolSet(requestedProtocols);
            Console.WriteLine($"Optimal protocol set: {string.Join(", ", optimalSet)}");

            // Check for circular dependencies
            var hasCircular = _dependencyManager.HasCircularDependency();
            Console.WriteLine($"Circular dependencies: {(hasCircular ? "Found" : "None")}");

            // Get dependency statistics
            var stats = _dependencyManager.GetDependencyStatistics();
            Console.WriteLine("Dependency Statistics:");
            foreach (var stat in stats)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value}");
            }
        }

        /// <summary>
        /// Demonstrate layered protocol architecture
        /// Note: This example is commented out to avoid circular dependency with Core project
        /// </summary>
        /*
        public async Task DemonstrateLayeredArchitecture()
        {
            var device = new CiscoDevice("Router1");
            
            // Layer 3 Routing Protocols
            var ospfProtocol = new EnhancedOspfProtocol();
            device.RegisterProtocol(ospfProtocol);

            // Configure and initialize
            var ospfConfig = new OspfConfiguration
            {
                RouterId = "10.1.1.1",
                ProcessId = 100
            };

            await _configManager.ApplyConfiguration(ProtocolType.OSPF, ospfConfig);
            
            // Monitor protocol state
            var routingState = ospfProtocol.GetRoutingState();
            Console.WriteLine($"OSPF State: {routingState.Status}");
            Console.WriteLine($"Routes: {routingState.RouteCount}");
            Console.WriteLine($"Neighbors: {routingState.NeighborCount}");

            // Monitor performance metrics
            var metrics = ospfProtocol.GetMetrics();
            var summary = metrics.GetMetricsSummary();
            Console.WriteLine("OSPF Performance Metrics:");
            foreach (var metric in summary)
            {
                Console.WriteLine($"  {metric.Key}: {metric.Value}");
            }

            // Simulate protocol operation
            await ospfProtocol.UpdateState(device);
            
            Console.WriteLine($"Performance Score: {((ProtocolMetrics)metrics).GetPerformanceScore():F1}");
        }
        */

        /// <summary>
        /// Demonstrate protocol health monitoring
        /// </summary>
        public void DemonstrateHealthMonitoring()
        {
            // This would integrate with the IProtocolService when implemented
            Console.WriteLine("Protocol Health Monitoring:");
            Console.WriteLine("- All protocols: Active");
            Console.WriteLine("- Dependencies: Satisfied");
            Console.WriteLine("- Performance: Optimal");
            Console.WriteLine("- Conflicts: None detected");
        }
    }

    /// <summary>
    /// Example Cisco device extension using enhanced protocols
    /// Note: This example is commented out to avoid circular dependency with Core project
    /// </summary>
    /*
    public class EnhancedCiscoDevice : CiscoDevice
    {
        private readonly IProtocolDependencyManager _dependencyManager;
        private readonly IProtocolConfigurationManager _configManager;

        public EnhancedCiscoDevice(string name) : base(name)
        {
            _dependencyManager = new ProtocolDependencyManager();
            _configManager = new ProtocolConfigurationManager();
            
            // Register Cisco-specific validation rules
            RegisterCiscoValidationRules();
        }

        private void RegisterCiscoValidationRules()
        {
            // Cisco-specific OSPF validation
            _configManager.RegisterValidationRule<OspfConfiguration>(ProtocolType.OSPF,
                config => config.ProcessId <= 65535,
                "Cisco OSPF process ID must be <= 65535");

            // Cisco-specific dependencies
            _dependencyManager.RegisterDependency(ProtocolType.EIGRP, 
                new ProtocolDependency(ProtocolType.CDP, DependencyType.Enhancement, 
                "CDP enhances EIGRP neighbor discovery on Cisco devices"));
        }

        public async Task<bool> ConfigureProtocolWithValidation<T>(ProtocolType type, T configuration) where T : class
        {
            // Validate dependencies first
            var activeProtocols = GetRegisteredProtocols().Select(p => p.Type);
            var dependencyResult = _dependencyManager.ValidateProtocolAddition(activeProtocols, type);
            
            if (!dependencyResult.IsValid)
            {
                AddLogEntry($"Cannot configure {type}: Dependency validation failed");
                return false;
            }

            // Validate configuration
            if (!_configManager.ValidateConfiguration(configuration))
            {
                var errors = _configManager.GetConfigurationErrors(configuration);
                AddLogEntry($"Configuration validation failed for {type}: {string.Join(", ", errors)}");
                return false;
            }

            // Apply configuration
            var success = await _configManager.ApplyConfiguration(type, configuration);
            if (success)
            {
                AddLogEntry($"Successfully configured {type}");
            }

            return success;
        }
    }
    */
}