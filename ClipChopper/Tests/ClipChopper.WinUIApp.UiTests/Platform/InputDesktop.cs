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
}
