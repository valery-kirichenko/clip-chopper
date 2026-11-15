using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class WindowStyleReader
{
    public static ExtendedWindowStyles ReadExtended(IntPtr windowHandle)
    {
        var style = NativeMethods.GetWindowLongPtr(windowHandle, WindowLongIndex.ExtendedStyle);
        if (style == 0 && Marshal.GetLastPInvokeError() != 0)
        {
            throw Win32Errors.CreateFromLastCall("GetWindowLongPtr");
        }

        return (ExtendedWindowStyles) (uint) style;
    }
}
