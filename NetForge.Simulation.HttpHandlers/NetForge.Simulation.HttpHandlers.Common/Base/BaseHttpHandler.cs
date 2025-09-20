namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// Base implementation of HTTP handler with common functionality
    /// </summary>
    public abstract class BaseHttpHandler : IHttpHandler
    {
        protected readonly IHttpAuthenticator _authenticator;
        protected readonly IHttpApiProvider _apiProvider;
        protected readonly IHttpContentProvider _contentProvider;
        protected INetworkDevice _device = null!;

        public abstract string VendorName { get; }
        public virtual int Priority => 100;
        public virtual IEnumerable<HttpVersion> SupportedVersions => new[] { HttpVersion.Http11, HttpVersion.Http20 };
        public virtual IEnumerable<HttpAuthMethod> SupportedAuthMethods => new[] { HttpAuthMethod.Basic, HttpAuthMethod.Digest };

        protected BaseHttpHandler(
            IHttpAuthenticator authenticator,
            IHttpApiProvider apiProvider,
            IHttpContentProvider contentProvider)
        {
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            _apiProvider = apiProvider ?? throw new ArgumentNullException(nameof(apiProvider));
            _contentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
        }

        public virtual async Task Initialize(INetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            await OnInitialize();
        }

        protected virtual async Task OnInitialize() { }

        public virtual async Task<HttpResult> HandleGetRequest(HttpContext context)
        {
            // Check authentication
            var authResult = await _authenticator.AuthenticateRequest(context);
            if (!authResult.IsAuthenticated)
            {
                return await _authenticator.GenerateChallenge(context, VendorName);
            }

            // Route request
            if (IsApiRequest(context))
            {
                return await HandleApiGetRequest(context);
            }
            else
            {
                return await HandleWebGetRequest(context);
            }
        }

        protected virtual async Task<HttpResult> HandleApiGetRequest(HttpContext context)
        {
            var endpoint = GetApiEndpoint(context.Request.Path);
            if (endpoint != null)
            {
                return await _apiProvider.HandleApiRequest(context, endpoint.Path);
            }

            return HttpResult.NotFound("API endpoint not found");
        }

        protected virtual async Task<HttpResult> HandleWebGetRequest(HttpContext context)
        {
            // Serve static content or generate dynamic pages
            if (IsStaticContent(context.Request.Path))
            {
                return await _contentProvider.ServeStaticContent(context);
            }
            else
            {
                return await GenerateWebInterface(context);
            }
        }

        public virtual async Task<HttpResult> HandlePostRequest(HttpContext context)
        {
            var authResult = await _authenticator.AuthenticateRequest(context);
            if (!authResult.IsAuthenticated)
            {
                return await _authenticator.GenerateChallenge(context, VendorName);
            }

            return await OnHandlePostRequest(context);
        }

        protected abstract Task<HttpResult> OnHandlePostRequest(HttpContext context);

        public abstract Task<string> GenerateWebInterface(HttpContext context);
        public abstract IEnumerable<HttpEndpoint> GetSupportedEndpoints();

        public virtual bool SupportsEndpoint(string path)
        {
            return GetSupportedEndpoints().Any(e => e.Matches(path));
        }

        protected virtual bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool IsStaticContent(string path)
        {
            var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".gif", ".ico", ".html" };
            return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual HttpEndpoint? GetApiEndpoint(string path)
        {
            return GetSupportedEndpoints().FirstOrDefault(e => e.Matches(path));
        }

        protected HttpResult Success(object content, string contentType = "application/json")
        {
            return new HttpResult
            {
                StatusCode = 200,
                Content = content,
                ContentType = contentType,
                IsSuccess = true
            };
        }

        protected HttpResult Error(int statusCode, string message)
        {
            return new HttpResult
            {
                StatusCode = statusCode,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }
    }
}