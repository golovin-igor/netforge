// TODO: Enhance ProgressIndicator for better user experience
// - Add progress percentage display for operations that support it
// - Implement different animation styles (spinner, progress bar, dots)
// - Add color coding for different operation types (info, warning, error)
// - Support for multiple concurrent progress indicators
// - Add elapsed time display for long-running operations
// - Implement cancellation support with user feedback
// - Add operation completion statistics
// - Support for nested progress indicators

namespace NetForge.Player;

public class ProgressIndicator : IDisposable
{
    // TODO: Add configuration options for progress display
    // - Configurable animation speed and style
    // - Color themes for different operation types
    // - Progress bar width and character set
    // - Message formatting options
    
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _animationTask;
    private readonly string _message;
    private bool _disposed;

    // TODO: Extend spinner character sets for different themes
    // - Add progress bar characters: ▓░, ━┅, █▉▊▋▌▍▎▏
    // - Add themed spinners: ⠁⠂⠄⡀⢀⠠⠐⠈, ◢◣◤◥, ←↖↑↗→↘↓↙
    private static readonly char[] SpinnerChars = { '|', '/', '-', '\\' };

    public ProgressIndicator(string message = "Loading")
    {
        _message = message;
        _cancellationTokenSource = new CancellationTokenSource();
        _animationTask = Task.Run(AnimateSpinner, _cancellationTokenSource.Token);
    }

    private async Task AnimateSpinner()
    {
        var spinnerIndex = 0;
        var originalCursorLeft = Console.CursorLeft;
        var originalCursorTop = Console.CursorTop;

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
                Console.Write($"{_message}... {SpinnerChars[spinnerIndex]}");

                spinnerIndex = (spinnerIndex + 1) % SpinnerChars.Length;
                await Task.Delay(100, _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
            Console.Write(new string(' ', _message.Length + 5));
            Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
        }
    }

    public static async Task<T> WithProgress<T>(string message, Func<Task<T>> operation)
    {
        using var progress = new ProgressIndicator(message);
        return await operation();
    }

    public static T WithProgress<T>(string message, Func<T> operation)
    {
        using var progress = new ProgressIndicator(message);
        return operation();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cancellationTokenSource.Cancel();
        try
        {
            _animationTask.Wait(1000);
        }
        catch (AggregateException)
        {
        }

        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}