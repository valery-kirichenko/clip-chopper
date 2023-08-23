; https://github.com/DomGries/InnoDependencyInstaller


; requires netcorecheck.exe and netcorecheck_x64.exe (see CodeDependencies.iss)
#define public Dependency_Path_NetCoreCheck "dependencies\"

; requires dxwebsetup.exe (see CodeDependencies.iss)
;#define public Dependency_Path_DirectX "dependencies\"

#include "CodeDependencies.iss"

[Setup]
#define MyAppSetupName 'ClipChopper'
#define MyAppVersion '2.0'
#define MyAppPublisher 'Valery Kirichenko'
#define MyAppURL 'https://github.com/valery-kirichenko/clip-chopper'

AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={autopf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\ClipChopper.exe
OutputDir={#SourcePath}\bin
AllowNoIcons=yes
PrivilegesRequired=admin
MinVersion=10.0.17763
SetupIconFile=../Assets/icon.ico
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\FFmpeg\*"; DestDir: "{app}\FFmpeg"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\ClipChopper.exe"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\ClipChopper.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"

[Run]
Filename: "{app}\ClipChopper.exe"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: unchecked nowait postinstall skipifsilent

[Code]
function IsWindowsAppSDKRuntimeInstalled(const Version: String): Boolean;
var
  ResultCode: Integer;
  ExecOutput: AnsiString;
  ResultString, TempFilename: String;
begin
  TempFilename := ExpandConstant('{tmp}\clipchopper-setup-was-output.txt');
  Exec(ExpandConstant('powershell'), '"(get-appxpackage *appruntime*).name" | out-file -encoding ASCII ' + TempFilename, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  LoadStringFromFile(TempFilename, ExecOutput);
  ResultString := String(ExecOutput);
  Log('WAS Temp: ' + TempFilename);
  Log('WAS Check: ' + ResultString);
  Result := pos('Microsoft.WindowsAppRuntime.' + Version, ResultString) > 0;
  Log('WAS Check Result: ' + IntToStr(Integer(Result)));
end;

function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet70Desktop;
  Dependency_AddVC2015To2022;

  if not IsWindowsAppSDKRuntimeInstalled('1.4-preview2') then begin
    Dependency_Add('windowsappruntimeinstall-x64.exe',
      '--quiet',
      'Windows App SDK Runtime 1.4 Preview 2',
      'https://aka.ms/windowsappsdk/1.4/1.4.230811000-preview2/windowsappruntimeinstall-x64.exe',
      '', False, False);
  end;
  Result := True;
end;
