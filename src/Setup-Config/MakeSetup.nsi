;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Paint.NET
;; Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
;;               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
;;               and Luke Walker
;; Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
;; See src;setup;License.rtf for complete licensing and attribution information.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

; MakeSetup.nsi
;
; Copies files necessary to do a Paint.NET installation.
; After the PDN setup completes, the temp files will be deleted.

;--------------------------------

!ifdef Compress
  SetCompressor /SOLID lzma
  !ifdef FullInstaller
    SetCompressorDictSize 32
  !else
    SetCompressorDictSize 8
  !endif
!else
  SetCompress off
!endif

; The name of the installer
Name "Paint.NET SFX"

; The default installation directory
InstallDir $TEMP\PdnSetup

!ifdef FullInstaller
!else
  SilentInstall silent
!endif

Icon ..\Setup\SetupIcon.ico

VIAddVersionKey ProductName "Paint.NET Setup"
VIAddVersionKey ProductVersion "2.72.0.0"
VIAddVersionKey FileVersion "2.72.0.0"
VIAddVersionKey LegalCopyright "Copyright © 2006 Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, and Luke Walker. Portions Copyright © 2006 Microsoft Corporation. All Rights Reserved."
VIAddVersionKey FileDescription "Installs Paint.NET."
VIProductVersion "2.72.0.0"

; The file to write
!ifdef Debug

!ifdef FullInstaller
  OutFile "..\Setup\Debug\PaintDotNetFullSetup.exe"
!else
  OutFile "..\Setup\Debug\PaintDotNetSetup.exe"
!endif

!else

!ifdef FullInstaller
  OutFile "..\Setup\Release\PaintDotNetFullSetup.exe"
!else
  OutFile "..\Setup\Release\PaintDotNetSetup.exe"
!endif 

!endif

;--------------------------------

; Pages

;Page directory
;Page instfiles

Var "Args"

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important
    
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Command-line parameters
  Call GetParameters
  Pop $Args
  
  ; Put file there
  File ..\Setup\Release\PaintDotNet.msi
  File ..\SetupFrontEnd\bin\Release\PaintDotNet.Resources.dll
  File ..\SetupFrontEnd\bin\Release\PaintDotNet.SystemLayer.dll
  File ..\SetupFrontEnd\bin\Release\PaintDotNet.Strings.*.resources
  File ..\SetupFrontEnd\bin\Release\PaintDotNet.Strings.resources
  File ..\SetupFrontEnd\bin\Release\SetupFrontEnd.exe
  File ..\SetupShim\Release\SetupShim.exe

!ifdef FullInstaller
  ; Ordering these files is important so that files that are similar
  ; are next to each other and can be compressed together via the
  ; solid archive compression.
  File /r /x CVS ..\..\programs\dotnet_2_0\*.ini
  File /r /x CVS ..\..\programs\dotnet_2_0\*.txt
  File /r /x CVS ..\..\programs\dotnet_2_0\*.dll
  File /r /x CVS ..\..\programs\dotnet_2_0\*.exe
  File /r /x CVS ..\..\programs\dotnet_2_0\*.bmp
  File /r /x CVS ..\..\programs\dotnet_2_0\*.msi
  File /r /x CVS ..\..\programs\dotnet_2_0\*.cab
!endif
  
!ifdef FullInstaller
  SetAutoClose true
  HideWindow
!endif

  ExecWait "SetupShim.exe $Args"

  Delete $INSTDIR\SetupShim.exe
  Delete $INSTDIR\SetupFrontEnd.exe
  Delete $INSTDIR\PaintDotNet.Strings.resources
  Delete $INSTDIR\PaintDotNet.Strings.*.resources
  Delete $INSTDIR\PaintDotNet.Resources.dll
  Delete $INSTDIR\PaintDotNet.SystemLayer.dll
  Delete $INSTDIR\PaintDotNet.msi
  
  RMDir /r $INSTDIR
  
SectionEnd

; GetParameters
; input, none
; output, top of stack (replaces, with e.g. whatever)
; modifies no other variables.
Function GetParameters

    Push $R0
    Push $R1
    Push $R2
    Push $R3

    StrCpy $R2 1
    StrLen $R3 $CMDLINE

    ;Check for quote or space
    StrCpy $R0 $CMDLINE $R2
    StrCmp $R0 '"' 0 +3
        StrCpy $R1 '"'
        Goto loop
    StrCpy $R1 " "

    loop:
        IntOp $R2 $R2 + 1
        StrCpy $R0 $CMDLINE 1 $R2
        StrCmp $R0 $R1 get
        StrCmp $R2 $R3 get
        Goto loop

    get:
        IntOp $R2 $R2 + 1
        StrCpy $R0 $CMDLINE 1 $R2
        StrCmp $R0 " " get
        StrCpy $R0 $CMDLINE "" $R2

    Pop $R3
    Pop $R2
    Pop $R1
    Exch $R0

FunctionEnd
