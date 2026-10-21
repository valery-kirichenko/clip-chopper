using ClipChopper.WinUIApp.UiTests.Driver;
using ClipChopper.WinUIApp.UiTests.Infrastructure;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

[Collection("UI")]
public sealed class AdminPickerTests
{
    private readonly TestClipFixture _clip;

    public AdminPickerTests(TestClipFixture clip)
    {
        _clip = clip;
    }

    /// <summary>
    /// Regression test for #60: as administrator, each file picker opens, and the app keeps running after Cancel.
    /// </summary>
    [AdminUiTheory]
    [InlineData(PickerButton.OpenFolder)]
    [InlineData(PickerButton.TakeSnapshot)]
    [InlineData(PickerButton.SaveClip)]
    public void OpensPickerAsAdministrator(PickerButton picker)
    {
        using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = _clip.ClipPath });
        app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
        Assert.True(app.IsElevated, "AppElevated: the app runs as administrator, so the test checks the bug of #60.");

        var dialog = app.OpenPicker(picker);
        dialog.Cancel();

        Assert.False(app.HasExited, "AppAlive: the app still runs after the picker closed.");
        var state = app.Page.ReadState();
        Assert.True(state.IsOpened, "VideoOpened: the play button is enabled after the picker closed.");
        Assert.Equal("test-clip.mp4", state.SelectedFileName);
    }
}
