using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetForge.Player.Configuration;
using NetForge.Player.Core;
using NetForge.Player.Services;
using NetForge.Simulation.Common;
using NetForge.Simulation.Topology;

namespace NetForge.Player;

class Program
{
    private static IHost? _host;
    private static PlayerApplication? _application;

    static async Task<int> Main(string[] args)
    {
        try
        {
            // Handle command line arguments
            if (args.Length > 0)
            {
                return HandleCommandLineArgs(args);
            }

            // Build and configure host
            _host = CreateHostBuilder(args).Build();

            // Setup console cancellation
            Console.CancelKeyPress += OnCancelKeyPress;

            // Get application instance and run
            _application = _host.Services.GetRequiredService<PlayerApplication>();
            await _application.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();

            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Console.WriteLine(ex.StackTrace);
            }

            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Load configuration from multiple sources
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddEnvironmentVariables("NETFORGE_")
                      .AddCommandLine(args);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();

                // Configure logging based on environment
                if (context.HostingEnvironment.IsDevelopment())
                {
                    logging.AddConsole()
                           .AddDebug()
                           .SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    logging.AddConsole()
                           .SetMinimumLevel(LogLevel.Information);
                }

                // Add file logging if configured
                var logPath = context.Configuration["Logging:FilePath"];
                if (!string.IsNullOrEmpty(logPath))
                {
                    // TODO: Add file logging provider
                }
            })
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                services.Configure<PlayerConfiguration>(context.Configuration.GetSection("Player"));
                services.AddSingleton(provider =>
                {
                    var config = context.Configuration.GetSection("Player").Get<PlayerConfiguration>();
                    return config ?? new PlayerConfiguration();
                });

                // Register core services
                services.AddSingleton<PlayerApplication>();
                services.AddSingleton<ICommandProcessor, CommandProcessor>();

                // TODO: Register NetForge simulation services when available
                // services.AddNetForgeSimulation(); // Extension method from NetForge.Simulation.Common

                // Register network services
                services.AddSingleton<INetworkManager, NetworkManager>();
                services.AddSingleton<ISessionManager, SessionManager>();

                // Register terminal services (if enabled)
                services.AddSingleton<ITerminalServerManager, TerminalServerManager>();

                // Register all player commands
                RegisterCommands(services);

                // Add hosted services for background tasks
                services.AddHostedService<NetworkManagerBackgroundService>();
            })
            .UseConsoleLifetime();

    private static void RegisterCommands(IServiceCollection services)
    {
        // Auto-discover and register all commands from the current assembly
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IPlayerCommand).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var commandType in commandTypes)
        {
            services.AddTransient(typeof(IPlayerCommand), commandType);
        }
    }

    private static int HandleCommandLineArgs(string[] args)
    {
        var arg = args[0].ToLowerInvariant();

        return arg switch
        {
            "--version" or "-v" => ShowVersion(),
            "--help" or "-h" or "-?" => ShowHelp(),
            "--check" or "-c" => CheckEnvironment(),
            _ => RunWithArgs(args)
        };
    }

    private static int ShowVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var buildDate = GetBuildDate();

        Console.WriteLine($"NetForge.Player Version {version}");
        Console.WriteLine($"Build Date: {buildDate}");
        Console.WriteLine($"Runtime: .NET {Environment.Version}");
        Console.WriteLine($"Platform: {Environment.OSVersion}");

        return 0;
    }

    private static int ShowHelp()
    {
        Console.WriteLine("NetForge.Player - Interactive Network Simulation Console");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  NetForge.Player                    Start interactive mode");
        Console.WriteLine("  NetForge.Player --version          Show version information");
        Console.WriteLine("  NetForge.Player --help             Show this help message");
        Console.WriteLine("  NetForge.Player --check            Check environment and prerequisites");
        Console.WriteLine();
        Console.WriteLine("Interactive Commands:");
        Console.WriteLine("  help                               Show available commands");
        Console.WriteLine("  create device <name> --type <type> --vendor <vendor>");
        Console.WriteLine("                                     Create a new network device");
        Console.WriteLine("  list devices                       List all devices");
        Console.WriteLine("  connect <device>                   Connect to device console");
        Console.WriteLine("  exit                               Exit the application");
        Console.WriteLine();
        Console.WriteLine("Environment Variables:");
        Console.WriteLine("  NETFORGE_LogLevel                  Set logging level (Debug, Info, Warning, Error)");
        Console.WriteLine("  NETFORGE_Player__ConfigPath        Path to configuration file");
        Console.WriteLine("  NETFORGE_Player__DataPath          Path to data directory");

        return 0;
    }

    private static int CheckEnvironment()
    {
        Console.WriteLine("NetForge.Player Environment Check");
        Console.WriteLine("=================================\n");

        var allChecks = true;

        // Check .NET version
        Console.Write(".NET Runtime Version: ");
        Console.WriteLine($"{Environment.Version} ✓");

        // Check OS compatibility
        Console.Write("Operating System: ");
        Console.WriteLine($"{Environment.OSVersion} ✓");

        // Check available memory
        Console.Write("Available Memory: ");
        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            Console.WriteLine($"{totalMemory / 1024 / 1024} MB ✓");
        }
        catch
        {
            Console.WriteLine("Unable to determine ✗");
            allChecks = false;
        }

        // Check assembly loading
        Console.Write("NetForge Assemblies: ");
        try
        {
            // Try to load key NetForge assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.StartsWith("NetForge") == true)
                .Count();

            Console.WriteLine($"{assemblies} loaded ✓");
        }
        catch
        {
            Console.WriteLine("Load failed ✗");
            allChecks = false;
        }

        Console.WriteLine();
        Console.WriteLine(allChecks ? "Environment check passed ✓" : "Environment check failed ✗");

        return allChecks ? 0 : 1;
    }

    private static int RunWithArgs(string[] args)
    {
        Console.WriteLine("Command-line execution not yet implemented.");
        Console.WriteLine("Use NetForge.Player without arguments to start interactive mode.");
        return 1;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("\nShutdown requested...");

        // Cancel the event to prevent immediate termination
        e.Cancel = true;

        // Stop the application gracefully
        _application?.Stop();

        // Stop the host
        _host?.StopAsync().Wait(TimeSpan.FromSeconds(5));
    }

    private static string GetBuildDate()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileInfo = new FileInfo(assembly.Location);
            return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "Unknown";
        }
    }
}

