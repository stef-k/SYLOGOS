#define MyAppName "EFARMOGH SYLOGOY TRITEKNON EVROY"
#define MyAppVersion "2.2.0"
#define MyAppPublisher "Stef Karyotidis"
#define MyAppURL "https://github.com/stef-k/SYLOGOS"
#define MyAppExeName "SYLOGOS.exe"

[Setup]
AppId="{{6945CA7B-9110-43BE-90C1-47EBA573D290}}"
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes

; ✅ License inside publish folder (relative path, safe on all systems)
LicenseFile=bin\Release\publish\LICENSE.txt

PrivilegesRequired=lowest
UninstallDisplayIcon="{app}\{#MyAppExeName}"

; ✅ Output installer to local Output folder
OutputDir=Output
OutputBaseFilename=EFARMOGH_SYLOGOY_TRITEKNON_EVROY_setup

; ✅ Use icon from publish folder
SetupIconFile=bin\Release\publish\logo.ico

Compression=lzma
SolidCompression=yes
WizardStyle=modern

VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName}
VersionInfoProductName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; ✅ Copy everything from publish folder into the install directory
Source: "bin\Release\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; ✅ Launch application after install
Filename: "{app}\{#MyAppExeName}"; \
    Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; \
    Flags: nowait postinstall skipifsilent

; ✅ Offer to open Help PDF
Filename: "{app}\help.pdf"; \
    Description: "Open the Help Manual (PDF)"; \
    Flags: postinstall shellexec skipifsilent unchecked
