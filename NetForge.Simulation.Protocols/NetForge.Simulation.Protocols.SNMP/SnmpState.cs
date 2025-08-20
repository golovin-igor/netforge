using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpState : BaseProtocolState
{
    public bool AgentRunning { get; set; } = false;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public long TotalRequests => _totalRequests;
    public long TotalResponses => _totalResponses;
    public long TotalErrors => _totalErrors;
    public long TotalTraps => _totalTraps;

    public Dictionary<string, SnmpVariable> MibDatabase { get; set; } = new();
    public Dictionary<string, DateTime> LastRequestTime { get; set; } = new();
    public Dictionary<string, int> CommunityAccess { get; set; } = new();

    public int ActiveSessions { get; set; } = 0;
    public DateTime LastActivity { get; set; } = DateTime.MinValue;

    public TimeSpan SystemUpTime => DateTime.Now - StartTime;

    public override Dictionary<string, object> GetStateData()
    {
        var baseData = base.GetStateData();
        baseData["AgentRunning"] = AgentRunning;
        baseData["StartTime"] = StartTime;
        baseData["SystemUpTime"] = SystemUpTime.TotalMilliseconds;
        baseData["TotalRequests"] = TotalRequests;
        baseData["TotalResponses"] = TotalResponses;
        baseData["TotalErrors"] = TotalErrors;
        baseData["TotalTraps"] = TotalTraps;
        baseData["MibVariables"] = MibDatabase.Count;
        baseData["ActiveSessions"] = ActiveSessions;
        baseData["LastActivity"] = LastActivity;
        return baseData;
    }

    private long _totalRequests = 0;
    private long _totalResponses = 0; 
    private long _totalErrors = 0;
    private long _totalTraps = 0;
    
    public void IncrementRequests() => Interlocked.Increment(ref _totalRequests);
    public void IncrementResponses() => Interlocked.Increment(ref _totalResponses);
    public void IncrementErrors() => Interlocked.Increment(ref _totalErrors);
    public void IncrementTraps() => Interlocked.Increment(ref _totalTraps);

    public void UpdateActivity()
    {
        LastActivity = DateTime.Now;
        MarkStateChanged();
    }
}

public class SnmpVariable
{
    public string Oid { get; set; } = "";
    public string Name { get; set; } = "";
    public object Value { get; set; } = "";
    public SnmpType Type { get; set; } = SnmpType.OctetString;
    public bool IsReadOnly { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public SnmpVariable() { }

    public SnmpVariable(string oid, string name, object value, SnmpType type = SnmpType.OctetString, bool isReadOnly = true)
    {
        Oid = oid;
        Name = name;
        Value = value;
        Type = type;
        IsReadOnly = isReadOnly;
        LastUpdated = DateTime.Now;
    }
}

public enum SnmpType
{
    Integer,
    OctetString,
    Null,
    ObjectIdentifier,
    Sequence,
    IpAddress,
    Counter,
    Gauge,
    TimeTicks,
    Opaque
}

public class SnmpRequest
{
    public string Community { get; set; } = "";
    public SnmpRequestType RequestType { get; set; }
    public string Oid { get; set; } = "";
    public object Value { get; set; } = "";
    public string ClientAddress { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int RequestId { get; set; }
}

public enum SnmpRequestType
{
    Get,
    GetNext,
    GetBulk,
    Set,
    Trap,
    Response
}

public class SnmpResponse
{
    public int RequestId { get; set; }
    public SnmpErrorStatus ErrorStatus { get; set; } = SnmpErrorStatus.NoError;
    public int ErrorIndex { get; set; } = 0;
    public List<SnmpVarBind> VarBinds { get; set; } = new();
}

public class SnmpVarBind
{
    public string Oid { get; set; } = "";
    public object Value { get; set; } = "";
    public SnmpType Type { get; set; } = SnmpType.OctetString;
}

public enum SnmpErrorStatus
{
    NoError = 0,
    TooBig = 1,
    NoSuchName = 2,
    BadValue = 3,
    ReadOnly = 4,
    GenErr = 5
}