using NetForge.Simulation.DataTypes.Validation;
using NetForge.Simulation.DataTypes.ValueObjects;

namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a BGP configuration with validation
    /// </summary>
    public class BgpConfig : ConfigurationBase
    {
        public int LocalAs { get; set; }
        public string RouterId { get; set; } = "";
        public Dictionary<string, BgpNeighbor> Neighbors { get; set; } = new Dictionary<string, BgpNeighbor>();
        public Dictionary<string, BgpNeighbor> Peers { get; set; } = new Dictionary<string, BgpNeighbor>();
        public Dictionary<string, BgpGroup> Groups { get; set; } = new Dictionary<string, BgpGroup>();
        public List<string> Networks { get; set; } = new List<string>();
        public bool IsEnabled { get; set; } = true;

        public BgpConfig(int localAs)
        {
            LocalAs = localAs;
        }

        public override ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate BGP AS number (1-65535, excluding reserved ranges)
            if (LocalAs < 1 || LocalAs > 65535)
                results.Add(ValidationResult.Error($"BGP Local AS must be between 1 and 65535 (got {LocalAs})"));
            else if (LocalAs == 23456)
                results.Add(ValidationResult.Error("BGP AS 23456 is reserved (RFC 4893)"));
            else if (LocalAs >= 64496 && LocalAs <= 64511)
                results.Add(ValidationResult.Error($"BGP AS {LocalAs} is reserved for documentation (RFC 5398)"));
            else if (LocalAs >= 65536 && LocalAs <= 65551)
                results.Add(ValidationResult.Error($"BGP AS {LocalAs} is reserved for private use"));

            // Validate Router ID if provided
            if (!string.IsNullOrEmpty(RouterId))
            {
                if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(RouterId))
                    results.Add(ValidationResult.Error($"BGP Router ID must be a valid IP address (got '{RouterId}')"));
            }

            // Validate neighbors
            foreach (var neighbor in Neighbors.Values)
            {
                var neighborValidation = neighbor.Validate();
                results.Add(neighborValidation);
            }

            // Validate groups
            foreach (var group in Groups.Values)
            {
                var groupValidation = group.Validate();
                results.Add(groupValidation);
            }

            // Validate networks
            foreach (var network in Networks)
            {
                if (!NetworkPrefix.IsValidCidr(network) && !DataTypes.ValueObjects.IpAddress.IsValidIpAddress(network))
                    results.Add(ValidationResult.Error($"BGP network '{network}' must be a valid IP address or CIDR notation"));
            }

            return CombineResults(results.ToArray());
        }

        public void AddNeighbor(string ipAddress, int remoteAs, string description = "", bool isEnabled = true)
        {
            var neighbor = new BgpNeighbor(ipAddress, remoteAs, description, isEnabled);
            Neighbors[ipAddress] = neighbor;
            Peers[ipAddress] = neighbor;
        }
    }

    public class BgpNeighbor : IValidatable
    {
        public string IpAddress { get; set; }
        public int RemoteAs { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public int HoldTime { get; set; } = 180;
        public Dictionary<string, string> AddressFamilies { get; set; } = new Dictionary<string, string>();
        public List<string> ImportPolicies { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public string State { get; set; } = "Idle";
        public System.DateTime StateTime { get; set; } = System.DateTime.Now;
        public List<string> ReceivedRoutes { get; set; } = new List<string>();
        public int MessagesReceived { get; set; } = 0;
        public int MessagesSent { get; set; } = 0;
        public string UpdateSource { get; set; } = "";
        public bool IsActive { get; set; } = false;
        public string RouteMapIn { get; set; } = "";
        public string RouteMapOut { get; set; } = "";
        public bool SendCommunity { get; set; } = false;
        public bool SendCommunityExtended { get; set; } = false;
        public bool AdvertiseCommunity { get; set; } = false;
        public bool AdvertiseExtCommunity { get; set; } = false;

        public BgpNeighbor(string ipAddress, int remoteAs, string description = "", bool isEnabled = true)
        {
            IpAddress = ipAddress;
            RemoteAs = remoteAs;
            Description = description;
            IsEnabled = isEnabled;
            State = "Idle";
            StateTime = System.DateTime.Now;
            ReceivedRoutes = new List<string>();
            MessagesReceived = 0;
            MessagesSent = 0;
            UpdateSource = "";
            IsActive = false;
            RouteMapIn = "";
            RouteMapOut = "";
            SendCommunity = false;
            SendCommunityExtended = false;
            AdvertiseCommunity = false;
            AdvertiseExtCommunity = false;
        }

        public ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate IP address
            if (string.IsNullOrWhiteSpace(IpAddress))
                results.Add(ValidationResult.Error("BGP Neighbor IP address is required"));
            else if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(IpAddress))
                results.Add(ValidationResult.Error($"BGP Neighbor IP address must be valid (got '{IpAddress}')"));

            // Validate Remote AS
            if (RemoteAs < 1 || RemoteAs > 65535)
                results.Add(ValidationResult.Error($"BGP Neighbor Remote AS must be between 1 and 65535 (got {RemoteAs})"));

            // Validate Hold Time (must be 0 or between 3-65535)
            if (HoldTime != 0 && (HoldTime < 3 || HoldTime > 65535))
                results.Add(ValidationResult.Error($"BGP Hold Time must be 0 or between 3 and 65535 seconds (got {HoldTime})"));

            // Validate Update Source if provided
            if (!string.IsNullOrEmpty(UpdateSource) && !DataTypes.ValueObjects.IpAddress.IsValidIpAddress(UpdateSource))
                results.Add(ValidationResult.Error($"BGP Update Source must be a valid IP address (got '{UpdateSource}')"));

            // Validate State
            var validStates = new[] { "Idle", "Connect", "Active", "OpenSent", "OpenConfirm", "Established" };
            if (!string.IsNullOrEmpty(State) && !validStates.Contains(State))
                results.Add(ValidationResult.Error($"BGP Neighbor State must be one of: {string.Join(", ", validStates)} (got '{State}')"));

            return results.Count == 0 ? ValidationResult.Success() : ValidationResult.WithErrors(results.SelectMany(r => r.Errors).ToArray());
        }
    }

    public class BgpGroup : IValidatable
    {
        public string Name { get; set; }
        public int RemoteAs { get; set; }
        public string Description { get; set; } = "";
        public List<string> Members { get; set; } = new List<string>();
        public List<string> ImportPolicies { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public int PeerAs { get; set; }
        public Dictionary<string, BgpNeighbor> Neighbors { get; set; } = new Dictionary<string, BgpNeighbor>();
        public string Type { get; set; } = "external";

        public BgpGroup(string name, int remoteAs)
        {
            Name = name;
            RemoteAs = remoteAs;
            PeerAs = remoteAs;
        }

        public ValidationResult Validate()
        {
            var results = new List<ValidationResult>();

            // Validate name
            if (string.IsNullOrWhiteSpace(Name))
                results.Add(ValidationResult.Error("BGP Group name is required"));

            // Validate Remote AS
            if (RemoteAs < 1 || RemoteAs > 65535)
                results.Add(ValidationResult.Error($"BGP Group Remote AS must be between 1 and 65535 (got {RemoteAs})"));

            // Validate Peer AS
            if (PeerAs < 1 || PeerAs > 65535)
                results.Add(ValidationResult.Error($"BGP Group Peer AS must be between 1 and 65535 (got {PeerAs})"));

            // Validate Type
            var validTypes = new[] { "internal", "external" };
            if (!string.IsNullOrEmpty(Type) && !validTypes.Contains(Type.ToLowerInvariant()))
                results.Add(ValidationResult.Error($"BGP Group Type must be one of: {string.Join(", ", validTypes)} (got '{Type}')"));

            // Validate member IP addresses
            foreach (var member in Members)
            {
                if (!DataTypes.ValueObjects.IpAddress.IsValidIpAddress(member))
                    results.Add(ValidationResult.Error($"BGP Group member '{member}' must be a valid IP address"));
            }

            // Validate neighbors in group
            foreach (var neighbor in Neighbors.Values)
            {
                var neighborValidation = neighbor.Validate();
                results.Add(neighborValidation);
            }

            return results.Count == 0 ? ValidationResult.Success() : ValidationResult.WithErrors(results.SelectMany(r => r.Errors).ToArray());
        }
    }
}
