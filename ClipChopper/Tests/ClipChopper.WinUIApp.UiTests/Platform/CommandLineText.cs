using System.Text;

namespace ClipChopper.WinUIApp.UiTests.Platform;

/// <summary>
/// Joins tokens into one command line. CommandLineToArgvW reads each token back as it was.
/// </summary>
internal static class CommandLineText
{
    public static string Join(IReadOnlyList<string> tokens)
    {
        return string.Join(' ', tokens.Select(Quote));
    }

    private static string Quote(string token)
    {
        if (token.Length > 0 && token.IndexOfAny([' ', '\t', '"']) < 0)
        {
            return token;
        }

        var builder = new StringBuilder("\"");
        var backslashes = 0;
        foreach (var character in token)
        {
            if (character == '\\')
            {
                backslashes++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', backslashes * 2 + 1);
            }
            else
            {
                builder.Append('\\', backslashes);
            }

            builder.Append(character);
            backslashes = 0;
        }

        builder.Append('\\', backslashes * 2);
        return builder.Append('"').ToString();
    }
}
