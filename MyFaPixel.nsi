; Script generated with the Venis Install Wizard

; Define your application name
!define APPNAME "MyFaPixel"
!define APPNAMEANDVERSION "MyFaPixel 1.0"

; Main Install settings
Name "${APPNAMEANDVERSION}"
InstallDir "$PROGRAMFILES\MyFaPixel"
InstallDirRegKey HKLM "Software\${APPNAME}" ""
OutFile "MyFaPixelSetup.exe"

; Modern interface settings
!include "MUI.nsh"

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Set languages (first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Estonian"
!insertmacro MUI_LANGUAGE "Latvian"
!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_RESERVEFILE_LANGDLL

Section "MyFaPixel" Section1

	; Set Section properties
	SetOverwrite on

	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\"
	File "Main Program\bin\Release\ClassLibrary.dll"
	File "Main Program\bin\Release\ControlLibrary.dll"
	File "Main Program\bin\Release\FontEditor.exe"
	File "Main Program\bin\Release\MainProgram.exe"
	File "Main Program\bin\Release\MessageEditor.exe"
	File "Main Program\bin\Release\Window Library.dll"
	CreateDirectory "$SMPROGRAMS\MyFaPixel"
	CreateShortCut "$SMPROGRAMS\MyFaPixel\Uninstall.lnk" "$INSTDIR\uninstall.exe"
	CreateShortCut "$DESKTOP\MyFaPixel Integrator.lnk" "$INSTDIR\MainProgram.exe"
	CreateShortCut "$DESKTOP\MyFaPixel Font Editor.lnk" "$INSTDIR\FontEditor.exe"
	CreateShortCut "$DESKTOP\MyFaPixel Message Editor.lnk" "$INSTDIR\MessageEditor.exe"
	CreateShortCut "$SMPROGRAMS\MyFaPixel\MyFaPixel Integrator.lnk" "$INSTDIR\MainProgram.exe"
	CreateShortCut "$SMPROGRAMS\MyFaPixel\MyFaPixel Font Editor.lnk" "$INSTDIR\FontEditor.exe"
	CreateShortCut "$SMPROGRAMS\MyFaPixel\MyFaPixel Message Editor.lnk" "$INSTDIR\MessageEditor.exe"
	CreateDirectory "$DOCUMENTS\MyFaPixel"
	CreateDirectory "$DOCUMENTS\MyFaPixel\Fonts"
	CreateDirectory "$DOCUMENTS\MyFaPixel\Message Collections"

SectionEnd

Section -FinishSection

	WriteRegStr HKLM "Software\${APPNAME}" "" "$INSTDIR"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteUninstaller "$INSTDIR\uninstall.exe"

SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${Section1} "Program Files"
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;Uninstall section
Section Uninstall

	;Remove from registry...
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
	DeleteRegKey HKLM "SOFTWARE\${APPNAME}"

	; Delete self
	Delete "$INSTDIR\uninstall.exe"

	; Delete Shortcuts
	Delete "$SMPROGRAMS\MyFaPixel\Uninstall.lnk"
	Delete "$DESKTOP\MyFaPixel Integrator.lnk"
	Delete "$DESKTOP\MyFaPixel Font Editor.lnk"
	Delete "$DESKTOP\MyFaPixel Message Editor.lnk"
	Delete "$SMPROGRAMS\MyFaPixel\MyFaPixel Integrator.lnk"
	Delete "$SMPROGRAMS\MyFaPixel\MyFaPixel Font Editor.lnk"
	Delete "$SMPROGRAMS\MyFaPixel\MyFaPixel Message Editor.lnk"

	; Clean up MyFaPixel
	Delete "$INSTDIR\ClassLibrary.dll"
	Delete "$INSTDIR\ControlLibrary.dll"
	Delete "$INSTDIR\FontEditor.exe"
	Delete "$INSTDIR\MainProgram.exe"
	Delete "$INSTDIR\MessageEditor.exe"
	Delete "$INSTDIR\Window Library.dll"

	; Remove remaining directories
	RMDir "$SMPROGRAMS\MyFaPixel"
	RMDir "$INSTDIR\"

SectionEnd

; On initialization
Function .onInit

	!insertmacro MUI_LANGDLL_DISPLAY

FunctionEnd

BrandingText "MyFaJoArCo"

; eof