using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetForge.Simulation.DataTypes.ValueObjects;

namespace NetForge.Simulation.DataTypes.ValueObjects;

/// <summary>
/// Value object representing a network prefix in CIDR notation with validation and network operations
/// </summary>
[JsonConverter(typeof(NetworkPrefixJsonConverter))]
public readonly record struct NetworkPrefix
{
    private readonly IpAddress _address;
    private readonly int _prefixLength;

    /// <summary>
    /// Creates a new network prefix value object
    /// </summary>
    /// <param name="cidr">The CIDR notation string (e.g., "192.168.1.0/24")</param>
    /// <exception cref="ArgumentException">Thrown when the CIDR notation is invalid</exception>
    public NetworkPrefix(string cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr))
            throw new ArgumentException("Network prefix cannot be null or empty", nameof(cidr));

        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid CIDR notation: {cidr}. Expected format: 'address/prefix'", nameof(cidr));

        _address = new IpAddress(parts[0]);

        if (!int.TryParse(parts[1], out _prefixLength))
            throw new ArgumentException($"Invalid prefix length: {parts[1]}", nameof(cidr));

        ValidatePrefixLength(_prefixLength, _address.Version);
    }

    /// <summary>
    /// Creates a new network prefix from an IP address and prefix length
    /// </summary>
    /// <param name="address">The network address</param>
    /// <param name="prefixLength">The prefix length</param>
    /// <exception cref="ArgumentException">Thrown when the prefix length is invalid for the address version</exception>
    public NetworkPrefix(IpAddress address, int prefixLength)
    {
        _address = address;
        _prefixLength = prefixLength;

        ValidatePrefixLength(prefixLength, address.Version);
    }

    /// <summary>
    /// Gets the network address
    /// </summary>
    public IpAddress Address => _address;

    /// <summary>
    /// Gets the prefix length
    /// </summary>
    public int PrefixLength => _prefixLength;

    /// <summary>
    /// Gets the IP version (4 or 6)
    /// </summary>
    public int Version => _address.Version;

    /// <summary>
    /// Gets the network address (first address in the range)
    /// </summary>
    public IpAddress NetworkAddress
    {
        get
        {
            if (!IPAddress.TryParse(_address.Value, out var addr))
                return _address;

            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = addr.GetAddressBytes();
                var mask = GetSubnetMaskBytes();

                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] & mask[i]);
                }

                return new IpAddress(new IPAddress(bytes).ToString());
            }

            return _address; // IPv6 network calculation more complex, return as-is for now
        }
    }

    /// <summary>
    /// Gets the broadcast address (last address in the range for IPv4)
    /// </summary>
    public IpAddress BroadcastAddress
    {
        get
        {
            if (!IPAddress.TryParse(_address.Value, out var addr) ||
                addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return _address;

            var bytes = addr.GetAddressBytes();
            var mask = GetSubnetMaskBytes();

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] | (~mask[i]));
            }

            return new IpAddress(new IPAddress(bytes).ToString());
        }
    }

    /// <summary>
    /// Gets the subnet mask as an IP address
    /// </summary>
    public IpAddress SubnetMask
    {
        get
        {
            if (Version == 4)
            {
                var mask = GetSubnetMaskBytes();
                return new IpAddress(new IPAddress(mask).ToString());
            }

            // IPv6 doesn't have traditional subnet masks
            throw new InvalidOperationException("Subnet mask is not applicable to IPv6 addresses");
        }
    }

    /// <summary>
    /// Gets the number of host addresses in this network
    /// </summary>
    public long HostCount
    {
        get
        {
            if (Version == 4)
            {
                int hostBits = 32 - _prefixLength;
                if (hostBits == 0) return 1; // Single host
                return (1L << hostBits) - 2; // Subtract network and broadcast addresses
            }
            else
            {
                int hostBits = 128 - _prefixLength;
                // For IPv6, return a simplified calculation for reasonable prefix lengths
                if (hostBits > 63) return long.MaxValue; // Too large to represent
                return 1L << hostBits;
            }
        }
    }

    /// <summary>
    /// Checks if the specified IP address is within this network prefix
    /// </summary>
    /// <param name="address">The IP address to check</param>
    /// <returns>True if the address is within this network</returns>
    public bool Contains(IpAddress address)
    {
        if (!IPAddress.TryParse(_address.Value, out var networkAddr) ||
            !IPAddress.TryParse(address.Value, out var testAddr))
            return false;

        if (networkAddr.AddressFamily != testAddr.AddressFamily)
            return false;

        var networkBytes = networkAddr.GetAddressBytes();
        var testBytes = testAddr.GetAddressBytes();

        if (Version == 4)
        {
            var mask = GetSubnetMaskBytes();
            for (int i = 0; i < networkBytes.Length; i++)
            {
                if ((networkBytes[i] & mask[i]) != (testBytes[i] & mask[i]))
                    return false;
            }
        }
        else
        {
            // IPv6 comparison
            int byteCount = _prefixLength / 8;
            int bitCount = _prefixLength % 8;

            // Check complete bytes
            for (int i = 0; i < byteCount; i++)
            {
                if (networkBytes[i] != testBytes[i])
                    return false;
            }

            // Check partial byte if any
            if (bitCount > 0 && byteCount < networkBytes.Length)
            {
                byte mask = (byte)(0xFF << (8 - bitCount));
                if ((networkBytes[byteCount] & mask) != (testBytes[byteCount] & mask))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if this network overlaps with another network
    /// </summary>
    /// <param name="other">The other network to check</param>
    /// <returns>True if the networks overlap</returns>
    public bool OverlapsWith(NetworkPrefix other)
    {
        return Contains(other.NetworkAddress) ||
               other.Contains(NetworkAddress) ||
               Contains(other.BroadcastAddress) ||
               other.Contains(BroadcastAddress);
    }

    /// <summary>
    /// Gets all subnets when this network is subdivided
    /// </summary>
    /// <param name="newPrefixLength">The new prefix length for subnets</param>
    /// <returns>Enumerable of subnet prefixes</returns>
    /// <exception cref="ArgumentException">Thrown when new prefix length is not larger than current</exception>
    public IEnumerable<NetworkPrefix> GetSubnets(int newPrefixLength)
    {
        if (newPrefixLength <= _prefixLength)
            throw new ArgumentException("New prefix length must be larger than current prefix length");

        ValidatePrefixLength(newPrefixLength, Version);

        var subnets = new List<NetworkPrefix>();
        int subnetCount = 1 << (newPrefixLength - _prefixLength);

        if (Version == 4)
        {
            var networkBytes = NetworkAddress.Value;
            if (!IPAddress.TryParse(networkBytes, out var networkAddr))
                return subnets;

            var bytes = networkAddr.GetAddressBytes();
            uint network = BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
            uint increment = (uint)(1 << (32 - newPrefixLength));

            for (int i = 0; i < subnetCount; i++)
            {
                var subnetBytes = BitConverter.GetBytes(network + (uint)(i * increment)).Reverse().ToArray();
                var subnetAddr = new IPAddress(subnetBytes);
                subnets.Add(new NetworkPrefix(new IpAddress(subnetAddr.ToString()), newPrefixLength));
            }
        }

        return subnets;
    }

    /// <summary>
    /// Validates whether a string is a valid CIDR notation
    /// </summary>
    /// <param name="cidr">The string to validate</param>
    /// <returns>True if the string is valid CIDR notation</returns>
    public static bool IsValidCidr(string? cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr))
            return false;

        try
        {
            _ = new NetworkPrefix(cidr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to parse CIDR notation from a string
    /// </summary>
    /// <param name="cidr">The string to parse</param>
    /// <param name="networkPrefix">The parsed network prefix if successful</param>
    /// <returns>True if parsing was successful</returns>
    public static bool TryParse(string? cidr, out NetworkPrefix networkPrefix)
    {
        if (IsValidCidr(cidr))
        {
            networkPrefix = new NetworkPrefix(cidr!);
            return true;
        }

        networkPrefix = default;
        return false;
    }

    /// <summary>
    /// Parses CIDR notation from a string, throwing an exception if invalid
    /// </summary>
    /// <param name="cidr">The string to parse</param>
    /// <returns>The parsed network prefix</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not valid CIDR notation</exception>
    public static NetworkPrefix Parse(string cidr) => new(cidr);

    /// <summary>
    /// Implicit conversion from NetworkPrefix to string
    /// </summary>
    public static implicit operator string(NetworkPrefix prefix) => prefix.ToString();

    /// <summary>
    /// Explicit conversion from string to NetworkPrefix
    /// </summary>
    public static explicit operator NetworkPrefix(string cidr) => new(cidr);

    /// <summary>
    /// Returns the string representation of the network prefix in CIDR notation
    /// </summary>
    public override string ToString() => $"{_address}/{_prefixLength}";

    private static void ValidatePrefixLength(int prefixLength, int ipVersion)
    {
        int maxLength = ipVersion == 4 ? 32 : 128;

        if (prefixLength < 0 || prefixLength > maxLength)
            throw new ArgumentException($"Invalid prefix length for IPv{ipVersion}: {prefixLength}. Must be between 0 and {maxLength}");
    }

    private byte[] GetSubnetMaskBytes()
    {
        if (Version != 4)
            throw new InvalidOperationException("Subnet mask bytes only applicable to IPv4");

        uint mask = 0xFFFFFFFF;
        mask = mask << (32 - _prefixLength);

        return new[]
        {
            (byte)((mask >> 24) & 0xFF),
            (byte)((mask >> 16) & 0xFF),
            (byte)((mask >> 8) & 0xFF),
            (byte)(mask & 0xFF)
        };
    }

    /// <summary>
    /// Common network prefixes for convenience
    /// </summary>
    public static class Common
    {
        // Private network ranges (RFC 1918)
        public static readonly NetworkPrefix PrivateClassA = new("10.0.0.0/8");
        public static readonly NetworkPrefix PrivateClassB = new("172.16.0.0/12");
        public static readonly NetworkPrefix PrivateClassC = new("192.168.0.0/16");

        // Loopback
        public static readonly NetworkPrefix LoopbackV4 = new("127.0.0.0/8");
        public static readonly NetworkPrefix LoopbackV6 = new("::1/128");

        // Link-local
        public static readonly NetworkPrefix LinkLocalV4 = new("169.254.0.0/16");
        public static readonly NetworkPrefix LinkLocalV6 = new("fe80::/10");

        // Multicast
        public static readonly NetworkPrefix MulticastV4 = new("224.0.0.0/4");
        public static readonly NetworkPrefix MulticastV6 = new("ff00::/8");

        // Documentation (RFC 5737)
        public static readonly NetworkPrefix TestNet1 = new("192.0.2.0/24");
        public static readonly NetworkPrefix TestNet2 = new("198.51.100.0/24");
        public static readonly NetworkPrefix TestNet3 = new("203.0.113.0/24");
    }
}

/// <summary>
/// JSON converter for NetworkPrefix value objects
/// </summary>
public class NetworkPrefixJsonConverter : JsonConverter<NetworkPrefix>
{
    public override NetworkPrefix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return string.IsNullOrEmpty(value) ? default : new NetworkPrefix(value);
    }

    public override void Write(Utf8JsonWriter writer, NetworkPrefix value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}