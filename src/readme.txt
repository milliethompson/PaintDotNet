Paint.NET Source Code Readme

Prerequisites
-------------
1. Windows XP, Server 2003, or Vista.

2. Visual Studio .NET 2005 with Service Pack 1
   You should install the C++ x64 compiler as well, which may not come installed
   by default. 
   Not tested with any Express editions of Visual Studio, or with Visual Studio
   2008. You're completely on your own in those cases.

3. .NET Framework 2.0

4. Tablet PC SDK v1.7
   Download and install this from Microsoft:
   http://www.microsoft.com/downloads/details.aspx?FamilyID=b46d4b83-a821-40bc-aa85-c9ee3d6e9699&DisplayLang=en


Instructions
------------
1. Open src/paintdotnet.sln with Microsoft Visual Studio .NET 2005. 

2. Set the project configuration to Release or Debug
    
3. Go to the "Build" menu and click "Rebuild Solution."

4. You will get a number of compiler errors for src/Resources/InvariantStrings.cs.
   Go to that file and inspect its contents, and follow the instructions in
   order to make the file compilable.

5. Once that's done, go back to the Build menu and click Rebuild Solution again.

6. Assuming all went well, the program files are now in src/bin/[Debug|Release].

You should make sure that the /skipRepairAttempt command line parameter is 
present in the Debug tab of the 'paintdotnet' project's properties. Otherwise, 
Paint.NET will see that some files are missing and attempt to repair itself.
Not all of these files are necessary when doing development or debugging.

Also, you will need to make sure that mt.exe and signtool.exe are in a
directory that is in your PATH. These are available as part of the Windows SDK
which can be found at Microsoft's website. Usually it's sufficient to copy
these to %SYSTEMROOT%, which is usually C:\Windows.


Directory Layout
----------------
src/
    The main folder containing all the Paint.NET source code.

src/Base
    This assembly houses base framework-style code. This assembly was introduced
    because there was code in Core that SystemLayer could not access and thus
    had to duplicate.

src/bin
    This is where the main Paint.NET executable and DLLs will be placed.
    When you build PDN, you should be able to run PaintDotNet.exe from this
    directory.

src/BuildTools
    Some exe's that are used by the build process.

src/Core

src/Data
    Contains all data-related code, including loading and saving of images.

src/Effects
    Contains the code that is built for the PaintDotNet.Effects.dll. This is
    the Effects subsystem of Paint.NET that plugins will have to reference.
    
src/GeneratedCode
    Contains a project with build events that generate code for other projects
    in the paintdotnet solution. Currently it only generates the user blend
    ops in the Data project.

src/Interop.WIA
    Contains the .NET interop DLL for the Windows Image Acquisition (WIA)
    Automation Layer.

src/Manifests
    Contains XML manifests that are embedded in some of the EXE's that are
    built. These are used in Windows Vista to mark the executable as either
    requiring administrator privilege, or to run with the privilege that
    the calling process has. The latter is important for the installer so
    that the executable is not deemed 'legacy', and thus certain
    compatibility modes are bypassed.

src/obj
    Intermediate files used during compilation go here.

src/Resources
    Contains all the resources for Paint.NET, and some code for managing them.

src/SharpZipLib
    Contains the DLL for #ziplib, by Mike Krueger.

src/ShellExtension
    Contains the code for a Windows Explorer shell extension that displays
    thumbnails. This is a COM object written in C++.

src/Strings
    Contains the strings.resx for English/neutral locales.
    
src/StylusReader
    Contains the code for interfacing with the Tablet PC SDK.

src/SystemLayer
    All P/Invoke and "system dependent" code, as well as hacks or workarounds,
    go in to the SystemLayer assembly.

src/WIAAutSDK
    Contains the WIA 2.0 Automation library.

src/WIAProxy32
    Contains the code for the WIA proxy executable.