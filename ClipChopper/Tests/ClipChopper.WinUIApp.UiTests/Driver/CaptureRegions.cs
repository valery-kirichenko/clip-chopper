using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// Sample rectangles in the capture, derived from element bounds so that they hold at any display scale.
/// The bounds are screen rectangles in physical pixels, the same pixels as the capture.
/// </summary>
public static class CaptureRegions
{
    /// <summary>
    /// A band of the title bar, in the middle of the window width. The title bar has no controls there.
    /// </summary>
    public static Rectangle TitleBar(Rectangle windowBounds)
    {
        var width = windowBounds.Width / 40;
        var top = windowBounds.Height * 3 / 100;
        var height = Math.Max(windowBounds.Height * 2 / 100, 1);
        return new Rectangle((windowBounds.Width - width) / 2, top, width, height);
    }

    /// <summary>
    /// A band of the video area, between the title bar and the slider, across the full slider width.
    /// </summary>
    public static Rectangle Video(Rectangle windowBounds, Rectangle sliderBounds)
    {
        var sliderTop = sliderBounds.Top - windowBounds.Top;
        var top = sliderTop / 4;
        var height = Math.Max(sliderTop / 4, 1);
        return new Rectangle(sliderBounds.Left - windowBounds.Left, top, sliderBounds.Width, height);
    }
}
