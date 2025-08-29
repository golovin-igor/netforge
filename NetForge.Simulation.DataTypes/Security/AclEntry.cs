namespace NetForge.Simulation.Common.Security
{
    /// <summary>
    /// Represents an ACL entry
    /// </summary>
    public class AclEntry
    {
        public required string Action { get; set; } // permit or deny
        public required string Protocol { get; set; } // ip, tcp, udp, icmp
        public required string SourceAddress { get; set; }
        public required string SourceWildcard { get; set; }
        public required string DestAddress { get; set; }
        public required string DestWildcard { get; set; }
        public int? SourcePort { get; set; }
        public int? DestPort { get; set; }

        public bool Matches(string sourceIp, string destIp, string protocol)
        {
            // Simplified ACL matching logic
            if (!string.IsNullOrEmpty(Protocol) && Protocol != protocol && Protocol != "ip")
                return false;

            if (SourceAddress == "any" || MatchesWithWildcard(sourceIp, SourceAddress, SourceWildcard))
            {
                if (DestAddress == "any" || MatchesWithWildcard(destIp, DestAddress, DestWildcard))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesWithWildcard(string ip, string network, string wildcard)
        {
            // Simplified wildcard matching
            if (network == "any" || string.IsNullOrEmpty(network))
                return true;

            if (wildcard == "0.0.0.0")
                return ip == network;

            // For simplicity, just check if IPs are in same subnet
            return ip.StartsWith(network.Substring(0, network.LastIndexOf('.')));
        }
    }
}
