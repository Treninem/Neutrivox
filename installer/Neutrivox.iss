#define MyAppName "Neutrivox"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Neutrivox"
#define MyAppExeName "Neutrivox.exe"

[Setup]
AppId={{B9B2F3E3-2B0A-4F16-AE5D-5D2E8B9A71C4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Neutrivox
DefaultGroupName=Neutrivox
OutputDir=..\artifacts\installer
OutputBaseFilename=Neutrivox-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
WizardStyle=modern

[Files]
Source: "..\artifacts\windows\Neutrivox.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Neutrivox"; Filename: "{app}\Neutrivox.exe"
Name: "{userdesktop}\Neutrivox"; Filename: "{app}\Neutrivox.exe"

[Run]
Filename: "{app}\Neutrivox.exe"; Description: "Запустить Neutrivox"; Flags: nowait postinstall skipifsilent
