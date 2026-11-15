using ClipChopper.WinUIApp.UiTests.Driver;
using ClipChopper.WinUIApp.UiTests.Infrastructure;
using ClipChopper.WinUIApp.UiTests.Native;
using ClipChopper.WinUIApp.UiTests.Platform;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

[Collection("UI")]
public sealed class LaunchModeTests
{
    private readonly TestClipFixture _clip;

    public LaunchModeTests(TestClipFixture clip)
    {
        _clip = clip;
    }

    [UiFact]
    public void ShowsWindowOnlyOnExpectedDesktop()
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.SeekTo(2000);
        app.Page.MarkStart(2000);

        var windowsOnInputDesktop = InputDesktop.FindVisibleWindows(app.ProcessId);

        if (UiTestMode.IsVisible)
        {
            Assert.NotEmpty(windowsOnInputDesktop);
        }
        else
        {
            Assert.Empty(windowsOnInputDesktop);
            Assert.NotEqual(app.DesktopName, InputDesktop.ReadName());
        }
    }

    /// <summary>
    /// The style check can fail in both modes. In the hidden mode, the foreground check holds by design,
    /// because the app window is on a desktop that gets no input.
    /// </summary>
    [UiFact]
    public void NeverTakesForeground()
    {
        using var watcher = ForegroundWatcher.Start();
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.SeekTo(2000);
        app.Page.MarkStart(2000);
        app.Page.SeekTo(7000);
        app.Page.MarkEnd(7000);
        app.Capture("foreground").Dispose();

        var foregroundProcessIds = watcher.Stop();
        var windowStyle = WindowStyleReader.ReadExtended(app.Window.Properties.NativeWindowHandle.Value);

        Assert.True(foregroundProcessIds.Count > 0, "The watcher saw no foreground window, so the focus check proves nothing.");
        Assert.DoesNotContain(app.ProcessId, foregroundProcessIds);
        Assert.True(windowStyle.HasFlag(ExtendedWindowStyles.NoActivate), $"The app window does not have WS_EX_NOACTIVATE. Style: {windowStyle}.");
        if (!UiTestMode.IsVisible)
        {
            Assert.NotEqual(app.DesktopName, InputDesktop.ReadName());
        }
    }

    [UiFact]
    public void KeepsPlayerMutedInTestMode()
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        app.Page.SeekTo(2000);
        app.Page.MarkStart(2000);
        app.Capture("muted").Dispose();

        Assert.True(app.Page.IsMuted(), "The player must stay muted after the video opens.");
        Assert.False(app.Page.VolumeSlider.IsEnabled, "The volume slider must stay disabled in test mode.");
    }
}
