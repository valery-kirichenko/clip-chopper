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
    private const int ControlAreaLeftMargin = 76;
    private const int ControlAreaTopMargin = 12;
    private const int ControlAreaRightMargin = 8;
    private const int ControlAreaBottomMargin = 6;

    /// <summary>
    /// Converts a size in logical pixels to physical pixels at the DPI of the window.
    /// </summary>
    public static int Scale(int logicalPixels, uint windowDpi)
    {
        return (int) (logicalPixels * windowDpi / LogicalDpi);
    }

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

    /// <summary>
    /// The control area under the video: the timeline and the volume slider, down to the bottom of the window.
    /// </summary>
    public static Rectangle ControlArea(Rectangle windowBounds, Rectangle sliderBounds, uint windowDpi)
    {
        var left = Math.Max(0, sliderBounds.Left - windowBounds.Left - Scale(ControlAreaLeftMargin, windowDpi));
        var top = Math.Max(0, sliderBounds.Top - windowBounds.Top - Scale(ControlAreaTopMargin, windowDpi));
        var width = windowBounds.Width - left - Scale(ControlAreaRightMargin, windowDpi);
        var height = windowBounds.Height - top - Scale(ControlAreaBottomMargin, windowDpi);
        return new Rectangle(left, top, width, height);
    }
}
