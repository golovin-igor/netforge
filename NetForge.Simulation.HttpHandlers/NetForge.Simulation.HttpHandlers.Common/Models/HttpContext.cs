namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP request context containing all request information
    /// </summary>
    public class HttpContext
    {
        public HttpRequest Request { get; set; } = new();
        public HttpResponse Response { get; set; } = new();
        public INetworkDevice Device { get; set; } = null!;
        public HttpSession? Session { get; set; }
        public HttpUser? User { get; set; }
        public Dictionary<string, object> Items { get; set; } = new();
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Get typed item from context
        /// </summary>
        public T? GetItem<T>(string key) where T : class
        {
            return Items.TryGetValue(key, out var value) ? value as T : null;
        }

        /// <summary>
        /// Set item in context
        /// </summary>
        public void SetItem<T>(string key, T value) where T : class
        {
            Items[key] = value;
        }
    }
}