using System.Drawing;
using System.Globalization;
using ClipChopper.WinUIApp.UiTests.Driver;
using ClipChopper.WinUIApp.UiTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace ClipChopper.WinUIApp.UiTests.Tests;

[Collection("UI")]
public sealed class ThemeRangeTests
{
    private const int StartMs = 2000;
    private const int EndMs = 7000;

    private readonly TestClipFixture _clip;
    private readonly ITestOutputHelper _output;

    public ThemeRangeTests(TestClipFixture clip, ITestOutputHelper output)
    {
        _clip = clip;
        _output = output;
    }

    [UiTheory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void CapturesMarkedRangeInTheme(string theme)
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { Theme = theme, OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.MarkRange(StartMs, EndMs);
        var state = app.Page.ReadStateWhen(s => s.FragmentStopMs < 8000, TimeSpan.FromSeconds(10));
        var sliderBounds = app.Page.SeekSlider.BoundingRectangle;
        var windowBounds = app.Window.BoundingRectangle;
        using var capture = app.Capture($"range-{theme}");

        var titleLuminance = MeanLuminance(capture, (capture.Width / 2) - 10, 7, 20, 6);
        if (theme == "Light")
        {
            Assert.True(titleLuminance > 200, $"ThemeApplied: title bar luminance is {titleLuminance:F0}, expected over 200.");
        }
        else
        {
            Assert.True(titleLuminance < 60, $"ThemeApplied: title bar luminance is {titleLuminance:F0}, expected under 60.");
        }

        Assert.NotNull(state.FragmentStartMs);
        Assert.InRange(state.FragmentStartMs.Value, StartMs - MainWindowPage.FrameToleranceMs, StartMs + MainWindowPage.FrameToleranceMs);
        Assert.NotNull(state.FragmentStopMs);
        Assert.InRange(state.FragmentStopMs.Value, EndMs - MainWindowPage.FrameToleranceMs, EndMs + MainWindowPage.FrameToleranceMs);

        var videoDeviation = LuminanceDeviation(capture, (capture.Width / 2) - 50, (capture.Height / 2) - 50, 100, 100);
        Assert.True(videoDeviation > 10, $"VideoRendered: luminance deviation of the video area is {videoDeviation:F1}, expected over 10.");

        var measurement = RangeProbe.Measure(
            capture,
            windowBounds,
            sliderBounds,
            (double) StartMs / state.SeekMaximum,
            (double) EndMs / state.SeekMaximum
        );
        var text = string.Create(
            CultureInfo.InvariantCulture,
            $"theme={theme} track={measurement.TrackColor} range={measurement.RangeColor} runStart={measurement.RunStartRatio:F3} runEnd={measurement.RunEndRatio:F3} contrast={measurement.ContrastRatio:F2}"
        );
        File.WriteAllText(Path.Combine(TestPaths.ArtifactsDir, $"range-{theme}.txt"), text);
        _output.WriteLine(text);
    }

    private static double MeanLuminance(Bitmap bitmap, int x, int y, int width, int height)
    {
        return Luminances(bitmap, x, y, width, height).Average();
    }

    private static double LuminanceDeviation(Bitmap bitmap, int x, int y, int width, int height)
    {
        var values = Luminances(bitmap, x, y, width, height).ToList();
        var mean = values.Average();
        return Math.Sqrt(values.Average(value => (value - mean) * (value - mean)));
    }

    private static IEnumerable<double> Luminances(Bitmap bitmap, int x, int y, int width, int height)
    {
        for (var row = y; row < y + height; row++)
        {
            for (var column = x; column < x + width; column++)
            {
                var color = bitmap.GetPixel(column, row);
                yield return (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);
            }
        }
    }
}
