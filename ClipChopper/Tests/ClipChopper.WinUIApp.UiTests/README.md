# ClipChopper.WinUIApp.UiTests

UI automation tests for the WinUI app.

## What the tests do

The tests start the built app and open a generated test clip with the `--open` option. They mark a range, read the state of the main controls through UI Automation, and save screenshots of the window. The library FlaUI (UIA3) drives the app.

## Requirements

- A Windows desktop session. The test host must not run as administrator.
- The .NET 10 SDK.

The first build downloads FFmpeg, as for the app. The tests use it to make the test clip.

## Run

Run this command from the repository root:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64
```

The command builds the app with the runtime identifier `win-x64` first. The test run itself takes about 10 to 20 seconds.

## What you see

App windows open at position 100, 100 with size 1280 x 720 in physical screen pixels and take the focus. The test host is per-monitor DPI aware, so the UI Automation bounds and the screenshots use physical pixels. The sample regions come from element bounds and the window DPI, so the checks follow the display scale. The tests passed at 100% and at 150%. At a higher scale, the app content is larger, so less of it fits in the window. Do not use the mouse or the keyboard during the run. The app has keyboard shortcuts: J, K, L, Space, and the arrow keys seek or play the video. A key press or a click in the app window can make a test fail. Do not use the app window while the tests run. Each test closes its window. A failed test closes it too.

## Output

The tests write these files to `artifacts/ui-tests` in the repository root. Git ignores this folder.

- `opened.png`: the window after the test clip opened.
- `marked.png`: the window after the test marked the start and the end.
- `range-Light.png` and `range-Dark.png`: the window with a marked range in each theme.
- `range-Light.txt` and `range-Dark.txt`: the colors and the contrast ratio that the test measured on the range.

## Launch options

The app has two options for tests: `--theme Light|Dark` and `--open <path>`. They are off by default and have no effect on a normal start.

## Write a new test

Start the app only through `ClipChopperApp`. This class holds the launch arguments, the window position, and the window size.

```csharp
using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = path, ExtraArguments = ["--option"] });
app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
using var shot = app.Capture("my-screenshot");
```

Use `app.Page` to find controls, to seek, to mark, and to read the state. Mark a test with `[UiFact]` or `[UiTheory]` and put it in the collection `UI`.

## Skips

A test skips when the test host runs as administrator or when no interactive desktop exists. A skip is not a pass. A locked screen also skips the tests, because the input desktop is not available then. The tests do not check a remote desktop session. CI does not run these tests.

## Notes

- The app builds with the runtime identifier `win-x64`. Without it, the app can fail to start from a deep folder path, for example a git worktree.
- The Debug build of the app writes the Flyleaf log to `C:\Flyleaf\log.txt`.
- The contrast ratio of the range in the dark theme is low. Issue #36 tracks this. The tests write the ratio to a file and do not assert on it.
