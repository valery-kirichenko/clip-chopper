using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public static class RangeProbe
{
    private const int ColorTolerance = 6;

    /// <summary>
    /// Finds the marked range on the slider in the capture.
    /// The bounds are screen rectangles in physical pixels, the same pixels as the capture.
    /// </summary>
    public static RangeMeasurement Measure(
        Bitmap capture,
        Rectangle windowBounds,
        Rectangle sliderBounds,
        double startRatio,
        double endRatio)
    {
        var y = sliderBounds.Top + (sliderBounds.Height / 2) - windowBounds.Top;
        var left = sliderBounds.Left - windowBounds.Left;
        var width = sliderBounds.Width;

        Color PixelAt(double ratio)
        {
            var x = Math.Clamp(left + (int) (ratio * width), 0, capture.Width - 1);
            return capture.GetPixel(x, y);
        }

        var trackColor = PixelAt(startRatio / 2);
        var rangeColor = PixelAt((startRatio + endRatio) / 2);

        bool DiffersFromTrack(int offset)
        {
            var x = Math.Clamp(left + offset, 0, capture.Width - 1);
            return !IsClose(capture.GetPixel(x, y), trackColor);
        }

        var middle = Math.Clamp((int) (((startRatio + endRatio) / 2) * width), 0, width - 1);
        var runStart = middle;
        while (runStart > 0 && DiffersFromTrack(runStart - 1))
        {
            runStart--;
        }

        var runEnd = middle;
        while (runEnd < width - 1 && DiffersFromTrack(runEnd + 1))
        {
            runEnd++;
        }

        return new RangeMeasurement(
            trackColor,
            rangeColor,
            (double) runStart / width,
            (double) runEnd / width,
            ContrastRatio(trackColor, rangeColor)
        );
    }

    private static bool IsClose(Color first, Color second)
    {
        return Math.Abs(first.R - second.R) <= ColorTolerance
            && Math.Abs(first.G - second.G) <= ColorTolerance
            && Math.Abs(first.B - second.B) <= ColorTolerance;
    }

    private static double ContrastRatio(Color first, Color second)
    {
        var lighter = Math.Max(Luminance(first), Luminance(second));
        var darker = Math.Min(Luminance(first), Luminance(second));
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double Luminance(Color color)
    {
        static double Channel(byte value)
        {
            var s = value / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
    }
}
