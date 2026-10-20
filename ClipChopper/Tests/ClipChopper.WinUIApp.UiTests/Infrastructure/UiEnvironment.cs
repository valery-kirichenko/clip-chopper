using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public static class UiEnvironment
{
    private const uint DesktopSwitchDesktop = 0x0100;

    public static string? SkipReason { get; } = FindSkipReason();

    private static string? FindSkipReason()
    {
        using var identity = WindowsIdentity.GetCurrent();
        if (new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
        {
            return "The test host runs as administrator. UI tests must not start the app as administrator.";
        }

        if (!Environment.UserInteractive)
        {
            return "The session is not interactive.";
        }

        if (Process.GetCurrentProcess().SessionId == 0)
        {
            return "The test host runs in session 0, which has no desktop.";
        }

        var desktop = OpenInputDesktop(0, false, DesktopSwitchDesktop);
        if (desktop == IntPtr.Zero)
        {
            return "The input desktop is not available (the screen may be locked).";
        }

        CloseDesktop(desktop);
        return null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, bool inherit, uint desiredAccess);

    [DllImport("user32.dll")]
    private static extern bool CloseDesktop(IntPtr desktop);
}
