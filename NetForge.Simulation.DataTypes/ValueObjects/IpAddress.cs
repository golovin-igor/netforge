using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetForge.Simulation.DataTypes.ValueObjects;

/// <summary>
/// Value object representing an IP address with validation and formatting
/// </summary>
[JsonConverter(typeof(IpAddressJsonConverter))]
public readonly record struct IpAddress
{
    private readonly string _value;

    /// <summary>
    /// Creates a new IP address value object
    /// </summary>
    /// <param name="value">The IP address string</param>
    /// <exception cref="ArgumentException">Thrown when the IP address is invalid</exception>
    public IpAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IP address cannot be null or empty", nameof(value));

        if (!IsValidIpAddress(value))
            throw new ArgumentException($"Invalid IP address format: {value}", nameof(value));

        _value = value.Trim();
    }

    /// <summary>
    /// Gets the string representation of the IP address
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets whether this is a loopback address (127.x.x.x or ::1)
    /// </summary>
    public bool IsLoopback => IPAddress.TryParse(_value, out var addr) && IPAddress.IsLoopback(addr);

    /// <summary>
    /// Gets whether this is a private address (RFC 1918)
    /// </summary>
    public bool IsPrivate
    {
        get
        {
            if (!IPAddress.TryParse(_value, out var addr) || addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            var bytes = addr.GetAddressBytes();
            return (bytes[0] == 10) ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }
    }

    /// <summary>
    /// Gets whether this is a multicast address
    /// </summary>
    public bool IsMulticast => IPAddress.TryParse(_value, out var addr) &&
                              ((addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                addr.GetAddressBytes()[0] >= 224 && addr.GetAddressBytes()[0] <= 239) ||
                               (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                                addr.IsIPv6Multicast));

    /// <summary>
    /// Gets the IP address version (4 or 6)
    /// </summary>
    public int Version => IPAddress.TryParse(_value, out var addr) ?
        (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 6) : 0;

    /// <summary>
    /// Validates whether a string is a valid IP address
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if the string is a valid IP address</returns>
    public static bool IsValidIpAddress(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && IPAddress.TryParse(value, out _);
    }

    /// <summary>
    /// Attempts to parse an IP address from a string
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <param name="ipAddress">The parsed IP address if successful</param>
    /// <returns>True if parsing was successful</returns>
    public static bool TryParse(string? value, out IpAddress ipAddress)
    {
        if (IsValidIpAddress(value))
        {
            ipAddress = new IpAddress(value!);
            return true;
        }

        ipAddress = default;
        return false;
    }

    /// <summary>
    /// Parses an IP address from a string, throwing an exception if invalid
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>The parsed IP address</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid IP address</exception>
    public static IpAddress Parse(string value) => new(value);

    /// <summary>
    /// Implicit conversion from IpAddress to string
    /// </summary>
    public static implicit operator string(IpAddress ipAddress) => ipAddress.Value;

    /// <summary>
    /// Explicit conversion from string to IpAddress
    /// </summary>
    public static explicit operator IpAddress(string value) => new(value);

    /// <summary>
    /// Conversion to System.Net.IPAddress
    /// </summary>
    public static implicit operator IPAddress(IpAddress ipAddress) => IPAddress.Parse(ipAddress.Value);

    /// <summary>
    /// Conversion from System.Net.IPAddress
    /// </summary>
    public static implicit operator IpAddress(IPAddress ipAddress) => new(ipAddress.ToString());

    /// <summary>
    /// Returns the string representation of the IP address
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Checks if this IP address is in the same subnet as another
    /// </summary>
    /// <param name="other">The other IP address</param>
    /// <param name="subnetMask">The subnet mask</param>
    /// <returns>True if both addresses are in the same subnet</returns>
    public bool IsInSameSubnet(IpAddress other, IpAddress subnetMask)
    {
        if (!IPAddress.TryParse(Value, out var thisAddr) ||
            !IPAddress.TryParse(other.Value, out var otherAddr) ||
            !IPAddress.TryParse(subnetMask.Value, out var maskAddr))
            return false;

        if (thisAddr.AddressFamily != otherAddr.AddressFamily)
            return false;

        var thisBytes = thisAddr.GetAddressBytes();
        var otherBytes = otherAddr.GetAddressBytes();
        var maskBytes = maskAddr.GetAddressBytes();

        for (int i = 0; i < thisBytes.Length; i++)
        {
            if ((thisBytes[i] & maskBytes[i]) != (otherBytes[i] & maskBytes[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Common IP addresses for convenience
    /// </summary>
    public static class Common
    {
        public static readonly IpAddress Loopback = new("127.0.0.1");
        public static readonly IpAddress LoopbackV6 = new("::1");
        public static readonly IpAddress Any = new("0.0.0.0");
        public static readonly IpAddress AnyV6 = new("::");
        public static readonly IpAddress Broadcast = new("255.255.255.255");
        public static readonly IpAddress GoogleDns = new("8.8.8.8");
        public static readonly IpAddress CloudflareDns = new("1.1.1.1");
    }
}

/// <summary>
/// JSON converter for IpAddress value objects
/// </summary>
public class IpAddressJsonConverter : JsonConverter<IpAddress>
{
    public override IpAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? default : new IpAddress(value);
    }

    public override void Write(Utf8JsonWriter writer, IpAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}