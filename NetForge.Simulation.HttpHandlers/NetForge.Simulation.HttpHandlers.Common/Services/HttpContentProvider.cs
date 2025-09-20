namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// HTTP content provider implementation for serving static and dynamic content
    /// </summary>
    public class HttpContentProvider : IHttpContentProvider
    {
        private readonly Dictionary<string, string> _contentTypes = new();
        private readonly Dictionary<string, byte[]> _cache = new();
        private readonly Dictionary<string, DateTime> _cacheExpiration = new();
        private readonly object _cacheLock = new();

        public HttpContentProvider()
        {
            InitializeContentTypes();
        }

        /// <summary>
        /// Serve static content
        /// </summary>
        public async Task<HttpResult> ServeStaticContent(HttpContext context)
        {
            var path = context.Request.Path;

            // Security check - prevent directory traversal
            if (path.Contains("..") || Path.IsPathRooted(path))
            {
                return HttpResult.Forbidden("Access denied");
            }

            // Map virtual path to physical path
            var physicalPath = MapToPhysicalPath(path);
            if (!FileExists(physicalPath))
            {
                return HttpResult.NotFound("File not found");
            }

            // Check cache first
            var cacheKey = $"{context.Device?.Vendor}:{path}";
            byte[] content;

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(cacheKey, out var cachedContent) &&
                    _cacheExpiration.TryGetValue(cacheKey, out var expiration) &&
                    expiration > DateTime.UtcNow)
                {
                    content = cachedContent;
                }
                else
                {
                    content = await ReadFileAsync(physicalPath);
                    CacheContent(cacheKey, content, TimeSpan.FromMinutes(10));
                }
            }

            var extension = Path.GetExtension(path);
            var contentType = GetContentType(extension);

            // Check for compression support
            var acceptEncoding = context.Request.GetHeader("Accept-Encoding");
            if (acceptEncoding.Contains("gzip") && ShouldCompress(path))
            {
                content = await CompressContent(content, "gzip");
                contentType += "; charset=utf-8";
            }

            return HttpResult.Ok(content, contentType);
        }

        /// <summary>
        /// Serve dynamic content
        /// </summary>
        public async Task<HttpResult> ServeDynamicContent(HttpContext context, string template, Dictionary<string, object> data)
        {
            var content = await ProcessTemplate(template, data);
            return HttpResult.Ok(content, "text/html");
        }

        /// <summary>
        /// Get content type for file extension
        /// </summary>
        public string GetContentType(string fileExtension)
        {
            return _contentTypes.TryGetValue(fileExtension.ToLower(), out var contentType)
                ? contentType
                : "application/octet-stream";
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Read file content
        /// </summary>
        public async Task<byte[]> ReadFileAsync(string path)
        {
            return await File.ReadAllBytesAsync(path);
        }

        /// <summary>
        /// Get directory listing
        /// </summary>
        public async Task<IEnumerable<string>> GetDirectoryListing(string path)
        {
            if (!Directory.Exists(path))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.GetFiles(path).Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Compress content if supported
        /// </summary>
        public async Task<byte[]> CompressContent(byte[] content, string encoding)
        {
            if (encoding.ToLower() == "gzip")
            {
                using var outputStream = new MemoryStream();
                using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionMode.Compress))
                {
                    await gzipStream.WriteAsync(content, 0, content.Length);
                }
                return outputStream.ToArray();
            }

            return content;
        }

        /// <summary>
        /// Cache content
        /// </summary>
        public void CacheContent(string key, byte[] content, TimeSpan expiration)
        {
            lock (_cacheLock)
            {
                _cache[key] = content;
                _cacheExpiration[key] = DateTime.UtcNow.Add(expiration);
            }
        }

        /// <summary>
        /// Get cached content
        /// </summary>
        public byte[]? GetCachedContent(string key)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out var content) &&
                    _cacheExpiration.TryGetValue(key, out var expiration) &&
                    expiration > DateTime.UtcNow)
                {
                    return content;
                }
                return null;
            }
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
                _cacheExpiration.Clear();
            }
        }

        private void InitializeContentTypes()
        {
            _contentTypes[".html"] = "text/html";
            _contentTypes[".htm"] = "text/html";
            _contentTypes[".css"] = "text/css";
            _contentTypes[".js"] = "application/javascript";
            _contentTypes[".json"] = "application/json";
            _contentTypes[".xml"] = "application/xml";
            _contentTypes[".txt"] = "text/plain";
            _contentTypes[".png"] = "image/png";
            _contentTypes[".jpg"] = "image/jpeg";
            _contentTypes[".jpeg"] = "image/jpeg";
            _contentTypes[".gif"] = "image/gif";
            _contentTypes[".ico"] = "image/x-icon";
            _contentTypes[".svg"] = "image/svg+xml";
            _contentTypes[".woff"] = "font/woff";
            _contentTypes[".woff2"] = "font/woff2";
            _contentTypes[".ttf"] = "font/ttf";
            _contentTypes[".eot"] = "application/vnd.ms-fontobject";
        }

        private string MapToPhysicalPath(string virtualPath)
        {
            // This would map virtual paths to physical paths based on vendor
            // For now, return a placeholder implementation
            return Path.Combine("wwwroot", virtualPath.TrimStart('/'));
        }

        private async Task<string> ProcessTemplate(string template, Dictionary<string, object> data)
        {
            var result = template;

            foreach (var kvp in data)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
            }

            return result;
        }

        private bool ShouldCompress(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            var compressibleTypes = new[] { ".html", ".css", ".js", ".json", ".xml", ".txt" };
            return compressibleTypes.Contains(extension);
        }
    }
}