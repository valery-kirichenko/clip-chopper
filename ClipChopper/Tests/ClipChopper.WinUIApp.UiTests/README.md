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

App windows open at position 100, 100 with size 1280 x 720 in physical screen pixels and take the focus. The track color test opens its window at position 0, 0 with a size of 1280 x 720 logical pixels. This is 1280 x 720 physical pixels at 100%, 1600 x 900 at 125%, and 1920 x 1080 at 150%. Then the volume slider fits at every display scale. The screen must hold that window. If it does not, the test fails with a message about the window size or about the volume slider. The test host is per-monitor DPI aware, so the UI Automation bounds and the screenshots use physical pixels. The sample regions come from element bounds and the window DPI, so the checks follow the display scale. The tests passed at 100% and at 150%. The #36 tests ran at 100% only. A plain test checks the size conversion for 125% and 150%, and a run at 100% with the same logical window size checked the layout of the track color test. The pixel samples at a real display scale of 125% or 150% are not checked. At a higher scale, the app content is larger, so less of it fits in the window. Do not use the mouse or the keyboard during the run. The app has keyboard shortcuts: J, K, L, Space, and the arrow keys seek or play the video. A key press or a click in the app window can make a test fail. Do not use the app window while the tests run. Each test closes its window. A failed test closes it too.

## Output

The tests write these files to `artifacts/ui-tests` in the repository root. Git ignores this folder.

- `opened.png`: the window after the test clip opened.
- `marked.png`: the window after the test marked the start and the end.
- `range-Light.png` and `range-Dark.png`: the window with a marked range in each theme.
- `range-Light.txt` and `range-Dark.txt`: the colors and the contrast ratio that the test measured on the range.
- `range-Dark-contrast.png`: the window that the dark theme contrast test checks.
- `track-Light.png` and `track-Dark.png`: the window that the track color test checks in each theme.
- `track-Light.txt` and `track-Dark.txt`: the track colors that the test measured on the seek slider and on the volume slider.
- `track-area-Light.png` and `track-area-Dark.png`: the control area of the window (the timeline and the volume slider) from the track color test.

## Launch options

The app has three options for tests: `--theme Light|Dark`, `--open <path>`, and `--expose-range`. The last one makes the app publish the marked range in the `ItemStatus` of the seek slider. `ClipChopperApp` sets it by default. The options are off by default and have no effect on a normal start.

The names live in `ClipChopper/Applications/ClipChopper.WinUIApp/CommandLine/LaunchOptionNames.cs`. The app reads the command line once, with `LaunchOptionsParser`, and keeps the result in a `LaunchOptions` record. The parser ignores unknown arguments and bad values. If an option repeats, the last valid value wins.

The `.csproj` of the test project lists five files of that folder and one file of the `Automation` folder (`RangeStatusFormat`) one by one, so the tests use the same names, the same `LaunchTheme` enum, and the same range format. `LaunchArguments` builds the command line from a `LaunchOptions` record. To add an option, change `LaunchOptions`, `LaunchOptionNames`, `LaunchOptionsParser`, and `LaunchArguments`, and add a parser test.

## Write a new test

Start the app only through `ClipChopperApp`. This class holds the launch arguments, the window position, and the window size.

```csharp
using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = path, Theme = LaunchTheme.Dark });
app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
using var shot = app.Capture("my-screenshot");
```

Use `app.Page` to find controls, to seek, to mark, and to read the state. Mark a test with `[UiFact]` or `[UiTheory]` and put it in the collection `UI`.

## Skips

A test skips when the test host runs as administrator or when no interactive desktop exists. A skip is not a pass. A locked screen also skips the tests, because the input desktop is not available then. The tests do not check a remote desktop session. CI does not run these tests.

The tests of the launch option parser, the command line builder, the range format, and the size conversion are plain `[Fact]` and `[Theory]` tests. They run without a desktop. To run only them, use this command:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64 --filter "FullyQualifiedName~LaunchOptionsParserTests|FullyQualifiedName~LaunchArgumentsTests|FullyQualifiedName~RangeStatusFormatTests|FullyQualifiedName~CaptureRegionsTests"
```

## Notes

- The app builds with the runtime identifier `win-x64`. Without it, the app can fail to start from a deep folder path, for example a git worktree.
- The Debug build of the app writes the Flyleaf log to `C:\Flyleaf\log.txt`.
- In the dark theme, the test `MarkedRangeIsVisibleInDarkTheme` asserts a range contrast of 2.5 or more against the track (#36). The light theme has no contrast check, because #36 does not change it. Both themes check that the range starts at the marked position.
- The dark theme contrast check depends on the Windows accent color. The threshold is 2.5, and a custom dark accent can fail it even with the fix.
- The test `InactiveTrackColorMatchesVolumeSlider` checks that the inactive track of the volume slider has the color of the seek slider track in both themes (#36). It samples one pixel on each track, away from the thumb and from the marked range.
