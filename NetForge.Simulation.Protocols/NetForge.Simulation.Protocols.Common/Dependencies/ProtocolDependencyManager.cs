using NetForge.Simulation.Common.Interfaces;
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
    public class ProtocolDependency(ProtocolType requiredProtocol, DependencyType type, string reason)
    {
        /// <summary>
        /// The protocol type that is depended upon
        /// </summary>
        public ProtocolType RequiredProtocol { get; set; } = requiredProtocol;

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
        public List<ProtocolType> MissingRequired { get; set; } = new();
        public List<ProtocolType> MissingOptional { get; set; } = new();
        public List<ProtocolType> Conflicts { get; set; } = new();
        public List<ProtocolType> SuggestedAdditions { get; set; } = new();
    }

    /// <summary>
    /// Manages protocol dependency relationships and validation
    /// </summary>
    public interface IProtocolDependencyManager
    {
        // Dependency registration
        void RegisterDependency(ProtocolType protocol, ProtocolDependency dependency);
        void RegisterDependencies(ProtocolType protocol, IEnumerable<ProtocolDependency> dependencies);
        void RemoveDependency(ProtocolType protocol, ProtocolType dependency);

        // Dependency query
        IEnumerable<ProtocolDependency> GetDependencies(ProtocolType protocol);
        IEnumerable<ProtocolDependency> GetDependents(ProtocolType protocol);
        IEnumerable<ProtocolType> GetRequiredProtocols(ProtocolType protocol);
        IEnumerable<ProtocolType> GetOptionalProtocols(ProtocolType protocol);
        IEnumerable<ProtocolType> GetConflictingProtocols(ProtocolType protocol);

        // Dependency validation
        DependencyValidationResult ValidateDependencies(IEnumerable<ProtocolType> activeProtocols);
        DependencyValidationResult ValidateProtocolAddition(IEnumerable<ProtocolType> activeProtocols, ProtocolType newProtocol);
        DependencyValidationResult ValidateProtocolRemoval(IEnumerable<ProtocolType> activeProtocols, ProtocolType protocolToRemove);

        // Dependency resolution
        IEnumerable<ProtocolType> ResolveDependencies(ProtocolType protocol);
        IEnumerable<ProtocolType> GetOptimalProtocolSet(IEnumerable<ProtocolType> requestedProtocols);
        IEnumerable<ProtocolType> GetMinimalProtocolSet(IEnumerable<ProtocolType> requestedProtocols);

        // Dependency graph analysis
        bool HasCircularDependency();
        IEnumerable<List<ProtocolType>> GetDependencyChains(ProtocolType protocol);
        Dictionary<ProtocolType, int> GetProtocolDependencyLevels();

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
        private readonly Dictionary<ProtocolType, List<ProtocolDependency>> _dependencies = new();
        private readonly Dictionary<ProtocolType, List<ProtocolType>> _dependents = new();

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
            RegisterDependency(ProtocolType.OSPF, new ProtocolDependency(
                ProtocolType.ARP, DependencyType.Required, "Required for neighbor MAC address resolution"));

            // BGP often benefits from OSPF or other IGP for next-hop resolution
            RegisterDependency(ProtocolType.BGP, new ProtocolDependency(
                ProtocolType.OSPF, DependencyType.Optional, "Provides IGP routes for next-hop resolution"));

            // Management protocols are generally independent but may conflict with each other on ports
            // SSH and Telnet can coexist but may compete for management access
            RegisterDependency(ProtocolType.SSH, new ProtocolDependency(
                ProtocolType.TELNET, DependencyType.Enhancement, "SSH is more secure than Telnet for management"));

            // CDP is Cisco-specific and conflicts with LLDP in some scenarios
            // Note: In reality they can coexist, but for demonstration purposes
            RegisterDependency(ProtocolType.CDP, new ProtocolDependency(
                ProtocolType.LLDP, DependencyType.Optional, "LLDP provides similar discovery with broader vendor support"));

            // HSRP and VRRP are competing redundancy protocols
            RegisterDependency(ProtocolType.HSRP, new ProtocolDependency(
                ProtocolType.VRRP, DependencyType.Conflict, "HSRP and VRRP provide competing redundancy mechanisms"));

            RegisterDependency(ProtocolType.VRRP, new ProtocolDependency(
                ProtocolType.HSRP, DependencyType.Conflict, "VRRP and HSRP provide competing redundancy mechanisms"));

            // EIGRP is Cisco-specific and may conflict with other routing protocols
            RegisterDependency(ProtocolType.EIGRP, new ProtocolDependency(
                ProtocolType.RIP, DependencyType.Conflict, "EIGRP and RIP may cause routing loops if both active"));

            // STP requires a Layer 2 environment
            RegisterDependency(ProtocolType.STP, new ProtocolDependency(
                ProtocolType.LLDP, DependencyType.Optional, "LLDP helps with topology discovery for STP"));
        }

        /// <summary>
        /// Register a dependency for a protocol
        /// </summary>
        public void RegisterDependency(ProtocolType protocol, ProtocolDependency dependency)
        {
            if (!_dependencies.ContainsKey(protocol))
            {
                _dependencies[protocol] = new List<ProtocolDependency>();
            }

            // Remove existing dependency of same type to same protocol
            _dependencies[protocol].RemoveAll(d => d.RequiredProtocol == dependency.RequiredProtocol);
            _dependencies[protocol].Add(dependency);

            // Update dependents mapping
            if (!_dependents.ContainsKey(dependency.RequiredProtocol))
            {
                _dependents[dependency.RequiredProtocol] = new List<ProtocolType>();
            }

            if (!_dependents[dependency.RequiredProtocol].Contains(protocol))
            {
                _dependents[dependency.RequiredProtocol].Add(protocol);
            }
        }

        /// <summary>
        /// Register multiple dependencies for a protocol
        /// </summary>
        public void RegisterDependencies(ProtocolType protocol, IEnumerable<ProtocolDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                RegisterDependency(protocol, dependency);
            }
        }

        /// <summary>
        /// Remove a dependency relationship
        /// </summary>
        public void RemoveDependency(ProtocolType protocol, ProtocolType dependency)
        {
            if (_dependencies.TryGetValue(protocol, out var deps))
            {
                deps.RemoveAll(d => d.RequiredProtocol == dependency);
            }

            if (_dependents.TryGetValue(dependency, out var dependents))
            {
                dependents.Remove(protocol);
            }
        }

        /// <summary>
        /// Get all dependencies for a protocol
        /// </summary>
        public IEnumerable<ProtocolDependency> GetDependencies(ProtocolType protocol)
        {
            return _dependencies.GetValueOrDefault(protocol, new List<ProtocolDependency>());
        }

        /// <summary>
        /// Get all protocols that depend on the given protocol
        /// </summary>
        public IEnumerable<ProtocolDependency> GetDependents(ProtocolType protocol)
        {
            var result = new List<ProtocolDependency>();

            foreach (var dependent in _dependents.GetValueOrDefault(protocol, new List<ProtocolType>()))
            {
                var deps = GetDependencies(dependent);
                result.AddRange(deps.Where(d => d.RequiredProtocol == protocol));
            }

            return result;
        }

        /// <summary>
        /// Get required protocols for a given protocol
        /// </summary>
        public IEnumerable<ProtocolType> GetRequiredProtocols(ProtocolType protocol)
        {
            return GetDependencies(protocol)
                .Where(d => d.Type == DependencyType.Required)
                .Select(d => d.RequiredProtocol);
        }

        /// <summary>
        /// Get optional protocols for a given protocol
        /// </summary>
        public IEnumerable<ProtocolType> GetOptionalProtocols(ProtocolType protocol)
        {
            return GetDependencies(protocol)
                .Where(d => d.Type == DependencyType.Optional || d.Type == DependencyType.Enhancement)
                .Select(d => d.RequiredProtocol);
        }

        /// <summary>
        /// Get conflicting protocols for a given protocol
        /// </summary>
        public IEnumerable<ProtocolType> GetConflictingProtocols(ProtocolType protocol)
        {
            return GetDependencies(protocol)
                .Where(d => d.Type == DependencyType.Conflict)
                .Select(d => d.RequiredProtocol);
        }

        /// <summary>
        /// Validate dependencies for a set of active protocols
        /// </summary>
        public DependencyValidationResult ValidateDependencies(IEnumerable<ProtocolType> activeProtocols)
        {
            var result = new DependencyValidationResult { IsValid = true };
            var activeSet = new HashSet<ProtocolType>(activeProtocols);

            foreach (var protocol in activeProtocols)
            {
                var dependencies = GetDependencies(protocol);

                foreach (var dep in dependencies)
                {
                    switch (dep.Type)
                    {
                        case DependencyType.Required:
                            if (!activeSet.Contains(dep.RequiredProtocol))
                            {
                                result.IsValid = false;
                                result.Errors.Add($"{protocol} requires {dep.RequiredProtocol}: {dep.Reason}");
                                result.MissingRequired.Add(dep.RequiredProtocol);
                            }
                            break;

                        case DependencyType.Optional:
                        case DependencyType.Enhancement:
                            if (!activeSet.Contains(dep.RequiredProtocol))
                            {
                                result.Warnings.Add($"{protocol} would benefit from {dep.RequiredProtocol}: {dep.Reason}");
                                result.MissingOptional.Add(dep.RequiredProtocol);
                            }
                            break;

                        case DependencyType.Conflict:
                            if (activeSet.Contains(dep.RequiredProtocol))
                            {
                                result.IsValid = false;
                                result.Errors.Add($"{protocol} conflicts with {dep.RequiredProtocol}: {dep.Reason}");
                                result.Conflicts.Add(dep.RequiredProtocol);
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
        public DependencyValidationResult ValidateProtocolAddition(IEnumerable<ProtocolType> activeProtocols, ProtocolType newProtocol)
        {
            var newSet = new List<ProtocolType>(activeProtocols) { newProtocol };
            return ValidateDependencies(newSet);
        }

        /// <summary>
        /// Validate removing a protocol from existing set
        /// </summary>
        public DependencyValidationResult ValidateProtocolRemoval(IEnumerable<ProtocolType> activeProtocols, ProtocolType protocolToRemove)
        {
            var result = new DependencyValidationResult { IsValid = true };
            var activeSet = new HashSet<ProtocolType>(activeProtocols);

            // Check if any remaining protocols depend on the one being removed
            foreach (var protocol in activeProtocols.Where(p => p != protocolToRemove))
            {
                var requiredProtocols = GetRequiredProtocols(protocol);
                if (requiredProtocols.Contains(protocolToRemove))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Cannot remove {protocolToRemove}: {protocol} depends on it");
                }
            }

            return result;
        }

        /// <summary>
        /// Resolve all dependencies for a protocol recursively
        /// </summary>
        public IEnumerable<ProtocolType> ResolveDependencies(ProtocolType protocol)
        {
            var resolved = new HashSet<ProtocolType>();
            var visiting = new HashSet<ProtocolType>();

            ResolveDependenciesRecursive(protocol, resolved, visiting);
            return resolved;
        }

        private void ResolveDependenciesRecursive(ProtocolType protocol, HashSet<ProtocolType> resolved, HashSet<ProtocolType> visiting)
        {
            if (resolved.Contains(protocol))
                return;

            if (visiting.Contains(protocol))
                throw new InvalidOperationException($"Circular dependency detected involving {protocol}");

            visiting.Add(protocol);

            var requiredDeps = GetRequiredProtocols(protocol);
            foreach (var dep in requiredDeps)
            {
                ResolveDependenciesRecursive(dep, resolved, visiting);
            }

            visiting.Remove(protocol);
            resolved.Add(protocol);
        }

        /// <summary>
        /// Get optimal protocol set including optional enhancements
        /// </summary>
        public IEnumerable<ProtocolType> GetOptimalProtocolSet(IEnumerable<ProtocolType> requestedProtocols)
        {
            var result = new HashSet<ProtocolType>();

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
        public IEnumerable<ProtocolType> GetMinimalProtocolSet(IEnumerable<ProtocolType> requestedProtocols)
        {
            var result = new HashSet<ProtocolType>();

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
            var visited = new HashSet<ProtocolType>();
            var visiting = new HashSet<ProtocolType>();

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

        private bool HasCircularDependencyRecursive(ProtocolType protocol, HashSet<ProtocolType> visited, HashSet<ProtocolType> visiting)
        {
            if (visiting.Contains(protocol))
                return true;

            if (visited.Contains(protocol))
                return false;

            visiting.Add(protocol);

            var requiredDeps = GetRequiredProtocols(protocol);
            foreach (var dep in requiredDeps)
            {
                if (HasCircularDependencyRecursive(dep, visited, visiting))
                {
                    return true;
                }
            }

            visiting.Remove(protocol);
            visited.Add(protocol);
            return false;
        }

        /// <summary>
        /// Get dependency chains for a protocol
        /// </summary>
        public IEnumerable<List<ProtocolType>> GetDependencyChains(ProtocolType protocol)
        {
            var chains = new List<List<ProtocolType>>();
            var currentChain = new List<ProtocolType>();

            GetDependencyChainsRecursive(protocol, currentChain, chains, new HashSet<ProtocolType>());
            return chains;
        }

        private void GetDependencyChainsRecursive(ProtocolType protocol, List<ProtocolType> currentChain,
            List<List<ProtocolType>> chains, HashSet<ProtocolType> visited)
        {
            if (visited.Contains(protocol))
                return;

            visited.Add(protocol);
            currentChain.Add(protocol);

            var requiredDeps = GetRequiredProtocols(protocol);
            if (!requiredDeps.Any())
            {
                // End of chain
                chains.Add(new List<ProtocolType>(currentChain));
            }
            else
            {
                foreach (var dep in requiredDeps)
                {
                    GetDependencyChainsRecursive(dep, currentChain, chains, new HashSet<ProtocolType>(visited));
                }
            }

            currentChain.Remove(protocol);
        }

        /// <summary>
        /// Get protocol dependency levels (0 = no dependencies, higher = more dependent)
        /// </summary>
        public Dictionary<ProtocolType, int> GetProtocolDependencyLevels()
        {
            var levels = new Dictionary<ProtocolType, int>();
            var processed = new HashSet<ProtocolType>();

            foreach (var protocol in _dependencies.Keys)
            {
                CalculateDependencyLevel(protocol, levels, processed);
            }

            return levels;
        }

        private int CalculateDependencyLevel(ProtocolType protocol, Dictionary<ProtocolType, int> levels, HashSet<ProtocolType> processed)
        {
            if (levels.ContainsKey(protocol))
                return levels[protocol];

            if (processed.Contains(protocol))
                return 0; // Circular dependency - return safe value

            processed.Add(protocol);

            var requiredDeps = GetRequiredProtocols(protocol);
            if (!requiredDeps.Any())
            {
                levels[protocol] = 0;
            }
            else
            {
                var maxDepLevel = requiredDeps.Max(dep => CalculateDependencyLevel(dep, levels, processed));
                levels[protocol] = maxDepLevel + 1;
            }

            processed.Remove(protocol);
            return levels[protocol];
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
                ["TotalProtocols"] = Enum.GetValues<ProtocolType>().Length,
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
