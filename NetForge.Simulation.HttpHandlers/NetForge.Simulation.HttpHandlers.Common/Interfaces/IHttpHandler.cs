namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// Core HTTP handler interface for vendor-specific web management
    /// </summary>
    public interface IHttpHandler
    {
        /// <summary>
        /// Vendor name this handler supports
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Priority for handler selection (higher = preferred)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Supported HTTP versions
        /// </summary>
        IEnumerable<HttpVersion> SupportedVersions { get; }

        /// <summary>
        /// Supported authentication methods
        /// </summary>
        IEnumerable<HttpAuthMethod> SupportedAuthMethods { get; }

        /// <summary>
        /// Handle HTTP GET request
        /// </summary>
        Task<HttpResult> HandleGetRequest(HttpContext context);

        /// <summary>
        /// Handle HTTP POST request
        /// </summary>
        Task<HttpResult> HandlePostRequest(HttpContext context);

        /// <summary>
        /// Handle HTTP PUT request
        /// </summary>
        Task<HttpResult> HandlePutRequest(HttpContext context);

        /// <summary>
        /// Handle HTTP DELETE request
        /// </summary>
        Task<HttpResult> HandleDeleteRequest(HttpContext context);

        /// <summary>
        /// Get supported endpoints for this vendor
        /// </summary>
        IEnumerable<HttpEndpoint> GetSupportedEndpoints();

        /// <summary>
        /// Check if endpoint is supported
        /// </summary>
        bool SupportsEndpoint(string path);

        /// <summary>
        /// Generate vendor-specific web interface
        /// </summary>
        Task<string> GenerateWebInterface(HttpContext context);

        /// <summary>
        /// Initialize handler with device context
        /// </summary>
        Task Initialize(INetworkDevice device);
    }
}