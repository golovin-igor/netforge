// TODO: Enhance ConsoleBanner with additional features
// - Add version information display below banner
// - Support for different banner themes and colors
// - Add configurable banner text and styling
// - Include startup time and system information
// - Add seasonal or themed banner variations
// - Support for custom ASCII art loading from files
// - Add banner animation effects
// - Include build information and update notifications

using System.Text;

namespace NetForge.Player;

public static class ConsoleBanner
{
    // Each glyph is exactly 6 lines tall. Widths can vary; we just join with a spacer.
    private static readonly Dictionary<char, string[]> Glyphs = new()
    {
        // N
        ['N'] = ["███╗   ██╗", "████╗  ██║", "██╔██╗ ██║", "██║╚██╗██║", "██║ ╚████║", "╚═╝  ╚═══╝"],

        // E
        ['E'] = ["███████╗", "██╔════╝", "█████╗  ", "██╔══╝  ", "███████╗", "╚══════╝"],

        // T
        ['T'] = ["████████╗", "╚══██╔══╝", "   ██║   ", "   ██║   ", "   ██║   ", "   ╚═╝   "],

        // F
        ['F'] = ["███████╗", "██╔════╝", "█████╗  ", "██╔══╝  ", "██║     ", "╚═╝     "],

        // O
        ['O'] = [" ██████╗ ", "██╔═══██╗", "██║   ██║", "██║   ██║", "╚██████╔╝", " ╚═════╝ "],

        // R
        ['R'] = ["██████╗ ", "██╔══██╗", "██████╔╝", "██╔══██╗", "██║  ██║", "╚═╝  ╚═╝"],

        // G  (open on the right so it doesn't look like 'O')
        ['G'] = [" ██████╗ ", "██╔════╝ ", "██║  ███║", "██║   ██║", "╚██████╔╝", " ╚═════╝ "],

        // (Optional) space between words
        [' '] = ["  ", "  ", "  ", "  ", "  ", "  "],
    };

    internal static void Print()
    {
        // TODO: Make banner configurable and themeable
        // - Load banner text from configuration
        // - Support different color schemes
        // - Add gradient color effects
        // - Include version and build information
        // - Add startup timestamp and system info
        
        var text = "NETFORGE";

        Console.OutputEncoding = Encoding.UTF8; // ensure box chars render
        
        // TODO: Add color theme support
        // - Support multiple color schemes (dark, light, rainbow)
        // - Add gradient effects for banner text
        // - Implement seasonal color themes
        Console.ForegroundColor = ConsoleColor.Yellow;

        // Render line by line (6 lines tall)
        for (int row = 0; row < 6; row++)
        {
            var line = string.Join(" ", text.Select(ch => Glyphs[ch][row]));
            Console.WriteLine(line);
        }
        Console.ResetColor();

        // TODO: Enhance footer with more information
        // - Version information
        // - Build timestamp
        // - System requirements status
        // - Quick help hint
        // - License information
        Console.WriteLine("\nNetwork Device Simulator for Education and Testing");
        
        // TODO: Add version and build information display
        // Console.WriteLine($"Version: {GetVersion()} | Build: {GetBuildDate()}");
        // Console.WriteLine($"System: {Environment.OSVersion} | Runtime: {Environment.Version}");
        // Console.WriteLine("Type 'help' for available commands or 'quit' to exit");
    }

}
