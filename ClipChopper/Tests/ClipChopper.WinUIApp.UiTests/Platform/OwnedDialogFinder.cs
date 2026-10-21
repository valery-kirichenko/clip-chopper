using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class OwnedDialogFinder
{
    private const string DialogClassName = "#32770";

    public static IntPtr? FindVisible(IntPtr owner)
    {
        var candidate = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, DialogClassName, null);
        while (candidate != IntPtr.Zero)
        {
            if (NativeMethods.GetWindow(candidate, GetWindowCommand.Owner) == owner && NativeMethods.IsWindowVisible(candidate))
            {
                return candidate;
            }

            candidate = NativeMethods.FindWindowEx(IntPtr.Zero, candidate, DialogClassName, null);
        }

        return null;
    }

    public static bool IsOpen(IntPtr dialog)
    {
        return NativeMethods.IsWindow(dialog) && NativeMethods.IsWindowVisible(dialog);
    }
}
