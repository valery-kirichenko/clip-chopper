using System.Drawing;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class WindowCapture
{
    public static Bitmap Print(IntPtr windowHandle, Size size)
    {
        var bitmap = new Bitmap(size.Width, size.Height);
        try
        {
            using var graphics = Graphics.FromImage(bitmap);
            var hdc = graphics.GetHdc();
            try
            {
                if (!NativeMethods.PrintWindow(windowHandle, hdc, PrintWindowFlags.RenderFullContent))
                {
                    throw new InvalidOperationException("PrintWindow failed for the app window.");
                }
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}
