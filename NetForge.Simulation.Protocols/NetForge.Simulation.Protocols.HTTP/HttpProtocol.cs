using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common.Base;

namespace NetForge.Simulation.Protocols.HTTP
{
    /// <summary>
    /// HTTP protocol implementation for web-based device management
    /// Provides optional web interface for device configuration and monitoring
    /// </summary>
    public class HttpProtocol : BaseManagementProtocol
    {
        private HttpServer? _httpServer;
        private readonly HttpSessionManager _sessionManager = new();

        public override NetworkProtocolType Type => NetworkProtocolType.HTTP;
        public override string Name => "HTTP Protocol";
        public override string Version => "1.1.0";
        public override int DefaultPort => 80;

        protected override BaseProtocolState CreateInitialState()
        {
            return new HttpState();
        }

        protected override void OnInitialized()
        {
            var httpConfig = GetHttpConfig();
            if (httpConfig.IsEnabled)
            {
                StartHttpServer(httpConfig);
            }
        }

        protected override async Task RunProtocolCalculation(INetworkDevice device)
        {
            var httpState = (HttpState)_state;
            var httpConfig = GetHttpConfig();

            if (!httpConfig.IsEnabled)
            {
                await StopHttpServer();
                httpState.IsActive = false;
                return;
            }

            // Update active sessions and statistics
            await _sessionManager.UpdateSessions();

            httpState.ActiveSessions = _sessionManager.GetActiveSessions().Count;
            httpState.TotalRequests = _sessionManager.GetTotalRequestCount();
            httpState.LastActivity = _sessionManager.GetLastActivity();

            LogProtocolEvent($"HTTP server stats: {httpState.ActiveSessions} active sessions, {httpState.TotalRequests} total requests");

            await Task.CompletedTask;
        }

        private void StartHttpServer(HttpConfig config)
        {
            try
            {
                _httpServer = new HttpServer(_device, config, _sessionManager);
                _httpServer.RequestReceived += OnHttpRequestReceived;
                _httpServer.Start();

                var httpState = (HttpState)_state;
                httpState.IsServerRunning = true;
                httpState.IsActive = true;

                LogProtocolEvent($"HTTP server started on port {config.Port}");
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Failed to start HTTP server: {ex.Message}");
                ((HttpState)_state).IsActive = false;
            }
        }

        private async Task StopHttpServer()
        {
            if (_httpServer != null)
            {
                await _httpServer.Stop();
                _httpServer.Dispose();
                _httpServer = null;

                var httpState = (HttpState)_state;
                httpState.IsServerRunning = false;
                LogProtocolEvent("HTTP server stopped");
            }
        }

        private async void OnHttpRequestReceived(object sender, HttpRequestEventArgs e)
        {
            var request = e.Request;
            var response = e.Response;

            LogProtocolEvent($"HTTP {request.Method} request: {request.Url} from {request.ClientEndpoint}");

            try
            {
                // Route HTTP request to appropriate handler
                var responseContent = await ProcessHttpRequest(request);
                await response.WriteResponse(responseContent);
            }
            catch (Exception ex)
            {
                await response.WriteErrorResponse(500, "Internal Server Error", ex.Message);
                LogProtocolEvent($"Error processing HTTP request: {ex.Message}");
            }
        }

        private async Task<HttpResponseContent> ProcessHttpRequest(HttpRequest request)
        {
            // Basic web interface for device management
            switch (request.Url.ToLower())
            {
                case "/":
                case "/index.html":
                    return await GenerateDeviceOverviewPage();

                case "/interfaces":
                    return await GenerateInterfacesPage();

                case "/routes":
                    return await GenerateRoutesPage();

                case "/protocols":
                    return await GenerateProtocolsPage();

                case "/api/device/info":
                    return await GenerateDeviceInfoJson();

                default:
                    return new HttpResponseContent(404, "Not Found", "Page not found");
            }
        }

        private async Task<HttpResponseContent> GenerateDeviceOverviewPage()
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>{_device.Name} - Device Management</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 10px; border-radius: 5px; }}
        .section {{ margin: 20px 0; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{_device.Name} - Network Device Management</h1>
        <p>Vendor: {_device.Vendor}</p>
    </div>

    <div class='section'>
        <h2>Quick Links</h2>
        <ul>
            <li><a href='/interfaces'>Interfaces</a></li>
            <li><a href='/routes'>Routing Table</a></li>
            <li><a href='/protocols'>Protocol Status</a></li>
            <li><a href='/api/device/info'>Device Info (JSON)</a></li>
        </ul>
    </div>

    <div class='section'>
        <h2>Device Status</h2>
        <p>Management Interface: HTTP on port {GetHttpConfig().Port}</p>
        <p>Status: Operational</p>
        <p>Uptime: Running</p>
    </div>
</body>
</html>";

            return new HttpResponseContent(200, "OK", html, "text/html");
        }

        private async Task<HttpResponseContent> GenerateInterfacesPage()
        {
            var interfaces = _device.GetAllInterfaces();
            var html = "<h1>Interface Status</h1><table><tr><th>Interface</th><th>IP Address</th><th>Status</th><th>Description</th></tr>";

            foreach (var (name, config) in interfaces)
            {
                var status = config?.IsUp == true ? "Up" : "Down";
                html += $"<tr><td>{name}</td><td>{config?.IpAddress ?? "N/A"}</td><td>{status}</td><td>{config?.Description ?? ""}</td></tr>";
            }

            html += "</table>";
            return new HttpResponseContent(200, "OK", html, "text/html");
        }

        private async Task<HttpResponseContent> GenerateRoutesPage()
        {
            var routes = _device.GetRoutingTable();
            var html = "<h1>Routing Table</h1><table><tr><th>Network</th><th>Mask</th><th>Next Hop</th><th>Interface</th><th>Protocol</th><th>Metric</th></tr>";

            foreach (var route in routes)
            {
                html += $"<tr><td>{route.Network}</td><td>{route.Mask}</td><td>{route.NextHop}</td><td>{route.Interface}</td><td>{route.Protocol}</td><td>{route.Metric}</td></tr>";
            }

            html += "</table>";
            return new HttpResponseContent(200, "OK", html, "text/html");
        }

        private async Task<HttpResponseContent> GenerateProtocolsPage()
        {
            var html = "<h1>Protocol Status</h1><table><tr><th>Protocol</th><th>Status</th><th>Last Update</th></tr>";

            // Add basic protocol status information
            html += $"<tr><td>HTTP</td><td>Active</td><td>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>";

            html += "</table>";
            return new HttpResponseContent(200, "OK", html, "text/html");
        }

        private async Task<HttpResponseContent> GenerateDeviceInfoJson()
        {
            var deviceInfo = new
            {
                name = _device.Name,
                hostname = _device.GetHostname(),
                vendor = _device.Vendor,
                uptime = 0, // Device uptime not available
                interfaces = _device.GetAllInterfaces().Count,
                routes = _device.GetRoutingTable().Count(),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = System.Text.Json.JsonSerializer.Serialize(deviceInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return new HttpResponseContent(200, "OK", json, "application/json");
        }

        protected override object GetProtocolConfiguration()
        {
            return _device?.GetHttpConfiguration() as HttpConfig ?? new HttpConfig { IsEnabled = false, Port = 80 };
        }

        protected override void OnApplyConfiguration(object configuration)
        {
            if (configuration is HttpConfig httpConfig)
            {
                _device?.SetHttpConfiguration(httpConfig);

                // Restart server if configuration changed
                _ = Task.Run(async () =>
                {
                    await StopHttpServer();
                    if (httpConfig.IsEnabled)
                    {
                        StartHttpServer(httpConfig);
                    }
                });
            }
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            return new[] { "Generic", "Cisco", "Juniper", "Arista" }; // All vendors can support HTTP
        }

        protected override async Task<bool> AuthenticateClient(string credentials, Dictionary<string, object> context)
        {
            // Simple authentication for HTTP protocol
            if (string.IsNullOrEmpty(credentials))
                return false;

            var httpConfig = GetHttpConfig();
            if (!httpConfig.AuthenticationRequired)
                return true;

            // Parse basic auth credentials (username:password)
            try
            {
                var decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
                var parts = decodedCredentials.Split(':');
                if (parts.Length == 2)
                {
                    return parts[0] == httpConfig.Username && parts[1] == httpConfig.Password;
                }
            }
            catch
            {
                // Invalid base64 or format
            }

            return false;
        }

        protected override async Task ManageActiveSessions(INetworkDevice device)
        {
            await _sessionManager.UpdateSessions();
            var httpState = (HttpState)_state;
            httpState.ActiveSessions = _sessionManager.GetActiveSessions().Count;
        }

        protected override async Task ProcessConnectionRequests(INetworkDevice device)
        {
            // Connection processing is handled by the HttpServer
            var httpState = (HttpState)_state;
            httpState.LastActivity = _sessionManager.GetLastActivity();
            await Task.CompletedTask;
        }

        private HttpConfig GetHttpConfig()
        {
            return _device?.GetHttpConfiguration() as HttpConfig ?? new HttpConfig { IsEnabled = false, Port = 80 };
        }

        protected override void OnDispose()
        {
            _httpServer?.Dispose();
            _sessionManager?.Dispose();
        }
    }

}
