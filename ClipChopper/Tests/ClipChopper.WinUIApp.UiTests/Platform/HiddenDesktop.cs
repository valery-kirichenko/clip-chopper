using ClipChopper.WinUIApp.UiTests.Native;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// A desktop object in the window station WinSta0. It is not the input desktop,
/// so its windows are not visible to the user and get no user input.
/// </summary>
internal sealed class HiddenDesktop : IDisposable
{
    private IntPtr _handle;

    private HiddenDesktop(IntPtr handle, string name)
    {
        _handle = handle;
        Name = name;
        StartupName = @"WinSta0\" + name;
    }

    public IntPtr Handle => _handle;

    public string Name { get; }

    public string StartupName { get; }

    public static HiddenDesktop Create()
    {
        var name = "ClipChopperUiTest-" + Guid.NewGuid().ToString("N");
        var handle = NativeMethods.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, DesktopAccessRights.GenericAll, IntPtr.Zero);
        if (handle == IntPtr.Zero)
        {
            throw Win32Errors.CreateFromLastCall("CreateDesktop");
        }

        return new HiddenDesktop(handle, name);
    }

    public void Dispose()
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.CloseDesktop(_handle);
        _handle = IntPtr.Zero;
    }
}
