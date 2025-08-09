namespace NetSim.Simulation.Protocols.Discovery
{
    /// <summary>
    /// Represents a CDP neighbor entry
    /// </summary>
    public class CdpNeighbor
    {
        public string DeviceId { get; set; }
        public string LocalInterface { get; set; }
        public string PortId { get; set; }
        public string Platform { get; set; }
        public string IpAddress { get; set; }
        public int HoldTime { get; set; } = 120;

        public CdpNeighbor(string deviceId, string localIntf, string portId, string platform, string ip)
        {
            DeviceId = deviceId;
            LocalInterface = localIntf;
            PortId = portId;
            Platform = platform;
            IpAddress = ip;
        }
    }
} 
