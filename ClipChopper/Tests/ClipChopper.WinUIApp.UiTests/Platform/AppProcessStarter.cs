using System.Diagnostics;
using System.Runtime.InteropServices;
using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// Starts the app suspended on a chosen desktop, puts it into a kill-on-close job, and then resumes it.
/// </summary>
internal static class AppProcessStarter
{
    public static StartedProcess Start(string exePath, IReadOnlyList<string> arguments, string? desktopName)
    {
        var commandLine = Marshal.StringToHGlobalUni(CommandLineText.Join([exePath, .. arguments]));
        var desktop = desktopName == null ? IntPtr.Zero : Marshal.StringToHGlobalUni(desktopName);
        var info = default(ProcessInformation);
        try
        {
            var startupInfo = new StartupInfo
            {
                Size = (uint) Marshal.SizeOf<StartupInfo>(),
                Desktop = desktop,
            };
            if (!NativeMethods.CreateProcess(exePath, commandLine, IntPtr.Zero, IntPtr.Zero, false, ProcessCreationFlags.Suspended, IntPtr.Zero, Path.GetDirectoryName(exePath)!, ref startupInfo, out info))
            {
                throw Win32Errors.CreateFromLastCall("CreateProcess");
            }

            return Resume(info);
        }
        finally
        {
            CloseIfSet(info.ThreadHandle);
            CloseIfSet(info.ProcessHandle);
            Marshal.FreeHGlobal(commandLine);
            if (desktop != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(desktop);
            }
        }
    }

    private static StartedProcess Resume(ProcessInformation info)
    {
        Process? process = null;
        KillOnCloseJob? job = null;
        try
        {
            process = Process.GetProcessById((int) info.ProcessId);
            job = KillOnCloseJob.Assign(process);
            if (NativeMethods.ResumeThread(info.ThreadHandle) == uint.MaxValue)
            {
                throw Win32Errors.CreateFromLastCall("ResumeThread");
            }

            return new StartedProcess(process, job);
        }
        catch
        {
            if (job != null)
            {
                job.Dispose();
            }
            else
            {
                NativeMethods.TerminateProcess(info.ProcessHandle, 1);
            }

            process?.Dispose();
            throw;
        }
    }

    private static void CloseIfSet(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            NativeMethods.CloseHandle(handle);
        }
    }
}
