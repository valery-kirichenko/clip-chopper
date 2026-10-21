using ClipChopper.WinUIApp.CommandLine;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class LaunchOptionsParserTests
{
    private const string ClipPath = @"C:\some folder\клип.mp4";

    [Fact]
    public void ReturnsNormalStartForNoArguments()
    {
        Assert.Equal(new LaunchOptions(), LaunchOptionsParser.Parse([]));
    }

    [Theory]
    [InlineData(LaunchTheme.Light)]
    [InlineData(LaunchTheme.Dark)]
    public void ReadsTheme(LaunchTheme theme)
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, theme.ToString()]);

        Assert.Equal(theme, options.Theme);
    }

    [Theory]
    [InlineData("dark", LaunchTheme.Dark)]
    [InlineData("LIGHT", LaunchTheme.Light)]
    public void ReadsThemeInAnyCase(string value, LaunchTheme expected)
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, value]);

        Assert.Equal(expected, options.Theme);
    }

    [Theory]
    [InlineData("Blue")]
    [InlineData("1")]
    [InlineData("Default")]
    [InlineData("")]
    public void IgnoresBadTheme(string value)
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, value]);

        Assert.Null(options.Theme);
    }

    [Theory]
    [InlineData(@"C:\a.mp4")]
    [InlineData(ClipPath)]
    public void ReadsOpenPathAsOneToken(string path)
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Open, path]);

        Assert.Equal(path, options.OpenFile);
    }

    [Fact]
    public void ReadsExposeRangeFlag()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.ExposeRange]);

        Assert.True(options.ExposeRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ReadsAllOptionsInAnyOrder(int order)
    {
        string[] args = order == 0
            ? [LaunchOptionNames.ExposeRange, LaunchOptionNames.Open, "x", LaunchOptionNames.Theme, "Dark"]
            : [LaunchOptionNames.Theme, "Dark", LaunchOptionNames.Open, "x", LaunchOptionNames.ExposeRange];

        var options = LaunchOptionsParser.Parse(args);

        Assert.Equal(new LaunchOptions { Theme = LaunchTheme.Dark, OpenFile = "x", ExposeRange = true }, options);
    }

    [Theory]
    [InlineData(LaunchOptionNames.Theme)]
    [InlineData(LaunchOptionNames.Open)]
    public void IgnoresOptionWithoutValueAtTheEnd(string name)
    {
        Assert.Equal(new LaunchOptions(), LaunchOptionsParser.Parse([name]));
    }

    [Fact]
    public void ReadsNextOptionWhenThemeValueIsMissing()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, LaunchOptionNames.Open, "x"]);

        Assert.Equal(new LaunchOptions { OpenFile = "x" }, options);
    }

    [Fact]
    public void ReadsNextOptionWhenOpenValueIsMissing()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Open, LaunchOptionNames.ExposeRange]);

        Assert.Equal(new LaunchOptions { ExposeRange = true }, options);
    }

    [Fact]
    public void IgnoresEmptyOpenPath()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Open, string.Empty]);

        Assert.Null(options.OpenFile);
    }

    [Fact]
    public void UsesLastOpenPathOfRepeatedOption()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Open, "a", LaunchOptionNames.Open, "b"]);

        Assert.Equal("b", options.OpenFile);
    }

    [Fact]
    public void UsesLastThemeOfRepeatedOption()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, "Light", LaunchOptionNames.Theme, "Dark"]);

        Assert.Equal(LaunchTheme.Dark, options.Theme);
    }

    [Fact]
    public void KeepsEarlierThemeWhenLaterThemeIsBad()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Theme, "Dark", LaunchOptionNames.Theme, "Blue"]);

        Assert.Equal(LaunchTheme.Dark, options.Theme);
    }

    [Fact]
    public void KeepsExposeRangeWhenFlagRepeats()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.ExposeRange, LaunchOptionNames.ExposeRange]);

        Assert.True(options.ExposeRange);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("clip.mp4")]
    [InlineData("--open=clip.mp4")]
    public void IgnoresUnknownAndPositionalTokens(string token)
    {
        Assert.Equal(new LaunchOptions(), LaunchOptionsParser.Parse([token]));
    }

    [Fact]
    public void ComparesOptionNamesWithCase()
    {
        var options = LaunchOptionsParser.Parse([LaunchOptionNames.Open.ToUpperInvariant(), "x"]);

        Assert.Equal(new LaunchOptions(), options);
    }
}
