using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class InputDesktop
{
    public static bool IsAvailable()
    {
        var desktop = NativeMethods.OpenInputDesktop(0, false, DesktopAccessRights.SwitchDesktop);
        if (desktop == IntPtr.Zero)
        {
            return false;
        }

        NativeMethods.CloseDesktop(desktop);
        return true;
    }

    /// <summary>
    /// Reads the name of the input desktop, the desktop of the user.
    /// </summary>
    public static unsafe string ReadName()
    {
        const int MaxNameLength = 256;
        var desktop = NativeMethods.OpenInputDesktop(0, false, DesktopAccessRights.ReadObjects);
        if (desktop == IntPtr.Zero)
        {
            throw Win32Errors.CreateFromLastCall("OpenInputDesktop");
        }

        try
        {
            var buffer = stackalloc char[MaxNameLength];
            if (!NativeMethods.GetUserObjectInformation(desktop, UserObjectInformationIndex.Name, buffer, MaxNameLength * sizeof(char), out var lengthNeeded))
            {
                throw Win32Errors.CreateFromLastCall("GetUserObjectInformation");
            }

            return new string(buffer, 0, (int) lengthNeeded / sizeof(char) - 1);
        }
        finally
        {
            NativeMethods.CloseDesktop(desktop);
        }
    }

    /// <summary>
    /// Finds the visible top-level windows of one process on the input desktop, the desktop of the user.
    /// </summary>
    public static IReadOnlyList<IntPtr> FindVisibleWindows(int processId)
    {
        var desktop = NativeMethods.OpenInputDesktop(0, false, DesktopAccessRights.Enumerate | DesktopAccessRights.ReadObjects);
        if (desktop == IntPtr.Zero)
        {
            throw Win32Errors.CreateFromLastCall("OpenInputDesktop");
        }

        try
        {
            return DesktopWindows.FindVisible(desktop, processId);
        }
        finally
        {
            NativeMethods.CloseDesktop(desktop);
        }
    }
}
