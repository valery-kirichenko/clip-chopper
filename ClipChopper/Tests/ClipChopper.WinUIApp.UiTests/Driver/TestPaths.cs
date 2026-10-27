using System.Reflection;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public static class TestPaths
{
    public static string AppExePath { get; } = ReadMetadata("AppExePath");

    public static string ArtifactsDir { get; } = ReadMetadata("ArtifactsDir");

    private static string ReadMetadata(string key)
    {
        return typeof(TestPaths).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == key)
            .Value!;
    }
}
