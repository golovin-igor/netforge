namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// HTTP API provider implementation for REST API functionality
    /// </summary>
    public class HttpApiProvider : IHttpApiProvider
    {
        private readonly Dictionary<string, Dictionary<string, HttpEndpoint>> _endpointsByVendor = new();
        private readonly Dictionary<string, Func<HttpContext, Task<HttpResult>>> _handlers = new();

        public HttpApiProvider()
        {
            RegisterDefaultEndpoints();
        }

        /// <summary>
        /// Handle API request
        /// </summary>
        public async Task<HttpResult> HandleApiRequest(HttpContext context, string endpointPath)
        {
            var vendorName = context.Device?.Vendor ?? "Generic";
            var method = context.Request.Method;

            if (_endpointsByVendor.TryGetValue(vendorName, out var vendorEndpoints))
            {
                if (vendorEndpoints.TryGetValue($"{method}:{endpointPath}", out var endpoint))
                {
                    if (_handlers.TryGetValue(endpointPath, out var handler))
                    {
                        return await handler(context);
                    }
                }
            }

            return HttpResult.NotFound("API endpoint not found");
        }

        /// <summary>
        /// Get API endpoints for vendor
        /// </summary>
        public IEnumerable<HttpEndpoint> GetApiEndpoints(string vendorName)
        {
            if (_endpointsByVendor.TryGetValue(vendorName, out var endpoints))
            {
                return endpoints.Values;
            }
            return Enumerable.Empty<HttpEndpoint>();
        }

        /// <summary>
        /// Check if endpoint exists
        /// </summary>
        public bool HasEndpoint(string vendorName, string path)
        {
            if (_endpointsByVendor.TryGetValue(vendorName, out var endpoints))
            {
                return endpoints.ContainsKey(path);
            }
            return false;
        }

        /// <summary>
        /// Get API documentation for vendor
        /// </summary>
        public async Task<string> GetApiDocumentation(string vendorName)
        {
            var endpoints = GetApiEndpoints(vendorName);
            var doc = new StringBuilder();

            doc.AppendLine($"# {vendorName} HTTP API Documentation");
            doc.AppendLine();
            doc.AppendLine("## Endpoints");
            doc.AppendLine();

            foreach (var endpoint in endpoints.OrderBy(e => e.Path))
            {
                doc.AppendLine($"### {endpoint.Method} {endpoint.Path}");
                doc.AppendLine($"{endpoint.Description}");
                doc.AppendLine();
            }

            return doc.ToString();
        }

        /// <summary>
        /// Execute API operation
        /// </summary>
        public async Task<HttpResult> ExecuteApiOperation(HttpContext context, string operation, Dictionary<string, object> parameters)
        {
            // This would execute vendor-specific operations
            // For now, return a placeholder implementation
            return HttpResult.Ok(new
            {
                operation,
                parameters,
                result = "Operation executed successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Register API endpoint
        /// </summary>
        public void RegisterEndpoint(string vendorName, HttpEndpoint endpoint)
        {
            if (!_endpointsByVendor.ContainsKey(vendorName))
            {
                _endpointsByVendor[vendorName] = new Dictionary<string, HttpEndpoint>();
            }

            var key = $"{endpoint.Method}:{endpoint.Path}";
            _endpointsByVendor[vendorName][key] = endpoint;
        }

        /// <summary>
        /// Register API handler
        /// </summary>
        public void RegisterHandler(string path, Func<HttpContext, Task<HttpResult>> handler)
        {
            _handlers[path] = handler;
        }

        private void RegisterDefaultEndpoints()
        {
            var vendors = new[] { "Cisco", "Juniper", "Arista", "Dell", "Generic" };

            foreach (var vendor in vendors)
            {
                // System information endpoints
                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/system/info",
                    Method = "GET",
                    Description = "Get system information"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/system/status",
                    Method = "GET",
                    Description = "Get system status"
                });

                // Interface endpoints
                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/interfaces",
                    Method = "GET",
                    Description = "Get all interfaces"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/interfaces/{name}",
                    Method = "GET",
                    Description = "Get interface details"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/interfaces/{name}/configure",
                    Method = "POST",
                    Description = "Configure interface"
                });

                // Configuration endpoints
                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/config/running",
                    Method = "GET",
                    Description = "Get running configuration"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/config/startup",
                    Method = "GET",
                    Description = "Get startup configuration"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/config/save",
                    Method = "POST",
                    Description = "Save configuration"
                });

                // Protocol endpoints
                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/protocols",
                    Method = "GET",
                    Description = "Get protocol status"
                });

                RegisterEndpoint(vendor, new HttpEndpoint
                {
                    Path = "/api/protocols/{protocol}/configure",
                    Method = "POST",
                    Description = "Configure protocol"
                });
            }
        }
    }
}