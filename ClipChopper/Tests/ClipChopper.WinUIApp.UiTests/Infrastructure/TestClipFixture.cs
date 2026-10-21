using System.Diagnostics;
using ClipChopper.WinUIApp.UiTests.Driver;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public sealed class TestClipFixture : IDisposable
{
    private readonly string? _folder;

    public TestClipFixture()
    {
        if (UiEnvironment.SkipReason != null)
        {
            ClipPath = string.Empty;
            return;
        }

        _folder = Path.Combine(Path.GetTempPath(), "ClipChopperUiTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        ClipPath = Path.Combine(_folder, "test-clip.mp4");
        MakeClip(ClipPath);
    }

    public string ClipPath { get; }

    /// <summary>
    /// Copies the test clip to the same folder under another file name and returns the new path.
    /// </summary>
    public string CopyClipAs(string fileName)
    {
        var path = Path.Combine(Path.GetDirectoryName(ClipPath)!, fileName);
        File.Copy(ClipPath, path);
        return path;
    }

    public void Dispose()
    {
        if (_folder != null && Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    private static void MakeClip(string path)
    {
        var ffmpeg = Path.Combine(Path.GetDirectoryName(TestPaths.AppExePath)!, "FFmpeg", "ffmpeg.exe");
        var startInfo = new ProcessStartInfo(ffmpeg)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        string[] arguments =
        [
            "-hide_banner", "-loglevel", "error", "-y",
            "-f", "lavfi", "-i", "testsrc2=size=640x360:rate=30:duration=10",
            "-f", "lavfi", "-i", "sine=frequency=440:duration=10",
            "-c:v", "libx264", "-pix_fmt", "yuv420p", "-g", "30",
            "-c:a", "aac", "-shortest", path,
        ];
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0 || !File.Exists(path))
        {
            throw new InvalidOperationException($"ffmpeg could not make the test clip (exit code {process.ExitCode}): {error}");
        }
    }
}
