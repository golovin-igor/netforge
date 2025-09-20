namespace NetForge.Simulation.HttpHandlers.Common
{
    /// <summary>
    /// HTTP content provider interface for serving static and dynamic content
    /// </summary>
    public interface IHttpContentProvider
    {
        /// <summary>
        /// Serve static content
        /// </summary>
        Task<HttpResult> ServeStaticContent(HttpContext context);

        /// <summary>
        /// Serve dynamic content
        /// </summary>
        Task<HttpResult> ServeDynamicContent(HttpContext context, string template, Dictionary<string, object> data);

        /// <summary>
        /// Get content type for file extension
        /// </summary>
        string GetContentType(string fileExtension);

        /// <summary>
        /// Check if file exists
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Read file content
        /// </summary>
        Task<byte[]> ReadFileAsync(string path);

        /// <summary>
        /// Get directory listing
        /// </summary>
        Task<IEnumerable<string>> GetDirectoryListing(string path);

        /// <summary>
        /// Compress content if supported
        /// </summary>
        Task<byte[]> CompressContent(byte[] content, string encoding);

        /// <summary>
        /// Cache content
        /// </summary>
        void CacheContent(string key, byte[] content, TimeSpan expiration);

        /// <summary>
        /// Get cached content
        /// </summary>
        byte[]? GetCachedContent(string key);
    }
}