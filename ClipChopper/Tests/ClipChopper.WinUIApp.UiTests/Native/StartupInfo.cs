using System.Runtime.InteropServices;

namespace ClipChopper.WinUIApp.UiTests.Native;

/// <summary>
/// STARTUPINFOW. The string fields are pointers to native memory that the caller owns.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct StartupInfo
{
    public uint Size;
    public IntPtr Reserved;
    public IntPtr Desktop;
    public IntPtr Title;
    public uint X;
    public uint Y;
    public uint XSize;
    public uint YSize;
    public uint XCountChars;
    public uint YCountChars;
    public uint FillAttribute;
    public uint Flags;
    public ushort ShowWindow;
    public ushort Reserved2Size;
    public IntPtr Reserved2;
    public IntPtr StandardInput;
    public IntPtr StandardOutput;
    public IntPtr StandardError;
}
