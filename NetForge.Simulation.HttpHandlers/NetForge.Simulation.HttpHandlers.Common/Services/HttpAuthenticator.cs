using System.Security.Cryptography;
using System.Text;

namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// HTTP authentication service implementation
    /// </summary>
    public class HttpAuthenticator : IHttpAuthenticator
    {
        private readonly Dictionary<string, HttpUser> _users = new();
        private readonly Dictionary<string, HttpSession> _sessions = new();
        private readonly object _lock = new();

        public HttpAuthenticator()
        {
            // Initialize with default admin user
            var adminUser = new HttpUser
            {
                Username = "admin",
                PasswordHash = HashPassword("admin"),
                Role = "admin",
                IsEnabled = true,
                Permissions = new List<string> { "read", "write", "admin" }
            };
            _users[adminUser.Username] = adminUser;
        }

        /// <summary>
        /// Authenticate HTTP request
        /// </summary>
        public async Task<HttpAuthResult> AuthenticateRequest(HttpContext context)
        {
            // Check for session-based authentication first
            if (context.Request.Cookies.TryGetValue("SESSIONID", out var sessionId))
            {
                var user = await GetUserBySession(sessionId);
                if (user != null && user.IsEnabled && !user.IsLocked())
                {
                    context.User = user;
                    return HttpAuthResult.Success(user, HttpAuthMethod.Cookie);
                }
            }

            // Check for basic authentication
            var authHeader = context.Request.GetHeader("Authorization");
            if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return await AuthenticateBasic(authHeader.Substring(6));
            }

            // Check for bearer token authentication
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return await AuthenticateBearer(authHeader.Substring(7));
            }

            return HttpAuthResult.Failure("No valid authentication method found");
        }

        /// <summary>
        /// Generate authentication challenge
        /// </summary>
        public async Task<HttpResult> GenerateChallenge(HttpContext context, string vendorName)
        {
            var result = HttpResult.Unauthorized("Authentication required");
            result.Headers["WWW-Authenticate"] = $"Basic realm=\"{vendorName} Management Interface\"";
            return result;
        }

        /// <summary>
        /// Create user session
        /// </summary>
        public async Task<HttpSession> CreateSession(HttpUser user)
        {
            var session = new HttpSession
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            };

            lock (_lock)
            {
                _sessions[session.Id] = session;
            }

            return session;
        }

        /// <summary>
        /// Validate user credentials
        /// </summary>
        public async Task<HttpUser?> ValidateCredentials(string username, string password)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(username, out var user))
                {
                    if (user.IsEnabled && !user.IsLocked())
                    {
                        if (VerifyPassword(password, user.PasswordHash))
                        {
                            user.LastLogin = DateTime.UtcNow;
                            user.FailedAttempts = 0;
                            return user;
                        }
                        else
                        {
                            user.FailedAttempts++;
                            if (user.FailedAttempts >= 5)
                            {
                                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                            }
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Get user by session ID
        /// </summary>
        public async Task<HttpUser?> GetUserBySession(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    if (session.IsActive && session.User.IsEnabled && !session.User.IsLocked())
                    {
                        session.LastActivity = DateTime.UtcNow;
                        return session.User;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Logout user session
        /// </summary>
        public async Task Logout(string sessionId)
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    session.IsActive = false;
                    _sessions.Remove(sessionId);
                }
            }
        }

        /// <summary>
        /// Add user
        /// </summary>
        public void AddUser(HttpUser user)
        {
            lock (_lock)
            {
                _users[user.Username] = user;
            }
        }

        /// <summary>
        /// Remove user
        /// </summary>
        public void RemoveUser(string username)
        {
            lock (_lock)
            {
                _users.Remove(username);
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public IEnumerable<HttpUser> GetUsers()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }

        private async Task<HttpAuthResult> AuthenticateBasic(string credentials)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
                var parts = decoded.Split(':');
                if (parts.Length == 2)
                {
                    var user = await ValidateCredentials(parts[0], parts[1]);
                    if (user != null)
                    {
                        return HttpAuthResult.Success(user, HttpAuthMethod.Basic);
                    }
                }
            }
            catch (Exception)
            {
                // Invalid credentials format
            }

            return HttpAuthResult.Failure("Invalid basic authentication credentials");
        }

        private async Task<HttpAuthResult> AuthenticateBearer(string token)
        {
            // Simple token validation - in production would validate JWT
            var user = await GetUserBySession(token);
            if (user != null)
            {
                return HttpAuthResult.Success(user, HttpAuthMethod.Bearer);
            }

            return HttpAuthResult.Failure("Invalid bearer token");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}