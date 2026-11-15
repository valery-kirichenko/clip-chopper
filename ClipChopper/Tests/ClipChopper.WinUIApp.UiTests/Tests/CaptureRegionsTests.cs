using ClipChopper.WinUIApp.UiTests.Driver;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class CaptureRegionsTests
{
    [Theory]
    [InlineData(96u, 1280, 720)]
    [InlineData(120u, 1600, 900)]
    [InlineData(144u, 1920, 1080)]
    public void ScalesALogicalWindowSizeToPhysicalPixels(uint dpi, int expectedWidth, int expectedHeight)
    {
        Assert.Equal(expectedWidth, CaptureRegions.Scale(1280, dpi));
        Assert.Equal(expectedHeight, CaptureRegions.Scale(720, dpi));
    }

    [Theory]
    [InlineData(96u, 150)]
    [InlineData(120u, 187)]
    [InlineData(144u, 225)]
    public void ScalesTheVolumeSliderWidth(uint dpi, int expectedWidth)
    {
        Assert.Equal(expectedWidth, CaptureRegions.Scale(150, dpi));
    }
}
