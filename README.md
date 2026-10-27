# Clip Chopper

[![Latest release](https://img.shields.io/github/v/release/valery-kirichenko/clip-chopper)](https://github.com/valery-kirichenko/clip-chopper/releases/latest)
[![CodeQL](https://github.com/valery-kirichenko/clip-chopper/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/valery-kirichenko/clip-chopper/actions/workflows/codeql-analysis.yml)

### Clip Chopper allows you to quickly trim multiple videos preserving their original video and audio quality

[Download latest release](https://github.com/valery-kirichenko/clip-chopper/releases/latest)

![main](https://user-images.githubusercontent.com/3050013/121750730-9c2eda00-cb15-11eb-8042-cdc6d01ad8a9.png)

## Build from source

You need Windows, the .NET 10 SDK, and PowerShell. Build the app with this command:

```powershell
dotnet build ClipChopper\Applications\ClipChopper.WinUIApp\ClipChopper.WinUIApp.csproj -c Release -p:Platform=x64
```

To run the UI tests, see `ClipChopper/Tests/ClipChopper.WinUIApp.UiTests/README.md`.

### FFmpeg

The repository does not contain the FFmpeg files. The first build downloads them into `ClipChopper\Applications\ClipChopper.WinUIApp\FFmpeg`. A later build skips the download, because the file `ffmpeg-version.txt` in that folder holds the version that the build needs.

The build runs the script `ClipChopper\Scripts\Get-FFmpeg.ps1`. The script uses this build:

- Vendor: gyan.dev, version 9.0.2, `full_build` with shared libraries.
- URL: <https://github.com/GyanD/codexffmpeg/releases/download/9.0.2/ffmpeg-9.0.2-full_build-shared.zip>
- SHA-256: `8D31E162F1616E37AAB3FA2DB991B97E1B8DBEB1C8465FD81A81BE16FF91B328`

If the SHA-256 of the download is different, the script stops with an error. It copies only `ffmpeg.exe`, `ffprobe.exe`, and the DLL files from the `bin` folder of the archive.

To run the script by hand, use this command from the root of the repository:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ClipChopper\Scripts\Get-FFmpeg.ps1
```

Add `-Force` to download again. Add `-Destination <folder>` to use another folder.

To set up the files offline, download the zip on another computer. Copy `ffmpeg.exe`, `ffprobe.exe`, and all DLL files from the `bin` folder of the zip into the `FFmpeg` folder of the app. Then create the file `ffmpeg-version.txt` in that folder with the text `9.0.2`. The build then makes no download.

### Installer

To build the installer, publish the app first. Then compile `ClipChopper\Applications\ClipChopper.WinUIApp\Installer\Setup.iss` with Inno Setup. The installer takes the FFmpeg files from the `publish\FFmpeg` folder.

### FFmpeg license

This FFmpeg build is licensed under the GPL version 3. The source code and the license are on <https://www.gyan.dev/ffmpeg/builds/> and <https://ffmpeg.org/>.

