using NetForge.Simulation.Common;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.ISIS;

public class IsisState : BaseProtocolState
{
    public Dictionary<string, IsisNeighbor> Neighbors { get; set; } = new();
    public Dictionary<string, IsisLsp> LspDatabase { get; set; } = new();
    public Dictionary<string, IsisRoute> Routes { get; set; } = new();
    public List<IsisRoute> CalculatedRoutes { get; set; } = new();
    public bool TopologyChanged { get; set; } = true;
    public bool LspChanged { get; set; } = true;
    public string SystemId { get; set; } = "";
    public IsisLevel Level { get; set; } = IsisLevel.Level1;
    public Dictionary<string, DateTime> LspTimers { get; set; } = new();
    public bool IsDis { get; set; } = false; // Designated Intermediate System
    public string AreaId { get; set; } = "49.0001";
    
    public override void MarkStateChanged()
    {
        base.MarkStateChanged();
        TopologyChanged = true;
    }
    
    public void MarkLspChanged()
    {
        LspChanged = true;
        MarkStateChanged();
    }
    
    public IsisNeighbor GetOrCreateNeighbor(string id, Func<IsisNeighbor> factory)
    {
        if (!Neighbors.ContainsKey(id))
        {
            Neighbors[id] = factory();
            MarkStateChanged();
        }
        UpdateNeighborActivity(id);
        return Neighbors[id];
    }
    
    public void AddOrUpdateLsp(IsisLsp lsp)
    {
        var lspKey = $"{lsp.LspId}";
        var existingLsp = LspDatabase.ContainsKey(lspKey) ? LspDatabase[lspKey] : null;
        
        if (existingLsp == null || existingLsp.SequenceNumber < lsp.SequenceNumber)
        {
            LspDatabase[lspKey] = lsp;
            LspTimers[lspKey] = DateTime.Now;
            MarkLspChanged();
        }
    }
    
    public List<string> GetExpiredLsps(int maxAgeSeconds = 1200) // 20 minutes default
    {
        var expiredLsps = new List<string>();
        var now = DateTime.Now;
        
        foreach (var kvp in LspTimers)
        {
            if ((now - kvp.Value).TotalSeconds > maxAgeSeconds)
            {
                expiredLsps.Add(kvp.Key);
            }
        }
        
        return expiredLsps;
    }
    
    public void RemoveLsp(string lspId)
    {
        if (LspDatabase.Remove(lspId))
        {
            LspTimers.Remove(lspId);
            MarkLspChanged();
        }
    }
    
    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["Neighbors"] = Neighbors;
        baseData["LspDatabase"] = LspDatabase;
        baseData["Routes"] = Routes;
        baseData["TopologyChanged"] = TopologyChanged;
        baseData["SystemId"] = SystemId;
        baseData["Level"] = Level;
        baseData["IsDis"] = IsDis;
        baseData["AreaId"] = AreaId;
        return baseData;
    }
}

public class IsisNeighbor
{
    public string SystemId { get; set; } = "";
    public string InterfaceName { get; set; } = "";
    public string CircuitId { get; set; } = "";
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public int HoldTime { get; set; } = 30; // Default IS-IS hold time
    public IsisNeighborState State { get; set; } = IsisNeighborState.Down;
    public IsisLevel Level { get; set; } = IsisLevel.Level1;
    public int Priority { get; set; } = 64;
    public List<string> AreaAddresses { get; set; } = new();
    public Dictionary<string, object> Tlvs { get; set; } = new();
    
    public bool IsActive => State == IsisNeighborState.Up && 
                           (DateTime.Now - LastSeen).TotalSeconds < HoldTime;
}

public enum IsisNeighborState
{
    Down,
    Initializing,
    Up
}

public enum IsisLevel
{
    Level1 = 1,
    Level2 = 2,
    Level1Level2 = 3
}

public class IsisLsp
{
    public string LspId { get; set; } = "";
    public uint SequenceNumber { get; set; } = 1;
    public ushort RemainingLifetime { get; set; } = 1200; // 20 minutes
    public ushort Checksum { get; set; } = 0;
    public IsisLevel Level { get; set; } = IsisLevel.Level1;
    public List<IsisTlv> Tlvs { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public string OriginatingSystem { get; set; } = "";
    public bool IsOverloaded { get; set; } = false;
    
    public bool IsExpired => RemainingLifetime == 0 || 
                           (DateTime.Now - LastUpdate).TotalSeconds > RemainingLifetime;
}

public class IsisTlv
{
    public byte Type { get; set; }
    public byte Length { get; set; }
    public byte[] Value { get; set; } = Array.Empty<byte>();
    public string Description { get; set; } = "";
}

public class IsisRoute
{
    public string Destination { get; set; } = "";
    public string Mask { get; set; } = "";
    public string NextHop { get; set; } = "";
    public string Interface { get; set; } = "";
    public int Metric { get; set; } = 10;
    public IsisLevel Level { get; set; } = IsisLevel.Level1;
    public IsisRouteType RouteType { get; set; } = IsisRouteType.Internal;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public string OriginatingLsp { get; set; } = "";
    public List<string> Path { get; set; } = new();
}

public enum IsisRouteType
{
    Internal,
    External,
    Level1,
    Level2,
    InterArea
}

public class IsisConfig
{
    public bool IsEnabled { get; set; } = false;
    public string SystemId { get; set; } = "";
    public string AreaId { get; set; } = "49.0001";
    public IsisLevel Level { get; set; } = IsisLevel.Level1Level2;
    public Dictionary<string, bool> Interfaces { get; set; } = new();
    public Dictionary<string, int> InterfaceMetrics { get; set; } = new();
    public Dictionary<string, int> InterfacePriorities { get; set; } = new();
    public int HelloInterval { get; set; } = 10; // seconds
    public int HoldTime { get; set; } = 30; // seconds
    public int LspRefreshInterval { get; set; } = 900; // 15 minutes
    public int LspMaxLifetime { get; set; } = 1200; // 20 minutes
    public bool IsOverloaded { get; set; } = false;
    public Dictionary<string, string> RedistributeRoutes { get; set; } = new();
    public List<string> SummaryAddresses { get; set; } = new();
    
    public IsisConfig Clone()
    {
        return new IsisConfig
        {
            IsEnabled = IsEnabled,
            SystemId = SystemId,
            AreaId = AreaId,
            Level = Level,
            Interfaces = new Dictionary<string, bool>(Interfaces),
            InterfaceMetrics = new Dictionary<string, int>(InterfaceMetrics),
            InterfacePriorities = new Dictionary<string, int>(InterfacePriorities),
            HelloInterval = HelloInterval,
            HoldTime = HoldTime,
            LspRefreshInterval = LspRefreshInterval,
            LspMaxLifetime = LspMaxLifetime,
            IsOverloaded = IsOverloaded,
            RedistributeRoutes = new Dictionary<string, string>(RedistributeRoutes),
            SummaryAddresses = new List<string>(SummaryAddresses)
        };
    }
    
    public bool Validate()
    {
        if (string.IsNullOrEmpty(SystemId) || SystemId.Length != 14) // Format: xxxx.xxxx.xxxx
            return false;
            
        if (string.IsNullOrEmpty(AreaId))
            return false;
            
        if (HelloInterval < 1 || HelloInterval > 65535)
            return false;
            
        if (HoldTime <= HelloInterval)
            return false;
            
        if (LspMaxLifetime <= LspRefreshInterval)
            return false;
            
        return true;
    }
    
    public string GenerateSystemId(string deviceName)
    {
        // Generate a system ID based on device name
        // Format: 1921.6800.1001 (based on device hash)
        var hash = deviceName.GetHashCode();
        var part1 = Math.Abs(hash % 10000).ToString("D4");
        var part2 = Math.Abs((hash >> 8) % 10000).ToString("D4");
        var part3 = Math.Abs((hash >> 16) % 10000).ToString("D4");
        return $"{part1}.{part2}.{part3}";
    }
}