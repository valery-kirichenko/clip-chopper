namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record MainWindowState(
    string Title,
    bool IsOpened,
    string CurrentTime,
    string Duration,
    double SeekValue,
    double SeekMaximum,
    bool IsCropOn,
    string? SelectedFileName,
    bool IsAudioSelectorEnabled,
    int? FragmentStartMs,
    int? FragmentStopMs);
