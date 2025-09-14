using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetForge.Simulation.DataTypes.ValueObjects;

/// <summary>
/// Value object representing a network port number with validation and protocol information
/// </summary>
[JsonConverter(typeof(PortNumberJsonConverter))]
public readonly record struct PortNumber
{
    private readonly int _value;

    /// <summary>
    /// Creates a new port number value object
    /// </summary>
    /// <param name="value">The port number (1-65535)</param>
    /// <exception cref="ArgumentException">Thrown when the port number is invalid</exception>
    public PortNumber(int value)
    {
        if (value < 1 || value > 65535)
            throw new ArgumentException($"Port number must be between 1 and 65535 (got {value})", nameof(value));

        _value = value;
    }

    /// <summary>
    /// Gets the port number value
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// Gets whether this is a well-known port (1-1023)
    /// </summary>
    public bool IsWellKnown => _value <= 1023;

    /// <summary>
    /// Gets whether this is a registered port (1024-49151)
    /// </summary>
    public bool IsRegistered => _value >= 1024 && _value <= 49151;

    /// <summary>
    /// Gets whether this is a dynamic/private port (49152-65535)
    /// </summary>
    public bool IsDynamic => _value >= 49152;

    /// <summary>
    /// Gets whether this is a privileged port (requires admin/root to bind on Unix systems)
    /// </summary>
    public bool IsPrivileged => _value < 1024;

    /// <summary>
    /// Gets the port range category
    /// </summary>
    public PortRange Range
    {
        get
        {
            if (IsWellKnown) return PortRange.WellKnown;
            if (IsRegistered) return PortRange.Registered;
            return PortRange.Dynamic;
        }
    }

    /// <summary>
    /// Gets the common service name for well-known ports, if available
    /// </summary>
    public string? ServiceName => GetServiceName(_value);

    /// <summary>
    /// Gets the common protocol(s) for well-known ports, if available
    /// </summary>
    public string? Protocol => GetProtocol(_value);

    /// <summary>
    /// Validates whether a number is a valid port number
    /// </summary>
    /// <param name="value">The number to validate</param>
    /// <returns>True if the number is a valid port number</returns>
    public static bool IsValidPort(int value)
    {
        return value >= 1 && value <= 65535;
    }

    /// <summary>
    /// Attempts to parse a port number from a string
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <param name="portNumber">The parsed port number if successful</param>
    /// <returns>True if parsing was successful</returns>
    public static bool TryParse(string? value, out PortNumber portNumber)
    {
        if (int.TryParse(value, out var intValue) && IsValidPort(intValue))
        {
            portNumber = new PortNumber(intValue);
            return true;
        }

        portNumber = default;
        return false;
    }

    /// <summary>
    /// Parses a port number from a string, throwing an exception if invalid
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>The parsed port number</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid port number</exception>
    public static PortNumber Parse(string value)
    {
        if (!int.TryParse(value, out var intValue))
            throw new ArgumentException($"Invalid port number format: {value}", nameof(value));

        return new PortNumber(intValue);
    }

    /// <summary>
    /// Implicit conversion from PortNumber to int
    /// </summary>
    public static implicit operator int(PortNumber portNumber) => portNumber._value;

    /// <summary>
    /// Explicit conversion from int to PortNumber
    /// </summary>
    public static explicit operator PortNumber(int value) => new(value);

    /// <summary>
    /// Implicit conversion from PortNumber to string
    /// </summary>
    public static implicit operator string(PortNumber portNumber) => portNumber._value.ToString();

    /// <summary>
    /// Returns the string representation of the port number
    /// </summary>
    public override string ToString() => _value.ToString();

    private static string? GetServiceName(int port)
    {
        return port switch
        {
            20 => "FTP-DATA",
            21 => "FTP",
            22 => "SSH",
            23 => "Telnet",
            25 => "SMTP",
            53 => "DNS",
            67 => "DHCP Server",
            68 => "DHCP Client",
            69 => "TFTP",
            80 => "HTTP",
            110 => "POP3",
            123 => "NTP",
            143 => "IMAP",
            161 => "SNMP",
            162 => "SNMP Trap",
            179 => "BGP",
            389 => "LDAP",
            443 => "HTTPS",
            445 => "SMB",
            465 => "SMTPS",
            514 => "Syslog",
            515 => "LPD",
            520 => "RIP",
            521 => "RIPng",
            546 => "DHCPv6 Client",
            547 => "DHCPv6 Server",
            587 => "SMTP (Submission)",
            631 => "IPP",
            636 => "LDAPS",
            993 => "IMAPS",
            995 => "POP3S",
            1433 => "MSSQL",
            1521 => "Oracle",
            1723 => "PPTP",
            3306 => "MySQL",
            3389 => "RDP",
            5432 => "PostgreSQL",
            5500 => "VNC",
            5900 => "VNC",
            6379 => "Redis",
            8080 => "HTTP Alt",
            8443 => "HTTPS Alt",
            _ => null
        };
    }

    private static string? GetProtocol(int port)
    {
        return port switch
        {
            20 or 21 or 22 or 23 or 25 or 80 or 110 or 143 or 179 or 389 or 443 or 445 or 465 or 587 or 631 or 636 or 993 or 995 or 1433 or 1521 or 1723 or 3306 or 3389 or 5432 or 8080 or 8443 => "TCP",
            53 or 67 or 68 or 69 or 123 or 161 or 162 or 514 or 520 or 521 or 546 or 547 => "UDP",
            _ => null
        };
    }

    /// <summary>
    /// Port range categories
    /// </summary>
    public enum PortRange
    {
        /// <summary>
        /// Well-known ports (1-1023) - Reserved for system services
        /// </summary>
        WellKnown,

        /// <summary>
        /// Registered ports (1024-49151) - Assigned by IANA
        /// </summary>
        Registered,

        /// <summary>
        /// Dynamic/Private ports (49152-65535) - Available for any use
        /// </summary>
        Dynamic
    }

    /// <summary>
    /// Common port numbers for convenience
    /// </summary>
    public static class Common
    {
        // File Transfer
        public static readonly PortNumber FtpData = new(20);
        public static readonly PortNumber Ftp = new(21);
        public static readonly PortNumber Sftp = new(22); // SSH File Transfer
        public static readonly PortNumber Tftp = new(69);

        // Remote Access
        public static readonly PortNumber Ssh = new(22);
        public static readonly PortNumber Telnet = new(23);
        public static readonly PortNumber Rdp = new(3389);
        public static readonly PortNumber Vnc = new(5900);

        // Web Services
        public static readonly PortNumber Http = new(80);
        public static readonly PortNumber Https = new(443);
        public static readonly PortNumber HttpAlt = new(8080);
        public static readonly PortNumber HttpsAlt = new(8443);

        // Email
        public static readonly PortNumber Smtp = new(25);
        public static readonly PortNumber SmtpSubmission = new(587);
        public static readonly PortNumber Smtps = new(465);
        public static readonly PortNumber Pop3 = new(110);
        public static readonly PortNumber Pop3s = new(995);
        public static readonly PortNumber Imap = new(143);
        public static readonly PortNumber Imaps = new(993);

        // Network Services
        public static readonly PortNumber Dns = new(53);
        public static readonly PortNumber DhcpServer = new(67);
        public static readonly PortNumber DhcpClient = new(68);
        public static readonly PortNumber Ntp = new(123);
        public static readonly PortNumber Snmp = new(161);
        public static readonly PortNumber SnmpTrap = new(162);
        public static readonly PortNumber Syslog = new(514);

        // Routing Protocols
        public static readonly PortNumber Bgp = new(179);
        public static readonly PortNumber Rip = new(520);
        public static readonly PortNumber RipNg = new(521);

        // Directory Services
        public static readonly PortNumber Ldap = new(389);
        public static readonly PortNumber Ldaps = new(636);

        // Databases
        public static readonly PortNumber MsSql = new(1433);
        public static readonly PortNumber Oracle = new(1521);
        public static readonly PortNumber MySql = new(3306);
        public static readonly PortNumber PostgreSql = new(5432);
        public static readonly PortNumber Redis = new(6379);

        // File Sharing
        public static readonly PortNumber Smb = new(445);
        public static readonly PortNumber Lpd = new(515);
        public static readonly PortNumber Ipp = new(631);

        // VPN
        public static readonly PortNumber Pptp = new(1723);

        // IPv6 DHCP
        public static readonly PortNumber DhcpV6Client = new(546);
        public static readonly PortNumber DhcpV6Server = new(547);
    }
}

/// <summary>
/// JSON converter for PortNumber value objects
/// </summary>
public class PortNumberJsonConverter : JsonConverter<PortNumber>
{
    public override PortNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt32();
        return new PortNumber(value);
    }

    public override void Write(Utf8JsonWriter writer, PortNumber value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}