namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents an OSPF neighbor
    /// </summary>
    public class OspfNeighbor(string neighborId, string ipAddress, string interfaceName)
    {
        public string NeighborId { get; set; } = neighborId;
        public string IpAddress { get; set; } = ipAddress;
        public string State { get; set; } = "INIT";
        public string Interface { get; set; } = interfaceName;
        public int Priority { get; set; } = 1;
        public DateTime StateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Represents an OSPF interface configuration
    /// </summary>
    public class OspfInterface(string name, int area)
    {
        public string Name { get; set; } = name;
        public int Area { get; set; } = area;
        public int Cost { get; set; } = 10;
        public int Priority { get; set; } = 1;
        public string NetworkType { get; set; } = "broadcast";
        public string InterfaceType { get; set; } = "broadcast";
    }
    /// <summary>
    /// Represents an OSPF configuration
    /// </summary>
    public class OspfArea
    {
        public int AreaId { get; set; }
        public string Type { get; set; } = "normal";
        public Dictionary<string, int> Networks { get; set; } = new Dictionary<string, int>();
        public List<string> Interfaces { get; set; } = new List<string>();
    }

    public class OspfConfig(int processId)
    {
        public int ProcessId { get; set; } = processId;
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
