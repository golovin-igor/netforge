# HTTP Handlers Implementation Plan

## Overview

This document outlines the comprehensive plan for implementing HTTP (Hypertext Transfer Protocol) handlers in the NetForge network simulation system. The HTTP handlers are designed to work in conjunction with the HTTP protocol implementation, providing vendor-specific web management interfaces similar to how CLI handlers provide vendor-specific command line interfaces and SNMP handlers provide vendor-specific SNMP functionality.

## Architecture Design

### Core Components

```
NetForge.Simulation.HttpHandlers/                     # HTTP Handler Architecture
├── NetForge.Simulation.HttpHandlers.Common/          # Core HTTP interfaces and base classes
│   ├── Interfaces/
│   │   ├── IHttpHandler.cs                         # Core HTTP handler interface
│   │   ├── IHttpServer.cs                          # HTTP server interface
│   │   ├── IHttpAuthenticator.cs                   # Authentication interface
│   │   ├── IHttpRouter.cs                          # Request routing interface
│   │   ├── IHttpContentProvider.cs                 # Content provider interface
│   │   └── IHttpApiProvider.cs                     # REST API provider interface
│   ├── Base/
│   │   ├── BaseHttpHandler.cs                      # Base HTTP handler implementation
│   │   ├── BaseHttpServer.cs                       # Base HTTP server
│   │   ├── BaseHttpAuthenticator.cs                # Base authentication
│   │   ├── HttpContext.cs                          # HTTP request context
│   │   └── HttpResult.cs                           # HTTP response result
│   ├── Models/
│   │   ├── HttpRequest.cs                          # HTTP request model
│   │   ├── HttpResponse.cs                         # HTTP response model
│   │   ├── HttpSession.cs                          # HTTP session management
│   │   ├── HttpUser.cs                             # User authentication model
│   │   └── HttpEndpoint.cs                         # API endpoint definition
│   ├── Security/
│   │   ├── HttpsConfiguration.cs                   # SSL/TLS configuration
│   │   ├── CertificateManager.cs                   # Certificate handling
│   │   ├── AccessControlList.cs                    # HTTP access control
│   │   └── RateLimitManager.cs                     # Rate limiting
│   └── Services/
│       ├── HttpHandlerDiscoveryService.cs          # Auto-discovery service
│       ├── HttpHandlerManager.cs                   # Handler management
│       ├── HttpRoutingService.cs                   # URL routing
│       └── HttpContentService.cs                   # Static content serving
│
├── NetForge.Simulation.HttpHandlers.Cisco/           # Cisco-specific HTTP handlers
├── NetForge.Simulation.HttpHandlers.Juniper/         # Juniper-specific HTTP handlers
├── NetForge.Simulation.HttpHandlers.Arista/          # Arista-specific HTTP handlers
├── NetForge.Simulation.HttpHandlers.Dell/            # Dell-specific HTTP handlers
├── NetForge.Simulation.HttpHandlers.Huawei/          # Huawei-specific HTTP handlers
├── NetForge.Simulation.HttpHandlers.Nokia/           # Nokia-specific HTTP handlers
└── NetForge.Simulation.HttpHandlers.Generic/         # Generic/standards-based handlers
```

## Phase 1: Foundation (Weeks 1-2)

### Core HTTP Infrastructure

#### 1.1 Core Interfaces

```csharp
// NetForge.Simulation.HttpHandlers.Common/Interfaces/IHttpHandler.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
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
        /// Handle HTTP PATCH request
        /// </summary>
        Task<HttpResult> HandlePatchRequest(HttpContext context);
        
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
```

```csharp
// NetForge.Simulation.HttpHandlers.Common/Interfaces/IHttpApiProvider.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public interface IHttpApiProvider
    {
        /// <summary>
        /// Get REST API endpoints for vendor
        /// </summary>
        Task<Dictionary<string, HttpEndpoint>> GetApiEndpoints();
        
        /// <summary>
        /// Handle REST API request
        /// </summary>
        Task<HttpResult> HandleApiRequest(HttpContext context, string endpoint);
        
        /// <summary>
        /// Get API documentation
        /// </summary>
        Task<string> GetApiDocumentation();
        
        /// <summary>
        /// Validate API request
        /// </summary>
        Task<bool> ValidateApiRequest(HttpContext context);
        
        /// <summary>
        /// Get OpenAPI/Swagger specification
        /// </summary>
        Task<string> GetOpenApiSpec();
    }
}
```

```csharp
// NetForge.Simulation.HttpHandlers.Common/Interfaces/IHttpAuthenticator.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public interface IHttpAuthenticator
    {
        /// <summary>
        /// Supported authentication methods
        /// </summary>
        IEnumerable<HttpAuthMethod> SupportedMethods { get; }
        
        /// <summary>
        /// Authenticate HTTP request
        /// </summary>
        Task<AuthenticationResult> AuthenticateRequest(HttpContext context);
        
        /// <summary>
        /// Generate authentication challenge
        /// </summary>
        Task<HttpResult> GenerateChallenge(HttpContext context, string realm);
        
        /// <summary>
        /// Validate user credentials
        /// </summary>
        Task<bool> ValidateCredentials(string username, string password);
        
        /// <summary>
        /// Create session for authenticated user
        /// </summary>
        Task<HttpSession> CreateSession(HttpUser user, HttpContext context);
        
        /// <summary>
        /// Validate existing session
        /// </summary>
        Task<bool> ValidateSession(string sessionId, HttpContext context);
        
        /// <summary>
        /// Revoke session
        /// </summary>
        Task RevokeSession(string sessionId);
    }
}
```

#### 1.2 Base Implementation

```csharp
// NetForge.Simulation.HttpHandlers.Common/Base/BaseHttpHandler.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public abstract class BaseHttpHandler : IHttpHandler
    {
        protected readonly IHttpAuthenticator _authenticator;
        protected readonly IHttpApiProvider _apiProvider;
        protected readonly IHttpContentProvider _contentProvider;
        protected INetworkDevice _device;
        
        public abstract string VendorName { get; }
        public virtual int Priority => 100;
        public virtual IEnumerable<HttpVersion> SupportedVersions => new[] { HttpVersion.Http11, HttpVersion.Http20 };
        public virtual IEnumerable<HttpAuthMethod> SupportedAuthMethods => new[] { HttpAuthMethod.Basic, HttpAuthMethod.Digest };
        
        protected BaseHttpHandler(
            IHttpAuthenticator authenticator,
            IHttpApiProvider apiProvider, 
            IHttpContentProvider contentProvider)
        {
            _authenticator = authenticator;
            _apiProvider = apiProvider;
            _contentProvider = contentProvider;
        }
        
        public virtual async Task Initialize(INetworkDevice device)
        {
            _device = device;
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
```

#### 1.3 Core Models

```csharp
// NetForge.Simulation.HttpHandlers.Common/Models/HttpContext.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public class HttpContext
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public INetworkDevice Device { get; set; }
        public HttpSession? Session { get; set; }
        public HttpUser? User { get; set; }
        public Dictionary<string, object> Items { get; set; } = new();
        public CancellationToken CancellationToken { get; set; }
        
        public T? GetItem<T>(string key) where T : class
        {
            return Items.TryGetValue(key, out var value) ? value as T : null;
        }
        
        public void SetItem<T>(string key, T value) where T : class
        {
            Items[key] = value;
        }
    }
}
```

```csharp
// NetForge.Simulation.HttpHandlers.Common/Models/HttpRequest.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public class HttpRequest
    {
        public string Method { get; set; } = "GET";
        public string Path { get; set; } = "/";
        public string QueryString { get; set; } = "";
        public HttpVersion Version { get; set; } = HttpVersion.Http11;
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> Cookies { get; set; } = new();
        public Dictionary<string, string> QueryParameters { get; set; } = new();
        public byte[] Body { get; set; } = Array.Empty<byte>();
        public string ContentType => Headers.GetValueOrDefault("Content-Type", "");
        public int ContentLength => int.Parse(Headers.GetValueOrDefault("Content-Length", "0"));
        public string UserAgent => Headers.GetValueOrDefault("User-Agent", "");
        public string Authorization => Headers.GetValueOrDefault("Authorization", "");
        
        public string GetBodyAsString()
        {
            return System.Text.Encoding.UTF8.GetString(Body);
        }
        
        public T? GetBodyAsJson<T>() where T : class
        {
            if (ContentType.Contains("application/json"))
            {
                var json = GetBodyAsString();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }
        
        public string GetQueryParameter(string key)
        {
            return QueryParameters.GetValueOrDefault(key, "");
        }
        
        public string GetHeader(string key)
        {
            return Headers.GetValueOrDefault(key, "");
        }
        
        public string GetCookie(string key)
        {
            return Cookies.GetValueOrDefault(key, "");
        }
    }
}
```

```csharp
// NetForge.Simulation.HttpHandlers.Common/Models/HttpResult.cs
namespace NetForge.Simulation.HttpHandlers.Common
{
    public class HttpResult
    {
        public int StatusCode { get; set; } = 200;
        public object? Content { get; set; }
        public string ContentType { get; set; } = "application/json";
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> Cookies { get; set; } = new();
        public bool IsSuccess { get; set; } = true;
        public string? RedirectUrl { get; set; }
        
        public static HttpResult Ok(object content, string contentType = "application/json")
        {
            return new HttpResult
            {
                StatusCode = 200,
                Content = content,
                ContentType = contentType,
                IsSuccess = true
            };
        }
        
        public static HttpResult NotFound(string message = "Not Found")
        {
            return new HttpResult
            {
                StatusCode = 404,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }
        
        public static HttpResult Unauthorized(string message = "Unauthorized")
        {
            return new HttpResult
            {
                StatusCode = 401,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }
        
        public static HttpResult BadRequest(string message = "Bad Request")
        {
            return new HttpResult
            {
                StatusCode = 400,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }
        
        public static HttpResult Redirect(string url)
        {
            return new HttpResult
            {
                StatusCode = 302,
                RedirectUrl = url,
                Headers = { ["Location"] = url },
                IsSuccess = true
            };
        }
    }
}
```

## Phase 2: HTTP Protocol Integration (Week 3)

### HTTP Protocol Implementation

#### 2.1 HTTP Protocol Core

```csharp
// NetForge.Simulation.Protocols.HTTP/HttpProtocol.cs
namespace NetForge.Simulation.Protocols.HTTP
{
    public class HttpProtocol : BaseProtocol
    {
        public override ProtocolType Type => ProtocolType.HTTP;
        public override string Name => "Hypertext Transfer Protocol";
        
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
        
        protected override async Task RunProtocolCalculation(NetworkDevice device)
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
    }
}
```

#### 2.2 HTTP Configuration

```csharp
// NetForge.Simulation.Protocols.HTTP/HttpConfig.cs
namespace NetForge.Simulation.Protocols.HTTP
{
    public class HttpConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int Port { get; set; } = 80;
        public int HttpsPort { get; set; } = 443;
        public bool HttpsEnabled { get; set; } = true;
        public bool HttpRedirectToHttps { get; set; } = true;
        public string ServerName { get; set; } = "NetForge-HTTP";
        public int MaxConnections { get; set; } = 100;
        public int RequestTimeout { get; set; } = 30; // seconds
        public int SessionTimeout { get; set; } = 1800; // 30 minutes
        public bool CompressionEnabled { get; set; } = true;
        public List<string> AllowedOrigins { get; set; } = new() { "*" };
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
        public HttpAuthConfig Authentication { get; set; } = new();
        public HttpsConfig Https { get; set; } = new();
        public List<HttpVirtualHost> VirtualHosts { get; set; } = new();
        
        public HttpConfig Clone()
        {
            return new HttpConfig
            {
                IsEnabled = IsEnabled,
                Port = Port,
                HttpsPort = HttpsPort,
                HttpsEnabled = HttpsEnabled,
                HttpRedirectToHttps = HttpRedirectToHttps,
                ServerName = ServerName,
                MaxConnections = MaxConnections,
                RequestTimeout = RequestTimeout,
                SessionTimeout = SessionTimeout,
                CompressionEnabled = CompressionEnabled,
                AllowedOrigins = new List<string>(AllowedOrigins),
                CustomHeaders = new Dictionary<string, string>(CustomHeaders),
                Authentication = Authentication.Clone(),
                Https = Https.Clone(),
                VirtualHosts = VirtualHosts.Select(v => v.Clone()).ToList()
            };
        }
        
        public bool Validate()
        {
            if (Port < 1 || Port > 65535) return false;
            if (HttpsPort < 1 || HttpsPort > 65535) return false;
            if (MaxConnections < 1) return false;
            if (RequestTimeout < 1) return false;
            if (SessionTimeout < 1) return false;
            
            return Authentication.Validate() && Https.Validate();
        }
    }
    
    public class HttpAuthConfig
    {
        public bool BasicAuthEnabled { get; set; } = true;
        public bool DigestAuthEnabled { get; set; } = false;
        public bool TokenAuthEnabled { get; set; } = false;
        public string Realm { get; set; } = "NetForge Device Management";
        public Dictionary<string, HttpUser> Users { get; set; } = new();
        public List<string> RequiredRoles { get; set; } = new();
        public int MaxFailedAttempts { get; set; } = 5;
        public int LockoutDuration { get; set; } = 300; // 5 minutes
        
        public HttpAuthConfig Clone()
        {
            return new HttpAuthConfig
            {
                BasicAuthEnabled = BasicAuthEnabled,
                DigestAuthEnabled = DigestAuthEnabled,
                TokenAuthEnabled = TokenAuthEnabled,
                Realm = Realm,
                Users = new Dictionary<string, HttpUser>(Users),
                RequiredRoles = new List<string>(RequiredRoles),
                MaxFailedAttempts = MaxFailedAttempts,
                LockoutDuration = LockoutDuration
            };
        }
        
        public bool Validate()
        {
            if (string.IsNullOrEmpty(Realm)) return false;
            if (MaxFailedAttempts < 1) return false;
            if (LockoutDuration < 0) return false;
            
            return true;
        }
    }
    
    public class HttpsConfig
    {
        public string CertificatePath { get; set; } = "";
        public string CertificatePassword { get; set; } = "";
        public bool RequireClientCertificate { get; set; } = false;
        public List<string> SupportedProtocols { get; set; } = new() { "TLSv1.2", "TLSv1.3" };
        public List<string> SupportedCiphers { get; set; } = new();
        public bool StrictTransportSecurity { get; set; } = true;
        public int HstsMaxAge { get; set; } = 31536000; // 1 year
        
        public HttpsConfig Clone()
        {
            return new HttpsConfig
            {
                CertificatePath = CertificatePath,
                CertificatePassword = CertificatePassword,
                RequireClientCertificate = RequireClientCertificate,
                SupportedProtocols = new List<string>(SupportedProtocols),
                SupportedCiphers = new List<string>(SupportedCiphers),
                StrictTransportSecurity = StrictTransportSecurity,
                HstsMaxAge = HstsMaxAge
            };
        }
        
        public bool Validate()
        {
            if (HstsMaxAge < 0) return false;
            return true;
        }
    }
}
```

## Phase 3: Vendor-Specific HTTP Handlers (Weeks 4-6)

### Cisco HTTP Handler Implementation

```csharp
// NetForge.Simulation.HttpHandlers.Cisco/CiscoHttpHandler.cs
namespace NetForge.Simulation.HttpHandlers.Cisco
{
    public class CiscoHttpHandler : BaseHttpHandler
    {
        public override string VendorName => "Cisco";
        public override int Priority => 200; // Higher priority for vendor-specific
        
        public CiscoHttpHandler(
            IHttpAuthenticator authenticator,
            IHttpApiProvider apiProvider,
            IHttpContentProvider contentProvider) 
            : base(authenticator, apiProvider, contentProvider)
        {
        }
        
        public override async Task<string> GenerateWebInterface(HttpContext context)
        {
            var deviceInfo = await GetDeviceInfo();
            var interfaceStatus = await GetInterfaceStatus();
            var routingTable = await GetRoutingTable();
            
            return GenerateCiscoWebPage(deviceInfo, interfaceStatus, routingTable);
        }
        
        private string GenerateCiscoWebPage(DeviceInfo device, List<InterfaceInfo> interfaces, List<RouteInfo> routes)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Cisco {device.Model} - {device.Hostname}</title>
    <link rel=""stylesheet"" href=""/static/cisco/styles.css"">
    <script src=""/static/cisco/scripts.js""></script>
</head>
<body>
    <div class=""cisco-header"">
        <img src=""/static/cisco/logo.png"" alt=""Cisco"" class=""logo"">
        <h1>{device.Hostname}</h1>
        <div class=""device-info"">
            <span>Model: {device.Model}</span>
            <span>IOS Version: {device.SoftwareVersion}</span>
            <span>Uptime: {device.Uptime}</span>
        </div>
    </div>
    
    <div class=""navigation"">
        <ul>
            <li><a href=""/"" class=""active"">Dashboard</a></li>
            <li><a href=""/interfaces"">Interfaces</a></li>
            <li><a href=""/routing"">Routing</a></li>
            <li><a href=""/protocols"">Protocols</a></li>
            <li><a href=""/security"">Security</a></li>
            <li><a href=""/monitoring"">Monitoring</a></li>
            <li><a href=""/configuration"">Configuration</a></li>
        </ul>
    </div>
    
    <div class=""content"">
        <div class=""dashboard-widgets"">
            <div class=""widget interface-summary"">
                <h3>Interface Summary</h3>
                <div class=""interface-stats"">
                    <div class=""stat"">
                        <span class=""value"">{interfaces.Count(i => i.IsUp)}</span>
                        <span class=""label"">Interfaces Up</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{interfaces.Count(i => !i.IsUp)}</span>
                        <span class=""label"">Interfaces Down</span>
                    </div>
                </div>
                <table class=""interface-table"">
                    <thead>
                        <tr>
                            <th>Interface</th>
                            <th>Status</th>
                            <th>IP Address</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", interfaces.Take(5).Select(i => $@"
                        <tr class=""{(i.IsUp ? "up" : "down")}"">
                            <td>{i.Name}</td>
                            <td><span class=""status {(i.IsUp ? "up" : "down")}"">{(i.IsUp ? "Up" : "Down")}</span></td>
                            <td>{i.IpAddress}</td>
                            <td>{i.Description}</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>
            
            <div class=""widget routing-summary"">
                <h3>Routing Summary</h3>
                <div class=""routing-stats"">
                    <div class=""stat"">
                        <span class=""value"">{routes.Count}</span>
                        <span class=""label"">Total Routes</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{routes.Count(r => r.Protocol == "Connected")}</span>
                        <span class=""label"">Connected</span>
                    </div>
                    <div class=""stat"">
                        <span class=""value"">{routes.Count(r => r.Protocol == "OSPF")}</span>
                        <span class=""label"">OSPF</span>
                    </div>
                </div>
            </div>
            
            <div class=""widget system-resources"">
                <h3>System Resources</h3>
                <div class=""resource-bars"">
                    <div class=""resource"">
                        <label>CPU Usage</label>
                        <div class=""progress-bar"">
                            <div class=""progress"" style=""width: {device.CpuUsage}%""></div>
                        </div>
                        <span>{device.CpuUsage}%</span>
                    </div>
                    <div class=""resource"">
                        <label>Memory Usage</label>
                        <div class=""progress-bar"">
                            <div class=""progress"" style=""width: {device.MemoryUsage}%""></div>
                        </div>
                        <span>{device.MemoryUsage}%</span>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <script>
        // Auto-refresh every 30 seconds
        setTimeout(function() {{ location.reload(); }}, 30000);
    </script>
</body>
</html>";
        }
        
        protected override async Task<HttpResult> OnHandlePostRequest(HttpContext context)
        {
            // Handle Cisco-specific POST requests
            var path = context.Request.Path.ToLower();
            
            return path switch
            {
                "/api/interfaces/configure" => await ConfigureInterface(context),
                "/api/routing/add" => await AddRoute(context),
                "/api/protocols/configure" => await ConfigureProtocol(context),
                "/api/save-config" => await SaveConfiguration(context),
                _ => HttpResult.NotFound("Endpoint not found")
            };
        }
        
        private async Task<HttpResult> ConfigureInterface(HttpContext context)
        {
            var request = context.Request.GetBodyAsJson<InterfaceConfigRequest>();
            if (request == null)
            {
                return HttpResult.BadRequest("Invalid request format");
            }
            
            try
            {
                // Apply interface configuration using CLI
                var commands = new List<string>
                {
                    "configure terminal",
                    $"interface {request.InterfaceName}",
                    request.IsEnabled ? "no shutdown" : "shutdown",
                    $"ip address {request.IpAddress} {request.SubnetMask}",
                    $"description {request.Description}",
                    "exit",
                    "exit"
                };
                
                var results = new List<string>();
                foreach (var command in commands)
                {
                    var result = await _device.ProcessCommandAsync(command);
                    results.Add(result);
                }
                
                return HttpResult.Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Configuration failed: {ex.Message}");
            }
        }
        
        public override IEnumerable<HttpEndpoint> GetSupportedEndpoints()
        {
            return new[]
            {
                new HttpEndpoint { Path = "/", Method = "GET", Description = "Dashboard" },
                new HttpEndpoint { Path = "/interfaces", Method = "GET", Description = "Interface Management" },
                new HttpEndpoint { Path = "/routing", Method = "GET", Description = "Routing Table" },
                new HttpEndpoint { Path = "/protocols", Method = "GET", Description = "Protocol Status" },
                new HttpEndpoint { Path = "/security", Method = "GET", Description = "Security Configuration" },
                new HttpEndpoint { Path = "/monitoring", Method = "GET", Description = "System Monitoring" },
                new HttpEndpoint { Path = "/configuration", Method = "GET", Description = "Device Configuration" },
                new HttpEndpoint { Path = "/api/interfaces/configure", Method = "POST", Description = "Configure Interface" },
                new HttpEndpoint { Path = "/api/routing/add", Method = "POST", Description = "Add Static Route" },
                new HttpEndpoint { Path = "/api/protocols/configure", Method = "POST", Description = "Configure Protocol" },
                new HttpEndpoint { Path = "/api/save-config", Method = "POST", Description = "Save Configuration" }
            };
        }
        
        private async Task<DeviceInfo> GetDeviceInfo()
        {
            return new DeviceInfo
            {
                Hostname = _device.Hostname ?? _device.Name,
                Model = _device.DeviceType,
                SoftwareVersion = "15.1(4)M12a", // Simulated IOS version
                Uptime = GetUptime(),
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage()
            };
        }
        
        private async Task<List<InterfaceInfo>> GetInterfaceStatus()
        {
            var interfaces = new List<InterfaceInfo>();
            
            foreach (var kvp in _device.GetAllInterfaces())
            {
                var config = _device.GetInterface(kvp.Key);
                if (config != null)
                {
                    interfaces.Add(new InterfaceInfo
                    {
                        Name = kvp.Key,
                        IsUp = config.IsUp && !config.IsShutdown,
                        IpAddress = config.IpAddress ?? "Not configured",
                        SubnetMask = config.SubnetMask ?? "",
                        Description = config.Description ?? ""
                    });
                }
            }
            
            return interfaces;
        }
        
        private string GetUptime()
        {
            // Simulate uptime calculation
            var random = new Random(_device.Name.GetHashCode());
            var days = random.Next(1, 100);
            var hours = random.Next(0, 24);
            var minutes = random.Next(0, 60);
            return $"{days} days, {hours} hours, {minutes} minutes";
        }
        
        private int GetCpuUsage()
        {
            // Simulate CPU usage
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Hour);
            return random.Next(5, 25);
        }
        
        private int GetMemoryUsage()
        {
            // Simulate memory usage
            var random = new Random(_device.Name.GetHashCode() + DateTime.Now.Minute);
            return random.Next(30, 70);
        }
    }
}
```

### Juniper HTTP Handler Implementation

```csharp
// NetForge.Simulation.HttpHandlers.Juniper/JuniperHttpHandler.cs
namespace NetForge.Simulation.HttpHandlers.Juniper
{
    public class JuniperHttpHandler : BaseHttpHandler
    {
        public override string VendorName => "Juniper";
        public override int Priority => 200;
        
        public JuniperHttpHandler(
            IHttpAuthenticator authenticator,
            IHttpApiProvider apiProvider,
            IHttpContentProvider contentProvider) 
            : base(authenticator, apiProvider, contentProvider)
        {
        }
        
        public override async Task<string> GenerateWebInterface(HttpContext context)
        {
            var deviceInfo = await GetJuniperDeviceInfo();
            
            return GenerateJunosWebPage(deviceInfo);
        }
        
        private string GenerateJunosWebPage(JuniperDeviceInfo device)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Juniper {device.Model} - J-Web</title>
    <link rel=""stylesheet"" href=""/static/juniper/jweb.css"">
</head>
<body class=""juniper-theme"">
    <div class=""juniper-header"">
        <img src=""/static/juniper/juniper-logo.png"" alt=""Juniper Networks"" class=""logo"">
        <h1>J-Web Device Manager</h1>
        <div class=""device-status"">
            <span class=""hostname"">{device.Hostname}</span>
            <span class=""model"">{device.Model}</span>
            <span class=""junos-version"">JUNOS {device.JunosVersion}</span>
        </div>
    </div>
    
    <div class=""main-container"">
        <nav class=""side-navigation"">
            <ul>
                <li><a href=""/"" class=""active"">Dashboard</a></li>
                <li><a href=""/configure"">Configure</a></li>
                <li><a href=""/monitor"">Monitor</a></li>
                <li><a href=""/maintain"">Maintain</a></li>
                <li><a href=""/diagnose"">Diagnose</a></li>
                <li><a href=""/reports"">Reports</a></li>
            </ul>
        </nav>
        
        <main class=""content-area"">
            <div class=""dashboard-grid"">
                <div class=""widget chassis-status"">
                    <h3>Chassis</h3>
                    <div class=""status-grid"">
                        <div class=""status-item"">
                            <span class=""label"">Temperature</span>
                            <span class=""value ok"">Normal</span>
                        </div>
                        <div class=""status-item"">
                            <span class=""label"">Power</span>
                            <span class=""value ok"">OK</span>
                        </div>
                        <div class=""status-item"">
                            <span class=""label"">CPU</span>
                            <span class=""value"">{device.CpuUsage}%</span>
                        </div>
                    </div>
                </div>
                
                <div class=""widget interface-status"">
                    <h3>Interface Status</h3>
                    <div class=""interface-summary"">
                        <div class=""summary-item up"">
                            <span class=""count"">{device.InterfacesUp}</span>
                            <span class=""label"">Up</span>
                        </div>
                        <div class=""summary-item down"">
                            <span class=""count"">{device.InterfacesDown}</span>
                            <span class=""label"">Down</span>
                        </div>
                    </div>
                </div>
                
                <div class=""widget routing-engine"">
                    <h3>Routing Engine</h3>
                    <table>
                        <tr><td>Uptime</td><td>{device.Uptime}</td></tr>
                        <tr><td>Load Average</td><td>{device.LoadAverage}</td></tr>
                        <tr><td>Memory Usage</td><td>{device.MemoryUsage}%</td></tr>
                    </table>
                </div>
            </div>
        </main>
    </div>
</body>
</html>";
        }
        
        protected override async Task<HttpResult> OnHandlePostRequest(HttpContext context)
        {
            var path = context.Request.Path.ToLower();
            
            return path switch
            {
                "/api/configure/commit" => await CommitConfiguration(context),
                "/api/configure/rollback" => await RollbackConfiguration(context),
                "/api/interfaces/set" => await SetInterface(context),
                "/api/protocols/enable" => await EnableProtocol(context),
                _ => HttpResult.NotFound("J-Web endpoint not found")
            };
        }
        
        private async Task<HttpResult> CommitConfiguration(HttpContext context)
        {
            try
            {
                // Simulate Juniper commit process
                var commitResult = await _device.ProcessCommandAsync("commit check");
                if (commitResult.Contains("error") || commitResult.Contains("Error"))
                {
                    return HttpResult.BadRequest("Configuration validation failed");
                }
                
                await _device.ProcessCommandAsync("commit");
                return HttpResult.Ok(new { success = true, message = "Configuration committed successfully" });
            }
            catch (Exception ex)
            {
                return HttpResult.Error(500, $"Commit failed: {ex.Message}");
            }
        }
        
        public override IEnumerable<HttpEndpoint> GetSupportedEndpoints()
        {
            return new[]
            {
                new HttpEndpoint { Path = "/", Method = "GET", Description = "J-Web Dashboard" },
                new HttpEndpoint { Path = "/configure", Method = "GET", Description = "Configuration Interface" },
                new HttpEndpoint { Path = "/monitor", Method = "GET", Description = "System Monitoring" },
                new HttpEndpoint { Path = "/maintain", Method = "GET", Description = "System Maintenance" },
                new HttpEndpoint { Path = "/diagnose", Method = "GET", Description = "Diagnostics" },
                new HttpEndpoint { Path = "/reports", Method = "GET", Description = "System Reports" },
                new HttpEndpoint { Path = "/api/configure/commit", Method = "POST", Description = "Commit Configuration" },
                new HttpEndpoint { Path = "/api/configure/rollback", Method = "POST", Description = "Rollback Configuration" }
            };
        }
    }
}
```

## Phase 4: Advanced Features (Weeks 7-8)

### REST API Framework

#### 4.1 OpenAPI Integration

```csharp
// NetForge.Simulation.HttpHandlers.Common/Services/OpenApiService.cs
namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    public class OpenApiService
    {
        private readonly IEnumerable<IHttpHandler> _handlers;
        
        public OpenApiService(IEnumerable<IHttpHandler> handlers)
        {
            _handlers = handlers;
        }
        
        public async Task<string> GenerateOpenApiSpec(string vendorName, string deviceType)
        {
            var handler = _handlers.FirstOrDefault(h => h.VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                throw new InvalidOperationException($"No handler found for vendor: {vendorName}");
            }
            
            var endpoints = handler.GetSupportedEndpoints();
            var spec = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = $"{vendorName} Network Device API",
                    Version = "1.0.0",
                    Description = $"REST API for {vendorName} {deviceType} network device management"
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "https://device-ip", Description = "Device HTTPS Interface" },
                    new OpenApiServer { Url = "http://device-ip", Description = "Device HTTP Interface" }
                },
                Paths = new OpenApiPaths()
            };
            
            foreach (var endpoint in endpoints.Where(e => e.Path.StartsWith("/api/")))
            {
                spec.Paths.Add(endpoint.Path, CreateOpenApiPathItem(endpoint));
            }
            
            return JsonSerializer.Serialize(spec, new JsonSerializerOptions { WriteIndented = true });
        }
        
        private OpenApiPathItem CreateOpenApiPathItem(HttpEndpoint endpoint)
        {
            var operation = new OpenApiOperation
            {
                Summary = endpoint.Description,
                Description = $"{endpoint.Method} operation for {endpoint.Path}",
                Tags = new List<OpenApiTag> { new OpenApiTag { Name = GetTagFromPath(endpoint.Path) } },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse { Description = "Success" },
                    ["400"] = new OpenApiResponse { Description = "Bad Request" },
                    ["401"] = new OpenApiResponse { Description = "Unauthorized" },
                    ["404"] = new OpenApiResponse { Description = "Not Found" },
                    ["500"] = new OpenApiResponse { Description = "Internal Server Error" }
                }
            };
            
            var pathItem = new OpenApiPathItem();
            
            switch (endpoint.Method.ToUpper())
            {
                case "GET":
                    pathItem.Operations[OperationType.Get] = operation;
                    break;
                case "POST":
                    pathItem.Operations[OperationType.Post] = operation;
                    break;
                case "PUT":
                    pathItem.Operations[OperationType.Put] = operation;
                    break;
                case "DELETE":
                    pathItem.Operations[OperationType.Delete] = operation;
                    break;
                case "PATCH":
                    pathItem.Operations[OperationType.Patch] = operation;
                    break;
            }
            
            return pathItem;
        }
    }
}
```

### WebSocket Support for Real-time Updates

```csharp
// NetForge.Simulation.HttpHandlers.Common/Services/WebSocketService.cs
namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    public class WebSocketService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ILogger<WebSocketService> _logger;
        
        public WebSocketService(ILogger<WebSocketService> logger)
        {
            _logger = logger;
        }
        
        public async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _connections.TryAdd(connectionId, webSocket);
            
            try
            {
                await HandleWebSocketMessages(connectionId, webSocket, context);
            }
            finally
            {
                _connections.TryRemove(connectionId, out _);
            }
        }
        
        private async Task HandleWebSocketMessages(string connectionId, WebSocket webSocket, HttpContext context)
        {
            var buffer = new byte[4096];
            
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessWebSocketMessage(connectionId, message, context);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket error for connection {ConnectionId}", connectionId);
                    break;
                }
            }
        }
        
        private async Task ProcessWebSocketMessage(string connectionId, string message, HttpContext context)
        {
            try
            {
                var request = JsonSerializer.Deserialize<WebSocketRequest>(message);
                if (request == null) return;
                
                var response = request.Type switch
                {
                    "subscribe" => await HandleSubscribe(connectionId, request, context),
                    "unsubscribe" => await HandleUnsubscribe(connectionId, request, context),
                    "command" => await HandleCommand(connectionId, request, context),
                    _ => new WebSocketResponse { Type = "error", Data = "Unknown message type" }
                };
                
                await SendWebSocketMessage(connectionId, response);
            }
            catch (Exception ex)
            {
                await SendWebSocketMessage(connectionId, new WebSocketResponse 
                { 
                    Type = "error", 
                    Data = $"Error processing message: {ex.Message}" 
                });
            }
        }
        
        public async Task BroadcastUpdate(string eventType, object data)
        {
            var message = new WebSocketResponse
            {
                Type = "update",
                EventType = eventType,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
            
            var tasks = _connections.Values.Select(ws => SendMessage(ws, message));
            await Task.WhenAll(tasks);
        }
        
        private async Task SendMessage(WebSocket webSocket, WebSocketResponse message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
```

## Phase 5: Security & Performance (Week 9)

### Security Features

#### 5.1 Advanced Authentication

```csharp
// NetForge.Simulation.HttpHandlers.Common/Security/JwtAuthenticator.cs
namespace NetForge.Simulation.HttpHandlers.Common.Security
{
    public class JwtAuthenticator : BaseHttpAuthenticator
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly int _expirationMinutes;
        
        public JwtAuthenticator(string secretKey, string issuer, int expirationMinutes = 60)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _expirationMinutes = expirationMinutes;
        }
        
        public override IEnumerable<HttpAuthMethod> SupportedMethods => 
            new[] { HttpAuthMethod.Bearer, HttpAuthMethod.Basic };
        
        public override async Task<AuthenticationResult> AuthenticateRequest(HttpContext context)
        {
            var authorization = context.Request.Authorization;
            if (string.IsNullOrEmpty(authorization))
            {
                return AuthenticationResult.Unauthenticated();
            }
            
            if (authorization.StartsWith("Bearer "))
            {
                var token = authorization.Substring("Bearer ".Length);
                return await ValidateJwtToken(token);
            }
            else if (authorization.StartsWith("Basic "))
            {
                var credentials = authorization.Substring("Basic ".Length);
                return await ValidateBasicAuth(credentials);
            }
            
            return AuthenticationResult.Unauthenticated();
        }
        
        private async Task<AuthenticationResult> ValidateJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                
                var jwtToken = (JwtSecurityToken)validatedToken;
                var username = jwtToken.Claims.First(x => x.Type == "username").Value;
                
                return AuthenticationResult.Authenticated(username);
            }
            catch
            {
                return AuthenticationResult.Unauthenticated();
            }
        }
        
        public async Task<string> GenerateJwtToken(HttpUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("username", user.Username),
                    new Claim("role", user.Role),
                    new Claim("vendor", user.VendorAccess)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
```

#### 5.2 Rate Limiting

```csharp
// NetForge.Simulation.HttpHandlers.Common/Security/RateLimitManager.cs
namespace NetForge.Simulation.HttpHandlers.Common.Security
{
    public class RateLimitManager
    {
        private readonly ConcurrentDictionary<string, ClientRateLimit> _clientLimits = new();
        private readonly RateLimitConfig _config;
        
        public RateLimitManager(RateLimitConfig config)
        {
            _config = config;
        }
        
        public async Task<bool> IsRequestAllowed(HttpContext context)
        {
            if (!_config.IsEnabled)
                return true;
            
            var clientId = GetClientIdentifier(context);
            var limit = _clientLimits.GetOrAdd(clientId, k => new ClientRateLimit(_config));
            
            return limit.IsRequestAllowed();
        }
        
        public async Task<HttpResult> CreateRateLimitResponse(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var limit = _clientLimits.GetOrAdd(clientId, k => new ClientRateLimit(_config));
            
            var retryAfter = limit.GetRetryAfterSeconds();
            
            return new HttpResult
            {
                StatusCode = 429,
                Content = new { error = "Rate limit exceeded", retryAfter },
                Headers = 
                {
                    ["Retry-After"] = retryAfter.ToString(),
                    ["X-Rate-Limit-Limit"] = _config.RequestsPerMinute.ToString(),
                    ["X-Rate-Limit-Remaining"] = "0",
                    ["X-Rate-Limit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(retryAfter).ToUnixTimeSeconds().ToString()
                }
            };
        }
        
        private string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as client identifier
            // In real implementation, might use authenticated user ID
            return context.RemoteEndpoint?.ToString() ?? "unknown";
        }
    }
    
    public class ClientRateLimit
    {
        private readonly Queue<DateTime> _requestTimes = new();
        private readonly RateLimitConfig _config;
        private readonly object _lock = new();
        
        public ClientRateLimit(RateLimitConfig config)
        {
            _config = config;
        }
        
        public bool IsRequestAllowed()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddMinutes(-1);
                
                // Remove old requests outside the window
                while (_requestTimes.Count > 0 && _requestTimes.Peek() < windowStart)
                {
                    _requestTimes.Dequeue();
                }
                
                // Check if under limit
                if (_requestTimes.Count < _config.RequestsPerMinute)
                {
                    _requestTimes.Enqueue(now);
                    return true;
                }
                
                return false;
            }
        }
        
        public int GetRetryAfterSeconds()
        {
            lock (_lock)
            {
                if (_requestTimes.Count == 0)
                    return 0;
                
                var oldestRequest = _requestTimes.Peek();
                var secondsUntilReset = (int)(61 - (DateTime.UtcNow - oldestRequest).TotalSeconds);
                return Math.Max(secondsUntilReset, 1);
            }
        }
    }
}
```

## Phase 6: Testing & Documentation (Week 10)

### Unit Testing Framework

```csharp
// NetForge.Simulation.HttpHandlers.Tests/CiscoHttpHandlerTests.cs
namespace NetForge.Simulation.HttpHandlers.Tests
{
    [TestFixture]
    public class CiscoHttpHandlerTests
    {
        private Mock<INetworkDevice> _mockDevice;
        private Mock<IHttpAuthenticator> _mockAuthenticator;
        private Mock<IHttpApiProvider> _mockApiProvider;
        private Mock<IHttpContentProvider> _mockContentProvider;
        private CiscoHttpHandler _handler;
        
        [SetUp]
        public void SetUp()
        {
            _mockDevice = new Mock<INetworkDevice>();
            _mockAuthenticator = new Mock<IHttpAuthenticator>();
            _mockApiProvider = new Mock<IHttpApiProvider>();
            _mockContentProvider = new Mock<IHttpContentProvider>();
            
            _handler = new CiscoHttpHandler(_mockAuthenticator.Object, _mockApiProvider.Object, _mockContentProvider.Object);
        }
        
        [Test]
        public async Task HandleGetRequest_WhenAuthenticated_ReturnsSuccess()
        {
            // Arrange
            _mockAuthenticator
                .Setup(x => x.AuthenticateRequest(It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticationResult.Authenticated("admin"));
            
            var context = CreateTestContext("/");
            
            // Act
            var result = await _handler.HandleGetRequest(context);
            
            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.IsSuccess, Is.True);
        }
        
        [Test]
        public async Task HandleGetRequest_WhenNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticator
                .Setup(x => x.AuthenticateRequest(It.IsAny<HttpContext>()))
                .ReturnsAsync(AuthenticationResult.Unauthenticated());
            
            _mockAuthenticator
                .Setup(x => x.GenerateChallenge(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(HttpResult.Unauthorized("Authentication required"));
            
            var context = CreateTestContext("/");
            
            // Act
            var result = await _handler.HandleGetRequest(context);
            
            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(401));
            Assert.That(result.IsSuccess, Is.False);
        }
        
        private HttpContext CreateTestContext(string path)
        {
            return new HttpContext
            {
                Request = new HttpRequest { Path = path, Method = "GET" },
                Response = new HttpResponse(),
                Device = _mockDevice.Object
            };
        }
    }
}
```

### Integration Testing

```csharp
// NetForge.Simulation.HttpHandlers.IntegrationTests/HttpServerIntegrationTests.cs
namespace NetForge.Simulation.HttpHandlers.IntegrationTests
{
    [TestFixture]
    public class HttpServerIntegrationTests
    {
        private TestHttpServer _server;
        private HttpClient _client;
        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _server = new TestHttpServer();
            await _server.StartAsync();
            
            _client = new HttpClient();
            _client.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        }
        
        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _client?.Dispose();
            await _server?.StopAsync();
        }
        
        [Test]
        public async Task GET_Dashboard_ReturnsHtmlContent()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:password")));
            
            // Act
            var response = await _client.GetAsync("/");
            
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType.MediaType, Is.EqualTo("text/html"));
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Contains.Substring("<html"));
            Assert.That(content, Contains.Substring("Dashboard"));
        }
        
        [Test]
        public async Task POST_ApiEndpoint_WithValidData_ReturnsSuccess()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:password")));
            
            var requestData = new
            {
                interfaceName = "GigabitEthernet0/1",
                ipAddress = "192.168.1.1",
                subnetMask = "255.255.255.0",
                isEnabled = true
            };
            
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Act
            var response = await _client.PostAsync("/api/interfaces/configure", content);
            
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(responseContent);
            Assert.That(result.success, Is.True);
        }
    }
}
```

## Implementation Timeline

### Week 1-2: Foundation
- ✅ Core HTTP handler interfaces and base classes
- ✅ HTTP context and result models
- ✅ Authentication framework
- ✅ Basic routing system

### Week 3: Protocol Integration
- ✅ HTTP protocol implementation
- ✅ Server infrastructure
- ✅ Configuration system
- ✅ Session management

### Week 4-6: Vendor Handlers
- ✅ Cisco HTTP handler with IOS-style web interface
- ✅ Juniper handler with J-Web interface
- ✅ Arista and Dell handlers
- ✅ Generic handler for standard features

### Week 7-8: Advanced Features
- ✅ REST API framework
- ✅ OpenAPI specification generation
- ✅ WebSocket support for real-time updates
- ✅ Content compression and caching

### Week 9: Security & Performance
- ✅ JWT token authentication
- ✅ Rate limiting
- ✅ HTTPS/SSL support
- ✅ Security headers and CORS

### Week 10: Testing & Documentation
- ✅ Unit test framework
- ✅ Integration testing
- ✅ API documentation
- ✅ User guides and examples

## Success Metrics

### Functional Requirements
- ✅ Vendor-specific web interfaces operational
- ✅ REST API endpoints responding correctly
- ✅ Authentication and authorization working
- ✅ Real-time updates via WebSocket

### Performance Requirements
- ✅ Response time < 100ms for API calls
- ✅ Support for 100+ concurrent connections
- ✅ Memory usage < 50MB per device
- ✅ CPU usage < 5% during normal operation

### Security Requirements
- ✅ HTTPS/TLS encryption
- ✅ Multi-factor authentication support
- ✅ Rate limiting and DDoS protection
- ✅ Secure session management

### Integration Requirements
- ✅ Seamless integration with existing CLI handlers
- ✅ Protocol service bridge operational
- ✅ Configuration synchronization
- ✅ Event system integration

## Architecture Benefits

### 1. **Vendor-Specific Web Interfaces**
- Each vendor gets authentic-looking web management interface
- Cisco devices show IOS-style interfaces
- Juniper devices show J-Web interface
- Brand-specific styling and terminology

### 2. **REST API Standardization**
- OpenAPI/Swagger documentation
- Consistent API patterns across vendors
- JSON-based configuration and monitoring
- Modern development practices

### 3. **Real-time Capabilities**
- WebSocket connections for live updates
- Server-sent events for notifications
- Real-time monitoring dashboards
- Instant configuration feedback

### 4. **Enterprise Security**
- Multiple authentication methods
- Role-based access control
- Rate limiting and DoS protection
- Comprehensive audit logging

### 5. **Developer Experience**
- Auto-generated API documentation
- SDKs and client libraries
- Postman collections
- Integration examples

This HTTP Handlers implementation plan provides a comprehensive web management interface for NetForge network devices, following the same proven architectural patterns as the CLI and SNMP handler systems while adding modern web technologies and security features.