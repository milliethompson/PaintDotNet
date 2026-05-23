using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Paint.NET v1.1")]
[assembly: AssemblyDescription("Successor to Microsoft Paint that uses .NET, C#, and GDI+ technologies.")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Paint.NET v1.1")]
[assembly: AssemblyCopyright("Copyright © 2004 Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, and Luke Walker")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.1.*")]

// Change this to say "Final" for final builds. Otherwise the titlebar will contain
// a long version string. Final versions should just say the ApplicationProduct
// attribute (i.e., "Paint.NET" instead of "Paint.NET (Beta 2 build: 1.0.*.*)"
// Use this to hold the current milestone title, such as "Milestone 2" or "Beta 3"
[assembly: AssemblyConfiguration("Final")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
// can't sign the exe because we can't sign Interop.WIA.dll
[assembly: AssemblyKeyFile(/*@"..\..\paintdotnet.keys"*/ "")]
[assembly: AssemblyKeyName("")]
