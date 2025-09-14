using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NetForge.Simulation.DataTypes.ValueObjects;

/// <summary>
/// Value object representing a MAC address with validation and formatting
/// </summary>
[JsonConverter(typeof(MacAddressJsonConverter))]
public readonly record struct MacAddress
{
    private readonly string _value;

    /// <summary>
    /// Creates a new MAC address value object
    /// </summary>
    /// <param name="value">The MAC address string</param>
    /// <exception cref="ArgumentException">Thrown when the MAC address is invalid</exception>
    public MacAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MAC address cannot be null or empty", nameof(value));

        if (!IsValidMacAddress(value))
            throw new ArgumentException($"Invalid MAC address format: {value}", nameof(value));

        _value = NormalizeFormat(value.Trim());
    }

    /// <summary>
    /// Gets the normalized string representation of the MAC address (XX:XX:XX:XX:XX:XX format)
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets the MAC address in colon-separated format (XX:XX:XX:XX:XX:XX)
    /// </summary>
    public string ColonSeparated => Value;

    /// <summary>
    /// Gets the MAC address in dash-separated format (XX-XX-XX-XX-XX-XX)
    /// </summary>
    public string DashSeparated => Value.Replace(':', '-');

    /// <summary>
    /// Gets the MAC address in dot-separated Cisco format (XXXX.XXXX.XXXX)
    /// </summary>
    public string CiscoFormat
    {
        get
        {
            var hex = Value.Replace(":", "");
            return $"{hex[..4]}.{hex.Substring(4, 4)}.{hex.Substring(8, 4)}".ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the MAC address as a continuous hex string (XXXXXXXXXXXX)
    /// </summary>
    public string HexString => Value.Replace(":", "");

    /// <summary>
    /// Gets the Organizationally Unique Identifier (OUI) - first 3 bytes
    /// </summary>
    public string Oui => Value[..8]; // First 3 bytes (XX:XX:XX)

    /// <summary>
    /// Gets whether this is a unicast address (LSB of first byte is 0)
    /// </summary>
    public bool IsUnicast => !IsMulticast;

    /// <summary>
    /// Gets whether this is a multicast address (LSB of first byte is 1)
    /// </summary>
    public bool IsMulticast
    {
        get
        {
            if (string.IsNullOrEmpty(_value)) return false;
            var firstByte = Convert.ToByte(_value[..2], 16);
            return (firstByte & 0x01) == 0x01;
        }
    }

    /// <summary>
    /// Gets whether this is a locally administered address
    /// </summary>
    public bool IsLocallyAdministered
    {
        get
        {
            if (string.IsNullOrEmpty(_value)) return false;
            var firstByte = Convert.ToByte(_value[..2], 16);
            return (firstByte & 0x02) == 0x02;
        }
    }

    /// <summary>
    /// Gets whether this is a globally unique address
    /// </summary>
    public bool IsGloballyUnique => !IsLocallyAdministered;

    /// <summary>
    /// Gets whether this is a broadcast MAC address (FF:FF:FF:FF:FF:FF)
    /// </summary>
    public bool IsBroadcast => Value.Equals("FF:FF:FF:FF:FF:FF", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates whether a string is a valid MAC address
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if the string is a valid MAC address</returns>
    public static bool IsValidMacAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Support different MAC address formats
        var patterns = new[]
        {
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", // XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX
            @"^[0-9A-Fa-f]{12}$", // XXXXXXXXXXXX
            @"^([0-9A-Fa-f]{4}\.){2}[0-9A-Fa-f]{4}$" // XXXX.XXXX.XXXX (Cisco format)
        };

        return patterns.Any(pattern => Regex.IsMatch(value, pattern));
    }

    /// <summary>
    /// Normalizes MAC address format to colon-separated uppercase
    /// </summary>
    private static string NormalizeFormat(string value)
    {
        // Remove all separators and convert to uppercase
        var hex = Regex.Replace(value, "[:-.]", "").ToUpperInvariant();

        // Insert colons every 2 characters
        return string.Join(":", Enumerable.Range(0, 6).Select(i => hex.Substring(i * 2, 2)));
    }

    /// <summary>
    /// Attempts to parse a MAC address from a string
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <param name="macAddress">The parsed MAC address if successful</param>
    /// <returns>True if parsing was successful</returns>
    public static bool TryParse(string? value, out MacAddress macAddress)
    {
        if (IsValidMacAddress(value))
        {
            macAddress = new MacAddress(value!);
            return true;
        }

        macAddress = default;
        return false;
    }

    /// <summary>
    /// Parses a MAC address from a string, throwing an exception if invalid
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>The parsed MAC address</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid MAC address</exception>
    public static MacAddress Parse(string value) => new(value);

    /// <summary>
    /// Generates a random MAC address
    /// </summary>
    /// <param name="locallyAdministered">Whether to set the locally administered bit</param>
    /// <returns>A randomly generated MAC address</returns>
    public static MacAddress GenerateRandom(bool locallyAdministered = true)
    {
        var random = new Random();
        var bytes = new byte[6];
        random.NextBytes(bytes);

        if (locallyAdministered)
        {
            bytes[0] |= 0x02; // Set locally administered bit
        }
        else
        {
            bytes[0] &= 0xFD; // Clear locally administered bit
        }

        bytes[0] &= 0xFE; // Clear multicast bit (ensure unicast)

        return new MacAddress(string.Join(":", bytes.Select(b => b.ToString("X2"))));
    }

    /// <summary>
    /// Implicit conversion from MacAddress to string
    /// </summary>
    public static implicit operator string(MacAddress macAddress) => macAddress.Value;

    /// <summary>
    /// Explicit conversion from string to MacAddress
    /// </summary>
    public static explicit operator MacAddress(string value) => new(value);

    /// <summary>
    /// Returns the string representation of the MAC address
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Common MAC addresses for convenience
    /// </summary>
    public static class Common
    {
        public static readonly MacAddress Broadcast = new("FF:FF:FF:FF:FF:FF");
        public static readonly MacAddress Zero = new("00:00:00:00:00:00");

        // IEEE Registration Authority OUIs for common vendors
        public static readonly string CiscoOui = "00:1B:53";
        public static readonly string IntelOui = "00:15:17";
        public static readonly string DellOui = "00:14:22";
        public static readonly string HpOui = "00:1F:29";
        public static readonly string VmwareOui = "00:50:56";
    }
}

/// <summary>
/// JSON converter for MacAddress value objects
/// </summary>
public class MacAddressJsonConverter : JsonConverter<MacAddress>
{
    public override MacAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? default : new MacAddress(value);
    }

    public override void Write(Utf8JsonWriter writer, MacAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}