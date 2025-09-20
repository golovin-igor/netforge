namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// Manages HTTP handlers and routes requests to appropriate vendor-specific handlers
    /// </summary>
    public class HttpHandlerManager
    {
        private readonly Dictionary<string, List<IHttpHandler>> _handlersByVendor = new();
        private readonly List<IHttpHandler> _allHandlers = new();
        private readonly HttpHandlerDiscoveryService _discoveryService;

        public HttpHandlerManager()
        {
            _discoveryService = new HttpHandlerDiscoveryService();
            DiscoverHandlers();
        }

        /// <summary>
        /// Get appropriate handler for vendor and path
        /// </summary>
        public IHttpHandler? GetHandler(string vendorName, string path)
        {
            // Get vendor-specific handlers first
            if (_handlersByVendor.TryGetValue(vendorName, out var vendorHandlers))
            {
                var handler = vendorHandlers
                    .Where(h => h.SupportsEndpoint(path))
                    .OrderByDescending(h => h.Priority)
                    .FirstOrDefault();

                if (handler != null)
                    return handler;
            }

            // Fallback to generic handlers
            return _allHandlers
                .Where(h => h.VendorName == "Generic" && h.SupportsEndpoint(path))
                .OrderByDescending(h => h.Priority)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get all handlers for a specific vendor
        /// </summary>
        public IEnumerable<IHttpHandler> GetHandlersForVendor(string vendorName)
        {
            return _handlersByVendor.TryGetValue(vendorName, out var handlers)
                ? handlers
                : Enumerable.Empty<IHttpHandler>();
        }

        /// <summary>
        /// Get all registered handlers
        /// </summary>
        public IEnumerable<IHttpHandler> GetAllHandlers() => _allHandlers;

        /// <summary>
        /// Get supported vendors
        /// </summary>
        public IEnumerable<string> GetSupportedVendors() => _handlersByVendor.Keys;

        /// <summary>
        /// Discover and register all available handlers
        /// </summary>
        private void DiscoverHandlers()
        {
            var handlers = _discoveryService.DiscoverHandlers();

            foreach (var handler in handlers)
            {
                _allHandlers.Add(handler);

                if (!_handlersByVendor.ContainsKey(handler.VendorName))
                {
                    _handlersByVendor[handler.VendorName] = new List<IHttpHandler>();
                }

                _handlersByVendor[handler.VendorName].Add(handler);
            }
        }
    }
}