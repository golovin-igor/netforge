namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP result model
    /// </summary>
    public class HttpResult
    {
        public int StatusCode { get; set; } = 200;
        public object? Content { get; set; }
        public string ContentType { get; set; } = "text/html";
        public bool IsSuccess { get; set; } = true;
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> Cookies { get; set; } = new();

        /// <summary>
        /// Create success result
        /// </summary>
        public static HttpResult Ok(object content = null, string contentType = "application/json")
        {
            return new HttpResult
            {
                StatusCode = 200,
                Content = content,
                ContentType = contentType,
                IsSuccess = true
            };
        }

        /// <summary>
        /// Create bad request result
        /// </summary>
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

        /// <summary>
        /// Create unauthorized result
        /// </summary>
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

        /// <summary>
        /// Create forbidden result
        /// </summary>
        public static HttpResult Forbidden(string message = "Forbidden")
        {
            return new HttpResult
            {
                StatusCode = 403,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }

        /// <summary>
        /// Create not found result
        /// </summary>
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

        /// <summary>
        /// Create internal server error result
        /// </summary>
        public static HttpResult Error(int statusCode, string message)
        {
            return new HttpResult
            {
                StatusCode = statusCode,
                Content = new { error = message },
                ContentType = "application/json",
                IsSuccess = false
            };
        }

        /// <summary>
        /// Create redirect result
        /// </summary>
        public static HttpResult Redirect(string url, int statusCode = 302)
        {
            return new HttpResult
            {
                StatusCode = statusCode,
                Content = null,
                ContentType = "text/plain",
                IsSuccess = true,
                Headers = { ["Location"] = url }
            };
        }
    }

    /// <summary>
    /// HTTP version enumeration
    /// </summary>
    public enum HttpVersion
    {
        Http10,
        Http11,
        Http20
    }

    /// <summary>
    /// HTTP authentication method enumeration
    /// </summary>
    public enum HttpAuthMethod
    {
        None,
        Basic,
        Digest,
        Bearer,
        Cookie,
        Custom
    }

    /// <summary>
    /// HTTP endpoint model
    /// </summary>
    public class HttpEndpoint
    {
        public string Path { get; set; } = "";
        public string Method { get; set; } = "GET";
        public string Description { get; set; } = "";
        public bool RequiresAuth { get; set; } = true;
        public List<string> RequiredRoles { get; set; } = new();
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Check if endpoint matches the given path and method
        /// </summary>
        public bool Matches(string path, string method = "GET")
        {
            return Path.Equals(path, StringComparison.OrdinalIgnoreCase) &&
                   Method.Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if endpoint matches the given path (method-agnostic)
        /// </summary>
        public bool Matches(string path)
        {
            return Path.Equals(path, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// HTTP user model
    /// </summary>
    public class HttpUser
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = "user";
        public List<string> Permissions { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public DateTime LastLogin { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// Check if user has permission
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if user has role
        /// </summary>
        public bool HasRole(string role)
        {
            return Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if user account is locked
        /// </summary>
        public bool IsLocked()
        {
            return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
        }
    }

    /// <summary>
    /// HTTP session model
    /// </summary>
    public class HttpSession
    {
        public string Id { get; set; } = "";
        public HttpUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Get session age in seconds
        /// </summary>
        public double AgeSeconds => (DateTime.UtcNow - CreatedAt).TotalSeconds;

        /// <summary>
        /// Get time since last activity in seconds
        /// </summary>
        public double IdleSeconds => (DateTime.UtcNow - LastActivity).TotalSeconds;

        /// <summary>
        /// Set session data
        /// </summary>
        public void SetData<T>(string key, T value) where T : class
        {
            Data[key] = value;
        }

        /// <summary>
        /// Get session data
        /// </summary>
        public T? GetData<T>(string key) where T : class
        {
            return Data.TryGetValue(key, out var value) ? value as T : null;
        }
    }
}