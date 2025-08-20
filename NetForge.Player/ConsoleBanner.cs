using System.Text;
using System.Linq;

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
        var text = "NETFORGE";

        Console.OutputEncoding = Encoding.UTF8; // ensure box chars render
        Console.ForegroundColor = ConsoleColor.Yellow;

        // Render line by line (6 lines tall)
        for (int row = 0; row < 6; row++)
        {
            var line = string.Join(" ", text.Select(ch => Glyphs[ch][row]));
            Console.WriteLine(line);
        }
        Console.ResetColor();

        // Optional: Print a footer
        Console.WriteLine("\nNetwork Device Simulator for Education and Testing");
    }

}
