using System.Diagnostics;
using System.Drawing;
using ClipChopper.WinUIApp.UiTests.Platform;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// The only code that starts the app, attaches UI Automation, and sets the window position and size.
/// A hidden (windowless) mode can be added here later.
/// </summary>
public sealed class ClipChopperApp : IDisposable
{
    private readonly Process _process;
    private readonly UIA3Automation _automation;
    private readonly KillOnCloseJob _job;
    private readonly IntPtr _windowHandle;

    private ClipChopperApp(Process process, UIA3Automation automation, KillOnCloseJob job, IntPtr windowHandle, Window window)
    {
        _process = process;
        _automation = automation;
        _job = job;
        _windowHandle = windowHandle;
        Window = window;
        Page = new MainWindowPage(window, automation);
    }

    public Window Window { get; }

    public MainWindowPage Page { get; }

    public static ClipChopperApp Launch(AppLaunchOptions options)
    {
        var process = Process.Start(CreateStartInfo(options))!;
        KillOnCloseJob? job = null;
        UIA3Automation? automation = null;
        try
        {
            job = KillOnCloseJob.Assign(process);
            automation = new UIA3Automation();
            var windowHandle = MainWindowFinder.WaitForHandle(process, options.StartTimeout, UiTiming.PollInterval);
            var window = automation.FromHandle(windowHandle).AsWindow();
            var transform = window.Patterns.Transform.Pattern;
            transform.Move(options.WindowPosition.X, options.WindowPosition.Y);
            transform.Resize(options.WindowSize.Width, options.WindowSize.Height);
            return new ClipChopperApp(process, automation, job, windowHandle, window);
        }
        catch
        {
            Cleanup(process, automation, job);
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
        Cleanup(_process, _automation, _job);
    }

    private static ProcessStartInfo CreateStartInfo(AppLaunchOptions options)
    {
        var startInfo = new ProcessStartInfo(TestPaths.AppExePath)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(TestPaths.AppExePath)!,
        };
        if (options.Theme != null)
        {
            startInfo.ArgumentList.Add("--theme");
            startInfo.ArgumentList.Add(options.Theme);
        }

        if (options.OpenFile != null)
        {
            startInfo.ArgumentList.Add("--open");
            startInfo.ArgumentList.Add(options.OpenFile);
        }

        foreach (var argument in options.ExtraArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static void Cleanup(Process process, UIA3Automation? automation, KillOnCloseJob? job)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(TimeSpan.FromSeconds(10));
        }

        automation?.Dispose();
        job?.Dispose();
        process.Dispose();
    }
}
