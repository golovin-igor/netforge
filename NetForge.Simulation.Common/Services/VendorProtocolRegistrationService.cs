using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces;
using NetForge.Simulation.Common.Vendor;
using NetForge.Simulation.Protocols;

namespace NetForge.Simulation.Common.Services;

public interface IVendorProtocolRegistrationService
{
    Task RegisterProtocolsAsync(INetworkDevice device);
    Task RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType);
    IEnumerable<NetworkProtocolType> GetSupportedProtocols(DeviceVendor vendor, string model);
}

public class VendorProtocolRegistrationService : IVendorProtocolRegistrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVendorRegistry _vendorRegistry;
    private readonly IVendorBasedProtocolService _protocolService;

    public VendorProtocolRegistrationService(
        IServiceProvider serviceProvider,
        IVendorRegistry vendorRegistry,
        IVendorBasedProtocolService protocolService)
    {
        _serviceProvider = serviceProvider;
        _vendorRegistry = vendorRegistry;
        _protocolService = protocolService;
    }

    public async Task RegisterProtocolsAsync(INetworkDevice device)
    {
        var deviceIdentity = device as IDeviceIdentity
            ?? throw new InvalidOperationException("Device must implement IDeviceIdentity");

        var protocolHost = device as IProtocolHost
            ?? throw new InvalidOperationException("Device must implement IProtocolHost");

        var vendor = deviceIdentity.Vendor;
        var model = deviceIdentity.Model;

        var supportedProtocols = GetSupportedProtocols(vendor, model);

        foreach (var protocolType in supportedProtocols)
        {
            await RegisterProtocolAsync(device, protocolType);
        }
    }

    public async Task RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType)
    {
        var protocolHost = device as IProtocolHost
            ?? throw new InvalidOperationException("Device must implement IProtocolHost");

        if (protocolHost.HasProtocol(protocolType))
        {
            return; // Protocol already registered
        }

        var protocol = await _protocolService.CreateProtocolAsync(protocolType);
        if (protocol != null)
        {
            await protocolHost.AddProtocolAsync(protocol);
        }
    }

    public IEnumerable<NetworkProtocolType> GetSupportedProtocols(DeviceVendor vendor, string model)
    {
        var vendorDescriptor = _vendorRegistry.GetDescriptor(vendor);
        if (vendorDescriptor == null)
        {
            return Enumerable.Empty<NetworkProtocolType>();
        }

        var modelDescriptor = vendorDescriptor.Models.FirstOrDefault(m => m.Name == model);
        return modelDescriptor?.SupportedProtocols ?? Enumerable.Empty<NetworkProtocolType>();
    }
}