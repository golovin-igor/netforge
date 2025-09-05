using NetForge.Interfaces.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Services
{
    /// <summary>
    /// Vendor-based protocol service that replaces the old plugin discovery system
    /// </summary>
    public class VendorBasedProtocolService
    {
        private readonly IVendorService _vendorService;
        private readonly IVendorRegistry _vendorRegistry;

        public VendorBasedProtocolService(IVendorService vendorService, IVendorRegistry vendorRegistry)
        {
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        /// <summary>
        /// Get protocols for a specific vendor, with Telnet always included
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <returns>Enumerable of protocol instances</returns>
        public IEnumerable<object> GetProtocolsForVendor(string vendorName)
        {
            var protocols = new List<object>();

            // Always include Telnet for management (when available)
            var telnetProtocol = _vendorService.CreateProtocol(vendorName, NetworkProtocolType.TELNET);
            if (telnetProtocol != null)
            {
                protocols.Add(telnetProtocol);
            }

            // Get all other protocols for the vendor
            var vendorProtocols = _vendorService.GetVendorProtocols(vendorName);
            foreach (var protocol in vendorProtocols)
            {
                // Skip Telnet since we already added it
                var protocolType = GetProtocolType(protocol);
                if (protocolType != NetworkProtocolType.TELNET)
                {
                    protocols.Add(protocol);
                }
            }

            return protocols;
        }

        /// <summary>
        /// Get a specific protocol instance by type and vendor
        /// </summary>
        /// <param name="networkProtocolType">Protocol type to create</param>
        /// <param name="vendorName">Vendor name (default: Generic)</param>
        /// <returns>Protocol instance or null if not available</returns>
        public object? GetProtocol(NetworkProtocolType networkProtocolType, string vendorName = "Generic")
        {
            return _vendorService.CreateProtocol(vendorName, networkProtocolType);
        }

        /// <summary>
        /// Get all supported vendors across all registered vendors
        /// </summary>
        /// <returns>Enumerable of vendor names</returns>
        public IEnumerable<string> GetSupportedVendors()
        {
            return _vendorRegistry.GetAllVendors()
                .Select(v => v.VendorName)
                .Distinct()
                .OrderBy(v => v);
        }

        /// <summary>
        /// Check if a specific protocol type is available for a vendor
        /// </summary>
        /// <param name="networkProtocolType">Protocol type to check</param>
        /// <param name="vendorName">Vendor name (optional)</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsProtocolAvailable(NetworkProtocolType networkProtocolType, string? vendorName = null)
        {
            if (string.IsNullOrEmpty(vendorName))
            {
                // Check if any vendor supports this protocol
                return _vendorRegistry.GetVendorsForProtocol(networkProtocolType).Any();
            }

            var vendor = _vendorRegistry.GetVendor(vendorName);
            return vendor?.SupportsProtocol(networkProtocolType) ?? false;
        }

        /// <summary>
        /// Get vendors that support a specific protocol
        /// </summary>
        /// <param name="networkProtocolType">Protocol type</param>
        /// <returns>Enumerable of vendor names</returns>
        public IEnumerable<string> GetVendorsForProtocol(NetworkProtocolType networkProtocolType)
        {
            return _vendorRegistry.GetVendorsForProtocol(networkProtocolType)
                .Select(v => v.VendorName)
                .OrderBy(v => v);
        }

        /// <summary>
        /// Get discovery statistics
        /// </summary>
        /// <returns>Discovery statistics</returns>
        public Dictionary<string, object> GetDiscoveryStatistics()
        {
            var vendors = _vendorRegistry.GetAllVendors().ToList();
            var totalProtocols = vendors.SelectMany(v => v.SupportedProtocols).Count();
            var protocolTypes = vendors.SelectMany(v => v.SupportedProtocols)
                .Select(p => p.ProtocolType)
                .Distinct()
                .Count();

            return new Dictionary<string, object>
            {
                ["TotalVendors"] = vendors.Count,
                ["TotalProtocols"] = totalProtocols,
                ["ProtocolTypes"] = protocolTypes,
                ["SupportedVendors"] = GetSupportedVendors().Count(),
                ["ProtocolsByVendor"] = vendors.ToDictionary(
                    v => v.VendorName,
                    v => v.SupportedProtocols.Count()),
                ["VendorsByProtocol"] = Enum.GetValues<NetworkProtocolType>()
                    .ToDictionary(
                        p => p.ToString(),
                        p => GetVendorsForProtocol(p).Count())
            };
        }

        /// <summary>
        /// Initialize protocols for a device
        /// </summary>
        /// <param name="device">Device to initialize protocols for</param>
        public void InitializeDeviceProtocols(object device)
        {
            _vendorService.InitializeDeviceProtocols(device);
        }

        /// <summary>
        /// Get protocol type via reflection (helper method)
        /// </summary>
        private NetworkProtocolType GetProtocolType(object protocol)
        {
            var typeProperty = protocol.GetType().GetProperty("ProtocolType") ?? 
                              protocol.GetType().GetProperty("Type");
            if (typeProperty?.GetValue(protocol) is NetworkProtocolType protocolType)
            {
                return protocolType;
            }
            // Return a default value if not found
            return (NetworkProtocolType)0; // Default to first enum value
        }
    }
}