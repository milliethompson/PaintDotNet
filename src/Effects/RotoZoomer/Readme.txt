RotoZoomer Effect DLL for Paint.NET v1.1
----------------------------------------
This project serves as an example of how to create a Paint.NET Effect
Plugin.

To build the RotoZoomer project without building Paint.NET, you must fix
the references to PdnLib.dll and PaintDotNet.Effects.dll:

1. Open the RotoZoomer.csproj file with Visual Studio .NET 2003
2. In the Solution Explorer, expand the References node. 
   Here you should see 5 references, two of which may yellow triangle 
   exclamation icons by them. 
3. Delete those two references.
4. Right click on References and select "Add Reference ..."
5. Click "Browse ..."
6. Navigate to the directory where you installed Paint.NET.
7. Add both "PdnLib.dll" and "PaintDotNet.Effects.dll"

Now when you bild RotoZoomer, copy its DLL to the "Effects" directory that
is in the directory you installed Paint.NET to. This is commonly located
at "C:\Program Files\Paint.NET v1.1\Effects".