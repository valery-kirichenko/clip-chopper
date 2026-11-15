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

    [LibraryImport(User32, EntryPoint = "CreateDesktopW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr CreateDesktop(string name, IntPtr device, IntPtr deviceMode, uint flags, DesktopAccessRights desiredAccess, IntPtr attributes);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool EnumDesktopWindows(IntPtr desktop, delegate* unmanaged<IntPtr, IntPtr, int> callback, IntPtr state);

    [LibraryImport(User32)]
    internal static partial uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [LibraryImport(User32, EntryPoint = "GetClassNameW")]
    internal static unsafe partial int GetClassName(IntPtr window, char* buffer, int maxCount);

    [LibraryImport(User32)]
    internal static partial IntPtr GetWindow(IntPtr window, GetWindowCommand command);

    [LibraryImport(User32)]
    internal static partial IntPtr GetForegroundWindow();

    [LibraryImport(User32, EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    internal static partial nint GetWindowLongPtr(IntPtr window, WindowLongIndex index);

    [LibraryImport(User32, EntryPoint = "GetUserObjectInformationW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetUserObjectInformation(IntPtr handle, UserObjectInformationIndex index, char* buffer, uint length, out uint lengthNeeded);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetProcessDpiAwarenessContext(IntPtr context);

    [LibraryImport(User32)]
    internal static partial IntPtr GetDpiAwarenessContextForProcess(IntPtr process);

    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);

    [LibraryImport(User32)]
    internal static partial uint GetDpiForWindow(IntPtr hwnd);

    [LibraryImport(Kernel32, EntryPoint = "CreateJobObjectW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr CreateJobObject(IntPtr attributes, string? name);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetInformationJobObject(IntPtr job, JobObjectInfoClass infoClass, ref JobObjectExtendedLimitInformation info, uint length);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [LibraryImport(Kernel32, EntryPoint = "CreateProcessW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreateProcess(
        string applicationName,
        IntPtr commandLine,
        IntPtr processAttributes,
        IntPtr threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
        ProcessCreationFlags creationFlags,
        IntPtr environment,
        string currentDirectory,
        ref StartupInfo startupInfo,
        out ProcessInformation processInformation
    );

    [LibraryImport(Kernel32, SetLastError = true)]
    internal static partial uint ResumeThread(IntPtr thread);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool TerminateProcess(IntPtr process, uint exitCode);

    [LibraryImport(Kernel32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(IntPtr handle);
}
