; Inno Setup script for SW Auto Attributes
; Build with Inno Setup 6.x (Unicode)

#define AppName "SW Auto Attributes"
#define AppVersion "20260527"
#define AppFileVersion "2026.5.27.0"
#define AppPublisher "canghai"
#define AppExeName "SWAutoAttributes.dll"
#define AddinGuid "b28bf6d5-6185-bbed-0e53-9b04a8317385"

[Setup]
AppId={{9F7C0E1C-9E3F-4E8F-9A9C-58C28E1A3E33}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName}
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppFileVersion}
VersionInfoVersion={#AppFileVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=SW_Auto_Attributes_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\bin\Release\net48\*.dll"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
Root: HKLM; Subkey: "SOFTWARE\SolidWorks\Addins\{{b28bf6d5-6185-bbed-0e53-9b04a8317385}"; ValueType: dword; ValueName: ""; ValueData: "0"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\SolidWorks\Addins\{{b28bf6d5-6185-bbed-0e53-9b04a8317385}"; ValueType: string; ValueName: "Title"; ValueData: "SW Auto Attributes"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\SolidWorks\Addins\{{b28bf6d5-6185-bbed-0e53-9b04a8317385}"; ValueType: string; ValueName: "Description"; ValueData: "自动按规则写入自定义属性"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\SolidWorks\AddinsStartup\{{b28bf6d5-6185-bbed-0e53-9b04a8317385}"; ValueType: dword; ValueName: ""; ValueData: "1"; Flags: uninsdeletekey

[Run]
Filename: "{win}\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"; Parameters: """{app}\{#AppExeName}"" /codebase /tlb"; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "{win}\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"; Parameters: """{app}\{#AppExeName}"" /unregister"; Flags: runhidden waituntilterminated

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Messages]
WelcomeLabel2=This will install the SolidWorks add-in. SolidWorks must be closed during install.
