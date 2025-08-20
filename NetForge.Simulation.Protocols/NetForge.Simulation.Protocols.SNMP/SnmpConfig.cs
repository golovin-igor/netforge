using System.ComponentModel.DataAnnotations;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpConfig : ICloneable
{
    [Required]
    public bool IsEnabled { get; set; } = true;

    [Range(1, 65535)]
    public int Port { get; set; } = 161;

    [Range(1, 65535)]
    public int TrapPort { get; set; } = 162;

    public List<string> ReadCommunities { get; set; } = new() { "public" };
    public List<string> WriteCommunities { get; set; } = new() { "private" };

    public string SystemDescription { get; set; } = "NetForge Simulated Device";
    public string SystemContact { get; set; } = "";
    public string SystemLocation { get; set; } = "";

    public List<string> TrapDestinations { get; set; } = new();

    public bool EnableTraps { get; set; } = true;
    public bool EnableAuthentication { get; set; } = true;

    [Range(1, 86400)]
    public int SystemUpTimeRefresh { get; set; } = 60;

    public Dictionary<string, string> CustomOids { get; set; } = new();

    public object Clone()
    {
        return new SnmpConfig
        {
            IsEnabled = IsEnabled,
            Port = Port,
            TrapPort = TrapPort,
            ReadCommunities = new List<string>(ReadCommunities),
            WriteCommunities = new List<string>(WriteCommunities),
            SystemDescription = SystemDescription,
            SystemContact = SystemContact,
            SystemLocation = SystemLocation,
            TrapDestinations = new List<string>(TrapDestinations),
            EnableTraps = EnableTraps,
            EnableAuthentication = EnableAuthentication,
            SystemUpTimeRefresh = SystemUpTimeRefresh,
            CustomOids = new Dictionary<string, string>(CustomOids)
        };
    }

    public void Validate()
    {
        if (Port <= 0 || Port > 65535)
            throw new ArgumentOutOfRangeException(nameof(Port), "Port must be between 1 and 65535");

        if (TrapPort <= 0 || TrapPort > 65535)
            throw new ArgumentOutOfRangeException(nameof(TrapPort), "TrapPort must be between 1 and 65535");

        if (ReadCommunities.Count == 0)
            throw new InvalidOperationException("At least one read community must be specified");

        if (SystemUpTimeRefresh <= 0)
            throw new ArgumentOutOfRangeException(nameof(SystemUpTimeRefresh), "SystemUpTimeRefresh must be greater than 0");
    }
}