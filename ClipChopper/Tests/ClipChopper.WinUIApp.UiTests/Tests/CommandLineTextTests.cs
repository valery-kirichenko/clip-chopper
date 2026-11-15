using ClipChopper.WinUIApp.UiTests.Platform;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class CommandLineTextTests
{
    [Theory]
    [InlineData("a", "a")]
    [InlineData(@"C:\some folder\клип.mp4", @"""C:\some folder\клип.mp4""")]
    [InlineData("", @"""""")]
    [InlineData(@"a""b", @"""a\""b""")]
    [InlineData(@"C:\dir with space\", @"""C:\dir with space\\""")]
    [InlineData("a\tb", "\"a\tb\"")]
    [InlineData(@"say \""hi""", @"""say \\\""hi\""""")]
    public void QuotesOneToken(string token, string expected)
    {
        Assert.Equal(expected, CommandLineText.Join([token]));
    }

    [Fact]
    public void JoinsTokensWithOneSpace()
    {
        Assert.Equal(@"a ""b c""", CommandLineText.Join(["a", "b c"]));
    }
}
