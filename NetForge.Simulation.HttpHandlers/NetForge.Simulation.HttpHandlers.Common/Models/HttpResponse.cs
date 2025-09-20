namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP response model
    /// </summary>
    public class HttpResponse
    {
        public int StatusCode { get; set; } = 200;
        public string StatusMessage { get; set; } = "OK";
        public HttpVersion Version { get; set; } = HttpVersion.Http11;
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> Cookies { get; set; } = new();
        public byte[] Body { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Set response body as string
        /// </summary>
        public void SetBody(string content)
        {
            Body = System.Text.Encoding.UTF8.GetBytes(content);
            Headers["Content-Length"] = Body.Length.ToString();
        }

        /// <summary>
        /// Set response body as JSON
        /// </summary>
        public void SetJsonBody<T>(T content) where T : class
        {
            var json = System.Text.Json.JsonSerializer.Serialize(content);
            SetBody(json);
            Headers["Content-Type"] = "application/json";
        }

        /// <summary>
        /// Set response body as HTML
        /// </summary>
        public void SetHtmlBody(string html)
        {
            SetBody(html);
            Headers["Content-Type"] = "text/html";
        }

        /// <summary>
        /// Set response body as plain text
        /// </summary>
        public void SetTextBody(string text)
        {
            SetBody(text);
            Headers["Content-Type"] = "text/plain";
        }

        /// <summary>
        /// Set cookie
        /// </summary>
        public void SetCookie(string name, string value, CookieOptions? options = null)
        {
            var cookieValue = $"{name}={value}";
            if (options != null)
            {
                if (options.Expires.HasValue)
                    cookieValue += $"; Expires={options.Expires.Value:R}";
                if (options.MaxAge.HasValue)
                    cookieValue += $"; Max-Age={options.MaxAge.Value}";
                if (options.Domain != null)
                    cookieValue += $"; Domain={options.Domain}";
                if (options.Path != null)
                    cookieValue += $"; Path={options.Path}";
                if (options.Secure)
                    cookieValue += "; Secure";
                if (options.HttpOnly)
                    cookieValue += "; HttpOnly";
                if (options.SameSite != SameSiteMode.None)
                    cookieValue += $"; SameSite={options.SameSite}";
            }
            Cookies[name] = cookieValue;
        }

        /// <summary>
        /// Set header
        /// </summary>
        public void SetHeader(string name, string value)
        {
            Headers[name] = value;
        }

        /// <summary>
        /// Get header value
        /// </summary>
        public string GetHeader(string name)
        {
            return Headers.GetValueOrDefault(name, "");
        }

        /// <summary>
        /// Redirect to URL
        /// </summary>
        public void Redirect(string url, int statusCode = 302)
        {
            StatusCode = statusCode;
            StatusMessage = statusCode == 301 ? "Moved Permanently" : "Found";
            Headers["Location"] = url;
        }
    }

    /// <summary>
    /// Cookie options
    /// </summary>
    public class CookieOptions
    {
        public DateTime? Expires { get; set; }
        public int? MaxAge { get; set; }
        public string? Domain { get; set; }
        public string? Path { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    }

    /// <summary>
    /// SameSite mode for cookies
    /// </summary>
    public enum SameSiteMode
    {
        None,
        Lax,
        Strict
    }
}