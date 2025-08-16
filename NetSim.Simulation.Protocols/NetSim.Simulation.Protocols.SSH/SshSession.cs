using System.Net.Sockets;
using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Protocols.SSH
{
    /// <summary>
    /// Represents an SSH session for a connected client
    /// </summary>
    public class SshSession : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly SshConfig _config;
        private readonly NetworkDevice _device;
        private readonly string _sessionId;
        private readonly DateTime _connectionTime;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        
        private bool _isAuthenticated;
        private string? _username;
        private DateTime _lastActivity;
        private bool _isDisposed;
        
        public string SessionId => _sessionId;
        public string ClientEndpoint { get; }
        public DateTime ConnectionTime => _connectionTime;
        public DateTime LastActivity => _lastActivity;
        public bool IsAuthenticated => _isAuthenticated;
        public string? Username => _username;
        public bool IsConnected => _tcpClient.Connected && !_isDisposed;
        public string EncryptionAlgorithm { get; private set; } = "aes128-ctr"; // Simulated encryption
        
        public SshSession(TcpClient tcpClient, SshConfig config, NetworkDevice device)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _device = device ?? throw new ArgumentNullException(nameof(device));
            
            _sessionId = Guid.NewGuid().ToString("N")[..8];
            _connectionTime = DateTime.Now;
            _lastActivity = DateTime.Now;
            
            ClientEndpoint = _tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";
            
            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream, Encoding.UTF8);
            _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
            
            // Set encryption algorithm based on config
            SetEncryptionAlgorithm();
        }
        
        private void SetEncryptionAlgorithm()
        {
            // Parse encryption algorithms from config and select the first one
            var algorithms = _config.EncryptionAlgorithms.Split(',');
            if (algorithms.Length > 0)
            {
                EncryptionAlgorithm = algorithms[0].Trim();
            }
        }
        
        /// <summary>
        /// Set the session as authenticated
        /// </summary>
        /// <param name="username">Authenticated username</param>
        public void SetAuthenticated(string username)
        {
            _isAuthenticated = true;
            _username = username;
            _lastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Send a message to the client
        /// </summary>
        /// <param name="message">Message to send</param>
        public async Task SendMessage(string message)
        {
            if (!IsConnected)
                return;
            
            try
            {
                await _writer.WriteAsync(message);
                _lastActivity = DateTime.Now;
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error sending message to SSH session {_sessionId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send a response to the client
        /// </summary>
        /// <param name="response">Response to send</param>
        public async Task SendResponse(string response)
        {
            await SendMessage(response);
        }
        
        /// <summary>
        /// Send a prompt to the client
        /// </summary>
        /// <param name="hostname">Device hostname for prompt</param>
        public async Task SendPrompt(string hostname)
        {
            var prompt = $"{hostname}# ";
            await SendMessage(prompt);
        }
        
        /// <summary>
        /// Read a line from the client
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="echoInput">Whether to echo input back to client</param>
        /// <returns>Line read from client</returns>
        public async Task<string> ReadLineAsync(CancellationToken cancellationToken, bool echoInput = true)
        {
            if (!IsConnected)
                return string.Empty;
            
            try
            {
                var line = new StringBuilder();
                var buffer = new char[1];
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRead = await _reader.ReadAsync(buffer, 0, 1);
                    if (bytesRead == 0)
                        break; // Connection closed
                    
                    var ch = buffer[0];
                    
                    if (ch == '\r' || ch == '\n')
                    {
                        // Handle line ending
                        if (ch == '\r')
                        {
                            // Look for following \n
                            _reader.Peek(); // This will read the \n if present
                        }
                        
                        if (echoInput)
                        {
                            await _writer.WriteAsync("\r\n");
                        }
                        
                        _lastActivity = DateTime.Now;
                        return line.ToString();
                    }
                    else if (ch == '\b' || ch == '\x7f') // Backspace or Delete
                    {
                        if (line.Length > 0)
                        {
                            line.Remove(line.Length - 1, 1);
                            if (echoInput)
                            {
                                await _writer.WriteAsync("\b \b"); // Erase character
                            }
                        }
                    }
                    else if (ch >= 32 && ch <= 126) // Printable characters
                    {
                        line.Append(ch);
                        if (echoInput)
                        {
                            await _writer.WriteAsync(ch);
                        }
                    }
                    // Ignore other control characters
                }
                
                _lastActivity = DateTime.Now;
                return line.ToString();
            }
            catch (Exception ex)
            {
                _device.AddLogEntry($"Error reading from SSH session {_sessionId}: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Check if the session has timed out
        /// </summary>
        /// <returns>True if session has timed out</returns>
        public bool IsTimedOut()
        {
            var timeoutMinutes = _config.SessionTimeoutMinutes;
            return (DateTime.Now - _lastActivity).TotalMinutes > timeoutMinutes;
        }
        
        /// <summary>
        /// Disconnect the session
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _tcpClient.Close();
            }
            catch
            {
                // Ignore errors when disconnecting
            }
        }
        
        /// <summary>
        /// Get session statistics
        /// </summary>
        /// <returns>Dictionary of session statistics</returns>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["SessionId"] = _sessionId,
                ["ClientEndpoint"] = ClientEndpoint,
                ["ConnectionTime"] = _connectionTime,
                ["LastActivity"] = _lastActivity,
                ["IsAuthenticated"] = _isAuthenticated,
                ["Username"] = _username ?? "none",
                ["IsConnected"] = IsConnected,
                ["EncryptionAlgorithm"] = EncryptionAlgorithm,
                ["DurationMinutes"] = (DateTime.Now - _connectionTime).TotalMinutes
            };
        }
        
        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _isDisposed = true;
            
            try
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _networkStream?.Dispose();
                _tcpClient?.Close();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}