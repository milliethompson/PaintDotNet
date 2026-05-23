/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class SetupNgen
	{
        static void Ngen(string name, bool delete)
        {
            if (delete)
            {
                name = Path.ChangeExtension(name, null);
            }

            string ourPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            if (!delete)
            {
                name = Path.Combine(ourPath, name);
            }

            string ngenExe = System.Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v1.1.4322\ngen.exe");
            ProcessStartInfo psi = new ProcessStartInfo(ngenExe, (delete ? "/delete " : "") + "\"" + name + "\"");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process process = Process.Start(psi);
            process.WaitForExit();
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            bool delete = false;

            if (args.Length == 1)
            {
                delete = true;
            }

            string[] names = new string[] {
                                              "DotNetWidgets.dll",
                                              "ICSharpCode.SharpZipLib.dll",
                                              "Interop.WIA.dll",
                                              "PaintDotNet.Data.dll",
                                              "PaintDotNet.exe",
                                              "PaintDotNet.SystemLayer.dll",
                                              "PdnLib.dll",
                                              "Skybound.VisualStyles.dll"
                                          };

            foreach (string name in names)
            {
                try
                {
                    Ngen(name, delete);
                }

                // Since this is essentially an optional part of setup, we don't care
                // if it fails.
                catch
                {
                }
            }
		}
	}
}
