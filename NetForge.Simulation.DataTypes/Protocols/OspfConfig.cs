using NetForge.Simulation.DataTypes.Validation;
using NetForge.Simulation.DataTypes.ValueObjects;

namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents an OSPF neighbor with validation
    /// </summary>
    public class OspfNeighbor : IValidatable
    {
        public string NeighborId { get; set; }
        public string IpAddress { get; set; }
        public string State { get; set; } = "INIT";
        public string Interface { get; set; }
        public int Priority { get; set; } = 1;
        public DateTime StateTime { get; set; } = DateTime.Now;

        public OspfNeighbor(string neighborId, string ipAddress, string interfaceName)
        {
            NeighborId = neighborId;
            IpAddress = ipAddress;
            Interface = interfaceName;
        }

        public ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate Neighbor ID (Router ID format)
            if (string.IsNullOrWhiteSpace(NeighborId))
                results.Add(ValidationResult.Error("OSPF Neighbor ID is required"));
            else if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(NeighborId))
                results.Add(ValidationResult.Error($"OSPF Neighbor ID must be in Router ID format (got '{NeighborId}')"));

            // Validate IP address
            if (string.IsNullOrWhiteSpace(IpAddress))
                results.Add(ValidationResult.Error("OSPF Neighbor IP address is required"));
            else if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(IpAddress))
                results.Add(ValidationResult.Error($"OSPF Neighbor IP address must be valid (got '{IpAddress}')"));

            // Validate interface name
            if (string.IsNullOrWhiteSpace(Interface))
                results.Add(ValidationResult.Error("OSPF Neighbor interface is required"));

            // Validate state
            var validStates = new[] { "INIT", "ATTEMPT", "2-WAY", "EXSTART", "EXCHANGE", "LOADING", "FULL" };
            if (!string.IsNullOrEmpty(State) && !validStates.Contains(State.ToUpperInvariant()))
                results.Add(ValidationResult.Error($"OSPF Neighbor State must be one of: {string.Join(", ", validStates)} (got '{State}')"));

            // Validate priority (0-255)
            if (Priority < 0 || Priority > 255)
                results.Add(ValidationResult.Error($"OSPF Neighbor Priority must be between 0 and 255 (got {Priority})"));

            return results.Count == 0 ? ValidationResult.Success() : ValidationResult.WithErrors(results.SelectMany(r => r.Errors).ToArray());
        }
    }

    /// <summary>
    /// Represents an OSPF interface configuration with validation
    /// </summary>
    public class OspfInterface : IValidatable
    {
        public string Name { get; set; }
        public int Area { get; set; }
        public int Cost { get; set; } = 10;
        public int Priority { get; set; } = 1;
        public string NetworkType { get; set; } = "broadcast";
        public string InterfaceType { get; set; } = "broadcast";

        public OspfInterface(string name, int area)
        {
            Name = name;
            Area = area;
        }

        public ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate interface name
            if (string.IsNullOrWhiteSpace(Name))
                results.Add(ValidationResult.Error("OSPF Interface name is required"));

            // Validate area ID
            if (Area < 0)
                results.Add(ValidationResult.Error($"OSPF Interface Area ID cannot be negative (got {Area})"));

            // Validate cost (1-65535)
            if (Cost < 1 || Cost > 65535)
                results.Add(ValidationResult.Error($"OSPF Interface Cost must be between 1 and 65535 (got {Cost})"));

            // Validate priority (0-255)
            if (Priority < 0 || Priority > 255)
                results.Add(ValidationResult.Error($"OSPF Interface Priority must be between 0 and 255 (got {Priority})"));

            // Validate network type
            var validNetworkTypes = new[] { "broadcast", "point-to-point", "point-to-multipoint", "non-broadcast" };
            if (!string.IsNullOrEmpty(NetworkType) && !validNetworkTypes.Contains(NetworkType.ToLowerInvariant()))
                results.Add(ValidationResult.Error($"OSPF Interface Network Type must be one of: {string.Join(", ", validNetworkTypes)} (got '{NetworkType}')"));

            return results.Count == 0 ? ValidationResult.Success() : ValidationResult.WithErrors(results.SelectMany(r => r.Errors).ToArray());
        }
    }

    /// <summary>
    /// Represents an OSPF area configuration with validation
    /// </summary>
    public class OspfArea : IValidatable
    {
        public int AreaId { get; set; }
        public string Type { get; set; } = "normal";
        public Dictionary<string, int> Networks { get; set; } = new Dictionary<string, int>();
        public List<string> Interfaces { get; set; } = new List<string>();

        public ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate area ID
            if (AreaId < 0)
                results.Add(ValidationResult.Error($"OSPF Area ID cannot be negative (got {AreaId})"));

            // Validate area type
            var validAreaTypes = new[] { "normal", "stub", "nssa", "backbone" };
            if (!string.IsNullOrEmpty(Type) && !validAreaTypes.Contains(Type.ToLowerInvariant()))
                results.Add(ValidationResult.Error($"OSPF Area Type must be one of: {string.Join(", ", validAreaTypes)} (got '{Type}')"));

            // Validate that backbone area (Area 0) cannot be stub or NSSA
            if (AreaId == 0 && !string.IsNullOrEmpty(Type) && (Type.ToLowerInvariant() == "stub" || Type.ToLowerInvariant() == "nssa"))
                results.Add(ValidationResult.Error("OSPF Backbone Area (Area 0) cannot be configured as stub or NSSA"));

            // Validate networks in area
            foreach (var network in Networks.Keys)
            {
                if (!NetworkPrefix.IsValidCidr(network) && !DataTypes.ValueObjects.IpAddress.IsValidIpAddress(network))
                    results.Add(ValidationResult.Error($"OSPF Area network '{network}' must be a valid IP address or CIDR notation"));
            }

            return results.Count == 0 ? ValidationResult.Success() : ValidationResult.WithErrors(results.SelectMany(r => r.Errors).ToArray());
        }
    }

    public class OspfConfig : ConfigurationBase
    {
        public int ProcessId { get; set; }
        public string RouterId { get; set; } = "0.0.0.0";
        public List<string> Networks { get; set; } = new List<string>();
        public Dictionary<string, int> NetworkAreas { get; set; } = new Dictionary<string, int>(); // network -> area
        public List<OspfNeighbor> Neighbors { get; set; } = new List<OspfNeighbor>();
        public Dictionary<string, OspfInterface> Interfaces { get; set; } = new Dictionary<string, OspfInterface>();
        public Dictionary<int, OspfArea> Areas { get; set; } = new Dictionary<int, OspfArea>();
        public List<string> Redistribution { get; set; } = new List<string>();
        public List<string> PassiveInterfaces { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public List<string> ImportRoutes { get; set; } = new List<string>();
        public bool IsEnabled { get; set; } = true;

        public OspfConfig(int processId)
        {
            ProcessId = processId;
        }

        public override ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate Process ID (1-65535)
            if (ProcessId < 1 || ProcessId > 65535)
                results.Add(ValidationResult.Error($"OSPF Process ID must be between 1 and 65535 (got {ProcessId})"));

            // Validate Router ID
            if (string.IsNullOrWhiteSpace(RouterId))
                results.Add(ValidationResult.Error("OSPF Router ID is required"));
            else if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(RouterId))
                results.Add(ValidationResult.Error($"OSPF Router ID must be a valid IP address (got '{RouterId}')"));

            // Validate networks
            foreach (var network in Networks)
            {
                if (!NetworkPrefix.IsValidCidr(network) && !DataTypes.ValueObjects.IpAddress.IsValidIpAddress(network))
                    results.Add(ValidationResult.Error($"OSPF network '{network}' must be a valid IP address or CIDR notation"));
            }

            // Validate area IDs in NetworkAreas
            foreach (var area in NetworkAreas.Values)
            {
                if (area < 0)
                    results.Add(ValidationResult.Error($"OSPF Area ID cannot be negative (got {area})"));
            }

            // Validate neighbors
            foreach (var neighbor in Neighbors)
            {
                var neighborValidation = neighbor.Validate();
                results.Add(neighborValidation);
            }

            // Validate interfaces
            foreach (var ospfInterface in Interfaces.Values)
            {
                var interfaceValidation = ospfInterface.Validate();
                results.Add(interfaceValidation);
            }

            // Validate areas
            foreach (var area in Areas.Values)
            {
                var areaValidation = area.Validate();
                results.Add(areaValidation);
            }

            // Validate redistribution protocols
            var validRedistProtocols = new[] { "connected", "static", "rip", "eigrp", "bgp", "isis", "kernel" };
            foreach (var redist in Redistribution)
            {
                if (!string.IsNullOrEmpty(redist) && !validRedistProtocols.Contains(redist.ToLowerInvariant()))
                    results.Add(ValidationResult.WithWarnings($"OSPF redistribution protocol '{redist}' may not be supported"));
            }

            return CombineResults(results.ToArray());
        }

        public void AddArea(int areaId, string type = "normal")
        {
            if (!Areas.ContainsKey(areaId))
            {
                Areas[areaId] = new OspfArea { AreaId = areaId, Type = type };
            }
        }

        public void AddNetworkToArea(string network, int areaId)
        {
            if (!Areas.ContainsKey(areaId))
            {
                AddArea(areaId);
            }
            Areas[areaId].Networks[network] = areaId;
            NetworkAreas[network] = areaId;
            if (!Networks.Contains(network))
            {
                Networks.Add(network);
            }
        }

        public void AddInterfaceToArea(string interfaceName, int areaId)
        {
            if (!Areas.ContainsKey(areaId))
            {
                AddArea(areaId);
            }
            Areas[areaId].Interfaces.Add(interfaceName);
            if (!Interfaces.ContainsKey(interfaceName))
            {
                Interfaces[interfaceName] = new OspfInterface(interfaceName, areaId);
            }
            else
            {
                Interfaces[interfaceName].Area = areaId;
            }
        }
    }
}
