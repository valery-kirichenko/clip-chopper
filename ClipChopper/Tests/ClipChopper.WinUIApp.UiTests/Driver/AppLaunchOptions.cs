using System.Drawing;
using ClipChopper.WinUIApp.CommandLine;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record AppLaunchOptions
{
    public LaunchTheme? Theme { get; init; }

    public string? OpenFile { get; init; }

    /// <summary>
    /// When true, the app shows the marked range in the ItemStatus of the seek slider.
    /// </summary>
    public bool ExposeRange { get; init; } = true;

    /// <summary>
    /// The screen position of the window in physical pixels.
    /// </summary>
    public Point WindowPosition { get; init; } = new(100, 100);

    /// <summary>
    /// The window size in physical pixels. Without LogicalWindowSize, screenshots have this size.
    /// </summary>
    public Size WindowSize { get; init; } = new(1280, 720);

    /// <summary>
    /// The window size in logical pixels. When set, the window size is this size times the window DPI divided by 96, and WindowSize is ignored.
    /// </summary>
    public Size? LogicalWindowSize { get; init; }

    public TimeSpan StartTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
