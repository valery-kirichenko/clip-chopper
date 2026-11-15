using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static unsafe class DesktopWindows
{
    /// <summary>
    /// Finds the visible top-level windows of one process on a desktop.
    /// A zero desktop means the desktop of the calling thread.
    /// </summary>
    public static IReadOnlyList<IntPtr> FindVisible(IntPtr desktop, int processId)
    {
        var all = new List<IntPtr>();
        var state = GCHandle.Alloc(all);
        try
        {
            if (!NativeMethods.EnumDesktopWindows(desktop, &CollectWindow, GCHandle.ToIntPtr(state)) && Marshal.GetLastPInvokeError() != 0)
            {
                throw Win32Errors.CreateFromLastCall("EnumDesktopWindows");
            }
        }
        finally
        {
            state.Free();
        }

        return all.Where(window => IsVisibleWindowOf(window, processId)).ToList();
    }

    public static string ReadClassName(IntPtr window)
    {
        const int MaxClassNameLength = 256;
        var buffer = stackalloc char[MaxClassNameLength];
        var length = NativeMethods.GetClassName(window, buffer, MaxClassNameLength);
        return new string(buffer, 0, length);
    }

    private static bool IsVisibleWindowOf(IntPtr window, int processId)
    {
        NativeMethods.GetWindowThreadProcessId(window, out var windowProcessId);
        return windowProcessId == (uint) processId && NativeMethods.IsWindowVisible(window);
    }

    [UnmanagedCallersOnly]
    private static int CollectWindow(IntPtr window, IntPtr state)
    {
        ((List<IntPtr>) GCHandle.FromIntPtr(state).Target!).Add(window);
        return 1;
    }
}
