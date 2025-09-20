namespace NetForge.Simulation.Protocols.HTTP
{
    /// <summary>
    /// HTTP protocol configuration
    /// </summary>
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

        /// <summary>
        /// Create a copy of this configuration
        /// </summary>
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

        /// <summary>
        /// Validate configuration
        /// </summary>
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

    /// <summary>
    /// HTTP authentication configuration
    /// </summary>
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

    /// <summary>
    /// HTTPS configuration
    /// </summary>
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

    /// <summary>
    /// HTTP virtual host configuration
    /// </summary>
    public class HttpVirtualHost
    {
        public string Hostname { get; set; } = "";
        public string DocumentRoot { get; set; } = "";
        public Dictionary<string, string> Aliases { get; set; } = new();

        public HttpVirtualHost Clone()
        {
            return new HttpVirtualHost
            {
                Hostname = Hostname,
                DocumentRoot = DocumentRoot,
                Aliases = new Dictionary<string, string>(Aliases)
            };
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
    }
}