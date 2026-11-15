using System.Runtime.InteropServices;

namespace ClipChopper.WinUIApp.UiTests.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct ProcessInformation
{
    public IntPtr ProcessHandle;
    public IntPtr ThreadHandle;
    public uint ProcessId;
    public uint ThreadId;
}
