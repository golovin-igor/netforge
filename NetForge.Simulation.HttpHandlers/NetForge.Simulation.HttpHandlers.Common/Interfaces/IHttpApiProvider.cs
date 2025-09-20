namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP API provider interface for REST API functionality
    /// </summary>
    public interface IHttpApiProvider
    {
        /// <summary>
        /// Handle API request
        /// </summary>
        Task<HttpResult> HandleApiRequest(HttpContext context, string endpointPath);

        /// <summary>
        /// Get API endpoints for vendor
        /// </summary>
        IEnumerable<HttpEndpoint> GetApiEndpoints(string vendorName);

        /// <summary>
        /// Check if endpoint exists
        /// </summary>
        bool HasEndpoint(string vendorName, string path);

        /// <summary>
        /// Get API documentation for vendor
        /// </summary>
        Task<string> GetApiDocumentation(string vendorName);

        /// <summary>
        /// Execute API operation
        /// </summary>
        Task<HttpResult> ExecuteApiOperation(HttpContext context, string operation, Dictionary<string, object> parameters);
    }
}