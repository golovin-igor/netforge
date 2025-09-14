using System.Text;
using NetForge.Player.Configuration;

namespace NetForge.Player.Core;

/// <summary>
/// Interactive shell for command input with completion and history support
/// </summary>
public class InteractiveShell
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly PlayerConfiguration _configuration;
    private readonly List<string> _inputBuffer = new();
    private int _historyIndex = -1;
    private string _currentInput = "";

    public InteractiveShell(ICommandProcessor commandProcessor, PlayerConfiguration configuration)
    {
        _commandProcessor = commandProcessor;
        _configuration = configuration;
    }

    /// <summary>
    /// Read a line of input with tab completion and history support
    /// </summary>
    public async Task<string> ReadLineAsync()
    {
        Console.Write(GetPrompt());
        
        var input = new StringBuilder();
        var cursorPosition = 0;
        
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    var result = input.ToString();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        _inputBuffer.Add(result);
                        _historyIndex = _inputBuffer.Count;
                    }
                    return result;
                
                case ConsoleKey.Tab:
                    await HandleTabCompletionAsync(input, cursorPosition);
                    cursorPosition = input.Length;
                    break;
                
                case ConsoleKey.UpArrow:
                    HandleHistoryUp(input, ref cursorPosition);
                    break;
                
                case ConsoleKey.DownArrow:
                    HandleHistoryDown(input, ref cursorPosition);
                    break;
                
                case ConsoleKey.LeftArrow:
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    break;
                
                case ConsoleKey.RightArrow:
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                    break;
                
                case ConsoleKey.Home:
                    Console.SetCursorPosition(Console.CursorLeft - cursorPosition, Console.CursorTop);
                    cursorPosition = 0;
                    break;
                
                case ConsoleKey.End:
                    Console.SetCursorPosition(Console.CursorLeft + (input.Length - cursorPosition), Console.CursorTop);
                    cursorPosition = input.Length;
                    break;
                
                case ConsoleKey.Backspace:
                    if (cursorPosition > 0)
                    {
                        input.Remove(cursorPosition - 1, 1);
                        cursorPosition--;
                        RedrawInput(input.ToString(), cursorPosition);
                    }
                    break;
                
                case ConsoleKey.Delete:
                    if (cursorPosition < input.Length)
                    {
                        input.Remove(cursorPosition, 1);
                        RedrawInput(input.ToString(), cursorPosition);
                    }
                    break;
                
                case ConsoleKey.C when keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control):
                    Console.WriteLine("^C");
                    return "";
                
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        input.Insert(cursorPosition, keyInfo.KeyChar);
                        cursorPosition++;
                        
                        // Simple case: just append to end
                        if (cursorPosition == input.Length)
                        {
                            Console.Write(keyInfo.KeyChar);
                        }
                        else
                        {
                            RedrawInput(input.ToString(), cursorPosition);
                        }
                    }
                    break;
            }
        }
    }

    private string GetPrompt()
    {
        var promptColor = _configuration.Shell?.PromptColor ?? "Green";
        var prompt = _configuration.Shell?.Prompt ?? "NetForge> ";
        
        if (Enum.TryParse<ConsoleColor>(promptColor, out var color))
        {
            Console.ForegroundColor = color;
        }
        
        var result = prompt;
        Console.ResetColor();
        return result;
    }

    private async Task HandleTabCompletionAsync(StringBuilder input, int cursorPosition)
    {
        var completions = _commandProcessor.GetCompletions(input.ToString());
        
        if (completions.Count == 0)
            return;
        
        if (completions.Count == 1)
        {
            // Single completion - replace current token
            var tokens = input.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                input.Append(completions[0]);
                input.Append(' ');
            }
            else
            {
                // Replace last token
                var lastTokenStart = input.ToString().LastIndexOf(tokens[^1]);
                if (lastTokenStart >= 0)
                {
                    input.Remove(lastTokenStart, tokens[^1].Length);
                    input.Insert(lastTokenStart, completions[0]);
                    input.Append(' ');
                }
            }
            
            RedrawInput(input.ToString(), input.Length);
        }
        else
        {
            // Multiple completions - show list
            Console.WriteLine();
            
            // Show completions in columns
            const int maxColumns = 4;
            var maxWidth = completions.Max(c => c.Length) + 2;
            var columns = Math.Min(maxColumns, Console.WindowWidth / maxWidth);
            
            for (int i = 0; i < completions.Count; i++)
            {
                if (i % columns == 0 && i > 0)
                    Console.WriteLine();
                
                Console.Write(completions[i].PadRight(maxWidth));
            }
            
            Console.WriteLine();
            Console.Write(GetPrompt());
            Console.Write(input.ToString());
        }
    }

    private void HandleHistoryUp(StringBuilder input, ref int cursorPosition)
    {
        if (_historyIndex > 0)
        {
            if (_historyIndex == _inputBuffer.Count)
            {
                _currentInput = input.ToString();
            }
            
            _historyIndex--;
            input.Clear();
            input.Append(_inputBuffer[_historyIndex]);
            cursorPosition = input.Length;
            
            RedrawInput(input.ToString(), cursorPosition);
        }
    }

    private void HandleHistoryDown(StringBuilder input, ref int cursorPosition)
    {
        if (_historyIndex < _inputBuffer.Count)
        {
            _historyIndex++;
            
            input.Clear();
            if (_historyIndex == _inputBuffer.Count)
            {
                input.Append(_currentInput);
            }
            else
            {
                input.Append(_inputBuffer[_historyIndex]);
            }
            
            cursorPosition = input.Length;
            RedrawInput(input.ToString(), cursorPosition);
        }
    }

    private void RedrawInput(string input, int cursorPosition)
    {
        // Clear current line from cursor position
        var currentPos = Console.CursorLeft;
        var spaces = new string(' ', Math.Max(0, Console.WindowWidth - currentPos));
        Console.Write($"\r{GetPrompt()}{spaces}\r{GetPrompt()}");
        
        // Write new input
        Console.Write(input);
        
        // Position cursor
        var promptLength = GetPrompt().Length;
        Console.SetCursorPosition(promptLength + cursorPosition, Console.CursorTop);
    }
}