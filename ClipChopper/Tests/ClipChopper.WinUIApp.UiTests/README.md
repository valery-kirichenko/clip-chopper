# ClipChopper.WinUIApp.UiTests

UI automation tests for the WinUI app.

## What the tests do

The tests start the built app and open a generated test clip with the `--open` option. They mark a range, read the state of the main controls through UI Automation, and save screenshots of the window. The library FlaUI (UIA3) drives the app. By default, the app runs on a hidden desktop, so the tests show no window on your desktop.

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

## Hidden mode

By default, the test run starts the app on a separate desktop object. A desktop object is a separate set of windows in the window station of the user. The app window is on a desktop that you do not see and that gets no input. So the app shows no window on your desktop and cannot take your focus. The taskbar lists only the windows of your own desktop, so it does not list the app window. The tests find the window on that desktop, drive it through UI Automation, and take screenshots of it.

With the option `--ui-test`, the app also does four things. It shows its window without activation (`WS_EX_NOACTIVATE`), it sets the volume of its player to 0 before any audio opens, it turns off its volume slider, and it publishes the mute status in the `ItemStatus` of the volume slider. The app refreshes that status when a video finishes opening. The test `NeverTakesForeground` checks the no-activate window style in both modes. The test `KeepsPlayerMutedInTestMode` checks the mute status after the video opens and the state of the volume slider. No automated test measures the sound on the audio device.

To start the app on your own desktop, set the environment variable `CLIPCHOPPER_UITEST_VISIBLE` to `1`. Remove the variable to return to the hidden mode. In PowerShell:

```powershell
$env:CLIPCHOPPER_UITEST_VISIBLE = '1'
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64
Remove-Item Env:CLIPCHOPPER_UITEST_VISIBLE
```

Both modes start the app with the option `--ui-test`. The only difference between the modes is the desktop.

## What you see

In the hidden mode, nothing appears on your desktop. In the visible mode, app windows open at position 100, 100 with size 1280 x 720 in physical screen pixels. The app does not take the focus in either mode. The test host is per-monitor DPI aware, so the UI Automation bounds and the screenshots use physical pixels. The sample regions come from element bounds and the window DPI, so the checks follow the display scale. In the visible mode, the tests passed at 100% and at 150% (issue #68). The hidden mode ran at a display scale of 100% only. No hidden run at 150% exists. At a higher scale, the app content is larger, so less of it fits in the window. In the visible mode, do not use the mouse or the keyboard during the run. The app has keyboard shortcuts: J, K, L, Space, and the arrow keys seek or play the video. A key press or a click in the app window can make a test fail. Do not use the app window while the tests run. Each test closes its window. A failed test closes it too.

## Output

The tests write these files to `artifacts/ui-tests` in the repository root. Git ignores this folder.

- `opened.png`: the window after the test clip opened.
- `marked.png`: the window after the test marked the start and the end.
- `foreground.png`: the window at the end of the focus test.
- `muted.png`: the window at the end of the mute test.
- `range-Light.png` and `range-Dark.png`: the window with a marked range in each theme.
- `range-Light.txt` and `range-Dark.txt`: the colors and the contrast ratio that the test measured on the range.

## Launch options

The app has four options for tests: `--theme Light|Dark`, `--open <path>`, `--expose-range`, and `--ui-test`. The option `--expose-range` makes the app publish the marked range in the `ItemStatus` of the seek slider. `ClipChopperApp` sets it by default. The option `--ui-test` makes the app show its window without activation, set the player volume to 0, and turn off the volume slider. It also makes the app publish `Muted` or `NotMuted` in the `ItemStatus` of the volume slider. `ClipChopperApp` always sets it, in the hidden mode and in the visible mode. The options are off by default and have no effect on a normal start.

The names live in `ClipChopper/Applications/ClipChopper.WinUIApp/CommandLine/LaunchOptionNames.cs`. The app reads the command line once, with `LaunchOptionsParser`, and keeps the result in a `LaunchOptions` record. The parser ignores unknown arguments and bad values. If an option repeats, the last valid value wins.

The `.csproj` of the test project lists four files of that folder and two files of the `Automation` folder (`RangeStatusFormat` and `MuteStatusFormat`) one by one, so the tests use the same names, the same `LaunchTheme` enum, the same range format, and the same mute texts. `LaunchArguments` builds the command line from a `LaunchOptions` record. To add an option, change `LaunchOptions`, `LaunchOptionNames`, `LaunchOptionsParser`, and `LaunchArguments`, and add a parser test.

## Write a new test

Start the app only through `ClipChopperApp`. This class holds the launch arguments, the window position, and the window size.

```csharp
using var app = ClipChopperApp.Launch(new AppLaunchOptions { OpenFile = path, Theme = LaunchTheme.Dark });
app.Page.WaitUntilOpened(TimeSpan.FromSeconds(20));
using var shot = app.Capture("my-screenshot");
```

Use `app.Page` to find controls, to seek, to mark, and to read the state. Mark a test with `[UiFact]` or `[UiTheory]` and put it in the collection `UI`.

Drive the app with UI Automation patterns and property reads only. Do not use `Click()`, `Mouse`, `Keyboard`, `Focus()`, `FocusNative()`, `SetFocus`, `SetActiveWindow`, `SetForeground()`, or the FlaUI `Capture` methods. They move the cursor, send keys, or take the focus of the user. The test `DriverRulesTests.UsesNoSimulatedInput` fails if a `.cs` file of the test project holds one of these texts: `.Click(`, `.DoubleClick(`, `.RightClick(`, `.Focus()`, `.FocusNative(`, `SetFocus`, `SetActiveWindow`, `Mouse.`, `Keyboard.`, `SetForeground`, `SendInput`, `SetCursorPos`, `keybd_event`, `mouse_event`, `FlaUI.Core.Input`, or `FlaUI.Core.Capturing`. It also fails if the `.csproj` file holds `FlaUI.Core.Input` or `FlaUI.Core.Capturing`. The test is a text scan, so it cannot see a call through reflection or through another assembly.

## Skips

A test skips when the test host runs as administrator or when no interactive desktop exists. A skip is not a pass. A locked screen also skips the tests, because the input desktop is not available then. The tests do not check a remote desktop session. CI does not run these tests.

The tests of the launch option parser, the command line builder, the command line text, the range format, and the driver rules are plain `[Fact]` and `[Theory]` tests. They run without a desktop. To run only them, use this command:

```powershell
dotnet test ClipChopper/Tests/ClipChopper.WinUIApp.UiTests -p:Platform=x64 --filter "FullyQualifiedName~LaunchOptionsParserTests|FullyQualifiedName~LaunchArgumentsTests|FullyQualifiedName~RangeStatusFormatTests|FullyQualifiedName~CommandLineTextTests|FullyQualifiedName~DriverRulesTests"
```

## Notes

- The app builds with the runtime identifier `win-x64`. Without it, the app can fail to start from a deep folder path, for example a git worktree.
- The Debug build of the app writes the Flyleaf log to `C:\Flyleaf\log.txt`.
- The contrast ratio of the range in the dark theme is low. Issue #36 tracks this. The tests write the ratio to a file and do not assert on it.
