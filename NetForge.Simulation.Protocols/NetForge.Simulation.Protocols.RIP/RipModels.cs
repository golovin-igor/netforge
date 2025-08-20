using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.RIP;

/// <summary>
/// RIP protocol state following the state management pattern
/// </summary>
public class RipState : BaseProtocolState
{
    /// <summary>
    /// RIP routing table with learned routes
    /// Key format: "network/mask" (e.g., "192.168.1.0/255.255.255.0")
    /// </summary>
    public Dictionary<string, RipRoute> Routes { get; set; } = new();

    /// <summary>
    /// Last time periodic update was sent
    /// </summary>
    public DateTime LastPeriodicUpdate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Total number of updates sent
    /// </summary>
    public long UpdatesSent { get; set; }

    /// <summary>
    /// Total number of updates received
    /// </summary>
    public long UpdatesReceived { get; set; }

    /// <summary>
    /// Number of route changes since last update
    /// </summary>
    public int RouteChanges { get; set; }

    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["Routes"] = Routes;
        baseData["RouteCount"] = Routes.Count;
        baseData["ValidRoutes"] = Routes.Values.Count(r => r.State == RipRouteState.Valid);
        baseData["InvalidRoutes"] = Routes.Values.Count(r => r.State == RipRouteState.Invalid);
        baseData["LastPeriodicUpdate"] = LastPeriodicUpdate;
        baseData["UpdatesSent"] = UpdatesSent;
        baseData["UpdatesReceived"] = UpdatesReceived;
        baseData["RouteChanges"] = RouteChanges;
        return baseData;
    }

    /// <summary>
    /// Get all valid routes (metric < 16)
    /// </summary>
    public IEnumerable<RipRoute> GetValidRoutes()
    {
        return Routes.Values.Where(r => r.State == RipRouteState.Valid && r.Metric < 16);
    }

    /// <summary>
    /// Get routes that need to be advertised as unreachable
    /// </summary>
    public IEnumerable<RipRoute> GetPoisonRoutes()
    {
        return Routes.Values.Where(r => r.State == RipRouteState.Invalid);
    }

    /// <summary>
    /// Increment update counters
    /// </summary>
    public void IncrementUpdatesSent() => UpdatesSent++;
    public void IncrementUpdatesReceived() => UpdatesReceived++;
    public void IncrementRouteChanges() => RouteChanges++;
}

/// <summary>
/// Represents a single RIP route entry
/// </summary>
public class RipRoute
{
    /// <summary>
    /// Destination network address
    /// </summary>
    public string Network { get; set; } = "";

    /// <summary>
    /// Network subnet mask
    /// </summary>
    public string Mask { get; set; } = "";

    /// <summary>
    /// Next hop IP address
    /// </summary>
    public string NextHop { get; set; } = "";

    /// <summary>
    /// Outgoing interface
    /// </summary>
    public string Interface { get; set; } = "";

    /// <summary>
    /// RIP metric (hop count, 1-16)
    /// </summary>
    public int Metric { get; set; } = 16;

    /// <summary>
    /// Current state of this route
    /// </summary>
    public RipRouteState State { get; set; } = RipRouteState.Valid;

    /// <summary>
    /// Last time this route was updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>
    /// Time when route was marked as invalid
    /// </summary>
    public DateTime InvalidTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Source of this route (for debugging)
    /// </summary>
    public RipRouteSource Source { get; set; } = RipRouteSource.Learned;

    /// <summary>
    /// Check if route is currently valid for forwarding
    /// </summary>
    public bool IsValid => State == RipRouteState.Valid && Metric < 16;

    /// <summary>
    /// Check if route should be advertised with poison reverse
    /// </summary>
    public bool ShouldPoisonReverse(string outInterface)
    {
        return Interface == outInterface && State != RipRouteState.Valid;
    }

    public override string ToString()
    {
        return $"{Network}/{Mask} via {NextHop} dev {Interface} metric {Metric} [{State}]";
    }
}

/// <summary>
/// RIP route states based on RFC 2453
/// </summary>
public enum RipRouteState
{
    /// <summary>
    /// Route is valid and can be used for forwarding
    /// </summary>
    Valid,

    /// <summary>
    /// Route is invalid but still in table (timeout occurred)
    /// </summary>
    Invalid,

    /// <summary>
    /// Route is being held down to prevent loops
    /// </summary>
    Holddown,

    /// <summary>
    /// Route is being flushed from the table
    /// </summary>
    Flushing
}

/// <summary>
/// Source of RIP route information
/// </summary>
public enum RipRouteSource
{
    /// <summary>
    /// Route learned from RIP advertisements
    /// </summary>
    Learned,

    /// <summary>
    /// Route redistributed from other protocols
    /// </summary>
    Redistributed,

    /// <summary>
    /// Route for directly connected networks
    /// </summary>
    Connected,

    /// <summary>
    /// Default route
    /// </summary>
    Default
}