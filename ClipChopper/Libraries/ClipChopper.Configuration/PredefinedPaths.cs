using System.IO;

namespace ClipChopper.Configuration
{
    public static class PredefinedPaths
    {
        private const string ConfigFilename = "config.json";

        public static string DefaultOptionsPath { get; } =
            Path.Combine(Directory.GetCurrentDirectory(), ConfigFilename);

        public static string AlternativeOptionsPath { get; } =
            Path.Combine("/etc", "clip_chopper", ConfigFilename);
    }
}
