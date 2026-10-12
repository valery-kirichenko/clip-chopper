<#
.SYNOPSIS
Downloads the pinned FFmpeg build and extracts the files that ClipChopper needs.

.DESCRIPTION
Downloads the gyan.dev "full_build shared" FFmpeg zip from the GitHub release.
Makes sure that its SHA-256 matches the pinned value. Copies ffmpeg.exe,
ffprobe.exe and all DLLs from the "bin" folder of the zip into the destination
folder. A marker file with the version shows that the folder is current. If the
marker shows the pinned version, a second run does nothing.

.PARAMETER Destination
Target folder. Default: Applications\ClipChopper.WinUIApp\FFmpeg.

.PARAMETER Force
Download again, even when the marker file shows the pinned version.

.EXAMPLE
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ClipChopper\Scripts\Get-FFmpeg.ps1
#>
[CmdletBinding()]
param(
    [string] $Destination = (Join-Path $PSScriptRoot '..\Applications\ClipChopper.WinUIApp\FFmpeg'),
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$version = '9.0.2'
$archiveName = "ffmpeg-$version-full_build-shared.zip"
$url = "https://github.com/GyanD/codexffmpeg/releases/download/$version/$archiveName"
$expectedSha256 = '8D31E162F1616E37AAB3FA2DB991B97E1B8DBEB1C8465FD81A81BE16FF91B328'
$markerName = 'ffmpeg-version.txt'

$Destination = [System.IO.Path]::GetFullPath($Destination)
$markerPath = Join-Path $Destination $markerName

if (-not $Force -and (Test-Path -LiteralPath $markerPath) -and
    ((Get-Content -LiteralPath $markerPath -Raw).Trim() -eq $version)) {
    Write-Host "FFmpeg $version is already in '$Destination'. Nothing to do."
    return
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("ffmpeg-download-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

    $archivePath = Join-Path $tempRoot $archiveName
    Write-Host "Downloading $url"
    Invoke-WebRequest -Uri $url -OutFile $archivePath -UseBasicParsing

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $archiveStream = [System.IO.File]::OpenRead($archivePath)
    try {
        $actualSha256 = [System.BitConverter]::ToString($sha256.ComputeHash($archiveStream)).Replace('-', '')
    }
    finally {
        $archiveStream.Dispose()
        $sha256.Dispose()
    }
    if ($actualSha256 -ne $expectedSha256) {
        throw "SHA-256 mismatch for $archiveName. Expected $expectedSha256 but got $actualSha256. The file was not used."
    }
    Write-Host "SHA-256 matches: $actualSha256"

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $wanted = @($zip.Entries | Where-Object {
            $_.FullName -match '^[^/]+/bin/(ffmpeg\.exe|ffprobe\.exe|[^/]+\.dll)$'
        })
        if ($wanted.Count -eq 0) {
            throw "No files found in 'bin' inside $archiveName."
        }

        New-Item -ItemType Directory -Force -Path $Destination | Out-Null
        Get-ChildItem -LiteralPath $Destination -File -Force |
            Where-Object { $_.Extension -in '.dll', '.exe' -or $_.Name -eq $markerName } |
            Remove-Item -Force

        foreach ($entry in $wanted) {
            $target = Join-Path $Destination $entry.Name
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
        }
        Write-Host "Extracted $($wanted.Count) files to '$Destination'."
    }
    finally {
        $zip.Dispose()
    }

    Set-Content -LiteralPath $markerPath -Value $version -Encoding ASCII
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
