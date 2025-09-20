namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP request model
    /// </summary>
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

        /// <summary>
        /// Get body as string
        /// </summary>
        public string GetBodyAsString()
        {
            return System.Text.Encoding.UTF8.GetString(Body);
        }

        /// <summary>
        /// Get body as JSON object
        /// </summary>
        public T? GetBodyAsJson<T>() where T : class
        {
            if (ContentType.Contains("application/json"))
            {
                var json = GetBodyAsString();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }

        /// <summary>
        /// Get query parameter value
        /// </summary>
        public string GetQueryParameter(string key)
        {
            return QueryParameters.GetValueOrDefault(key, "");
        }

        /// <summary>
        /// Get header value
        /// </summary>
        public string GetHeader(string key)
        {
            return Headers.GetValueOrDefault(key, "");
        }

        /// <summary>
        /// Get cookie value
        /// </summary>
        public string GetCookie(string key)
        {
            return Cookies.GetValueOrDefault(key, "");
        }

        public string ContentType => Headers.GetValueOrDefault("Content-Type", "");
        public int ContentLength => int.Parse(Headers.GetValueOrDefault("Content-Length", "0"));
        public string UserAgent => Headers.GetValueOrDefault("User-Agent", "");
        public string Authorization => Headers.GetValueOrDefault("Authorization", "");
    }
}