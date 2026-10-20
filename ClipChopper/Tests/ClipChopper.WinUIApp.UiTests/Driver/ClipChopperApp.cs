using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record AppLaunchOptions
{
    public string? Theme { get; init; }

    public string? OpenFile { get; init; }

    public IReadOnlyList<string> ExtraArguments { get; init; } = [];

    public Point WindowPosition { get; init; } = new(100, 100);

    public Size WindowSize { get; init; } = new(1280, 720);

    public TimeSpan StartTimeout { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// The only code that starts the app, attaches UI Automation, and sets the window position and size.
/// A hidden (windowless) mode can be added here later.
/// </summary>
public sealed class ClipChopperApp : IDisposable
{
    private readonly Process _process;
    private const uint PrintWindowRenderFullContent = 2;

    private readonly IntPtr _job;
    private readonly IntPtr _windowHandle;

    private ClipChopperApp(Process process, UIA3Automation automation, IntPtr job, IntPtr windowHandle, Window window)
    {
        _process = process;
        Automation = automation;
        _job = job;
        _windowHandle = windowHandle;
        Window = window;
        Page = new MainWindowPage(window, automation);
    }

    public Window Window { get; }

    public UIA3Automation Automation { get; }

    public int ProcessId => _process.Id;

    public MainWindowPage Page { get; }

    public static ClipChopperApp Launch(AppLaunchOptions options)
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

        var application = Application.Launch(startInfo);
        var process = Process.GetProcessById(application.ProcessId);
        var job = IntPtr.Zero;
        UIA3Automation? automation = null;
        try
        {
            job = CreateKillOnCloseJob(process);
            automation = new UIA3Automation();
            var windowHandle = WaitForMainWindowHandle(process, options.StartTimeout);
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
        var bitmap = PrintMainWindow();
        var path = Path.Combine(TestPaths.ArtifactsDir, name + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        return bitmap;
    }

    public void Dispose()
    {
        Cleanup(_process, Automation, _job);
    }

    private static IntPtr WaitForMainWindowHandle(Process process, TimeSpan timeout)
    {
        var handle = IntPtr.Zero;
        var found = Retry.WhileFalse(
            () =>
            {
                process.Refresh();
                handle = process.MainWindowHandle;
                return handle != IntPtr.Zero && IsWindowVisible(handle);
            },
            timeout,
            TimeSpan.FromMilliseconds(100),
            throwOnTimeout: false
        );
        return found.Result ? handle : throw new TimeoutException("The app main window did not appear.");
    }

    private Bitmap PrintMainWindow()
    {
        var bounds = Window.BoundingRectangle;
        var bitmap = new Bitmap(bounds.Width, bounds.Height);
        try
        {
            using var graphics = Graphics.FromImage(bitmap);
            var hdc = graphics.GetHdc();
            try
            {
                if (!PrintWindow(_windowHandle, hdc, PrintWindowRenderFullContent))
                {
                    throw new InvalidOperationException("PrintWindow failed for the app window.");
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static void Cleanup(Process process, UIA3Automation? automation, IntPtr job)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(TimeSpan.FromSeconds(10));
        }

        automation?.Dispose();
        if (job != IntPtr.Zero)
        {
            CloseHandle(job);
        }

        process.Dispose();
    }

    private static IntPtr CreateKillOnCloseJob(Process process)
    {
        var job = CreateJobObject(IntPtr.Zero, null);
        if (job == IntPtr.Zero)
        {
            throw new InvalidOperationException("CreateJobObject failed.");
        }

        var info = new JobObjectExtendedLimitInformation();
        info.BasicLimitInformation.LimitFlags = JobObjectLimitKillOnJobClose;
        var length = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        var pointer = Marshal.AllocHGlobal(length);
        try
        {
            Marshal.StructureToPtr(info, pointer, false);
            if (!SetInformationJobObject(job, JobObjectExtendedLimitInformationClass, pointer, (uint) length)
                || !AssignProcessToJobObject(job, process.Handle))
            {
                throw new InvalidOperationException("Cannot put the app process into a job object.");
            }
        }
        catch
        {
            CloseHandle(job);
            throw;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }

        return job;
    }

    private const int JobObjectExtendedLimitInformationClass = 9;
    private const uint JobObjectLimitKillOnJobClose = 0x2000;

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;
        public IoCounters IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr attributes, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(IntPtr job, int infoClass, IntPtr info, uint length);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);
}
