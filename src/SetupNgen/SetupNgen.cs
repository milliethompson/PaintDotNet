/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using IWshRuntimeLibrary;
using PaintDotNet;
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
        static void InstallAssembly(string name, bool delete)
        {
            string ourPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            // ngen it
            if (delete)
            {
                name = Path.GetFileName(name);
                name = Path.ChangeExtension(name, null);
            }
            else
            {
                name = Path.Combine(ourPath, name);
            }

            string fxPath = @"%WINDIR%\Microsoft.NET\Framework\v1.1.4322\";

            string ngenExe = System.Environment.ExpandEnvironmentVariables(Path.Combine(fxPath, "ngen.exe"));
            ProcessStartInfo psi1 = new ProcessStartInfo(ngenExe, (delete ? "/delete " : "") + "\"" + name + "\"");
            psi1.UseShellExecute = false;
            psi1.CreateNoWindow = true;
            Console.WriteLine("ngen: " + name);
            Process process1 = Process.Start(psi1);
            process1.WaitForExit();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            //System.Windows.Forms.MessageBox.Show(Environment.CommandLine);

            // Syntax:
            //     SetupNgen </cleanUpStaging | </install | /delete> DESKTOPSHORTCUT=<0|1> PDNUPDATING=<0|1> SKIPCLEANUP=<0|1>>

            if (args.Length >= 2 && args[0] == "/cleanUpStaging")
            {
                // SetupNgen.exe is overloaded for cleaning up the "Staging" directory
                string stagingPath = args[1];

                // Sanity check: staging directory must ALWAYS have the word Staging in it (capitalized that way too)
                if (-1 != stagingPath.IndexOf("Staging"))
                {
                    foreach (string filePath in Directory.GetFiles(stagingPath, "*.msi"))
                    {
                        try
                        {
                            Console.WriteLine("delete: " + filePath);
                            System.IO.File.Delete(filePath);
                        }

                        catch
                        {
                        }
                    }

                    try
                    {
                        Console.WriteLine("rmdir: " + stagingPath);
                        Directory.Delete(stagingPath);
                    }

                    catch
                    {
                    }
                }
            }
            else
            {
                bool delete = false;

                if (args.Length < 4)
                {
                    return;
                }

                if (args[0] == "/delete")
                {
                    delete = true;
                }

                // otherwise we assume args[0] == "/install"

                // Pre-JIT and install to GAC
                string[] names1 = new string[] {
                                                   "Effects\\RotateZoom.dll",
                                                   "FileTypes\\TgaFileType.dll",
                                                   "DotNetWidgets.dll",
                                                   "ICSharpCode.SharpZipLib.dll",
                                                   "Interop.WIA.dll",
                                                   "PaintDotNet.Data.dll",
                                                   "PaintDotNet.Effects.dll",
                                                   "PaintDotNet.exe",
                                                   "PaintDotNet.Resources.dll",
                                                   "PaintDotNet.SystemLayer.dll",
                                                   "PdnLib.dll",
                                                   "Skybound.VisualStyles.dll"
                                               };

                foreach (string name in names1)
                {
                    try
                    {
                        InstallAssembly(name, delete);
                    }

                    // Since this is essentially an optional part of setup, we don't care
                    // if it fails.
                    catch
                    {
                    }
                }

                // Create desktop shortcut
                bool createShortcut = false;
                bool updating = false;
                bool skipCleanup = false;
                
                if (args[1] == "DESKTOPSHORTCUT=1")
                {
                    createShortcut = true;
                }

                if (args[2] == "PDNUPDATING=1")
                {
                    updating = true;
                }

                if (args[3] == "SKIPCLEANUP=1")
                {
                    skipCleanup = true;
                }

                // Create shortcuts
                object allUsersDesktop = "AllUsersDesktop";
                object allUsersPrograms = "AllUsersPrograms";

                WshShellClass shell = new WshShellClass();

                // Set up out strings
                string desktopDir = shell.SpecialFolders.Item(ref allUsersDesktop).ToString();
                string programsDir = shell.SpecialFolders.Item(ref allUsersPrograms).ToString();

                string linkName = PaintDotNet.PdnResources.GetString("Setup.DesktopShortcut.LinkName");
                string description = PaintDotNet.PdnResources.GetString("Setup.DesktopShortcut.Description");
                string desktopLinkPath = Path.Combine(desktopDir, linkName) + ".lnk"; // if we just use ChangeExtension it will overwrite the .NET part of Paint.NET :)
                string programsLinkPath = Path.Combine(programsDir, linkName) + ".lnk";
                string workingDirectory = PdnInfo.GetApplicationDir();
                string targetPath = Path.Combine(workingDirectory, "PaintDotNet.exe");

                // Desktop shortcut
                if ((delete && !skipCleanup) || (!createShortcut && skipCleanup))
                {
                    if (System.IO.File.Exists(desktopLinkPath))
                    {
                        Console.WriteLine("delete: " + desktopLinkPath);
                        System.IO.File.Delete(desktopLinkPath);
                    }
                }
                else if (createShortcut && !delete && !updating)
                {
                    IWshShortcut link = (IWshShortcut)shell.CreateShortcut(desktopLinkPath);
                    link.Description = description;
                    link.WorkingDirectory = workingDirectory;
                    link.TargetPath = targetPath;
                    Console.WriteLine("create shortcut: " + desktopLinkPath);
                    link.Save();
                }

                // Programs shortcut
                if (delete && !skipCleanup)
                {
                    if (System.IO.File.Exists(programsLinkPath))
                    {
                        Console.WriteLine("delete: " + programsLinkPath);
                        System.IO.File.Delete(programsLinkPath);
                    }
                }
                else if (!delete && !updating)
                {
                    IWshShortcut link = (IWshShortcut)shell.CreateShortcut(programsLinkPath);
                    link.Description = description;
                    link.WorkingDirectory = workingDirectory;
                    link.TargetPath = targetPath;
                    Console.WriteLine("create shortcut: " + programsLinkPath);
                    link.Save();
                }
            }
        }
    }
}
