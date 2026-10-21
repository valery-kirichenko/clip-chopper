using System.Diagnostics;
using System.Security.Principal;
using ClipChopper.WinUIApp.UiTests.Platform;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public static class UiEnvironment
{
    private static readonly bool IsAdministrator = FindIsAdministrator();

    private static readonly string? DesktopSkipReason = FindDesktopSkipReason();

    /// <summary>
    /// The skip reason for the normal UI tests, which must start the app without administrator rights.
    /// </summary>
    public static string? SkipReason { get; } = IsAdministrator
        ? "The test host runs as administrator. Only the administrator UI tests run in this session."
        : DesktopSkipReason;

    /// <summary>
    /// The skip reason for the UI tests that must start the app as administrator.
    /// </summary>
    public static string? AdminSkipReason { get; } = IsAdministrator
        ? DesktopSkipReason
        : "The test host does not run as administrator. This test checks the file pickers as administrator (#60). Run dotnet test from a PowerShell window that runs as administrator.";

    private static bool FindIsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static string? FindDesktopSkipReason()
    {
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
