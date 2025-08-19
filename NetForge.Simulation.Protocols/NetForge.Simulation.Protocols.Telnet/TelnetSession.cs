using System.Net;
using System.Net.Sockets;
using System.Text;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// Device mode enumeration for Telnet sessions
    /// </summary>
    public enum DeviceMode
    {
        UserExec,      // >
        PrivilegedExec, // #
        GlobalConfig,   // (config)#
        InterfaceConfig // (config-if)#
    }
    
    /// <summary>
    /// Represents a Telnet session with a connected client
    /// </summary>
    public class TelnetSession : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed = false;
        
        /// <summary>
        /// Unique session ID
        /// </summary>
        public string SessionId { get; }
        
        /// <summary>
        /// Client endpoint information
        /// </summary>
        public EndPoint ClientEndpoint { get; }
        
        /// <summary>
        /// When the session was created
        /// </summary>
        public DateTime CreatedAt { get; }
        
        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; private set; }
        
        /// <summary>
        /// Whether the session is authenticated
        /// </summary>
        public bool IsAuthenticated { get; private set; } = false;
        
        /// <summary>
        /// Current device mode (user exec, privileged exec, config, etc.)
        /// </summary>
        public DeviceMode CurrentMode { get; private set; } = DeviceMode.UserExec;
        
        /// <summary>
        /// Username if authenticated
        /// </summary>
        public string Username { get; private set; } = "";
        
        /// <summary>
        /// Whether the session is active
        /// </summary>
        public bool IsActive => !_disposed && _tcpClient.Connected;
        
        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public int TimeoutMinutes { get; set; } = 30;
        
        /// <summary>
        /// Event fired when a command is received from the client
        /// </summary>
        public event EventHandler<TelnetCommandEventArgs>? CommandReceived;
        
        /// <summary>
        /// Event fired when the session is disconnected
        /// </summary>
        public event EventHandler<TelnetSessionEventArgs>? SessionDisconnected;
        
        public TelnetSession(TcpClient tcpClient)
        {
            _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _stream = tcpClient.GetStream();
            _cancellationTokenSource = new CancellationTokenSource();
            
            SessionId = Guid.NewGuid().ToString("N")[..8];
            ClientEndpoint = tcpClient.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
            CreatedAt = DateTime.Now;
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Start processing commands from this session
        /// </summary>
        /// <returns>Task representing the session processing</returns>
        public async Task StartAsync()
        {
            try
            {
                await ProcessSessionAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telnet session {SessionId} error: {ex.Message}");
            }
            finally
            {
                OnSessionDisconnected();
            }
        }
        
        /// <summary>
        /// Send a response to the client
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Task representing the send operation</returns>
        public async Task SendResponse(string message)
        {
            if (_disposed || !_tcpClient.Connected)
                return;
                
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(bytes, _cancellationTokenSource.Token);
                await _stream.FlushAsync(_cancellationTokenSource.Token);
                LastActivity = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response to Telnet session {SessionId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send a prompt to the client
        /// </summary>
        /// <param name="hostname">Device hostname</param>
        /// <returns>Task representing the send operation</returns>
        public async Task SendPrompt(string hostname)
        {
            var modeIndicator = CurrentMode switch
            {
                DeviceMode.UserExec => ">",
                DeviceMode.PrivilegedExec => "#",
                DeviceMode.GlobalConfig => "(config)#",
                DeviceMode.InterfaceConfig => "(config-if)#",
                _ => ">"
            };
            
            await SendResponse($"{hostname}{modeIndicator} ");
        }
        
        /// <summary>
        /// Authenticate the session
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="validUsername">Valid username</param>
        /// <param name="validPassword">Valid password</param>
        /// <returns>True if authentication successful</returns>
        public bool Authenticate(string username, string password, string validUsername, string validPassword)
        {
            if (username == validUsername && password == validPassword)
            {
                IsAuthenticated = true;
                Username = username;
                LastActivity = DateTime.Now;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Update the current device mode
        /// </summary>
        /// <param name="mode">New device mode</param>
        public void UpdateMode(DeviceMode mode)
        {
            CurrentMode = mode;
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Check if the session has timed out
        /// </summary>
        /// <returns>True if timed out</returns>
        public bool IsTimedOut()
        {
            return (DateTime.Now - LastActivity).TotalMinutes > TimeoutMinutes;
        }
        
        /// <summary>
        /// Disconnect the session
        /// </summary>
        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();
        }
        
        private async Task ProcessSessionAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            var commandBuffer = new StringBuilder();
            
            while (!cancellationToken.IsCancellationRequested && _tcpClient.Connected)
            {
                try
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                        break; // Client disconnected
                    
                    var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    foreach (char c in data)
                    {
                        if (c == '\r' || c == '\n')
                        {
                            if (commandBuffer.Length > 0)
                            {
                                var command = commandBuffer.ToString().Trim();
                                commandBuffer.Clear();
                                
                                if (!string.IsNullOrEmpty(command))
                                {
                                    LastActivity = DateTime.Now;
                                    OnCommandReceived(command);
                                }
                            }
                        }
                        else if (c == '\b' || c == 127) // Backspace or DEL
                        {
                            if (commandBuffer.Length > 0)
                            {
                                commandBuffer.Length--;
                                // Echo backspace to client
                                await SendResponse("\b \b");
                            }
                        }
                        else if (c >= 32 && c < 127) // Printable characters
                        {
                            commandBuffer.Append(c);
                            // Echo character to client
                            await SendResponse(c.ToString());
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error reading from Telnet session {SessionId}: {ex.Message}");
                    break;
                }
            }
        }
        
        private void OnCommandReceived(string command)
        {
            CommandReceived?.Invoke(this, new TelnetCommandEventArgs(this, command));
        }
        
        private void OnSessionDisconnected()
        {
            SessionDisconnected?.Invoke(this, new TelnetSessionEventArgs(this));
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cancellationTokenSource.Cancel();
                _stream?.Dispose();
                _tcpClient?.Close();
                _cancellationTokenSource?.Dispose();
            }
        }
    }
    
    /// <summary>
    /// Event arguments for Telnet command events
    /// </summary>
    public class TelnetCommandEventArgs : EventArgs
    {
        public TelnetSession Session { get; }
        public string Command { get; }
        
        public TelnetCommandEventArgs(TelnetSession session, string command)
        {
            Session = session;
            Command = command;
        }
    }
    
    /// <summary>
    /// Event arguments for Telnet session events
    /// </summary>
    public class TelnetSessionEventArgs : EventArgs
    {
        public TelnetSession Session { get; }
        
        public TelnetSessionEventArgs(TelnetSession session)
        {
            Session = session;
        }
    }
}
