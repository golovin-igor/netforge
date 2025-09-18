using NetForge.Interfaces.Devices;
using NetForge.Simulation.Protocols.Common.Base;
using System.Text;
using System.Net;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;

namespace NetForge.Simulation.Protocols.CDP;

public class CdpTlvProcessor
{
    public List<CdpTlv> BuildTlvs(INetworkDevice device, string interfaceName, CdpConfig cdpConfig = null)
    {
        var tlvs = new List<CdpTlv>();

        // Device ID TLV (Type 1) - Required
        var deviceId = cdpConfig?.DeviceId ?? device.Name;
        tlvs.Add(BuildDeviceIdTlv(deviceId));

        // Address TLV (Type 2) - Management addresses
        var addresses = GetManagementAddresses(device);
        if (addresses.Any())
        {
            tlvs.Add(BuildAddressTlv(addresses));
        }

        // Port ID TLV (Type 3) - Required
        tlvs.Add(BuildPortIdTlv(interfaceName));

        // Capabilities TLV (Type 4) - Required
        tlvs.Add(BuildCapabilitiesTlv(device, cdpConfig));

        // Version TLV (Type 5) - Software version
        tlvs.Add(BuildVersionTlv(cdpConfig));

        // Platform TLV (Type 6) - Hardware platform
        tlvs.Add(BuildPlatformTlv(device, cdpConfig));

        // VTP Management Domain TLV (Type 9) - For switches
        if (cdpConfig?.Capabilities?.Contains("Switch") == true || device.DeviceType == "Switch")
        {
            tlvs.Add(BuildVtpManagementDomainTlv(""));
        }

        // Native VLAN TLV (Type 10) - For trunking interfaces
        var interfaceConfig = device.GetInterface(interfaceName);
        if (interfaceConfig != null && IsInterfaceTrunking(interfaceConfig))
        {
            tlvs.Add(BuildNativeVlanTlv(1)); // Default VLAN 1
        }

        // Duplex TLV (Type 11)
        tlvs.Add(BuildDuplexTlv(interfaceConfig));

        // MTU TLV (Type 17)
        tlvs.Add(BuildMtuTlv(interfaceConfig));

        // System Name TLV (Type 20)
        tlvs.Add(BuildSystemNameTlv(device.Name));

        return tlvs;
    }

    public void ProcessReceivedTlvs(CdpNeighbor neighbor, List<CdpTlv> tlvs)
    {
        foreach (var tlv in tlvs)
        {
            neighbor.Tlvs[tlv.Type] = tlv;

            switch (tlv.Type)
            {
                case CdpTlvType.DeviceId:
                    ProcessDeviceIdTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Address:
                    ProcessAddressTlv(neighbor, tlv);
                    break;

                case CdpTlvType.PortId:
                    ProcessPortIdTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Capabilities:
                    ProcessCapabilitiesTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Version:
                    ProcessVersionTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Platform:
                    ProcessPlatformTlv(neighbor, tlv);
                    break;

                case CdpTlvType.NativeVlan:
                    ProcessNativeVlanTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Duplex:
                    ProcessDuplexTlv(neighbor, tlv);
                    break;

                case CdpTlvType.Mtu:
                    ProcessMtuTlv(neighbor, tlv);
                    break;

                case CdpTlvType.SystemName:
                    ProcessSystemNameTlv(neighbor, tlv);
                    break;
            }
        }
    }

    private CdpTlv BuildDeviceIdTlv(string deviceId)
    {
        return new CdpTlv
        {
            Type = CdpTlvType.DeviceId,
            Length = (ushort)(4 + deviceId.Length),
            Value = Encoding.ASCII.GetBytes(deviceId)
        };
    }

    private CdpTlv BuildAddressTlv(List<string> addresses)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Number of addresses
        writer.Write((uint)addresses.Count);

        foreach (var address in addresses)
        {
            // Protocol type (1 = NLPID, 2 = 802.2)
            writer.Write((byte)1);
            // Protocol length
            writer.Write((byte)1);
            // Protocol (IP = 0xCC)
            writer.Write((byte)0xCC);
            // Address length
            writer.Write((ushort)4);
            // IP Address
            var ipBytes = IPAddress.Parse(address).GetAddressBytes();
            writer.Write(ipBytes);
        }

        return new CdpTlv
        {
            Type = CdpTlvType.Address,
            Length = (ushort)(4 + stream.Length),
            Value = stream.ToArray()
        };
    }

    private CdpTlv BuildPortIdTlv(string portId)
    {
        return new CdpTlv
        {
            Type = CdpTlvType.PortId,
            Length = (ushort)(4 + portId.Length),
            Value = Encoding.ASCII.GetBytes(portId)
        };
    }

    private CdpTlv BuildCapabilitiesTlv(INetworkDevice device, CdpConfig cdpConfig = null)
    {
        uint capabilities = 0;

        var caps = cdpConfig?.Capabilities ?? new List<string>();
        if (caps.Contains("Router") || device.DeviceType == "Router") capabilities |= 0x01;
        if (caps.Contains("Bridge")) capabilities |= 0x02;
        if (caps.Contains("Switch") || device.DeviceType == "Switch") capabilities |= 0x08;
        if (caps.Contains("Host")) capabilities |= 0x10;
        if (caps.Contains("IGMP")) capabilities |= 0x20;
        if (caps.Contains("Repeater")) capabilities |= 0x40;

        return new CdpTlv
        {
            Type = CdpTlvType.Capabilities,
            Length = 8,
            Value = BitConverter.GetBytes(capabilities)
        };
    }

    private CdpTlv BuildVersionTlv(CdpConfig cdpConfig = null)
    {
        var version = cdpConfig?.Version ?? "Unknown";
        return new CdpTlv
        {
            Type = CdpTlvType.Version,
            Length = (ushort)(4 + version.Length),
            Value = Encoding.ASCII.GetBytes(version)
        };
    }

    private CdpTlv BuildPlatformTlv(INetworkDevice device, CdpConfig cdpConfig = null)
    {
        var platform = cdpConfig?.Platform ?? $"{device.Vendor} {device.DeviceType}";
        return new CdpTlv
        {
            Type = CdpTlvType.Platform,
            Length = (ushort)(4 + platform.Length),
            Value = Encoding.ASCII.GetBytes(platform)
        };
    }

    private CdpTlv BuildVtpManagementDomainTlv(string domain)
    {
        if (string.IsNullOrEmpty(domain)) domain = "";

        return new CdpTlv
        {
            Type = CdpTlvType.VtpManagementDomain,
            Length = (ushort)(4 + domain.Length),
            Value = Encoding.ASCII.GetBytes(domain)
        };
    }

    private CdpTlv BuildNativeVlanTlv(ushort vlanId)
    {
        return new CdpTlv
        {
            Type = CdpTlvType.NativeVlan,
            Length = 6,
            Value = BitConverter.GetBytes(vlanId)
        };
    }

    private CdpTlv BuildDuplexTlv(IInterfaceConfig? interfaceConfig)
    {
        byte duplex = 0x01; // Default to full duplex
        if (interfaceConfig != null)
        {
            // Check interface duplex setting (simplified)
            duplex = 0x01; // Full duplex
        }

        return new CdpTlv
        {
            Type = CdpTlvType.Duplex,
            Length = 5,
            Value = new[] { duplex }
        };
    }

    private CdpTlv BuildMtuTlv(IInterfaceConfig? interfaceConfig)
    {
        uint mtu = 1500; // Default MTU
        if (interfaceConfig != null)
        {
            mtu = (uint)interfaceConfig.Mtu;
        }

        return new CdpTlv
        {
            Type = CdpTlvType.Mtu,
            Length = 8,
            Value = BitConverter.GetBytes(mtu)
        };
    }

    private CdpTlv BuildSystemNameTlv(string systemName)
    {
        return new CdpTlv
        {
            Type = CdpTlvType.SystemName,
            Length = (ushort)(4 + systemName.Length),
            Value = Encoding.ASCII.GetBytes(systemName)
        };
    }

    private void ProcessDeviceIdTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        neighbor.DeviceId = Encoding.ASCII.GetString(tlv.Value);
    }

    private void ProcessAddressTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        try
        {
            using var stream = new MemoryStream(tlv.Value);
            using var reader = new BinaryReader(stream);

            var addressCount = reader.ReadUInt32();
            if (addressCount > 0)
            {
                // Read first address (skip protocol info)
                reader.ReadByte(); // Protocol type
                reader.ReadByte(); // Protocol length
                reader.ReadByte(); // Protocol
                var addressLength = reader.ReadUInt16();

                if (addressLength == 4)
                {
                    var addressBytes = reader.ReadBytes(4);
                    neighbor.IpAddress = $"{addressBytes[0]}.{addressBytes[1]}.{addressBytes[2]}.{addressBytes[3]}";
                }
            }
        }
        catch
        {
            neighbor.IpAddress = "0.0.0.0";
        }
    }

    private void ProcessPortIdTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        neighbor.RemoteInterface = Encoding.ASCII.GetString(tlv.Value);
    }

    private void ProcessCapabilitiesTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        if (tlv.Value.Length >= 4)
        {
            var capabilities = BitConverter.ToUInt32(tlv.Value, 0);
            neighbor.Capabilities = ParseCapabilities(capabilities);
        }
    }

    private void ProcessVersionTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        neighbor.Version = Encoding.ASCII.GetString(tlv.Value);
    }

    private void ProcessPlatformTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        neighbor.Platform = Encoding.ASCII.GetString(tlv.Value);
    }

    private void ProcessNativeVlanTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        // Store in TLVs for later processing
    }

    private void ProcessDuplexTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        // Store in TLVs for later processing
    }

    private void ProcessMtuTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        // Store in TLVs for later processing
    }

    private void ProcessSystemNameTlv(CdpNeighbor neighbor, CdpTlv tlv)
    {
        // System name can be different from device ID
    }

    private List<string> GetManagementAddresses(INetworkDevice device)
    {
        var addresses = new List<string>();

        // Get management IP from first interface with IP
        // Since GetManagementIpAddress doesn't exist, we'll use interface IPs

        // Get interface IPs
        foreach (var interfaceName in device.GetAllInterfaces().Keys)
        {
            var iface = device.GetInterface(interfaceName);
            if (iface?.IpAddress != null && iface.IpAddress != "0.0.0.0")
            {
                addresses.Add(iface.IpAddress);
            }
        }

        return addresses.Distinct().ToList();
    }

    private string GetDeviceVersion(CdpConfig cdpConfig)
    {
        // Get device software version
        return cdpConfig?.Version ?? "Unknown";
    }

    private bool IsInterfaceTrunking(IInterfaceConfig interfaceConfig)
    {
        // Simplified check - in real implementation would check switchport mode
        return false;
    }

    private List<string> ParseCapabilities(uint capabilities)
    {
        var caps = new List<string>();
        if ((capabilities & 0x01) != 0) caps.Add("Router");
        if ((capabilities & 0x02) != 0) caps.Add("Trans-Bridge");
        if ((capabilities & 0x04) != 0) caps.Add("Source-Route-Bridge");
        if ((capabilities & 0x08) != 0) caps.Add("Switch");
        if ((capabilities & 0x10) != 0) caps.Add("Host");
        if ((capabilities & 0x20) != 0) caps.Add("IGMP");
        if ((capabilities & 0x40) != 0) caps.Add("Repeater");
        return caps;
    }
}
