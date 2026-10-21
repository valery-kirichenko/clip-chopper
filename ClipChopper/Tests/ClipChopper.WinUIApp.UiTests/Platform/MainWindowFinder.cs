using System.Diagnostics;
using ClipChopper.WinUIApp.UiTests.Native;
using FlaUI.Core.Tools;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class MainWindowFinder
{
    public static IntPtr WaitForHandle(Process process, TimeSpan timeout)
    {
        var handle = IntPtr.Zero;
        var found = Retry.WhileFalse(
            () =>
            {
                process.Refresh();
                handle = process.MainWindowHandle;
                return handle != IntPtr.Zero && NativeMethods.IsWindowVisible(handle);
            },
            timeout,
            TimeSpan.FromMilliseconds(100),
            throwOnTimeout: false
        );
        return found.Result ? handle : throw new TimeoutException("The app main window did not appear.");
    }
}
