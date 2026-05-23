Paint.NET Source Code Readme

Prerequisites
-------------
1. The source must be located in a directory that does not have spaces in its
   full path name. So extracting and building from your desktop won't work. The
   reason for this requirement is related to a limitation with the help file
   compiler.

   So, for example:

      Works: c:\src\pdn_2_5_src\
      Won't: c:\Documents and Settings\username\Desktop\pdn_2_5_src

2. Windows XP or Windows Server 2003, or newer. You might be fine with Windows
   2000 but this hasn't been tested.

3. Visual Studio .NET 2003 Professional

4. .NET Framework 1.1 with SP1

5. Tablet PC SDK v1.7
   Download and install this from Microsoft:
   http://www.microsoft.com/downloads/details.aspx?FamilyID=b46d4b83-a821-40bc-aa85-c9ee3d6e9699&DisplayLang=en


Instructions
------------
1. Open src/paintdotnet.sln with Microsoft Visual Studio .NET 2003. 

2. Make sure the project configuration is set to "Release and Package."
   This can be done by going to the "Build" menu, selecting "Configuration
   Manager...", selecting "Release and Package" under "Active Solution 
   Configuration:" and then clicking Close.
    
3. Go to the "Build" menu and click "Rebuild Solution."

4. The output files are now in src/Setup/Release:

   * PaintDotNet.msi
     This is the MSI that installs Paint.NET, but you shouldn't launch it
     directly.
     
   * PaintDotNetSetup.exe
     The "real" installer.
     
     Suitable for web-based distribution. This is fairly small and
     installs just Paint.NET. 
            
     Successful installation requires that the following be true:
        1. Windows 2000 or later is installed.
        2. .NET Framework 1.1 or later is installed.
        3. The user has Administrator privileges.
                
   * PaintDotNetFull.exe
     This is the "full" installer that will install .NET 1.1 if it is not 
     already installed. This file is over 20MB larger than PaintDotNetSetup.exe
     but provides a very convenient all-in-one installation package.

For normal development work, use either the 'Release' or 'Debug' 
configurations. This will skip the process of building all the setup packages,
help file, and merge modules.


Code Signing
------------
If you wish to sign your build of Paint.NET using Authenticode, you must set
the following 3 environment variables:

1. SIGNPDN=1
2. PDNSPC=[full path to your SPC certificate file]
3. PDNPVK=[full path to your PVK private key file]

Edit signfile.bat as desired to set the package description and URL, and
timestamp server URL.

signcode.exe must be present in your PATH as well. It comes with Visual Studio.

Note that this is NOT the same thing as .NET's strong naming facility.

Paint.NET is written to only execute update packages that are signed.

You will be prompted MANY times for your private key password during the build
process. This is normal.


Directory Layout
----------------
src/
    The main folder containing all the Paint.NET source code.

src/bin
    This is where the main Paint.NET executable and DLLs will be placed.
    When you build PDN, you should be able to run PaintDotNet.exe from this
    directory.

src/Data
    Contains all data-related code, including loading and saving of images.

src/dotnetwidgets
    Contains the DLL for DotNetWidgets which we use to provide a "Office XP"
    style user interface.

src/Effects
    Contains the code that is built for the PaintDotNet.Effects.dll. This is
    the Effects subsystem of Paint.NET that plugins will have to reference.

src/Help
    Contains all the help files that are compiled into PaintDotNet.chm.

src/makechm
    A quick program that is used for compiling the help file. The reason we
    have this is that hhc.exe (the help compiler) returns 1 on success, but
    the Visual Studio build environment requires tools to return 0 on success
    (which is normal!). So we bootstrap the hhc.exe and force it to return 0.

src/obj
    Intermediate files used during compilation go here.

src/PdnLib
    Contains the Paint.NET "library." This is code that is plausibly usable 
    either outside of Paint.NET or required for plugins to link against.

src/Resources
    Contains all the resources for Paint.NET, and some code for managing them.

src/Setup
    Contains a project that is used to build PaintDotNet.msi. Note that the
    MSI file is not complete until the "Setup-Config" project has finished!

src/Setup-Config
    This is the final stage of the build process. It modifies PaintDotNet.msi
    using a VBS script so that it defaults to "Install for Everyone" instead
    of "Install for Just Me." It then packages together PaintDotNet.msi with
    dotnetfx.exe using NSIS (Nullsoft Scriptable Installation System).

src/SetupFrontEnd
    Contains our front-end to the setup MSI. This was written so that we can
    localize (translate) the setup wizard, and also so that the installation
    options can be preserved when updating or reinstalling.

src/SetupNgen
    This is a program that is run as part of install and uninstall that "pre-
    JITs" our DLLs.

src/SharpZipLib
    Contains the DLL for #ziplib, by Mike Krueger.

src/ShellExtension
    Contains the code for a Windows Explorer shell extension that displays
    thumbnails. This is a COM object written in C++.

src/Skybound.VisualStyles
    Contains the DLL for the Skybound VisualStyles component.

src/Strings
    Contains the strings.resx for English/neutral locales.
    
src/Strings.de
    Contains the strings.resx for German locales.

src/SystemLayer
    All P/Invoke and "system dependent" code, as well as hacks or workarounds,
    go in to the SystemLayer assembly.

src/tools
    Contains various tools necessary for building Paint.NET.

src/WIAAutSDK
    Contains the WIA 2.0 Automation library.

