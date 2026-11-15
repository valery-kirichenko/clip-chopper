using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace ClipChopper.WinUIApp.Interop
{
    /// <summary>
    /// Shows a window without activation, so it never takes the focus from another window.
    /// The UI tests use it through the launch option --ui-test.
    /// </summary>
    internal static partial class NoActivateWindow
    {
        private const int ExtendedStyleIndex = -20;

        private const nint NoActivateStyle = 0x08000000;

        public static void Show(Window window)
        {
            var handle = WindowNative.GetWindowHandle(window);
            var style = GetWindowLongPtr(handle, ExtendedStyleIndex);
            if (SetWindowLongPtr(handle, ExtendedStyleIndex, style | NoActivateStyle) == 0)
            {
                var error = Marshal.GetLastPInvokeError();
                if (error != 0)
                {
                    throw new Win32Exception(error, $"SetWindowLongPtr failed: {Marshal.GetPInvokeErrorMessage(error)} (0x{error:X8})");
                }
            }

            window.AppWindow.Show(false);
        }

        [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static partial nint GetWindowLongPtr(nint window, int index);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static partial nint SetWindowLongPtr(nint window, int index, nint value);
    }
}
