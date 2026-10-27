using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class Win32Errors
{
    public static Win32Exception CreateFromLastCall(string call)
    {
        var error = Marshal.GetLastPInvokeError();
        return new Win32Exception(error, $"{call} failed: {Marshal.GetPInvokeErrorMessage(error)} (0x{error:X8})");
    }
}
