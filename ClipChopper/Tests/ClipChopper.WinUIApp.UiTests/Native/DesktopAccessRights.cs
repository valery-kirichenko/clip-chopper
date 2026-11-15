namespace ClipChopper.WinUIApp.UiTests.Native;

[Flags]
internal enum DesktopAccessRights : uint
{
    ReadObjects = 0x0001,
    Enumerate = 0x0040,
    SwitchDesktop = 0x0100,
    GenericAll = 0x10000000,
}
