namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// Configuration for the Telnet protocol
    /// </summary>
    public class TelnetConfig
    {
        /// <summary>
        /// Whether Telnet is enabled on the device
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// TCP port for the Telnet server
        /// </summary>
        public int Port { get; set; } = 23;
        
        /// <summary>
        /// Maximum number of concurrent Telnet sessions
        /// </summary>
        public int MaxSessions { get; set; } = 5;
        
        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;
        
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
        /// Whether to log Telnet commands
        /// </summary>
        public bool LogCommands { get; set; } = true;
        
        /// <summary>
        /// Banner message to display on connection
        /// </summary>
        public string BannerMessage { get; set; } = "Welcome to NetForge Device\r\n";
        
        /// <summary>
        /// Clone this configuration
        /// </summary>
        /// <returns>A copy of this configuration</returns>
        public TelnetConfig Clone()
        {
            return new TelnetConfig
            {
                IsEnabled = IsEnabled,
                Port = Port,
                MaxSessions = MaxSessions,
                SessionTimeoutMinutes = SessionTimeoutMinutes,
                RequireAuthentication = RequireAuthentication,
                Username = Username,
                Password = Password,
                EnablePrivilegeMode = EnablePrivilegeMode,
                EnablePassword = EnablePassword,
                LogCommands = LogCommands,
                BannerMessage = BannerMessage
            };
        }
        
        /// <summary>
        /// Validate the configuration
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return Port > 0 && Port <= 65535 &&
                   MaxSessions > 0 && MaxSessions <= 100 &&
                   SessionTimeoutMinutes > 0 && SessionTimeoutMinutes <= 1440 &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }
        
        /// <summary>
        /// Get configuration summary for display
        /// </summary>
        /// <returns>Configuration summary</returns>
        public override string ToString()
        {
            return $"Telnet: {(IsEnabled ? "Enabled" : "Disabled")}, Port: {Port}, " +
                   $"MaxSessions: {MaxSessions}, Auth: {(RequireAuthentication ? "Required" : "None")}";
        }
    }
}
