using ClipChopper.WinUIApp.CommandLine;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// Turns launch options into command line tokens for the app. LaunchOptionsParser reads them back.
/// </summary>
public static class LaunchArguments
{
    public static IReadOnlyList<string> From(LaunchOptions options)
    {
        var arguments = new List<string>();
        if (options.Theme is { } theme)
        {
            arguments.Add(LaunchOptionNames.Theme);
            arguments.Add(theme.ToString());
        }

        if (options.OpenFile is { } path)
        {
            arguments.Add(LaunchOptionNames.Open);
            arguments.Add(path);
        }

        if (options.ExposeRange)
        {
            arguments.Add(LaunchOptionNames.ExposeRange);
        }

        return arguments;
    }
}
