namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// Base HTTP server implementation
    /// </summary>
    public abstract class BaseHttpServer : IDisposable
    {
        protected readonly INetworkDevice _device;
        protected readonly HttpConfig _config;
        protected readonly HttpHandlerManager _handlerManager;
        protected readonly HttpSessionManager _sessionManager;

        public event EventHandler<HttpRequestEventArgs>? RequestReceived;
        public event EventHandler<HttpConnectionEventArgs>? ConnectionEstablished;

        public int ActiveConnections { get; protected set; }
        public long TotalRequests { get; protected set; }
        public double RequestsPerSecond { get; protected set; }

        protected BaseHttpServer(
            INetworkDevice device,
            HttpConfig config,
            HttpHandlerManager handlerManager)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            _sessionManager = new HttpSessionManager(_config.SessionTimeout);
        }

        /// <summary>
        /// Start the HTTP server
        /// </summary>
        public abstract Task StartAsync();

        /// <summary>
        /// Stop the HTTP server
        /// </summary>
        public abstract Task StopAsync();

        /// <summary>
        /// Process HTTP request
        /// </summary>
        protected async Task<HttpResult> ProcessRequest(HttpContext context)
        {
            TotalRequests++;
            UpdateRequestsPerSecond();

            // Create session if needed
            if (context.Request.Cookies.TryGetValue("SESSIONID", out var sessionId))
            {
                context.Session = _sessionManager.GetSession(sessionId);
            }

            // Get appropriate handler
            var handler = _handlerManager.GetHandler(_device.Vendor, context.Request.Path);
            if (handler == null)
            {
                return HttpResult.NotFound("No handler available for this vendor/path combination");
            }

            // Initialize handler if needed
            if (context.GetItem<bool>("HandlerInitialized") != true)
            {
                await handler.Initialize(_device);
                context.SetItem("HandlerInitialized", true);
            }

            // Process request based on HTTP method
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

        /// <summary>
        /// Raise request received event
        /// </summary>
        protected virtual void OnRequestReceived(HttpContext context)
        {
            RequestReceived?.Invoke(this, new HttpRequestEventArgs(context));
        }

        /// <summary>
        /// Raise connection established event
        /// </summary>
        protected virtual void OnConnectionEstablished(string remoteEndpoint)
        {
            ConnectionEstablished?.Invoke(this, new HttpConnectionEventArgs(remoteEndpoint));
        }

        /// <summary>
        /// Update requests per second metric
        /// </summary>
        protected void UpdateRequestsPerSecond()
        {
            // Simple implementation - in production would use sliding window
            RequestsPerSecond = TotalRequests / Math.Max(1, (DateTime.Now - _startTime).TotalSeconds);
        }

        private DateTime _startTime = DateTime.Now;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sessionManager?.Dispose();
            }
        }
    }

    /// <summary>
    /// HTTP request event arguments
    /// </summary>
    public class HttpRequestEventArgs : EventArgs
    {
        public HttpContext Context { get; }

        public HttpRequestEventArgs(HttpContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }

    /// <summary>
    /// HTTP connection event arguments
    /// </summary>
    public class HttpConnectionEventArgs : EventArgs
    {
        public string RemoteEndpoint { get; }

        public HttpConnectionEventArgs(string remoteEndpoint)
        {
            RemoteEndpoint = remoteEndpoint ?? throw new ArgumentNullException(nameof(remoteEndpoint));
        }
    }
}