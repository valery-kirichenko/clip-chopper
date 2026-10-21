using System.ComponentModel;
using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// Makes the test host per-monitor DPI aware (v2), so that UI Automation bounds, window rectangles,
/// and PrintWindow bitmaps use the same physical pixels at any display scale.
/// </summary>
internal static class DpiAwareness
{
    // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 in WinUser.h.
    private static readonly IntPtr PerMonitorAwareV2 = new(-4);

    public static void EnablePerMonitorV2()
    {
        if (NativeMethods.AreDpiAwarenessContextsEqual(NativeMethods.GetThreadDpiAwarenessContext(), PerMonitorAwareV2))
        {
            return;
        }

        if (!NativeMethods.SetProcessDpiAwarenessContext(PerMonitorAwareV2))
        {
            var error = Marshal.GetLastPInvokeError();
            throw new Win32Exception(error, $"SetProcessDpiAwarenessContext failed: {Marshal.GetPInvokeErrorMessage(error)} (0x{error:X8})");
        }
    }
}
