using ClipChopper.WinUIApp.UiTests.Driver;
using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Tests;

public sealed class DriverRulesTests
{
    private static readonly string[] ForbiddenTexts =
    {
        "FlaUI.Core.Input",
        "FlaUI.Core.Capturing",
        ".Click(",
        ".DoubleClick(",
        ".RightClick(",
        ".Focus()",
        ".FocusNative(",
        "SetFocus",
        "SetActiveWindow",
        "Mouse.",
        "Keyboard.",
        "SetForeground",
        "SendInput",
        "SetCursorPos",
        "keybd_event",
        "mouse_event",
    };

    private static readonly string[] ProjectFileForbiddenTexts =
    {
        "FlaUI.Core.Input",
        "FlaUI.Core.Capturing",
    };

    private static readonly string[] ExcludedFolders = { "bin", "obj" };

    [Fact]
    public void UsesNoSimulatedInput()
    {
        var projectDir = Path.GetFullPath(TestPaths.TestProjectDir);
        var violations = new List<string>();
        violations.AddRange(FindViolations(projectDir, "*.cs", ForbiddenTexts));
        violations.AddRange(FindViolations(projectDir, "*.csproj", ProjectFileForbiddenTexts));

        Assert.True(violations.Count == 0, "The test code must not move the cursor, send keys, or take the focus." + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static List<string> FindViolations(string projectDir, string filePattern, string[] forbiddenTexts)
    {
        var violations = new List<string>();
        foreach (var file in Directory.EnumerateFiles(projectDir, filePattern, SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(projectDir, file);
            var topFolder = relativePath.Split(Path.DirectorySeparatorChar)[0];
            if (ExcludedFolders.Contains(topFolder, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Path.GetFileName(file) == "DriverRulesTests.cs")
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (var forbidden in forbiddenTexts.Where(forbidden => text.Contains(forbidden, StringComparison.Ordinal)))
            {
                violations.Add($"{relativePath} holds \"{forbidden}\"");
            }
        }

        return violations;
    }
}
