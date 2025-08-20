using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetForge.Player;

public class ProgressIndicator : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _animationTask;
    private readonly string _message;
    private bool _disposed;

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