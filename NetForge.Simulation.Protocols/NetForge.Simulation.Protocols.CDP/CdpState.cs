namespace NetForge.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of a CDP protocol instance
    /// </summary>
    public class CdpState
    {
        /// <summary>
        /// Dictionary of CDP neighbors indexed by device ID
        /// </summary>
        public Dictionary<string, CdpNeighbor> Neighbors { get; set; } = new();
        
        /// <summary>
        /// Last time each neighbor was seen
        /// </summary>
        public Dictionary<string, DateTime> NeighborLastSeen { get; set; } = new();
        
        /// <summary>
        /// Local device ID for CDP advertisements
        /// </summary>
        public string LocalDeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Local platform for CDP advertisements
        /// </summary>
        public string LocalPlatform { get; set; } = string.Empty;
        
        /// <summary>
        /// Track if neighbor information has changed
        /// </summary>
        public bool NeighborsChanged { get; set; } = true;
        
        /// <summary>
        /// Last time CDP advertisements were sent
        /// </summary>
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Interface states for each CDP-enabled interface
        /// </summary>
        public Dictionary<string, CdpInterfaceState> InterfaceStates { get; set; } = new();
        
        /// <summary>
        /// Mark neighbors as changed
        /// </summary>
        public void MarkNeighborsChanged()
        {
            NeighborsChanged = true;
        }
        
        /// <summary>
        /// Get or create CDP neighbor
        /// </summary>
        public CdpNeighbor GetOrCreateNeighbor(string deviceId, string interfaceName, string remoteInterface = "")
        {
            if (!Neighbors.ContainsKey(deviceId))
            {
                Neighbors[deviceId] = new CdpNeighbor(deviceId, interfaceName);
                if (!string.IsNullOrEmpty(remoteInterface))
                {
                    Neighbors[deviceId].RemoteInterface = remoteInterface;
                }
            }
            return Neighbors[deviceId];
        }
        
        /// <summary>
        /// Remove CDP neighbor
        /// </summary>
        public void RemoveNeighbor(string deviceId)
        {
            if (Neighbors.Remove(deviceId))
            {
                NeighborLastSeen.Remove(deviceId);
                MarkNeighborsChanged();
            }
        }
        
        /// <summary>
        /// Check for neighbors that have aged out
        /// </summary>
        public List<string> GetAgedOutNeighbors(int holdTime = 180)
        {
            var agedOutNeighbors = new List<string>();
            var now = DateTime.Now;
            
            foreach (var kvp in NeighborLastSeen)
            {
                if ((now - kvp.Value).TotalSeconds > holdTime)
                {
                    agedOutNeighbors.Add(kvp.Key);
                }
            }
            
            return agedOutNeighbors;
        }
        
        /// <summary>
        /// Check for stale neighbors (alias for GetAgedOutNeighbors)
        /// </summary>
        public List<string> GetStaleNeighbors(int holdTime = 180)
        {
            return GetAgedOutNeighbors(holdTime);
        }
        
        /// <summary>
        /// Check if advertisements should be sent
        /// </summary>
        public bool ShouldSendAdvertisements(int interval = 60)
        {
            return (DateTime.Now - LastAdvertisementSent).TotalSeconds >= interval;
        }
    }
    
    /// <summary>
    /// Represents a CDP neighbor
    /// </summary>
    public class CdpNeighbor
    {
        public string DeviceId { get; set; }
        public string InterfaceName { get; set; }
        public string LocalInterface { get; set; } = string.Empty;
        public string RemoteInterface { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> Capabilities { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        public int HoldTime { get; set; } = 180;
        
        public CdpNeighbor(string deviceId, string interfaceName)
        {
            DeviceId = deviceId;
            InterfaceName = interfaceName;
            LocalInterface = interfaceName; // Local interface is the same as interface name
        }
        
        public TimeSpan GetAge()
        {
            return DateTime.Now - LastUpdate;
        }
        
        public bool HasAgedOut()
        {
            return GetAge().TotalSeconds > HoldTime;
        }
    }
    
    /// <summary>
    /// Interface state for CDP
    /// </summary>
    public class CdpInterfaceState
    {
        public string InterfaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
        public int AdvertisementInterval { get; set; } = 60;
        
        public CdpInterfaceState(string interfaceName)
        {
            InterfaceName = interfaceName;
        }
        
        public bool ShouldSendAdvertisement()
        {
            return (DateTime.Now - LastAdvertisementSent).TotalSeconds >= AdvertisementInterval;
        }
    }
} 
