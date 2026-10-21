using System.Diagnostics;
using System.Security.Principal;
using ClipChopper.WinUIApp.UiTests.Platform;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public static class UiEnvironment
{
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

        if (!InputDesktop.IsAvailable())
        {
            return "The input desktop is not available (the screen may be locked).";
        }

        return null;
    }
}
