using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// Makes the test host per-monitor DPI aware (v2), so that UI Automation bounds, window rectangles,
/// and PrintWindow bitmaps use the same physical pixels at the display scale of the monitor.
/// </summary>
internal static class DpiAwareness
{
    // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 in WinUser.h.
    private static readonly IntPtr PerMonitorAwareV2 = new(-4);

    public static void EnablePerMonitorV2()
    {
        if (NativeMethods.AreDpiAwarenessContextsEqual(NativeMethods.GetDpiAwarenessContextForProcess(IntPtr.Zero), PerMonitorAwareV2))
        {
            return;
        }

        if (!NativeMethods.SetProcessDpiAwarenessContext(PerMonitorAwareV2))
        {
            throw Win32Errors.CreateFromLastCall("SetProcessDpiAwarenessContext");
        }
    }
}
