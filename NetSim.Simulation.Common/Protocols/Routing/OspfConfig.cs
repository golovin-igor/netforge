namespace NetSim.Simulation.Protocols.Routing
{
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

    public class OspfConfig
    {
        public int ProcessId { get; set; }
        public string RouterId { get; set; }
        public List<string> Networks { get; set; } = new List<string>();
        public Dictionary<string, int> NetworkAreas { get; set; } = new Dictionary<string, int>(); // network -> area
        public List<OspfNeighbor> Neighbors { get; set; } = new List<OspfNeighbor>();
        public Dictionary<string, OspfInterface> Interfaces { get; set; } = new Dictionary<string, OspfInterface>();
        public Dictionary<int, OspfArea> Areas { get; set; } = new Dictionary<int, OspfArea>();
        public List<string> Redistribution { get; set; } = new List<string>();
        public List<string> PassiveInterfaces { get; set; } = new List<string>();
        public List<string> ExportPolicies { get; set; } = new List<string>();
        public List<string> ImportRoutes { get; set; } = new List<string>();
        public bool IsEnabled { get; set; }
        
        public OspfConfig(int processId)
        {
            ProcessId = processId;
            RouterId = "0.0.0.0";
            IsEnabled = true;
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
            if (Interfaces.ContainsKey(interfaceName))
            {
                Interfaces[interfaceName].Area = areaId;
            }
        }
    }
} 
