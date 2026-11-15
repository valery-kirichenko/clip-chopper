using System.Diagnostics;

namespace ClipChopper.WinUIApp.UiTests.Platform;

internal sealed record StartedProcess(Process Process, KillOnCloseJob Job);
