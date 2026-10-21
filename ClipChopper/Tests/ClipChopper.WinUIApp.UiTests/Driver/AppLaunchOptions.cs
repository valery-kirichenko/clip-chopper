using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record AppLaunchOptions
{
    public string? Theme { get; init; }

    public string? OpenFile { get; init; }

    public IReadOnlyList<string> ExtraArguments { get; init; } = [];

    public Point WindowPosition { get; init; } = new(100, 100);

    public Size WindowSize { get; init; } = new(1280, 720);

    public TimeSpan StartTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
