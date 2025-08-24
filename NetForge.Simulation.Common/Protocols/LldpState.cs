namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents the runtime state of an LLDP protocol instance
    /// </summary>
    public class LldpState
    {
        /// <summary>
        /// Dictionary of LLDP neighbors indexed by chassis ID and port ID
        /// </summary>
        public Dictionary<string, LldpNeighbor> Neighbors { get; set; } = new();

        /// <summary>
        /// Last time each neighbor was seen
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();

        /// <summary>
        /// Local chassis ID for LLDP advertisements
        /// </summary>
        public string LocalChassisId { get; set; } = string.Empty;

        /// <summary>
        /// Local system name for LLDP advertisements
        /// </summary>
        public string LocalSystemName { get; set; } = string.Empty;

        /// <summary>
        /// Track if neighbor information has changed
        /// </summary>
        public bool NeighborsChanged { get; set; } = true;

        /// <summary>
        /// Last time LLDP advertisements were sent
        /// </summary>
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Interface states for each LLDP-enabled interface
        /// </summary>
        public Dictionary<string, LldpInterfaceState> InterfaceStates { get; set; } = new();

        /// <summary>
        /// Mark neighbors as changed
        /// </summary>
        public void MarkNeighborsChanged()
        {
            NeighborsChanged = true;
        }

        /// <summary>
        /// Get or create LLDP neighbor
        /// </summary>
        public LldpNeighbor GetOrCreateNeighbor(string chassisId, string portId, string interfaceName)
        {
            var neighborKey = $"{chassisId}:{portId}";
            if (!Neighbors.ContainsKey(neighborKey))
            {
                Neighbors[neighborKey] = new LldpNeighbor(chassisId, portId, interfaceName);
            }
            return Neighbors[neighborKey];
        }

        /// <summary>
        /// Remove LLDP neighbor
        /// </summary>
        public void RemoveNeighbor(string chassisId, string portId)
        {
            var neighborKey = $"{chassisId}:{portId}";
            if (Neighbors.Remove(neighborKey))
            {
                NeighborLastSeen.Remove(neighborKey);
                MarkNeighborsChanged();
            }
        }

        /// <summary>
        /// Check for neighbors that have aged out
        /// </summary>
        public List<string> GetAgedOutNeighbors()
        {
            var agedOutNeighbors = new List<string>();
            var now = DateTime.Now;

            foreach (var kvp in Neighbors)
            {
                var neighbor = kvp.Value;
                if ((now - neighbor.LastUpdate).TotalSeconds > neighbor.TimeToLive)
                {
                    agedOutNeighbors.Add(kvp.Key);
                }
            }

            return agedOutNeighbors;
        }

        /// <summary>
        /// Check for stale neighbors (alias for GetAgedOutNeighbors)
        /// </summary>
        public List<string> GetStaleNeighbors()
        {
            return GetAgedOutNeighbors();
        }

        /// <summary>
        /// Check if advertisements should be sent
        /// </summary>
        public bool ShouldSendAdvertisements(int interval = 30)
        {
            return (DateTime.Now - LastAdvertisementSent).TotalSeconds >= interval;
        }
    }

    /// <summary>
    /// Represents an LLDP neighbor
    /// </summary>
    public class LldpNeighbor
    {
        public string ChassisId { get; set; }
        public string PortId { get; set; }
        public string InterfaceName { get; set; }
        public string LocalInterface { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string SystemDescription { get; set; } = string.Empty;
        public string PortDescription { get; set; } = string.Empty;
        public List<string> Capabilities { get; set; } = new();
        public List<string> SystemCapabilities { get; set; } = new();
        public string ManagementAddress { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        public int TimeToLive { get; set; } = 120;

        public LldpNeighbor(string chassisId, string portId, string interfaceName)
        {
            ChassisId = chassisId;
            PortId = portId;
            InterfaceName = interfaceName;
            LocalInterface = interfaceName; // Local interface is the same as interface name
        }

        public TimeSpan GetAge()
        {
            return DateTime.Now - LastUpdate;
        }

        public bool HasAgedOut()
        {
            return GetAge().TotalSeconds > TimeToLive;
        }
    }

    /// <summary>
    /// Interface state for LLDP
    /// </summary>
    public class LldpInterfaceState
    {
        public string InterfaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
        public int AdvertisementInterval { get; set; } = 30;
        public bool TransmitEnabled { get; set; } = true;
        public bool ReceiveEnabled { get; set; } = true;

        public LldpInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }

        public bool ShouldSendAdvertisement()
        {
            return TransmitEnabled &&
                   (DateTime.Now - LastAdvertisementSent).TotalSeconds >= AdvertisementInterval;
        }
    }
}
