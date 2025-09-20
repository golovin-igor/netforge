using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Events;
using NetForge.Simulation.Protocols.Common.Base;
using NetForge.Simulation.HttpHandlers.Common.Services;
using NetForge.Simulation.HttpHandlers.Common;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.HTTP
{
    /// <summary>
    /// HTTP protocol implementation with handler integration
    /// </summary>
    public class HttpProtocol : BaseProtocol
    {
        public override string Name => "Hypertext Transfer Protocol";
        public override NetworkProtocolType Type => NetworkProtocolType.HTTP;

        private HttpServer? _httpServer;
        private readonly HttpHandlerManager _handlerManager;

        public HttpProtocol()
        {
            _handlerManager = new HttpHandlerManager();
        }

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

            // Update server statistics
            await UpdateServerStatistics(httpState);

            // Process request queue
            await ProcessRequestQueue(httpState);

            // Clean up expired sessions
            await CleanupExpiredSessions(httpState);
        }

        private void StartHttpServer(HttpConfig config)
        {
            try
            {
                _httpServer = new HttpServer(_device, config, _handlerManager);
                _httpServer.RequestReceived += OnHttpRequestReceived;
                _httpServer.ConnectionEstablished += OnHttpConnectionEstablished;
                _httpServer.Start();

                LogProtocolEvent($"HTTP server started on port {config.Port} (HTTPS: {config.HttpsPort})");
                ((HttpState)_state).IsActive = true;
            }
            catch (Exception ex)
            {
                LogProtocolEvent($"Failed to start HTTP server: {ex.Message}");
                ((HttpState)_state).IsActive = false;
            }
        }

        private async void OnHttpRequestReceived(object sender, HttpRequestEventArgs e)
        {
            var context = e.Context;
            var request = context.Request;

            LogProtocolEvent($"HTTP {request.Method} {request.Path} from {context.RemoteEndpoint}");

            try
            {
                // Route to appropriate handler
                var response = await ProcessHttpRequest(context);
                await context.SendResponse(response);
            }
            catch (Exception ex)
            {
                await context.SendResponse(HttpResult.Error(500, $"Internal Server Error: {ex.Message}"));
                LogProtocolEvent($"Error processing HTTP request: {ex.Message}");
            }
        }

        private async Task<HttpResult> ProcessHttpRequest(HttpContext context)
        {
            // Find appropriate handler for the device vendor
            var handler = _handlerManager.GetHandler(_device.Vendor, context.Request.Path);
            if (handler == null)
            {
                return HttpResult.NotFound("Handler not found for this vendor/path combination");
            }

            // Route based on HTTP method
            return context.Request.Method.ToUpper() switch
            {
                "GET" => await handler.HandleGetRequest(context),
                "POST" => await handler.HandlePostRequest(context),
                "PUT" => await handler.HandlePutRequest(context),
                "DELETE" => await handler.HandleDeleteRequest(context),
                "PATCH" => await handler.HandlePatchRequest(context),
                _ => HttpResult.BadRequest($"Method {context.Request.Method} not supported")
            };
        }

        private HttpConfig GetHttpConfig()
        {
            return _device?.GetHttpConfiguration() ?? new HttpConfig();
        }

        protected override object GetProtocolConfiguration()
        {
            return GetHttpConfig();
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
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia" };
        }

        private async Task StopHttpServer()
        {
            if (_httpServer != null)
            {
                await _httpServer.StopAsync();
                _httpServer = null;
            }
        }

        private async Task UpdateServerStatistics(HttpState state)
        {
            if (_httpServer != null)
            {
                state.ActiveConnections = _httpServer.ActiveConnections;
                state.TotalRequests = _httpServer.TotalRequests;
                state.RequestsPerSecond = _httpServer.RequestsPerSecond;
            }
        }

        private async Task ProcessRequestQueue(HttpState state)
        {
            // Process any queued requests
            await Task.CompletedTask;
        }

        private async Task CleanupExpiredSessions(HttpState state)
        {
            // Clean up expired HTTP sessions
            await Task.CompletedTask;
        }
    }
}