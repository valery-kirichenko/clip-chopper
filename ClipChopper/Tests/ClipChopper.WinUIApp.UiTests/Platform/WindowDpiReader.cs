using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class WindowDpiReader
{
    public static uint Read(IntPtr windowHandle)
    {
        var dpi = NativeMethods.GetDpiForWindow(windowHandle);
        return dpi != 0 ? dpi : throw new InvalidOperationException("GetDpiForWindow failed for the app window.");
    }
}
