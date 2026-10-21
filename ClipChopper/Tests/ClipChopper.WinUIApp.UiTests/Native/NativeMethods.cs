using System.Runtime.InteropServices;

namespace ClipChopper.WinUIApp.UiTests.Native;

internal static partial class NativeMethods
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PrintWindow(IntPtr hwnd, IntPtr hdc, PrintWindowFlags flags);

    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsWindowVisible(IntPtr hwnd);

    [LibraryImport(User32, SetLastError = true)]
    internal static partial IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, DesktopAccessRights desiredAccess);

    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseDesktop(IntPtr desktop);

    [LibraryImport(Kernel32, EntryPoint = "CreateJobObjectW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr CreateJobObject(IntPtr attributes, string? name);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetInformationJobObject(IntPtr job, JobObjectInfoClass infoClass, ref JobObjectExtendedLimitInformation info, uint length);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(IntPtr handle);
}
