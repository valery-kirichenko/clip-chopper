using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// Sample rectangles in the capture, derived from element bounds so that they hold at the display scales that were checked, 100% and 150%.
/// The bounds are screen rectangles in physical pixels, the same pixels as the capture.
/// </summary>
public static class CaptureRegions
{
    private const int TitleBarLogicalHeight = 32;
    private const int LogicalDpi = 96;

    /// <summary>
    /// A band of the title bar, in the middle of the window width and of the title bar height.
    /// The title bar is 32 logical pixels high. The title bar has no controls in the middle of the window width.
    /// </summary>
    public static Rectangle TitleBar(Rectangle windowBounds, uint windowDpi)
    {
        var titleBarHeight = (int) (TitleBarLogicalHeight * windowDpi / LogicalDpi);
        var width = windowBounds.Width / 40;
        var top = titleBarHeight * 35 / 100;
        var height = Math.Max((titleBarHeight * 65 / 100) - top, 1);
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
