using ClipChopper.WinUIApp.CommandLine;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class RangeStatusFormatTests
{
    [Fact]
    public void FormatsStartAndStop()
    {
        Assert.Equal("start=2000;stop=7000", RangeStatusFormat.Format(2000, 7000));
    }

    [Fact]
    public void ParsesWhatFormatWrites()
    {
        var parsed = RangeStatusFormat.TryParse(RangeStatusFormat.Format(2000, 7000), out var startMs, out var stopMs);

        Assert.True(parsed);
        Assert.Equal(2000, startMs);
        Assert.Equal(7000, stopMs);
    }

    [Theory]
    [InlineData("start=1")]
    [InlineData("stop=1")]
    [InlineData("start=a;stop=1")]
    [InlineData("start=1;stop")]
    [InlineData("start=1=2;stop=3")]
    public void RejectsBadStatus(string status)
    {
        Assert.False(RangeStatusFormat.TryParse(status, out _, out _));
    }
}
