using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// Polls the foreground window every 10 ms on a background thread.
/// It records the process ID of each foreground window that it sees.
/// </summary>
internal sealed class ForegroundWatcher : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(10);

    private readonly HashSet<int> _processIds = new();
    private readonly object _lock = new();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Thread _thread;
    private bool _stopped;

    private ForegroundWatcher()
    {
        _thread = new Thread(Poll)
        {
            IsBackground = true,
            Name = "ForegroundWatcher",
        };
    }

    public static ForegroundWatcher Start()
    {
        var watcher = new ForegroundWatcher();
        watcher._thread.Start();
        return watcher;
    }

    public IReadOnlySet<int> Stop()
    {
        if (!_stopped)
        {
            _stopped = true;
            _cancellation.Cancel();
            _thread.Join();
            _cancellation.Dispose();
        }

        lock (_lock)
        {
            return new HashSet<int>(_processIds);
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private void Poll()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            var window = NativeMethods.GetForegroundWindow();
            if (window != IntPtr.Zero)
            {
                NativeMethods.GetWindowThreadProcessId(window, out var processId);
                lock (_lock)
                {
                    _processIds.Add((int) processId);
                }
            }

            _cancellation.Token.WaitHandle.WaitOne(PollInterval);
        }
    }
}
