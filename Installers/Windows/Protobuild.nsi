SetCompressor /SOLID /FINAL lzma

!define APPNAME "Protobuild"

;Include Modern UI

!include "Sections.nsh"
!include "MUI2.nsh"
!include "InstallOptions.nsh"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "..\protobuild.bmp"
!define MUI_ABORTWARNING

!define MUI_WELCOMEFINISHPAGE_BITMAP "..\panel.bmp"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_FUNCTION "StartProtobuildManager"
!define MUI_FINISHPAGE_RUN_TEXT "Create or open a new project"

!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

Name 'Protobuild'
OutFile 'ProtobuildWebInstall.exe'

InstallDir '$PROGRAMFILES\${APPNAME}'
VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Protobuild Manager"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Protobuild Developers"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "1.0.0.0"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "1.0.0.0"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Protobuild Manager Installer"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright Protobuild Developers"

RequestExecutionLevel admin

; The stuff to install
Section "All Components" CoreComponents ;No components page, name is not important
  SectionIn RO
  
  SetOutPath '$INSTDIR'
  File '..\..\Protobuild.exe'
  File '..\..\Protobuild.Manager\bin\Windows\AnyCPU\Release\Protobuild.Manager.exe'
  
  ExecWait "Protobuild.Manager.exe --silent-setup"
  
SectionEnd

Function StartProtobuildManager

  SetOutPath '$INSTDIR'
  Exec "Protobuild.Manager.exe"

FunctionEnd