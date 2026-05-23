Paint.NET Source Code Readme

Prerequisites
-------------
1. The source must be located in a directory that does not have spaces in its
   full path name. So extracting and building from your desktop won't work. The
   reason for this requirement is related to a limitation with the help file
   compiler.

   So, for example:

      Works: c:\src\pdn_2_1_src\
      Won't: c:\Documents and Settings\username\Desktop\pdn_2_1_src

2. Windows XP or Windows Server 2003, or newer. You might be fine with Windows
   2000 but this hasn't been tested.

3. Visual Studio .NET 2003 Professional

4. .NET Framework 1.1 with SP1
   Install .NET 1.1 using programs/dotnetfx.exe, and then apply SP1 with the
   appropriate executable from the programs/dotnet_1_1_sp1 directory.

5. Tablet PC SDK v1.7
   Install this from the programs directory, or download from Microsoft:
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
     Suitable for web-based distribution. This is fairly small and
     installs just Paint.NET. 
            
     Successful installation requires that the following be true:
        1. Windows XP SP1 or later is installed.
        2. .NET Framework 1.1 or later is installed.
        3. The user has Administrator priviledges.
                
   * PaintDotNetFull.exe
     This is the "full" installer that will install .NET 1.1 if it is not 
     already installed. This file is over 20MB larger than PaintDotNet.msi,
     but provides a very convenient "dummy-proof" installation.

For normal development work, use either the 'Release' or 'Debug' 
configurations. This will skip the process of building all the setup packages,
help file, and merge modules.


Directory Layout
----------------
src/
    The main folder containing all the Paint.NET source code.

src/bin
    This is where the main Paint.NET executable and DLLs will be placed.
    When you build PDN, you should be able to run PaintDotNet.exe from this
    directory, as all dependencies are in that directory.

src/CpuCount
    A small DLL written in C that allows us to detect the number of physical
    processors present in a system. The reason for having this is that a
    Pentium 4 (or Xeon) with HyperThreading normally shows up as having twice
    as many CPUs as it actually has. So a dual-Xeon shows up as having four
    "logical" CPUs but only two "physical" CPUs. We use this number to 
    optimize rendering by using an appropriate number of threads.

src/Cursors
    Contains all the *.cur files used by Paint.NET, mostly for tools.

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

src/icons
    Contains all the *.ico and *.bmp icons that are used throughout the 
    program.

src/makechm
    A quick program that is used for compiling the help file. The reason we
    have this is that hhc.exe (the help compiler) returns 1 on success, but
    the Visual Studio build environment requires tools to return 0 on success
    (which is normal!). So we bootstrap the hhc.exe and force it to return 0.

src/obj
    Intermediate files used during compilation go here.

src/PdnLib
    Contains the Paint.NET "library." This is code that is plausibly usable 
    either outside of Paint.NET or required for Effects to link against.

src/Setup
    Contains a project that is used to build PaintDotNet.msi. Note that the
    MSI file is not complete until the "Setup-Config" project has finished!

src/Setup-Config
    This is the final stage of the build process. It modifies PaintDotNet.msi
    using a VBS script so that it defaults to "Install for Everyone" instead
    of "Install for Just Me." It then packages together PaintDotNet.msi with
    dotnetfx.exe using NSIS (Nullsoft Scriptable Installation System).

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

src/SystemLayer
    All P/Invoke and "system dependent" code goes in to the SystemLayer
    assembly.

src/tools
    Contains various tools necessary for building Paint.NET.

src/WIAAutSDK
    Contains the WIA 2.0 Automation library.

