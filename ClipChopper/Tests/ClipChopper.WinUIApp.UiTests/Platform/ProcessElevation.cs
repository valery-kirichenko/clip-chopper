using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal static class ProcessElevation
{
    public static bool IsElevated(Process process)
    {
        if (!NativeMethods.OpenProcessToken(process.Handle, TokenAccessRights.Query, out var token))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "OpenProcessToken failed.");
        }

        try
        {
            var length = (uint) Marshal.SizeOf<TokenElevation>();
            if (!NativeMethods.GetTokenInformation(token, TokenInformationClass.Elevation, out var elevation, length, out _))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "GetTokenInformation failed.");
            }

            return elevation.TokenIsElevated != 0;
        }
        finally
        {
            NativeMethods.CloseHandle(token);
        }
    }
}
