# CLAUDE.md

This file gives Claude Code the facts about this repository.

## Project overview

Clip Chopper trims videos and keeps the original video and audio quality.

The main app is the WinUI 3 app `ClipChopper.WinUIApp`. Its output assembly is `ClipChopper`. The libraries are `ClipChopper.Common`, `ClipChopper.Configuration`, `ClipChopper.Core`, `ClipChopper.Logging`, and `ClipChopper.Models`. The WinUI app has no project reference to the libraries.

## Repo layout

The solution is `ClipChopper/ClipChopper.sln`. All paths below start at the repository root.

| Path                                                           | What lives there                                                     |
|----------------------------------------------------------------|----------------------------------------------------------------------|
| `ClipChopper/Applications/`                                    | The WinUI 3 app `ClipChopper.WinUIApp`                               |
| `ClipChopper/Libraries/`                                       | The five libraries                                                   |
| `ClipChopper/Applications/ClipChopper.WinUIApp/Installer/`     | The Inno Setup scripts `Setup.iss` and `CodeDependencies.iss`        |
| `ClipChopper/Scripts/Get-FFmpeg.ps1`                           | The script that downloads FFmpeg                                     |
| `ClipChopper/Directory.Build.props`                            | The target frameworks and the library package versions               |
| `ClipChopper/NuGet.Config`                                     | The package source, `nuget.org` only                                 |
| `.github/workflows/`                                           | The CI workflow                                                      |

## Build and test

Run these commands at the repository root in PowerShell:

```powershell
dotnet restore ClipChopper/ClipChopper.sln
dotnet build ClipChopper/ClipChopper.sln
dotnet build ClipChopper/ClipChopper.sln -c Release -p:Platform=x64
dotnet build ClipChopper/Applications/ClipChopper.WinUIApp/ClipChopper.WinUIApp.csproj -c Release -p:Platform=x64
```

- The solution builds on Windows only. The WinUI app targets `net10.0-windows10.0.22621.0`, and its build calls `powershell.exe`.
- The projects declare the `x64` platform only. The solution lists the platforms `ARM64`, `x64`, and `x86`. It maps the WinUI app to the platform of the same name, and it maps the libraries to `x64`.
- The plain solution build takes the first solution platform, `ARM64`, and makes an ARM64 WinUI app. The `-c Release -p:Platform=x64` form builds `x64`. It matches CI and the installer.
- There is no `global.json`. CI uses the SDK `10.0.x`.
- Build the libraries through the solution. Their project references use `$(SolutionDir)`, which is empty in a single-project build. MSBuild then drops the references, so some libraries (Logging, Configuration, Core) fail to compile and the others build without their references.
- The solution has no test project. Before you call a task done, run `dotnet build ClipChopper/ClipChopper.sln -c Release -p:Platform=x64`. CI runs the same command.
- To build the installer, publish the app to `ClipChopper/Applications/ClipChopper.WinUIApp/Installer/publish`. Then compile `Setup.iss` with Inno Setup. The installer goes to `Installer/bin`.

## FFmpeg

- `ClipChopper/Scripts/Get-FFmpeg.ps1` downloads the gyan.dev shared build of FFmpeg 9.0.2. It puts the files in `ClipChopper/Applications/ClipChopper.WinUIApp/FFmpeg/`.
- If `FFmpeg/ffmpeg-version.txt` does not hold the pinned version, the build target `DownloadFFmpeg` runs the script.
- If the SHA-256 of the zip file does not match the pinned value, the script stops.
- The FFmpeg binaries are not in git. The `.gitignore` rule for the `FFmpeg/` folder ignores them.
- To run the script by hand, use `powershell.exe -NoProfile -ExecutionPolicy Bypass -File ClipChopper/Scripts/Get-FFmpeg.ps1 -Destination ClipChopper/Applications/ClipChopper.WinUIApp/FFmpeg`. Add `-Force` to download again.
- Always pass `-Destination` when you run the script by hand. In Windows PowerShell 5.1, the default value of `-Destination` fails because `$PSScriptRoot` is empty there. The build passes `-Destination` itself.
- The FFmpeg version is in five places. Change all of them together: `Get-FFmpeg.ps1` (the version and the SHA-256), the MSBuild property `FFmpegVersion` in the WinUI project file, the cache key in the CI workflow, `README.md`, and this file.

## Stack and versions

- The libraries target `net10.0`. The WinUI app targets `net10.0-windows10.0.22621.0` and supports Windows 10 version 10.0.17763.0 and later.
- The libraries set `LangVersion` to 10.0. The WinUI app sets none, so it uses the default of the SDK.
- `Nullable` is on in all projects. `TreatWarningsAsErrors` is on in the libraries and off in the WinUI app. Fix each warning that your change adds. Do not suppress it.
- The WinUI app is unpackaged (`WindowsPackageType` is `None`).
- The WinUI app takes its package versions from its own project file: `Microsoft.WindowsAppSDK` 2.5.1, `FlyleafLib` 3.11.11, `FlyleafLib.Controls.WinUI` 1.4.11, `FFMpegCore` 5.5.0, `CommunityToolkit.WinUI.*` 8.2.250402, and `MediaInfo.Wrapper.Core` 21.9.3.
- The libraries take their package versions from properties in `ClipChopper/Directory.Build.props`: `NLog` 4.7.15, `Acolyte.NET` 2.2.0, `NExifTool` 0.11.0, `Microsoft.Extensions.Configuration*` 10.0.12, `Newtonsoft.Json` 13.0.1, and `MinVer` 3.1.0. To change one of them, edit the property, not the project file.

## Branch and PR model

- `master` is the only long-lived branch. Pull requests target `master`.
- Branch names start with `vvasilyev/`, for example `vvasilyev/tasks/55-claude-folder`.
- Issue titles and PR titles use the Conventional Commits form. The prefix sets the `type:` label:

  | Prefix             | `type:` label                                      |
  |--------------------|----------------------------------------------------|
  | `feat`             | `Feature`                                          |
  | `fix`              | `Bug`, or `Security` for a security defect         |
  | `refactor`, `test` | `Code Maintenance`                                 |
  | `docs`             | `Documentation`                                    |
  | `perf`             | `Performance`                                      |
  | `build`, `ci`      | `Infrastructure`                                   |
  | `chore`            | `Code Maintenance` or `Infrastructure`, by content |
  | `epic(vX.Y):`      | `Epic`                                             |
  | `meta:`            | `Meta`                                             |
  | `question(scope):` | `Discussion`                                       |

- An issue has one `type:` label, one or two `area:` labels, one `priority:` label, and zero or one `status:` label.
- A PR has one `type:` label, the `area:` labels of its issues, and the milestone of its issues. A PR has no `priority:` label and no `status:` label.
- The `area:` labels are Player, Save, Compression, Advanced Save, Core Library, Tests, Infrastructure, CLI, and Documentation.
- The PR body closes each issue with `Closes #N`. The PR template is `.github/pull_request_template.md`.

## CI

The repository has one workflow, `.github/workflows/codeql-analysis.yml`. It runs on pushes and pull requests to `master` and each Sunday at 03:00 UTC. It runs on `windows-2025`, restores the FFmpeg cache, and builds with `dotnet build ClipChopper/ClipChopper.sln -c Release -p:Platform=x64`. It has no test step and no format check.

## Things not to do

- Do not commit `ClipChopper/Applications/ClipChopper.WinUIApp/FFmpeg/`, `bin/`, `obj/`, `.vs/`, or `ClipChopper/Applications/ClipChopper.WinUIApp/Installer/publish/`.
- Do not commit `.claude/settings.local.json`. It holds personal permissions.
- Do not change the FFmpeg version in one place only. See the FFmpeg section.
- Do not add a platform to the projects. They declare `x64` only.
