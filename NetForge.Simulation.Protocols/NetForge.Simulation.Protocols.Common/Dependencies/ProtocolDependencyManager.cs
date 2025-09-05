using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Dependencies
{
    /// <summary>
    /// Types of protocol dependencies
    /// </summary>
    public enum DependencyType
    {
        Required,    // Protocol cannot function without this dependency
        Optional,    // Protocol can function but with reduced capabilities
        Enhancement, // Protocol gains additional features with this dependency
        Conflict     // Protocol cannot coexist with this dependency
    }

    /// <summary>
    /// Represents a dependency relationship between protocols
    /// </summary>
    public class ProtocolDependency(NetworkProtocolType requiredNetworkProtocol, DependencyType type, string reason)
    {
        /// <summary>
        /// The protocol type that is depended upon
        /// </summary>
        public NetworkProtocolType RequiredNetworkProtocol { get; set; } = requiredNetworkProtocol;

        /// <summary>
        /// Type of dependency relationship
        /// </summary>
        public DependencyType Type { get; set; } = type;

        /// <summary>
        /// Human-readable reason for the dependency
        /// </summary>
        public string Reason { get; set; } = reason;

        /// <summary>
        /// Minimum version required (if applicable)
        /// </summary>
        public string MinimumVersion { get; set; }

        /// <summary>
        /// Whether this dependency can be dynamically resolved
        /// </summary>
        public bool IsDynamic { get; set; } = false;

        /// <summary>
        /// Priority level for dependency resolution
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Result of dependency validation
    /// </summary>
    public class DependencyValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<NetworkProtocolType> MissingRequired { get; set; } = new();
        public List<NetworkProtocolType> MissingOptional { get; set; } = new();
        public List<NetworkProtocolType> Conflicts { get; set; } = new();
        public List<NetworkProtocolType> SuggestedAdditions { get; set; } = new();
    }

    /// <summary>
    /// Manages protocol dependency relationships and validation
    /// </summary>
    public interface IProtocolDependencyManager
    {
        // Dependency registration
        void RegisterDependency(NetworkProtocolType networkProtocol, ProtocolDependency dependency);
        void RegisterDependencies(NetworkProtocolType networkProtocol, IEnumerable<ProtocolDependency> dependencies);
        void RemoveDependency(NetworkProtocolType networkProtocol, NetworkProtocolType dependency);

        // Dependency query
        IEnumerable<ProtocolDependency> GetDependencies(NetworkProtocolType networkProtocol);
        IEnumerable<ProtocolDependency> GetDependents(NetworkProtocolType networkProtocol);
        IEnumerable<NetworkProtocolType> GetRequiredProtocols(NetworkProtocolType networkProtocol);
        IEnumerable<NetworkProtocolType> GetOptionalProtocols(NetworkProtocolType networkProtocol);
        IEnumerable<NetworkProtocolType> GetConflictingProtocols(NetworkProtocolType networkProtocol);

        // Dependency validation
        DependencyValidationResult ValidateDependencies(IEnumerable<NetworkProtocolType> activeProtocols);
        DependencyValidationResult ValidateProtocolAddition(IEnumerable<NetworkProtocolType> activeProtocols, NetworkProtocolType newNetworkProtocol);
        DependencyValidationResult ValidateProtocolRemoval(IEnumerable<NetworkProtocolType> activeProtocols, NetworkProtocolType networkProtocolToRemove);

        // Dependency resolution
        IEnumerable<NetworkProtocolType> ResolveDependencies(NetworkProtocolType networkProtocol);
        IEnumerable<NetworkProtocolType> GetOptimalProtocolSet(IEnumerable<NetworkProtocolType> requestedProtocols);
        IEnumerable<NetworkProtocolType> GetMinimalProtocolSet(IEnumerable<NetworkProtocolType> requestedProtocols);

        // Dependency graph analysis
        bool HasCircularDependency();
        IEnumerable<List<NetworkProtocolType>> GetDependencyChains(NetworkProtocolType networkProtocol);
        Dictionary<NetworkProtocolType, int> GetProtocolDependencyLevels();

        // Configuration and utilities
        void LoadDependencyConfiguration(string configurationPath);
        void SaveDependencyConfiguration(string configurationPath);
        Dictionary<string, object> GetDependencyStatistics();
    }

    /// <summary>
    /// Concrete implementation of protocol dependency manager
    /// </summary>
    public class ProtocolDependencyManager : IProtocolDependencyManager
    {
        private readonly Dictionary<NetworkProtocolType, List<ProtocolDependency>> _dependencies = new();
        private readonly Dictionary<NetworkProtocolType, List<NetworkProtocolType>> _dependents = new();

        public ProtocolDependencyManager()
        {
            InitializeDefaultDependencies();
        }

        /// <summary>
        /// Initialize common protocol dependencies
        /// </summary>
        private void InitializeDefaultDependencies()
        {
            // OSPF typically requires ARP for neighbor discovery
            RegisterDependency(NetworkProtocolType.OSPF, new ProtocolDependency(
                NetworkProtocolType.ARP, DependencyType.Required, "Required for neighbor MAC address resolution"));

            // BGP often benefits from OSPF or other IGP for next-hop resolution
            RegisterDependency(NetworkProtocolType.BGP, new ProtocolDependency(
                NetworkProtocolType.OSPF, DependencyType.Optional, "Provides IGP routes for next-hop resolution"));

            // Management protocols are generally independent but may conflict with each other on ports
            // SSH and Telnet can coexist but may compete for management access
            RegisterDependency(NetworkProtocolType.SSH, new ProtocolDependency(
                NetworkProtocolType.TELNET, DependencyType.Enhancement, "SSH is more secure than Telnet for management"));

            // CDP is Cisco-specific and conflicts with LLDP in some scenarios
            // Note: In reality they can coexist, but for demonstration purposes
            RegisterDependency(NetworkProtocolType.CDP, new ProtocolDependency(
                NetworkProtocolType.LLDP, DependencyType.Optional, "LLDP provides similar discovery with broader vendor support"));

            // HSRP and VRRP are competing redundancy protocols
            RegisterDependency(NetworkProtocolType.HSRP, new ProtocolDependency(
                NetworkProtocolType.VRRP, DependencyType.Conflict, "HSRP and VRRP provide competing redundancy mechanisms"));

            RegisterDependency(NetworkProtocolType.VRRP, new ProtocolDependency(
                NetworkProtocolType.HSRP, DependencyType.Conflict, "VRRP and HSRP provide competing redundancy mechanisms"));

            // EIGRP is Cisco-specific and may conflict with other routing protocols
            RegisterDependency(NetworkProtocolType.EIGRP, new ProtocolDependency(
                NetworkProtocolType.RIP, DependencyType.Conflict, "EIGRP and RIP may cause routing loops if both active"));

            // STP requires a Layer 2 environment
            RegisterDependency(NetworkProtocolType.STP, new ProtocolDependency(
                NetworkProtocolType.LLDP, DependencyType.Optional, "LLDP helps with topology discovery for STP"));
        }

        /// <summary>
        /// Register a dependency for a protocol
        /// </summary>
        public void RegisterDependency(NetworkProtocolType networkProtocol, ProtocolDependency dependency)
        {
            if (!_dependencies.ContainsKey(networkProtocol))
            {
                _dependencies[networkProtocol] = new List<ProtocolDependency>();
            }

            // Remove existing dependency of same type to same protocol
            _dependencies[networkProtocol].RemoveAll(d => d.RequiredNetworkProtocol == dependency.RequiredNetworkProtocol);
            _dependencies[networkProtocol].Add(dependency);

            // Update dependents mapping
            if (!_dependents.ContainsKey(dependency.RequiredNetworkProtocol))
            {
                _dependents[dependency.RequiredNetworkProtocol] = new List<NetworkProtocolType>();
            }

            if (!_dependents[dependency.RequiredNetworkProtocol].Contains(networkProtocol))
            {
                _dependents[dependency.RequiredNetworkProtocol].Add(networkProtocol);
            }
        }

        /// <summary>
        /// Register multiple dependencies for a protocol
        /// </summary>
        public void RegisterDependencies(NetworkProtocolType networkProtocol, IEnumerable<ProtocolDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                RegisterDependency(networkProtocol, dependency);
            }
        }

        /// <summary>
        /// Remove a dependency relationship
        /// </summary>
        public void RemoveDependency(NetworkProtocolType networkProtocol, NetworkProtocolType dependency)
        {
            if (_dependencies.TryGetValue(networkProtocol, out var deps))
            {
                deps.RemoveAll(d => d.RequiredNetworkProtocol == dependency);
            }

            if (_dependents.TryGetValue(dependency, out var dependents))
            {
                dependents.Remove(networkProtocol);
            }
        }

        /// <summary>
        /// Get all dependencies for a protocol
        /// </summary>
        public IEnumerable<ProtocolDependency> GetDependencies(NetworkProtocolType networkProtocol)
        {
            return _dependencies.GetValueOrDefault(networkProtocol, new List<ProtocolDependency>());
        }

        /// <summary>
        /// Get all protocols that depend on the given protocol
        /// </summary>
        public IEnumerable<ProtocolDependency> GetDependents(NetworkProtocolType networkProtocol)
        {
            var result = new List<ProtocolDependency>();

            foreach (var dependent in _dependents.GetValueOrDefault(networkProtocol, new List<NetworkProtocolType>()))
            {
                var deps = GetDependencies(dependent);
                result.AddRange(deps.Where(d => d.RequiredNetworkProtocol == networkProtocol));
            }

            return result;
        }

        /// <summary>
        /// Get required protocols for a given protocol
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetRequiredProtocols(NetworkProtocolType networkProtocol)
        {
            return GetDependencies(networkProtocol)
                .Where(d => d.Type == DependencyType.Required)
                .Select(d => d.RequiredNetworkProtocol);
        }

        /// <summary>
        /// Get optional protocols for a given protocol
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetOptionalProtocols(NetworkProtocolType networkProtocol)
        {
            return GetDependencies(networkProtocol)
                .Where(d => d.Type == DependencyType.Optional || d.Type == DependencyType.Enhancement)
                .Select(d => d.RequiredNetworkProtocol);
        }

        /// <summary>
        /// Get conflicting protocols for a given protocol
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetConflictingProtocols(NetworkProtocolType networkProtocol)
        {
            return GetDependencies(networkProtocol)
                .Where(d => d.Type == DependencyType.Conflict)
                .Select(d => d.RequiredNetworkProtocol);
        }

        /// <summary>
        /// Validate dependencies for a set of active protocols
        /// </summary>
        public DependencyValidationResult ValidateDependencies(IEnumerable<NetworkProtocolType> activeProtocols)
        {
            var result = new DependencyValidationResult { IsValid = true };
            var activeSet = new HashSet<NetworkProtocolType>(activeProtocols);

            foreach (var protocol in activeProtocols)
            {
                var dependencies = GetDependencies(protocol);

                foreach (var dep in dependencies)
                {
                    switch (dep.Type)
                    {
                        case DependencyType.Required:
                            if (!activeSet.Contains(dep.RequiredNetworkProtocol))
                            {
                                result.IsValid = false;
                                result.Errors.Add($"{protocol} requires {dep.RequiredNetworkProtocol}: {dep.Reason}");
                                result.MissingRequired.Add(dep.RequiredNetworkProtocol);
                            }
                            break;

                        case DependencyType.Optional:
                        case DependencyType.Enhancement:
                            if (!activeSet.Contains(dep.RequiredNetworkProtocol))
                            {
                                result.Warnings.Add($"{protocol} would benefit from {dep.RequiredNetworkProtocol}: {dep.Reason}");
                                result.MissingOptional.Add(dep.RequiredNetworkProtocol);
                            }
                            break;

                        case DependencyType.Conflict:
                            if (activeSet.Contains(dep.RequiredNetworkProtocol))
                            {
                                result.IsValid = false;
                                result.Errors.Add($"{protocol} conflicts with {dep.RequiredNetworkProtocol}: {dep.Reason}");
                                result.Conflicts.Add(dep.RequiredNetworkProtocol);
                            }
                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validate adding a new protocol to existing set
        /// </summary>
        public DependencyValidationResult ValidateProtocolAddition(IEnumerable<NetworkProtocolType> activeProtocols, NetworkProtocolType newNetworkProtocol)
        {
            var newSet = new List<NetworkProtocolType>(activeProtocols) { newNetworkProtocol };
            return ValidateDependencies(newSet);
        }

        /// <summary>
        /// Validate removing a protocol from existing set
        /// </summary>
        public DependencyValidationResult ValidateProtocolRemoval(IEnumerable<NetworkProtocolType> activeProtocols, NetworkProtocolType networkProtocolToRemove)
        {
            var result = new DependencyValidationResult { IsValid = true };
            var activeSet = new HashSet<NetworkProtocolType>(activeProtocols);

            // Check if any remaining protocols depend on the one being removed
            foreach (var protocol in activeProtocols.Where(p => p != networkProtocolToRemove))
            {
                var requiredProtocols = GetRequiredProtocols(protocol);
                if (requiredProtocols.Contains(networkProtocolToRemove))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Cannot remove {networkProtocolToRemove}: {protocol} depends on it");
                }
            }

            return result;
        }

        /// <summary>
        /// Resolve all dependencies for a protocol recursively
        /// </summary>
        public IEnumerable<NetworkProtocolType> ResolveDependencies(NetworkProtocolType networkProtocol)
        {
            var resolved = new HashSet<NetworkProtocolType>();
            var visiting = new HashSet<NetworkProtocolType>();

            ResolveDependenciesRecursive(networkProtocol, resolved, visiting);
            return resolved;
        }

        private void ResolveDependenciesRecursive(NetworkProtocolType networkProtocol, HashSet<NetworkProtocolType> resolved, HashSet<NetworkProtocolType> visiting)
        {
            if (resolved.Contains(networkProtocol))
                return;

            if (visiting.Contains(networkProtocol))
                throw new InvalidOperationException($"Circular dependency detected involving {networkProtocol}");

            visiting.Add(networkProtocol);

            var requiredDeps = GetRequiredProtocols(networkProtocol);
            foreach (var dep in requiredDeps)
            {
                ResolveDependenciesRecursive(dep, resolved, visiting);
            }

            visiting.Remove(networkProtocol);
            resolved.Add(networkProtocol);
        }

        /// <summary>
        /// Get optimal protocol set including optional enhancements
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetOptimalProtocolSet(IEnumerable<NetworkProtocolType> requestedProtocols)
        {
            var result = new HashSet<NetworkProtocolType>();

            // Add all requested protocols and their required dependencies
            foreach (var protocol in requestedProtocols)
            {
                var dependencies = ResolveDependencies(protocol);
                foreach (var dep in dependencies)
                {
                    result.Add(dep);
                }
            }

            // Add beneficial optional protocols if they don't conflict
            foreach (var protocol in requestedProtocols)
            {
                var optionalDeps = GetOptionalProtocols(protocol);
                foreach (var dep in optionalDeps)
                {
                    var conflicts = GetConflictingProtocols(dep);
                    if (!conflicts.Any(c => result.Contains(c)))
                    {
                        result.Add(dep);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get minimal protocol set with only required dependencies
        /// </summary>
        public IEnumerable<NetworkProtocolType> GetMinimalProtocolSet(IEnumerable<NetworkProtocolType> requestedProtocols)
        {
            var result = new HashSet<NetworkProtocolType>();

            foreach (var protocol in requestedProtocols)
            {
                var dependencies = ResolveDependencies(protocol);
                foreach (var dep in dependencies)
                {
                    result.Add(dep);
                }
            }

            return result;
        }

        /// <summary>
        /// Check for circular dependencies in the dependency graph
        /// </summary>
        public bool HasCircularDependency()
        {
            var visited = new HashSet<NetworkProtocolType>();
            var visiting = new HashSet<NetworkProtocolType>();

            foreach (var protocol in _dependencies.Keys)
            {
                if (!visited.Contains(protocol))
                {
                    if (HasCircularDependencyRecursive(protocol, visited, visiting))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasCircularDependencyRecursive(NetworkProtocolType networkProtocol, HashSet<NetworkProtocolType> visited, HashSet<NetworkProtocolType> visiting)
        {
            if (visiting.Contains(networkProtocol))
                return true;

            if (visited.Contains(networkProtocol))
                return false;

            visiting.Add(networkProtocol);

            var requiredDeps = GetRequiredProtocols(networkProtocol);
            foreach (var dep in requiredDeps)
            {
                if (HasCircularDependencyRecursive(dep, visited, visiting))
                {
                    return true;
                }
            }

            visiting.Remove(networkProtocol);
            visited.Add(networkProtocol);
            return false;
        }

        /// <summary>
        /// Get dependency chains for a protocol
        /// </summary>
        public IEnumerable<List<NetworkProtocolType>> GetDependencyChains(NetworkProtocolType networkProtocol)
        {
            var chains = new List<List<NetworkProtocolType>>();
            var currentChain = new List<NetworkProtocolType>();

            GetDependencyChainsRecursive(networkProtocol, currentChain, chains, new HashSet<NetworkProtocolType>());
            return chains;
        }

        private void GetDependencyChainsRecursive(NetworkProtocolType networkProtocol, List<NetworkProtocolType> currentChain,
            List<List<NetworkProtocolType>> chains, HashSet<NetworkProtocolType> visited)
        {
            if (visited.Contains(networkProtocol))
                return;

            visited.Add(networkProtocol);
            currentChain.Add(networkProtocol);

            var requiredDeps = GetRequiredProtocols(networkProtocol);
            if (!requiredDeps.Any())
            {
                // End of chain
                chains.Add(new List<NetworkProtocolType>(currentChain));
            }
            else
            {
                foreach (var dep in requiredDeps)
                {
                    GetDependencyChainsRecursive(dep, currentChain, chains, new HashSet<NetworkProtocolType>(visited));
                }
            }

            currentChain.Remove(networkProtocol);
        }

        /// <summary>
        /// Get protocol dependency levels (0 = no dependencies, higher = more dependent)
        /// </summary>
        public Dictionary<NetworkProtocolType, int> GetProtocolDependencyLevels()
        {
            var levels = new Dictionary<NetworkProtocolType, int>();
            var processed = new HashSet<NetworkProtocolType>();

            foreach (var protocol in _dependencies.Keys)
            {
                CalculateDependencyLevel(protocol, levels, processed);
            }

            return levels;
        }

        private int CalculateDependencyLevel(NetworkProtocolType networkProtocol, Dictionary<NetworkProtocolType, int> levels, HashSet<NetworkProtocolType> processed)
        {
            if (levels.ContainsKey(networkProtocol))
                return levels[networkProtocol];

            if (processed.Contains(networkProtocol))
                return 0; // Circular dependency - return safe value

            processed.Add(networkProtocol);

            var requiredDeps = GetRequiredProtocols(networkProtocol);
            if (!requiredDeps.Any())
            {
                levels[networkProtocol] = 0;
            }
            else
            {
                var maxDepLevel = requiredDeps.Max(dep => CalculateDependencyLevel(dep, levels, processed));
                levels[networkProtocol] = maxDepLevel + 1;
            }

            processed.Remove(networkProtocol);
            return levels[networkProtocol];
        }

        /// <summary>
        /// Load dependency configuration from file
        /// </summary>
        public void LoadDependencyConfiguration(string configurationPath)
        {
            // Implementation would load from JSON/XML configuration file
            // For now, this is a placeholder
        }

        /// <summary>
        /// Save dependency configuration to file
        /// </summary>
        public void SaveDependencyConfiguration(string configurationPath)
        {
            // Implementation would save to JSON/XML configuration file
            // For now, this is a placeholder
        }

        /// <summary>
        /// Get dependency statistics
        /// </summary>
        public Dictionary<string, object> GetDependencyStatistics()
        {
            var totalDependencies = _dependencies.Values.Sum(deps => deps.Count);
            var protocolsWithDependencies = _dependencies.Count(kvp => kvp.Value.Any());
            var circularDependencies = HasCircularDependency();

            var dependencyTypes = _dependencies.Values
                .SelectMany(deps => deps)
                .GroupBy(dep => dep.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return new Dictionary<string, object>
            {
                ["TotalProtocols"] = Enum.GetValues<NetworkProtocolType>().Length,
                ["ProtocolsWithDependencies"] = protocolsWithDependencies,
                ["TotalDependencies"] = totalDependencies,
                ["HasCircularDependencies"] = circularDependencies,
                ["DependencyTypes"] = dependencyTypes,
                ["AverageDependenciesPerProtocol"] = protocolsWithDependencies > 0 ?
                    (double)totalDependencies / protocolsWithDependencies : 0
            };
        }
    }
}
