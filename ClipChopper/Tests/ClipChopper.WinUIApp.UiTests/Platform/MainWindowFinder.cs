using System.Diagnostics;
using ClipChopper.WinUIApp.UiTests.Native;
using FlaUI.Core.Tools;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class MainWindowFinder
{
    public static IntPtr WaitForHandle(Process process, TimeSpan timeout, TimeSpan pollInterval)
    {
        var found = Retry.WhileNull(() => FindVisibleHandle(process), timeout, pollInterval);
        return found.Result ?? throw new TimeoutException("The app main window did not appear.");
    }

    private static IntPtr? FindVisibleHandle(Process process)
    {
        process.Refresh();
        var handle = process.MainWindowHandle;
        return handle != IntPtr.Zero && NativeMethods.IsWindowVisible(handle) ? handle : null;
    }
}
