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
            throw Win32Errors.CreateFromLastCall("CreateJobObject");
        }

        var info = new JobObjectExtendedLimitInformation();
        info.BasicLimitInformation.LimitFlags = JobObjectLimitFlags.KillOnJobClose;
        var length = (uint) Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        try
        {
            if (!NativeMethods.SetInformationJobObject(handle, JobObjectInfoClass.ExtendedLimitInformation, ref info, length))
            {
                throw Win32Errors.CreateFromLastCall("SetInformationJobObject");
            }

            if (!NativeMethods.AssignProcessToJobObject(handle, process.Handle))
            {
                throw Win32Errors.CreateFromLastCall("AssignProcessToJobObject");
            }
        }
        catch
        {
            NativeMethods.CloseHandle(handle);
            throw;
        }

        return new KillOnCloseJob(handle);
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
