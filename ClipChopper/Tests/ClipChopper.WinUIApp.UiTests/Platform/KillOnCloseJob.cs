using System.Diagnostics;
using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal sealed class KillOnCloseJob : IDisposable
{
    private readonly IntPtr _handle;

    private KillOnCloseJob(IntPtr handle)
    {
        _handle = handle;
    }

    public static KillOnCloseJob Assign(Process process)
    {
        var handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("CreateJobObject failed.");
        }

        var info = new JobObjectExtendedLimitInformation();
        info.BasicLimitInformation.LimitFlags = JobObjectLimitFlags.KillOnJobClose;
        var length = (uint) Marshal.SizeOf<JobObjectExtendedLimitInformation>();
        try
        {
            if (!NativeMethods.SetInformationJobObject(handle, JobObjectInfoClass.ExtendedLimitInformation, ref info, length)
                || !NativeMethods.AssignProcessToJobObject(handle, process.Handle))
            {
                throw new InvalidOperationException("Cannot put the app process into a job object.");
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
        NativeMethods.CloseHandle(_handle);
    }
}
