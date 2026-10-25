using System.Drawing;
using System.Globalization;
using ClipChopper.WinUIApp.CommandLine;
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

    // Without the fix the highest estimated range contrast is 2.1, with the fix the lowest estimate is 2.7.
    // #36 accepted less than 3.0 for the red, purple and violet accents.
    private const double MinDarkRangeContrast = 2.5;

    private const double RangeStartTolerance = 0.01;

    private const int TrackColorTolerance = 6;
    private const int VolumeTrackEndInset = 8;
    private const int VolumeThumbHalfWidth = 10;
    private const int MinDistanceFromVolumeThumb = 14;
    private const int BackgroundProbeOffset = 5;

    private readonly TestClipFixture _clip;
    private readonly ITestOutputHelper _output;

    public ThemeRangeTests(TestClipFixture clip, ITestOutputHelper output)
    {
        _clip = clip;
        _output = output;
    }

    [UiTheory]
    [InlineData(LaunchTheme.Light)]
    [InlineData(LaunchTheme.Dark)]
    public void CapturesMarkedRangeInTheme(LaunchTheme theme)
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { Theme = theme, OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.MarkRange(StartMs, EndMs);
        var state = app.Page.ReadState();
        var sliderBounds = app.Page.SeekSlider.BoundingRectangle;
        var windowBounds = app.Window.BoundingRectangle;
        using var capture = app.Capture($"range-{theme}");

        AssertThemeApplied(capture, windowBounds, app.WindowDpi, theme);

        Assert.NotNull(state.Fragment);
        Assert.InRange(state.Fragment.StartMs, StartMs - MainWindowPage.FrameToleranceMs, StartMs + MainWindowPage.FrameToleranceMs);
        Assert.InRange(state.Fragment.StopMs, EndMs - MainWindowPage.FrameToleranceMs, EndMs + MainWindowPage.FrameToleranceMs);

        var videoDeviation = LuminanceDeviation(capture, CaptureRegions.Video(windowBounds, sliderBounds));
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
        AssertRangeStartsAtMark(measurement, state.Fragment, state.SeekMaximum);
    }

    /// <summary>
    /// Regression test for #36: in the dark theme, the marked range must stand out from its track.
    /// </summary>
    [UiFact]
    public void MarkedRangeIsVisibleInDarkTheme()
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { Theme = LaunchTheme.Dark, OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.MarkRange(StartMs, EndMs);
        var state = app.Page.ReadState();
        var sliderBounds = app.Page.SeekSlider.BoundingRectangle;
        var windowBounds = app.Window.BoundingRectangle;
        using var capture = app.Capture("range-Dark-contrast");

        AssertThemeApplied(capture, windowBounds, app.WindowDpi, LaunchTheme.Dark);
        Assert.NotNull(state.Fragment);
        var measurement = RangeProbe.Measure(
            capture,
            windowBounds,
            sliderBounds,
            (double) StartMs / state.SeekMaximum,
            (double) EndMs / state.SeekMaximum
        );
        Assert.True(measurement.ContrastRatio >= MinDarkRangeContrast, $"RangeContrast: the range against the track is {measurement.ContrastRatio:F2}, expected {MinDarkRangeContrast} or more. Track {measurement.TrackColor}, range {measurement.RangeColor}. The result depends on the Windows accent color, and a custom dark accent can fail even with the fix.");
        AssertRangeStartsAtMark(measurement, state.Fragment, state.SeekMaximum);
    }

    /// <summary>
    /// Regression test for the review of PR #78: the inactive track of the volume slider must have the color of the seek slider track.
    /// </summary>
    [UiTheory]
    [InlineData(LaunchTheme.Light)]
    [InlineData(LaunchTheme.Dark)]
    public void InactiveTrackColorMatchesVolumeSlider(LaunchTheme theme)
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { Theme = theme, OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.MarkRange(StartMs, EndMs);
        var state = app.Page.ReadState();
        var seekBounds = app.Page.SeekSlider.BoundingRectangle;
        var volumeBounds = app.Page.VolumeSlider.BoundingRectangle;
        var volume = app.Page.VolumeSlider.Patterns.RangeValue.Pattern;
        var volumeRatio = volume.Value.Value / volume.Maximum.Value;
        var windowBounds = app.Window.BoundingRectangle;
        using var capture = app.Capture($"track-{theme}");

        AssertThemeApplied(capture, windowBounds, app.WindowDpi, theme);

        var seekPoint = new Point(
            seekBounds.Left + (int) ((double) StartMs / state.SeekMaximum / 2 * seekBounds.Width),
            seekBounds.Top + (seekBounds.Height / 2)
        );
        var volumePoint = new Point(volumeBounds.Right - VolumeTrackEndInset, volumeBounds.Top + (volumeBounds.Height / 2));
        var volumeThumbCenter = volumeBounds.Left + VolumeThumbHalfWidth + (volumeRatio * (volumeBounds.Width - (2 * VolumeThumbHalfWidth)));
        Assert.True(volumePoint.X - volumeThumbCenter >= MinDistanceFromVolumeThumb, $"VolumeSampleOnTrack: the volume ratio {volumeRatio:F2} puts the thumb too close to the sample point.");

        var seekTrack = RangeProbe.ColorAt(capture, windowBounds, seekPoint);
        var volumeTrack = RangeProbe.ColorAt(capture, windowBounds, volumePoint);
        AssertSampleIsTrack("seek slider", capture, windowBounds, seekPoint, seekTrack);
        AssertSampleIsTrack("volume slider", capture, windowBounds, volumePoint, volumeTrack);

        var text = string.Create(CultureInfo.InvariantCulture, $"theme={theme} seekTrack={seekTrack} volumeTrack={volumeTrack}");
        File.WriteAllText(Path.Combine(TestPaths.ArtifactsDir, $"track-{theme}.txt"), text);
        _output.WriteLine(text);
        Assert.True(RangeProbe.AreClose(seekTrack, volumeTrack, TrackColorTolerance), $"TrackColor: the inactive track of the volume slider is {volumeTrack}, the seek slider track is {seekTrack}, expected a difference of {TrackColorTolerance} or less in each channel.");
    }

    private static void AssertSampleIsTrack(string slider, Bitmap capture, Rectangle windowBounds, Point point, Color track)
    {
        var background = RangeProbe.ColorAt(capture, windowBounds, new Point(point.X, point.Y - BackgroundProbeOffset));
        Assert.False(RangeProbe.AreClose(track, background, TrackColorTolerance), $"SampleOnTrack: the {slider} sample {track} equals the background {background} above it, so the sample missed the track.");
    }

    private static void AssertThemeApplied(Bitmap capture, Rectangle windowBounds, uint windowDpi, LaunchTheme theme)
    {
        var titleLuminance = MeanLuminance(capture, CaptureRegions.TitleBar(windowBounds, windowDpi));
        if (theme == LaunchTheme.Light)
        {
            Assert.True(titleLuminance > 200, $"ThemeApplied: title bar luminance is {titleLuminance:F0}, expected over 200.");
        }
        else
        {
            Assert.True(titleLuminance < 60, $"ThemeApplied: title bar luminance is {titleLuminance:F0}, expected under 60.");
        }
    }

    private static void AssertRangeStartsAtMark(RangeMeasurement measurement, MarkedRange fragment, double seekMaximum)
    {
        var expected = fragment.StartMs / seekMaximum;
        Assert.True(Math.Abs(measurement.RunStartRatio - expected) <= RangeStartTolerance, $"RangePosition: the range pixels start at {measurement.RunStartRatio:F3}, expected {expected:F3} plus or minus {RangeStartTolerance}.");
    }

    private static double MeanLuminance(Bitmap bitmap, Rectangle area)
    {
        return Luminances(bitmap, area).Average();
    }

    private static double LuminanceDeviation(Bitmap bitmap, Rectangle area)
    {
        var values = Luminances(bitmap, area).ToList();
        var mean = values.Average();
        return Math.Sqrt(values.Average(value => (value - mean) * (value - mean)));
    }

    private static IEnumerable<double> Luminances(Bitmap bitmap, Rectangle area)
    {
        for (var row = area.Top; row < area.Bottom; row++)
        {
            for (var column = area.Left; column < area.Right; column++)
            {
                var color = bitmap.GetPixel(column, row);
                yield return (0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B);
            }
        }
    }
}
