using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Each glyph is exactly 6 lines tall. Widths can vary; we just join with a spacer.
    static readonly Dictionary<char, string[]> Glyphs = new()
    {
        // N
        ['N'] = new[]
        {
            "███╗   ██╗",
            "████╗  ██║",
            "██╔██╗ ██║",
            "██║╚██╗██║",
            "██║ ╚████║",
            "╚═╝  ╚═══╝",
        },

        // E
        ['E'] = new[]
        {
            "███████╗",
            "██╔════╝",
            "█████╗  ",
            "██╔══╝  ",
            "███████╗",
            "╚══════╝",
        },

        // T
        ['T'] = new[]
        {
            "████████╗",
            "╚══██╔══╝",
            "   ██║   ",
            "   ██║   ",
            "   ██║   ",
            "   ╚═╝   ",
        },

        // F
        ['F'] = new[]
        {
            "███████╗",
            "██╔════╝",
            "█████╗  ",
            "██╔══╝  ",
            "██║     ",
            "╚═╝     ",
        },

        // O
        ['O'] = new[]
        {
            " ██████╗ ",
            "██╔═══██╗",
            "██║   ██║",
            "██║   ██║",
            "╚██████╔╝",
            " ╚═════╝ ",
        },

        // R
        ['R'] = new[]
        {
            "██████╗ ",
            "██╔══██╗",
            "██████╔╝",
            "██╔══██╗",
            "██║  ██║",
            "╚═╝  ╚═╝",
        },

        // G  (open on the right so it doesn't look like 'O')
        ['G'] = new[]
        {
            " ██████╗ ",
            "██╔════╝ ",
            "██║  ███║",
            "██║   ██║",
            "╚██████╔╝",
            " ╚═════╝ ",
        },

        // (Optional) space between words
        [' '] = new[]
        {
            "  ",
            "  ",
            "  ",
            "  ",
            "  ",
            "  ",
        },
    };

    static void Main(string[] args)
    {
        // Change this to whatever text you want to print in this style:
        var text = (args.Length > 0 ? string.Join(' ', args) : "NETFORGE").ToUpperInvariant();

        Console.OutputEncoding = System.Text.Encoding.UTF8; // ensure box chars render

        // Validate that we have glyphs for all chars
        foreach (char ch in text)
        {
            if (!Glyphs.ContainsKey(ch))
            {
                Console.Error.WriteLine($"No glyph defined for '{ch}'. Add it to the Glyphs dictionary.");
                return;
            }
        }

        // Render line by line (6 lines tall)
        for (int row = 0; row < 6; row++)
        {
            var line = string.Join(" ", text.Select(ch => Glyphs[ch][row]));
            Console.WriteLine(line);
        }

        // Optional: Print a footer
        Console.WriteLine("\nNetwork Device Simulator for Education and Testing");
    }
}
