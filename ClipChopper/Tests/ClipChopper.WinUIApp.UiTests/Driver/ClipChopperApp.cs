using System.Diagnostics;
using System.Drawing;
using ClipChopper.WinUIApp.CommandLine;
using ClipChopper.WinUIApp.UiTests.Platform;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// The only code that starts the app, attaches UI Automation, and sets the window position and size.
/// It makes the test host per-monitor DPI aware first, so all positions and sizes are physical pixels.
/// By default, it starts the app on a hidden desktop. See UiTestMode for the visible mode.
/// </summary>
public sealed class ClipChopperApp : IDisposable
{
    private readonly Process _process;
    private readonly UIA3Automation _automation;
    private readonly KillOnCloseJob _job;
    private readonly HiddenDesktop? _desktop;
    private readonly IntPtr _windowHandle;

    private ClipChopperApp(Process process, UIA3Automation automation, KillOnCloseJob job, HiddenDesktop? desktop, IntPtr windowHandle, Window window)
    {
        _process = process;
        _automation = automation;
        _job = job;
        _desktop = desktop;
        _windowHandle = windowHandle;
        Window = window;
        Page = new MainWindowPage(window, automation);
    }

    public Window Window { get; }

    public MainWindowPage Page { get; }

    public uint WindowDpi => WindowDpiReader.Read(_windowHandle);

    public bool HasExited => _process.HasExited;

    public int ProcessId => _process.Id;

    public string? DesktopName => _desktop?.Name;

    public static ClipChopperApp Launch(AppLaunchOptions options)
    {
        DpiAwareness.EnablePerMonitorV2();
        HiddenDesktop? desktop = null;
        Process? process = null;
        KillOnCloseJob? job = null;
        UIA3Automation? automation = null;
        try
        {
            desktop = UiTestMode.IsVisible ? null : HiddenDesktop.Create();
            var started = AppProcessStarter.Start(TestPaths.AppExePath, BuildArguments(options), desktop?.StartupName);
            process = started.Process;
            job = started.Job;
            automation = new UIA3Automation();
            var windowHandle = MainWindowFinder.WaitForHandle(process, desktop?.Handle ?? IntPtr.Zero, options.StartTimeout, UiTiming.PollInterval);
            var window = automation.FromHandle(windowHandle).AsWindow();
            var transform = window.Patterns.Transform.Pattern;
            transform.Move(options.WindowPosition.X, options.WindowPosition.Y);
            transform.Resize(options.WindowSize.Width, options.WindowSize.Height);
            WaitForSize(window, options.WindowSize);
            return new ClipChopperApp(process, automation, job, desktop, windowHandle, window);
        }
        catch
        {
            Cleanup(process, automation, job, desktop);
            throw;
        }
    }

    public Bitmap Capture(string name)
    {
        var bitmap = WindowCapture.Print(_windowHandle, Window.BoundingRectangle.Size);
        var path = Path.Combine(TestPaths.ArtifactsDir, name + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        return bitmap;
    }

    public void Dispose()
    {
        Cleanup(_process, _automation, _job, _desktop);
    }

    private static IReadOnlyList<string> BuildArguments(AppLaunchOptions options)
    {
        var launchOptions = new LaunchOptions
        {
            Theme = options.Theme,
            OpenFile = options.OpenFile,
            ExposeRange = options.ExposeRange,
            UiTest = true,
        };
        return LaunchArguments.From(launchOptions);
    }

    private static void WaitForSize(Window window, Size size)
    {
        var actual = Size.Empty;
        var resized = Retry.WhileFalse(
            () =>
            {
                actual = window.BoundingRectangle.Size;
                return actual == size;
            },
            UiTiming.Timeout,
            UiTiming.PollInterval,
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!resized.Result)
        {
            throw new TimeoutException($"The app window is {actual.Width} x {actual.Height}, expected {size.Width} x {size.Height} physical pixels.", resized.LastException);
        }
    }

    private static void Cleanup(Process? process, UIA3Automation? automation, KillOnCloseJob? job, HiddenDesktop? desktop)
    {
        if (process is { HasExited: false })
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(TimeSpan.FromSeconds(10));
        }

        automation?.Dispose();
        job?.Dispose();
        process?.Dispose();
        desktop?.Dispose();
    }
}
