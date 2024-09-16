; https://github.com/DomGries/InnoDependencyInstaller


; requires netcorecheck.exe and netcorecheck_x64.exe (see CodeDependencies.iss)
#define public Dependency_Path_NetCoreCheck "dependencies\"

; requires dxwebsetup.exe (see CodeDependencies.iss)
;#define public Dependency_Path_DirectX "dependencies\"

#include "CodeDependencies.iss"

[Setup]
#define MyAppSetupName 'ClipChopper'
#define MyAppVersion '2.0.0'
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
Source: "publish\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion
Source: "UninsIS.dll"; Flags: dontcopy

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\ClipChopper.exe"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\ClipChopper.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"

[Run]
Filename: "{app}\ClipChopper.exe"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: unchecked nowait postinstall skipifsilent

[Code]
function DLLIsISPackageInstalled(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'IsISPackageInstalled@files:UninsIS.dll stdcall setuponly';

function DLLCompareISPackageVersion(AppId, InstallingVersion: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): longint;
  external 'CompareISPackageVersion@files:UninsIS.dll stdcall setuponly';

function DLLUninstallISPackage(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'UninstallISPackage@files:UninsIS.dll stdcall setuponly';

// Wrapper for UninsIS.dll IsISPackageInstalled() function
// Returns true if package is detected as installed, or false otherwise
function IsISPackageInstalled(): Boolean;
begin
  result := DLLIsISPackageInstalled('{#MyAppSetupName}',  // AppId
    DWORD(Is64BitInstallMode()),                   // Is64BitInstallMode
    DWORD(IsAdminInstallMode())) = 1;              // IsAdminInstallMode
  if result then
    Log('UninsIS.dll - Package detected as installed')
  else
    Log('UninsIS.dll - Package not detected as installed');
end;

// Wrapper for UninsIS.dll CompareISPackageVersion() function
// Returns:
// < 0 if version we are installing is < installed version
// 0   if version we are installing is = installed version
// > 0 if version we are installing is > installed version
function CompareISPackageVersion(): LongInt;
begin
  result := DLLCompareISPackageVersion('{#MyAppSetupName}',  // AppId
    '{#MyAppVersion}',                                  // InstallingVersion
    DWORD(Is64BitInstallMode()),                      // Is64BitInstallMode
    DWORD(IsAdminInstallMode()));                     // IsAdminInstallMode
  if result < 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} is older than installed version')
  else if result = 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} is the same as installed version')
  else
    Log('UninsIS.dll - This version {#MyAppVersion} is newer than installed version');
end;

// Wrapper for UninsIS.dll UninstallISPackage() function
// Returns 0 for success, non-zero for failure
function UninstallISPackage(): DWORD;
begin
  result := DLLUninstallISPackage('{#MyAppSetupName}',  // AppId
    DWORD(Is64BitInstallMode()),                 // Is64BitInstallMode
    DWORD(IsAdminInstallMode()));                // IsAdminInstallMode
  if result = 0 then
    Log('UninsIS.dll - Installed package uninstall completed successfully')
  else
    Log('UninsIS.dll - installed package uninstall did not complete successfully');
end;

function IsWindowsAppSDKRuntimeInstalled(const Version: String): Boolean;
var
  ResultCode: Integer;
  ExecOutput: AnsiString;
  ResultString, TempFilename: String;
begin
  TempFilename := ExpandConstant('{tmp}\clipchopper-setup-was-output.txt');
  Exec(ExpandConstant('powershell'), '"(get-appxpackage Microsoft.WindowsAppRuntime*).version" | out-file -encoding ASCII ' + TempFilename, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  LoadStringFromFile(TempFilename, ExecOutput);
  ResultString := String(ExecOutput);
  Log('WAS Temp: ' + TempFilename);
  Log('WAS Check: ' + ResultString);
  Result := pos(Version, ResultString) > 0;
  Log('WAS Check Result: ' + IntToStr(Integer(Result)));
end;

function InitializeSetup: Boolean;
var
  uninstall: Integer;
begin
  if IsISPackageInstalled() then begin
    uninstall := MsgBox('You already have previous version installed. To continue installation, it should be uninstalled first.' #13#13 'Proceed with uninstall?', mbInformation, MB_YESNO);
    if uninstall = IDYES then
      UninstallISPackage()
    else
      Exit;
  end;

  Dependency_AddDotNet80Desktop;
  Dependency_AddVC2015To2022;

  // if not IsWindowsAppSDKRuntimeInstalled('5001.70.1338.0') then begin
    Dependency_Add('windowsappruntimeinstall-x64.exe',
      '--quiet',
      'Windows App SDK Runtime 1.6',
      'https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe',
      '', False, False);
  // end;
  Result := True;
end;
