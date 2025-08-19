using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Common;
using System.Text;

namespace NetForge.Simulation.CliHandlers.AI
{
    /// <summary>
    /// Interface for AI-powered CLI streaming
    /// </summary>
    public interface IAICliService
    {
        IAsyncEnumerable<string> StreamCommandResponseAsync(string sessionId, NetworkDevice device, string command, object? topology = null);
    }

    /// <summary>
    /// AI-powered CLI handler that uses AI service for command processing
    /// </summary>
    public class AICliHandler : ICliHandler
    {
        private readonly IAICliService _aiService;
        private readonly SessionContextManager _sessionManager;
        private readonly Func<Task<object?>>? _topologyProvider;
        
        // Event for streaming response chunks
        public event Func<string, Task>? ResponseChunkReceived;

        public AICliHandler(
            IAICliService aiService,
            SessionContextManager sessionManager,
            Func<Task<object?>>? topologyProvider = null)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _topologyProvider = topologyProvider;
        }

        public bool CanHandle(CliContext context)
        {
            // This handler can handle ANY command if AI processing is enabled
            return true;
        }

        public bool CanHandlePrefix(CliContext context)
        {
            // Always return true to intercept all commands
            return true;
        }

        public CliResult Handle(CliContext context)
            => HandleAsync(context).GetAwaiter().GetResult();

        public async Task<CliResult> HandleAsync(CliContext context)
        {
            if (context.IsHelpRequest)
            {
                return await HandleHelpRequestAsync(context);
            }

            try
            {
                var sessionId = GetSessionId(context);
                
                // Get topology if available
                var topology = await GetTopologyAsync();
                
                // Stream the AI response
                var response = await StreamAIResponseAsync(context.Device, sessionId, context.FullCommand, topology);
                
                // Update session context
                await _sessionManager.UpdateSessionContextAsync(sessionId, context.FullCommand, response);
                
                return CliResult.Ok(response);
            }
            catch (Exception ex)
            {
                return CliResult.Failed(CliErrorType.ExecutionError, 
                    $"% AI processing error: {ex.Message}");
            }
        }

        private async Task<CliResult> HandleHelpRequestAsync(CliContext context)
        {
            try
            {
                var helpCommand = context.FullCommand + " ?";
                var sessionId = GetSessionId(context);
                var topology = await GetTopologyAsync();

                var response = await StreamAIResponseAsync(context.Device, sessionId, helpCommand, topology);
                return CliResult.Ok(response);
            }
            catch (Exception)
            {
                // Fallback help
                return CliResult.Ok("Available commands: Use tab completion or ? for help");
            }
        }

        private async Task<string> StreamAIResponseAsync(NetworkDevice device, string sessionId, string command, object? topology)
        {
            var responseBuilder = new StringBuilder();
            
            await foreach (var chunk in _aiService.StreamCommandResponseAsync(sessionId, device, command, topology))
            {
                if (!string.IsNullOrEmpty(chunk))
                {
                    responseBuilder.Append(chunk);
                    
                    // Fire event for real-time streaming
                    if (ResponseChunkReceived != null)
                    {
                        await ResponseChunkReceived(chunk);
                    }
                }
            }

            return responseBuilder.ToString();
        }

        private async Task<object?> GetTopologyAsync()
        {
            if (_topologyProvider != null)
            {
                return await _topologyProvider();
            }
            return null;
        }

        private string GetSessionId(CliContext context)
        {
            // Try to get session ID from context parameters
            if (context.Parameters.TryGetValue("SessionId", out var sessionId))
            {
                return sessionId;
            }
            
            // Fallback to device-based session ID
            return $"{context.Device.Name}_{context.Device.DeviceId ?? "default"}";
        }

        // Interface implementations
        public string GetHelp() => "AI-powered CLI command processing";

        public List<string> GetCompletions(CliContext context) => new();

        public (string, string)? GetCommandInfo() => ("*", "AI-powered command handler");

        public List<(string, string)> GetSubCommands(CliContext context) => new();

        public Dictionary<string, ICliHandler> GetSubHandlers() => new();
    }
} 
