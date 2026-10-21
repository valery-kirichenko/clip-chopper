# ClipChopper.WinUIApp.UiTests

UI automation tests for the WinUI app.

## What the tests do

The tests start the built app and open a generated test clip with the `--open` option. They mark a range, read the state of the main controls through UI Automation, and save screenshots of the window. The library FlaUI (UIA3) drives the app.

## Requirements

- A Windows desktop session. The normal tests need a test host without administrator rights. The test `AdminPickerTests` needs a test host that runs as administrator.
- The .NET 10 SDK.

The first build downloads FFmpeg, as for the app. The tests use it to make the test clip.

## Run

Run this command from the repository root:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64
```

The command builds the app with the runtime identifier `win-x64` first. The test run itself takes about 10 to 20 seconds.

## Administrator test

`AdminPickerTests` checks that the three file pickers open when the app runs as administrator (#60). It runs only when the test host runs as administrator. In a normal session, it skips. To run it, build in a normal PowerShell window, then run the test from a PowerShell window that runs as administrator:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64 --no-build --filter "FullyQualifiedName~AdminPickerTests"
```

The test host runs as administrator, so the app that it starts also runs as administrator. No UAC prompt shows. During the run, three file dialogs open and close. Do not click in them.

## What you see

App windows open at position 100, 100 with size 1280 x 720 in physical screen pixels and take the focus. The test host is per-monitor DPI aware, so the UI Automation bounds and the screenshots use physical pixels. The sample regions come from element bounds and the window DPI, so the checks follow the display scale. The tests passed at 100% and at 150%. At a higher scale, the app content is larger, so less of it fits in the window. Do not use the mouse or the keyboard during the run. The app has keyboard shortcuts: J, K, L, Space, and the arrow keys seek or play the video. A key press or a click in the app window can make a test fail. Do not use the app window while the tests run. Each test closes its window. A failed test closes it too.

## Output

The tests write these files to `artifacts/ui-tests` in the repository root. Git ignores this folder.

- `opened.png`: the window after the test clip opened.
- `marked.png`: the window after the test marked the start and the end.
- `range-Light.png` and `range-Dark.png`: the window with a marked range in each theme.
- `range-Light.txt` and `range-Dark.txt`: the colors and the contrast ratio that the test measured on the range.

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

Use `app.Page` to find controls, to seek, to mark, and to read the state. Mark a test with `[UiFact]` or `[UiTheory]` and put it in the collection `UI`. Mark a test that must run as administrator with `[AdminUiTheory]`.

## Skips

If the test host runs as administrator, the normal tests skip. If it does not, the administrator test skips. If no interactive desktop exists, all tests skip. A skip is not a pass. A locked screen also skips the tests, because the input desktop is not available then. The tests do not check a remote desktop session. CI does not run these tests.

The tests of the launch option parser, the command line builder, and the range format are plain `[Fact]` and `[Theory]` tests. They run without a desktop. To run only them, use this command:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64 --filter "FullyQualifiedName~LaunchOptionsParserTests|FullyQualifiedName~LaunchArgumentsTests|FullyQualifiedName~RangeStatusFormatTests"
```

## Notes

- The app builds with the runtime identifier `win-x64`. Without it, the app can fail to start from a deep folder path, for example a git worktree.
- The Debug build of the app writes the Flyleaf log to `C:\Flyleaf\log.txt`.
- The contrast ratio of the range in the dark theme is low. Issue #36 tracks this. The tests write the ratio to a file and do not assert on it.
