using System.Diagnostics;
using ClipChopper.WinUIApp.UiTests.Native;
using FlaUI.Core.Tools;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class MainWindowFinder
{
    private const string MainWindowClassName = "WinUIDesktopWin32WindowClass";

    /// <summary>
    /// Waits for the main window of the process. A zero desktop means the desktop of the test host.
    /// If the process exits first, it throws at once and names the exit code.
    /// </summary>
    public static IntPtr WaitForHandle(Process process, IntPtr desktop, TimeSpan timeout, TimeSpan pollInterval)
    {
        var found = Retry.WhileNull(() => FindHandle(process, desktop), timeout, pollInterval);
        return found.Result ?? throw new TimeoutException("The app main window did not appear.");
    }

    private static IntPtr? FindHandle(Process process, IntPtr desktop)
    {
        if (process.HasExited)
        {
            throw new InvalidOperationException($"The app exited with code 0x{process.ExitCode:X8} before its main window appeared. See the Application event log for the error of the app.");
        }

        foreach (var window in DesktopWindows.FindVisible(desktop, process.Id))
        {
            if (NativeMethods.GetWindow(window, GetWindowCommand.Owner) == IntPtr.Zero && DesktopWindows.ReadClassName(window) == MainWindowClassName)
            {
                return window;
            }
        }

        return null;
    }
}
