namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// Tells where the tests run the app. By default, the app runs on a hidden desktop.
/// If CLIPCHOPPER_UITEST_VISIBLE is 1, the app opens on the desktop of the test host.
/// </summary>
public static class UiTestMode
{
    public const string VisibleVariable = "CLIPCHOPPER_UITEST_VISIBLE";

    public static bool IsVisible { get; } = Environment.GetEnvironmentVariable(VisibleVariable) == "1";
}
