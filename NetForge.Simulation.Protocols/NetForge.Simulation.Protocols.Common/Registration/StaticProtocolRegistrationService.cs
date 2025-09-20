using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common.Services;

namespace NetForge.Simulation.Protocols.Common.Registration
{
    /// <summary>
    /// Static protocol registration service that avoids reflection entirely
    /// Uses compile-time type safety and declarative vendor configuration
    /// </summary>
    public class StaticProtocolRegistrationService : IProtocolRegistrationService
    {
        private readonly IServiceProvider _serviceProvider;

        public StaticProtocolRegistrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Register all protocols for a device based on vendor configuration
        /// Uses existing vendor service but with improved type safety
        /// </summary>
        public async Task RegisterProtocolsAsync(INetworkDevice device)
        {
            if (device == null)
                return;

            // Use the existing VendorBasedProtocolService to get protocols for the device vendor
            var protocolService = _serviceProvider.GetService<VendorBasedProtocolService>();
            if (protocolService != null)
            {
                var vendor = device.Vendor ?? "Generic";
                var protocols = protocolService.GetProtocolsForVendor(vendor);

                foreach (var protocol in protocols)
                {
                    if (protocol is IDeviceProtocol deviceProtocol)
                    {
                        device.RegisterProtocol(deviceProtocol);
                    }
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Register a specific protocol if supported by the device vendor
        /// </summary>
        public async Task<bool> RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType)
        {
            if (device == null)
                return false;

            var protocolService = _serviceProvider.GetService<VendorBasedProtocolService>();
            if (protocolService != null)
            {
                var vendor = device.Vendor ?? "Generic";
                var protocol = protocolService.GetProtocol(protocolType, vendor);
                if (protocol is IDeviceProtocol deviceProtocol)
                {
                    device.RegisterProtocol(deviceProtocol);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get all supported protocols for a vendor
        /// </summary>
        public NetworkProtocolType[] GetSupportedProtocols(string vendor)
        {
            var protocolService = _serviceProvider.GetService<VendorBasedProtocolService>();
            if (protocolService != null)
            {
                var protocols = protocolService.GetProtocolsForVendor(vendor);
                var protocolTypes = protocols
                    .Where(p => p is IDeviceProtocol)
                    .Cast<IDeviceProtocol>()
                    .Select(p => p.Type)
                    .ToArray();
                return protocolTypes;
            }
            return Array.Empty<NetworkProtocolType>();
        }

        /// <summary>
        /// Check if a protocol is supported by a vendor
        /// </summary>
        public bool IsProtocolSupported(string vendor, NetworkProtocolType protocolType)
        {
            var protocolService = _serviceProvider.GetService<VendorBasedProtocolService>();
            return protocolService?.IsProtocolAvailable(protocolType, vendor) ?? false;
        }

    }

    /// <summary>
    /// Interface for protocol registration service
    /// </summary>
    public interface IProtocolRegistrationService
    {
        Task RegisterProtocolsAsync(INetworkDevice device);
        Task<bool> RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType);
        NetworkProtocolType[] GetSupportedProtocols(string vendor);
        bool IsProtocolSupported(string vendor, NetworkProtocolType protocolType);
    }
}