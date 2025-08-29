using NetForge.Simulation.Protocols.Common.Base;
using System.Net;
using System.Net.Sockets;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Protocols.HTTP
{
    /// <summary>
    /// HTTP protocol state management
    /// </summary>
    public class HttpState : BaseProtocolState
    {
        /// <summary>
        /// Number of active HTTP sessions
        /// </summary>
        public int ActiveSessions { get; set; } = 0;

        /// <summary>
        /// Total number of HTTP requests processed
        /// </summary>
        public long TotalRequests { get; set; } = 0;

        /// <summary>
        /// Total number of HTTP responses sent
        /// </summary>
        public long TotalResponses { get; set; } = 0;

        /// <summary>
        /// Number of failed requests (4xx/5xx responses)
        /// </summary>
        public long FailedRequests { get; set; } = 0;

        /// <summary>
        /// Whether the HTTP server is currently running
        /// </summary>
        public bool IsServerRunning { get; set; } = false;

        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double AverageResponseTime { get; set; } = 0.0;

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["ActiveSessions"] = ActiveSessions;
            baseData["TotalRequests"] = TotalRequests;
            baseData["TotalResponses"] = TotalResponses;
            baseData["FailedRequests"] = FailedRequests;
            baseData["IsServerRunning"] = IsServerRunning;
            baseData["LastActivity"] = LastActivity;
            baseData["AverageResponseTime"] = AverageResponseTime;
            return baseData;
        }
    }

    /// <summary>
    /// HTTP configuration
    /// </summary>
    public class HttpConfig
    {
        /// <summary>
        /// Whether HTTP server is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Port number for HTTP server
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// Whether HTTPS is enabled
        /// </summary>
        public bool HttpsEnabled { get; set; } = false;

        /// <summary>
        /// HTTPS port number
        /// </summary>
        public int HttpsPort { get; set; } = 443;

        /// <summary>
        /// Maximum number of concurrent connections
        /// </summary>
        public int MaxConnections { get; set; } = 10;

        /// <summary>
        /// Authentication required for access
        /// </summary>
        public bool AuthenticationRequired { get; set; } = true;

        /// <summary>
        /// Default username for authentication
        /// </summary>
        public string Username { get; set; } = "admin";

        /// <summary>
        /// Default password for authentication
        /// </summary>
        public string Password { get; set; } = "admin";

        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Document root directory for static files
        /// </summary>
        public string DocumentRoot { get; set; } = "/www";
    }

    /// <summary>
    /// HTTP request representation
    /// </summary>
    public class HttpRequest(IPEndPoint clientEndpoint)
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = "/";
        public string Version { get; set; } = "HTTP/1.1";
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = "";
        public IPEndPoint ClientEndpoint { get; set; } = clientEndpoint;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// HTTP response representation
    /// </summary>
    public class HttpResponse
    {
        private readonly Stream _outputStream;
        public int StatusCode { get; set; } = 200;
        public string StatusText { get; set; } = "OK";
        public Dictionary<string, string> Headers { get; set; } = new();

        public HttpResponse(Stream outputStream)
        {
            _outputStream = outputStream;
            Headers["Server"] = "NetForge HTTP/1.1";
            Headers["Date"] = DateTime.UtcNow.ToString("R");
        }

        public async Task WriteResponse(HttpResponseContent content)
        {
            var response = $"HTTP/1.1 {content.StatusCode} {content.StatusText}\r\n";

            // Add content headers
            Headers["Content-Type"] = content.ContentType;
            Headers["Content-Length"] = content.Content.Length.ToString();

            foreach (var header in Headers)
            {
                response += $"{header.Key}: {header.Value}\r\n";
            }

            response += "\r\n" + content.Content;

            var bytes = System.Text.Encoding.UTF8.GetBytes(response);
            await _outputStream.WriteAsync(bytes, 0, bytes.Length);
            await _outputStream.FlushAsync();
        }

        public async Task WriteErrorResponse(int statusCode, string statusText, string message)
        {
            var content = new HttpResponseContent(statusCode, statusText,
                $"<html><body><h1>{statusCode} {statusText}</h1><p>{message}</p></body></html>",
                "text/html");
            await WriteResponse(content);
        }
    }

    /// <summary>
    /// HTTP response content
    /// </summary>
    public class HttpResponseContent(int statusCode, string statusText, string content, string contentType = "text/html")
    {
        public int StatusCode { get; set; } = statusCode;
        public string StatusText { get; set; } = statusText;
        public string Content { get; set; } = content;
        public string ContentType { get; set; } = contentType;
    }

    /// <summary>
    /// HTTP request event arguments
    /// </summary>
    public class HttpRequestEventArgs(HttpRequest request, HttpResponse response) : EventArgs
    {
        public HttpRequest Request { get; } = request;
        public HttpResponse Response { get; } = response;
    }

    /// <summary>
    /// Simple HTTP server implementation
    /// </summary>
    public class HttpServer(INetworkDevice device, HttpConfig config, HttpSessionManager sessionManager)
        : IDisposable
    {
        private readonly INetworkDevice _device = device;
        private readonly HttpSessionManager _sessionManager = sessionManager;
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;

        public event EventHandler<HttpRequestEventArgs>? RequestReceived;

        public void Start()
        {
            if (_isRunning) return;

            _listener = new TcpListener(IPAddress.Any, config.Port);
            _cancellationTokenSource = new CancellationTokenSource();
            _listener.Start();
            _isRunning = true;

            // Start accepting connections
            _ = Task.Run(AcceptConnections, _cancellationTokenSource.Token);
        }

        public async Task Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();

            await Task.Delay(100); // Give time for cleanup
        }

        private async Task AcceptConnections()
        {
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(tcpClient), _cancellationTokenSource.Token);
                }
                catch (ObjectDisposedException)
                {
                    break; // Server stopped
                }
                catch (Exception ex)
                {
                    _device.AddLogEntry($"HTTP server error accepting connection: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var request = await ParseHttpRequest(stream, (IPEndPoint)client.Client.RemoteEndPoint);
                    var response = new HttpResponse(stream);

                    RequestReceived?.Invoke(this, new HttpRequestEventArgs(request, response));
                }
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"HTTP client handling error: {ex.Message}");
            }
        }

        private async Task<HttpRequest> ParseHttpRequest(Stream stream, IPEndPoint clientEndpoint)
        {
            var request = new HttpRequest(clientEndpoint);
            var reader = new StreamReader(stream);

            // Read request line
            var requestLine = await reader.ReadLineAsync();
            if (requestLine != null)
            {
                var parts = requestLine.Split(' ');
                if (parts.Length >= 3)
                {
                    request.Method = parts[0];
                    request.Url = parts[1];
                    request.Version = parts[2];
                }
            }

            // Read headers
            string line;
            while ((line = await reader.ReadLineAsync()) != null && !string.IsNullOrEmpty(line))
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var name = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();
                    request.Headers[name] = value;
                }
            }

            return request;
        }

        public void Dispose()
        {
            Stop().Wait();
            _listener = null;
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// HTTP session manager
    /// </summary>
    public class HttpSessionManager : IDisposable
    {
        private readonly Dictionary<string, HttpSession> _sessions = new();
        private readonly object _lock = new object();
        private long _totalRequests = 0;

        public List<HttpSession> GetActiveSessions()
        {
            lock (_lock)
            {
                return _sessions.Values.Where(s => !s.IsExpired).ToList();
            }
        }

        public long GetTotalRequestCount() => _totalRequests;

        public DateTime GetLastActivity()
        {
            lock (_lock)
            {
                return _sessions.Values.Any() ? _sessions.Values.Max(s => s.LastActivity) : DateTime.MinValue;
            }
        }

        public async Task UpdateSessions()
        {
            lock (_lock)
            {
                var expiredSessions = _sessions.Values.Where(s => s.IsExpired).ToList();
                foreach (var session in expiredSessions)
                {
                    _sessions.Remove(session.SessionId);
                }
            }
            await Task.CompletedTask;
        }

        public void RecordRequest()
        {
            Interlocked.Increment(ref _totalRequests);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _sessions.Clear();
            }
        }
    }

    /// <summary>
    /// HTTP session representation
    /// </summary>
    public class HttpSession(IPEndPoint clientEndpoint)
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public IPEndPoint ClientEndpoint { get; set; } = clientEndpoint;
        public DateTime StartTime { get; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public bool IsAuthenticated { get; set; } = false;
        public string Username { get; set; } = "";
        public int TimeoutMinutes { get; set; } = 30;

        public bool IsExpired => (DateTime.Now - LastActivity).TotalMinutes > TimeoutMinutes;
    }
}
