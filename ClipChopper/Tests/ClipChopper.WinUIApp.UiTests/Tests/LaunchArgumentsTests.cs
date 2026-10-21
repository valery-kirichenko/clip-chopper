using ClipChopper.WinUIApp.CommandLine;
using ClipChopper.WinUIApp.UiTests.Driver;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class LaunchArgumentsTests
{
    public static TheoryData<LaunchOptions> RoundTripOptions => new()
    {
        new LaunchOptions { Theme = LaunchTheme.Dark },
        new LaunchOptions { OpenFile = @"C:\some folder\клип.mp4" },
        new LaunchOptions { ExposeRange = true },
        new LaunchOptions { Theme = LaunchTheme.Light, OpenFile = @"C:\some folder\клип.mp4", ExposeRange = true },
    };

    [Fact]
    public void BuildsNoTokensForNormalStart()
    {
        Assert.Empty(LaunchArguments.From(new LaunchOptions()));
    }

    [Theory]
    [MemberData(nameof(RoundTripOptions))]
    public void ParserReadsWhatTheBuilderWrites(LaunchOptions options)
    {
        Assert.Equal(options, LaunchOptionsParser.Parse(LaunchArguments.From(options)));
    }

    [Fact]
    public void BuildsTokensInFixedOrder()
    {
        var options = new LaunchOptions { Theme = LaunchTheme.Dark, OpenFile = "x", ExposeRange = true };

        var arguments = LaunchArguments.From(options);

        Assert.Equal(
            [LaunchOptionNames.Theme, "Dark", LaunchOptionNames.Open, "x", LaunchOptionNames.ExposeRange],
            arguments
        );
    }
}
