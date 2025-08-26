namespace NetForge.Simulation.Protocols.SSH
{
    /// <summary>
    /// Configuration for the SSH protocol
    /// </summary>
    public class SshConfig
    {
        /// <summary>
        /// Whether SSH is enabled on the device
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// TCP port for the SSH server
        /// </summary>
        public int Port { get; set; } = 22;
        
        /// <summary>
        /// SSH protocol version to use (1 or 2)
        /// </summary>
        public int ProtocolVersion { get; set; } = 2;
        
        /// <summary>
        /// Maximum number of concurrent SSH sessions
        /// </summary>
        public int MaxSessions { get; set; } = 10;
        
        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 60;
        
        /// <summary>
        /// Whether authentication is required
        /// </summary>
        public bool RequireAuthentication { get; set; } = true;
        
        /// <summary>
        /// Username for authentication (if required)
        /// </summary>
        public string Username { get; set; } = "admin";
        
        /// <summary>
        /// Password for authentication (if required)
        /// </summary>
        public string Password { get; set; } = "admin";
        
        /// <summary>
        /// Whether to enable privilege mode
        /// </summary>
        public bool EnablePrivilegeMode { get; set; } = true;
        
        /// <summary>
        /// Enable password for privilege mode
        /// </summary>
        public string EnablePassword { get; set; } = "enable";
        
        /// <summary>
        /// Whether to log SSH commands
        /// </summary>
        public bool LogCommands { get; set; } = true;
        
        /// <summary>
        /// Banner message to display on connection
        /// </summary>
        public string BannerMessage { get; set; } = "Welcome to NetForge Device (SSH)\r\n";
        
        /// <summary>
        /// SSH host key algorithm
        /// </summary>
        public string HostKeyAlgorithm { get; set; } = "ssh-rsa";
        
        /// <summary>
        /// SSH encryption algorithms (comma-separated)
        /// </summary>
        public string EncryptionAlgorithms { get; set; } = "aes128-ctr,aes192-ctr,aes256-ctr";
        
        /// <summary>
        /// SSH MAC algorithms (comma-separated)
        /// </summary>
        public string MacAlgorithms { get; set; } = "hmac-sha1,hmac-sha2-256,hmac-sha2-512";
        
        /// <summary>
        /// Key exchange timeout in seconds
        /// </summary>
        public int KeyExchangeTimeout { get; set; } = 120;
        
        /// <summary>
        /// Whether to allow password authentication
        /// </summary>
        public bool AllowPasswordAuthentication { get; set; } = true;
        
        /// <summary>
        /// Whether to allow public key authentication
        /// </summary>
        public bool AllowPublicKeyAuthentication { get; set; } = true;
        
        /// <summary>
        /// Maximum authentication attempts before disconnection
        /// </summary>
        public int MaxAuthAttempts { get; set; } = 3;
        
        /// <summary>
        /// Enable test mode (disables actual network binding for testing)
        /// </summary>
        public bool TestMode { get; set; } = false;
        
        /// <summary>
        /// Clone this configuration
        /// </summary>
        /// <returns>A copy of this configuration</returns>
        public SshConfig Clone()
        {
            return new SshConfig
            {
                IsEnabled = IsEnabled,
                Port = Port,
                ProtocolVersion = ProtocolVersion,
                MaxSessions = MaxSessions,
                SessionTimeoutMinutes = SessionTimeoutMinutes,
                RequireAuthentication = RequireAuthentication,
                Username = Username,
                Password = Password,
                EnablePrivilegeMode = EnablePrivilegeMode,
                EnablePassword = EnablePassword,
                LogCommands = LogCommands,
                BannerMessage = BannerMessage,
                HostKeyAlgorithm = HostKeyAlgorithm,
                EncryptionAlgorithms = EncryptionAlgorithms,
                MacAlgorithms = MacAlgorithms,
                KeyExchangeTimeout = KeyExchangeTimeout,
                AllowPasswordAuthentication = AllowPasswordAuthentication,
                AllowPublicKeyAuthentication = AllowPublicKeyAuthentication,
                MaxAuthAttempts = MaxAuthAttempts,
                TestMode = TestMode
            };
        }
        
        /// <summary>
        /// Validate the configuration
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return Port > 0 && Port <= 65535 &&
                   ProtocolVersion is 1 or 2 &&
                   MaxSessions > 0 && MaxSessions <= 100 &&
                   SessionTimeoutMinutes > 0 && SessionTimeoutMinutes <= 1440 &&
                   KeyExchangeTimeout > 0 && KeyExchangeTimeout <= 600 &&
                   MaxAuthAttempts > 0 && MaxAuthAttempts <= 10 &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(HostKeyAlgorithm);
        }
        
        /// <summary>
        /// Get configuration summary for display
        /// </summary>
        /// <returns>Configuration summary</returns>
        public override string ToString()
        {
            return $"SSH: {(IsEnabled ? "Enabled" : "Disabled")}, Port: {Port}, " +
                   $"Version: {ProtocolVersion}, MaxSessions: {MaxSessions}, " +
                   $"Auth: {(RequireAuthentication ? "Required" : "None")}";
        }
    }
}
