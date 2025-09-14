using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetForge.Player.Configuration;

namespace NetForge.Player.Core;

/// <summary>
/// Main application class for NetForge.Player interactive console
/// </summary>
public class PlayerApplication
{
    private readonly IServiceProvider _services;
    private readonly ICommandProcessor _commandProcessor;
    private readonly ILogger<PlayerApplication> _logger;
    private readonly PlayerConfiguration _configuration;
    private readonly InteractiveShell _shell;
    private bool _isRunning;

    public PlayerApplication(IServiceProvider services)
    {
        _services = services;
        _commandProcessor = services.GetRequiredService<ICommandProcessor>();
        _logger = services.GetRequiredService<ILogger<PlayerApplication>>();
        _configuration = services.GetRequiredService<PlayerConfiguration>();
        _shell = new InteractiveShell(_commandProcessor, _configuration);
    }

    /// <summary>
    /// Run the main application loop
    /// </summary>
    public async Task RunAsync()
    {
        _isRunning = true;
        
        try
        {
            DisplayStartupBanner();
            await InitializeServicesAsync();
            
            _logger.LogInformation("NetForge.Player started successfully");
            
            // Main REPL loop
            while (_isRunning)
            {
                try
                {
                    var input = await _shell.ReadLineAsync();
                    
                    if (string.IsNullOrWhiteSpace(input))
                        continue;
                    
                    var result = await _commandProcessor.ProcessAsync(input);
                    
                    // Display result
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        if (result.IsError)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error: {result.Message}");
                        }
                        else if (result.IsWarning)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Warning: {result.Message}");
                        }
                        else
                        {
                            Console.WriteLine(result.Message);
                        }
                        Console.ResetColor();
                    }
                    
                    // Handle exit command
                    if (result.ShouldExit)
                    {
                        _isRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
        finally
        {
            await ShutdownAsync();
        }
    }

    /// <summary>
    /// Stop the application
    /// </summary>
    public void Stop()
    {
        _logger.LogInformation("Stopping NetForge.Player...");
        _isRunning = false;
    }

    private void DisplayStartupBanner()
    {
        ConsoleBanner.Print();
        
        // Display version information
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"\nVersion: {version}");
        Console.WriteLine($"Runtime: .NET {Environment.Version}");
        Console.WriteLine($"Platform: {Environment.OSVersion}\n");
        
        SimulationInfo.Print();
        
        Console.WriteLine("Type 'help' for available commands or 'exit' to quit.\n");
    }

    private async Task InitializeServicesAsync()
    {
        _logger.LogDebug("Initializing services...");
        
        // Initialize any background services
        var networkManager = _services.GetService<INetworkManager>();
        if (networkManager != null)
        {
            await networkManager.InitializeAsync();
        }
        
        // Initialize terminal server if configured
        if (_configuration.Terminal?.Enabled == true)
        {
            var terminalServer = _services.GetService<ITerminalServerManager>();
            if (terminalServer != null)
            {
                await terminalServer.StartAsync();
                _logger.LogInformation("Terminal server started on port {Port}", 
                    _configuration.Terminal.Port);
            }
        }
        
        _logger.LogDebug("Services initialized");
    }

    private async Task ShutdownAsync()
    {
        _logger.LogInformation("Shutting down NetForge.Player...");
        
        try
        {
            // Stop terminal server
            var terminalServer = _services.GetService<ITerminalServerManager>();
            if (terminalServer != null)
            {
                await terminalServer.StopAsync();
            }
            
            // Save network state
            var networkManager = _services.GetService<INetworkManager>();
            if (networkManager != null)
            {
                await networkManager.SaveStateAsync();
            }
            
            Console.WriteLine("\nGoodbye!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shutdown");
        }
    }
}