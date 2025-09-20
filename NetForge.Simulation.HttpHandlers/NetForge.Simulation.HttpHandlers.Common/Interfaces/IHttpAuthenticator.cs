using NetForge.Interfaces.Devices;

namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP authentication interface
    /// </summary>
    public interface IHttpAuthenticator
    {
        /// <summary>
        /// Authenticate HTTP request
        /// </summary>
        Task<HttpAuthResult> AuthenticateRequest(HttpContext context);

        /// <summary>
        /// Generate authentication challenge
        /// </summary>
        Task<HttpResult> GenerateChallenge(HttpContext context, string vendorName);

        /// <summary>
        /// Create user session
        /// </summary>
        Task<HttpSession> CreateSession(HttpUser user);

        /// <summary>
        /// Validate user credentials
        /// </summary>
        Task<HttpUser?> ValidateCredentials(string username, string password);

        /// <summary>
        /// Get user by session ID
        /// </summary>
        Task<HttpUser?> GetUserBySession(string sessionId);

        /// <summary>
        /// Logout user session
        /// </summary>
        Task Logout(string sessionId);
    }

    /// <summary>
    /// HTTP authentication result
    /// </summary>
    public class HttpAuthResult
    {
        public bool IsAuthenticated { get; set; }
        public HttpUser? User { get; set; }
        public string? ErrorMessage { get; set; }
        public HttpAuthMethod AuthMethod { get; set; }

        public static HttpAuthResult Success(HttpUser user, HttpAuthMethod method)
        {
            return new HttpAuthResult
            {
                IsAuthenticated = true,
                User = user,
                AuthMethod = method
            };
        }

        public static HttpAuthResult Failure(string errorMessage)
        {
            return new HttpAuthResult
            {
                IsAuthenticated = false,
                ErrorMessage = errorMessage
            };
        }
    }
}