using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record AppLaunchOptions
{
    public string? Theme { get; init; }

    public string? OpenFile { get; init; }

    public IReadOnlyList<string> ExtraArguments { get; init; } = [];

    /// <summary>
    /// The screen position of the window in physical pixels.
    /// </summary>
    public Point WindowPosition { get; init; } = new(100, 100);

    /// <summary>
    /// The window size in physical pixels. Screenshots have this size.
    /// </summary>
    public Size WindowSize { get; init; } = new(1280, 720);

    public TimeSpan StartTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
