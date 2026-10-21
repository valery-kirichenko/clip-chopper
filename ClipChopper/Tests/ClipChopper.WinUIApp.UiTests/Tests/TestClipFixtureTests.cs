using ClipChopper.WinUIApp.UiTests.Infrastructure;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

[Collection("UI")]
public sealed class TestClipFixtureTests
{
    [UiFact]
    public void RemovesItsFolderWhenFfmpegFails()
    {
        var foldersBefore = ListFixtureFolders();
        var missingFfmpeg = Path.Combine(Path.GetTempPath(), "ClipChopperUiTests-no-ffmpeg", "ffmpeg.exe");

        Assert.ThrowsAny<Exception>(() => new TestClipFixture(missingFfmpeg));

        Assert.Equal(foldersBefore, ListFixtureFolders());
    }

    private static string[] ListFixtureFolders()
    {
        return Directory.GetDirectories(Path.GetTempPath(), "ClipChopperUiTests-*").Order().ToArray();
    }
}
