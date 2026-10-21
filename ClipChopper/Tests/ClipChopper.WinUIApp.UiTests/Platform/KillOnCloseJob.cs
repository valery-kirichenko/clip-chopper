using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal sealed class KillOnCloseJob : IDisposable
{
    private IntPtr _handle;

    private KillOnCloseJob(IntPtr handle)
    {
        _handle = handle;
    }

    public static KillOnCloseJob Assign(Process process)
    {
        var handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        if (handle == IntPtr.Zero)
        {
            throw CreateError("CreateJobObject");
        }

        var info = new JobObjectExtendedLimitInformation();
        info.BasicLimitInformation.LimitFlags = JobObjectLimitFlags.KillOnJobClose;
        var length = (uint) Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        try
        {
            if (!NativeMethods.SetInformationJobObject(handle, JobObjectInfoClass.ExtendedLimitInformation, ref info, length))
            {
                throw CreateError("SetInformationJobObject");
            }

            if (!NativeMethods.AssignProcessToJobObject(handle, process.Handle))
            {
                throw CreateError("AssignProcessToJobObject");
            }
        }
        catch
        {
            NativeMethods.CloseHandle(handle);
            throw;
        }

        return new KillOnCloseJob(handle);
    }

    private static Win32Exception CreateError(string call)
    {
        var error = Marshal.GetLastPInvokeError();
        return new Win32Exception(error, $"{call} failed: {Marshal.GetPInvokeErrorMessage(error)} (0x{error:X8})");
    }

    public void Dispose()
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.CloseHandle(_handle);
        _handle = IntPtr.Zero;
    }
}
