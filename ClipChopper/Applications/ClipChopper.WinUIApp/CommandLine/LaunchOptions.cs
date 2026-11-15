namespace ClipChopper.WinUIApp.CommandLine
{
    /// <summary>
    /// The launch options as typed values. A new instance describes a normal start.
    /// </summary>
    public sealed record LaunchOptions
    {
        public LaunchTheme? Theme { get; init; }

        public string? OpenFile { get; init; }

        public bool ExposeRange { get; init; }

        /// <summary>
        /// When true, the app shows its window without activation and sets the volume of its player to 0. The UI tests use it.
        /// </summary>
        public bool UiTest { get; init; }
    }
}
