using System;
using System.Collections.Generic;

namespace ClipChopper.WinUIApp.CommandLine
{
    /// <summary>
    /// Reads the launch options from the command line arguments without the program path.
    /// It ignores unknown arguments and bad values, so a bad command line gives a normal start.
    /// </summary>
    public static class LaunchOptionsParser
    {
        public static LaunchOptions Parse(IReadOnlyList<string> args)
        {
            var options = new LaunchOptions();
            for (var index = 0; index < args.Count; index++)
            {
                switch (args[index])
                {
                    case LaunchOptionNames.Theme:
                        if (TryReadValue(args, ref index, out var themeName))
                        {
                            options = options with { Theme = ParseTheme(themeName) ?? options.Theme };
                        }

                        break;
                    case LaunchOptionNames.Open:
                        if (TryReadValue(args, ref index, out var path))
                        {
                            options = options with { OpenFile = path };
                        }

                        break;
                    case LaunchOptionNames.ExposeRange:
                        options = options with { ExposeRange = true };
                        break;
                }
            }

            return options;
        }

        private static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string value)
        {
            value = string.Empty;
            var next = index + 1;
            if (next >= args.Count || args[next].Length == 0 || args[next].StartsWith(LaunchOptionNames.Prefix, StringComparison.Ordinal))
            {
                return false;
            }

            index = next;
            value = args[next];
            return true;
        }

        private static LaunchTheme? ParseTheme(string name)
        {
            foreach (var theme in Enum.GetValues<LaunchTheme>())
            {
                if (string.Equals(theme.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    return theme;
                }
            }

            return null;
        }
    }
}
