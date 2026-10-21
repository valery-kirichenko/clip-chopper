using System.Runtime.InteropServices;

namespace ClipChopper.WinUIApp.UiTests.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct TokenElevation
{
    public uint TokenIsElevated;
}
