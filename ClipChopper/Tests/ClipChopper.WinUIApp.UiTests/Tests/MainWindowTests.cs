using ClipChopper.WinUIApp.UiTests.Driver;
using ClipChopper.WinUIApp.UiTests.Infrastructure;
using Microsoft.Win32;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

[Collection("UI")]
public sealed class MainWindowTests
{
    private const string AppRegistryKey = @"Software\ClipChopper";

    private readonly TestClipFixture _clip;

    public MainWindowTests(TestClipFixture clip)
    {
        _clip = clip;
    }

    [UiFact]
    public void OpensFileFromLaunchArgument()
    {
        var previousPathBefore = ReadPreviousPath();
        MainWindowState state;
        using (var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath }))
        {
            app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
            state = app.Page.ReadState();
            app.Capture("opened").Dispose();
        }

        Assert.True(state.IsOpened, "VideoOpened: the play button is enabled.");
        Assert.Equal("ClipChopper", state.Title);
        Assert.Equal("00:00:10", state.Duration);
        Assert.InRange(state.SeekMaximum, 9900, 10100);
        Assert.Equal("00:00:00", state.CurrentTime);
        Assert.Equal("test-clip.mp4", state.SelectedFileName);
        Assert.True(state.IsAudioSelectorEnabled, "AudioTrackFound: the audio selector is enabled.");
        Assert.False(state.IsCropOn, "CropOff: the crop button is off.");
        Assert.NotNull(state.Fragment);
        Assert.Equal(0, state.Fragment.StartMs);
        Assert.InRange(state.Fragment.StopMs, state.SeekMaximum - 1, state.SeekMaximum + 1);
        Assert.Equal(previousPathBefore, ReadPreviousPath());
    }

    [UiFact]
    public void MarksStartAndEnd()
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));

        app.Page.SeekTo(2000);
        Assert.Equal("00:00:02", app.Page.ReadState().CurrentTime);

        app.Page.MarkStart(2000);
        var afterStart = app.Page.ReadState();
        Assert.NotNull(afterStart.Fragment);
        Assert.InRange(afterStart.Fragment.StartMs, 2000 - MainWindowPage.FrameToleranceMs, 2000 + MainWindowPage.FrameToleranceMs);
        Assert.InRange(afterStart.Fragment.StopMs, afterStart.SeekMaximum - 1, afterStart.SeekMaximum + 1);

        app.Page.SeekTo(7000);
        app.Page.MarkEnd(7000);
        var afterEnd = app.Page.ReadState();
        Assert.NotNull(afterEnd.Fragment);
        Assert.InRange(afterEnd.Fragment.StopMs, 7000 - MainWindowPage.FrameToleranceMs, 7000 + MainWindowPage.FrameToleranceMs);
        Assert.InRange(afterEnd.Fragment.StartMs, 2000 - MainWindowPage.FrameToleranceMs, 2000 + MainWindowPage.FrameToleranceMs);
        Assert.InRange(afterEnd.SeekValue, 7000 - MainWindowPage.FrameToleranceMs, 7000 + MainWindowPage.FrameToleranceMs);

        app.Capture("marked").Dispose();
    }

    private static object? ReadPreviousPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppRegistryKey);
        return key?.GetValue("PreviousPath");
    }
}
