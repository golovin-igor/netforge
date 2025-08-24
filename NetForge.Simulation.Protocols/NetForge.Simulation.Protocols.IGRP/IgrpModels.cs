using NetForge.Simulation.Common;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.IGRP;

public class IgrpState : BaseProtocolState
{
    public Dictionary<string, IgrpNeighbor> Neighbors { get; set; } = new();
    public Dictionary<string, IgrpRoute> Routes { get; set; } = new();
    public List<IgrpRoute> CalculatedRoutes { get; set; } = new();
    public bool TopologyChanged { get; set; } = true;
    public int AutonomousSystem { get; set; } = 1;
    public Dictionary<string, DateTime> RouteTimers { get; set; } = new();
    public Dictionary<string, DateTime> InvalidTimers { get; set; } = new();
    
    public override void MarkStateChanged()
    {
        base.MarkStateChanged();
        TopologyChanged = true;
    }
    
    public IgrpNeighbor GetOrCreateNeighbor(string id, Func<IgrpNeighbor> factory)
    {
        if (!Neighbors.ContainsKey(id))
        {
            Neighbors[id] = factory();
            MarkStateChanged();
        }
        UpdateNeighborActivity(id);
        return Neighbors[id];
    }
    
    public void AddOrUpdateRoute(IgrpRoute route)
    {
        var routeKey = $"{route.Network}/{route.Mask}";
        var existingRoute = Routes.ContainsKey(routeKey) ? Routes[routeKey] : null;
        
        if (existingRoute == null || existingRoute.Metric > route.Metric)
        {
            Routes[routeKey] = route;
            RouteTimers[routeKey] = DateTime.Now;
            MarkStateChanged();
        }
        else if (existingRoute.NextHop == route.NextHop)
        {
            // Update from same next hop - refresh timer
            RouteTimers[routeKey] = DateTime.Now;
            if (InvalidTimers.ContainsKey(routeKey))
            {
                InvalidTimers.Remove(routeKey);
            }
        }
    }
    
    public List<string> GetInvalidRoutes(int timeoutSeconds = 270)
    {
        var invalidRoutes = new List<string>();
        var now = DateTime.Now;
        
        foreach (var kvp in RouteTimers)
        {
            if ((now - kvp.Value).TotalSeconds > timeoutSeconds)
            {
                invalidRoutes.Add(kvp.Key);
            }
        }
        
        return invalidRoutes;
    }
    
    public List<string> GetFlushRoutes(int timeoutSeconds = 630)
    {
        var flushRoutes = new List<string>();
        var now = DateTime.Now;
        
        foreach (var kvp in InvalidTimers)
        {
            if ((now - kvp.Value).TotalSeconds > timeoutSeconds)
            {
                flushRoutes.Add(kvp.Key);
            }
        }
        
        return flushRoutes;
    }
    
    public void MarkRouteInvalid(string routeKey)
    {
        if (Routes.ContainsKey(routeKey))
        {
            Routes[routeKey].State = IgrpRouteState.Invalid;
            InvalidTimers[routeKey] = DateTime.Now;
            MarkStateChanged();
        }
    }
    
    public void RemoveRoute(string routeKey)
    {
        if (Routes.Remove(routeKey))
        {
            RouteTimers.Remove(routeKey);
            InvalidTimers.Remove(routeKey);
            MarkStateChanged();
        }
    }
    
    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["Neighbors"] = Neighbors;
        baseData["Routes"] = Routes;
        baseData["TopologyChanged"] = TopologyChanged;
        baseData["AutonomousSystem"] = AutonomousSystem;
        return baseData;
    }
}

public class IgrpNeighbor
{
    public string RouterId { get; set; } = "";
    public string InterfaceName { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public int HoldTime { get; set; } = 280; // Default IGRP hold time
    public IgrpNeighborState State { get; set; } = IgrpNeighborState.Down;
    public int AutonomousSystem { get; set; } = 1;
    public Dictionary<string, object> Capabilities { get; set; } = new();
    
    public bool IsActive => State == IgrpNeighborState.Up && 
                           (DateTime.Now - LastSeen).TotalSeconds < HoldTime;
}

public enum IgrpNeighborState
{
    Down,
    Up
}

public class IgrpRoute
{
    public string Network { get; set; } = "";
    public string Mask { get; set; } = "";
    public string NextHop { get; set; } = "";
    public string Interface { get; set; } = "";
    public int Metric { get; set; } = int.MaxValue;
    public int Bandwidth { get; set; } = 1544; // Default bandwidth in Kbps
    public int Delay { get; set; } = 20000; // Default delay in microseconds
    public int Reliability { get; set; } = 255; // Default reliability (0-255)
    public int Load { get; set; } = 1; // Default load (0-255)
    public int Mtu { get; set; } = 1500; // Default MTU
    public int HopCount { get; set; } = 0;
    public IgrpRouteState State { get; set; } = IgrpRouteState.Valid;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public string Source { get; set; } = "";
    
    // Calculate composite metric using IGRP formula
    public int CalculateMetric()
    {
        // IGRP metric = [K1 * Bandwidth + (K2 * Bandwidth) / (256 - Load) + K3 * Delay] * [K5 / (Reliability + K4)]
        // Default K values: K1=1, K2=0, K3=1, K4=0, K5=0
        // Simplified: Metric = Bandwidth + Delay
        var bandwidthComponent = Bandwidth > 0 ? (10000000 / Bandwidth) : int.MaxValue;
        var delayComponent = Delay / 10;
        return Math.Min(bandwidthComponent + delayComponent, int.MaxValue);
    }
}

public enum IgrpRouteState
{
    Valid,
    Invalid,
    Holddown
}

public class IgrpConfig
{
    public bool IsEnabled { get; set; } = false;
    public int AutonomousSystem { get; set; } = 1;
    public List<string> Networks { get; set; } = new();
    public Dictionary<string, bool> Interfaces { get; set; } = new();
    public int UpdateTimer { get; set; } = 90; // Default IGRP update timer (90 seconds)
    public int InvalidTimer { get; set; } = 270; // Route invalid timer (270 seconds)
    public int FlushTimer { get; set; } = 630; // Route flush timer (630 seconds)
    public int HoldTimer { get; set; } = 280; // Neighbor hold timer (280 seconds)
    public bool AutoSummary { get; set; } = true;
    public Dictionary<string, int> Variance { get; set; } = new();
    public Dictionary<string, int> MaximumPaths { get; set; } = new();
    public Dictionary<string, int> Metrics { get; set; } = new();
    
    public IgrpConfig Clone()
    {
        return new IgrpConfig
        {
            IsEnabled = IsEnabled,
            AutonomousSystem = AutonomousSystem,
            Networks = new List<string>(Networks),
            Interfaces = new Dictionary<string, bool>(Interfaces),
            UpdateTimer = UpdateTimer,
            InvalidTimer = InvalidTimer,
            FlushTimer = FlushTimer,
            HoldTimer = HoldTimer,
            AutoSummary = AutoSummary,
            Variance = new Dictionary<string, int>(Variance),
            MaximumPaths = new Dictionary<string, int>(MaximumPaths),
            Metrics = new Dictionary<string, int>(Metrics)
        };
    }
    
    public bool Validate()
    {
        if (AutonomousSystem <= 0 || AutonomousSystem > 65535)
            return false;
        
        if (UpdateTimer < 10 || UpdateTimer > 3600)
            return false;
            
        if (InvalidTimer <= UpdateTimer)
            return false;
            
        if (FlushTimer <= InvalidTimer)
            return false;
            
        return true;
    }
}