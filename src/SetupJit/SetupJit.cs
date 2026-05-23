using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace SetupJit
{
    /// <summary>
    /// Summary description for SetupJit.
    /// </summary>
    public class SetupJit 
    {
        static void NgenAssembly(string fileName, bool delete)
        {
            string dir = @"%WINDIR%\Microsoft.NET\Framework\v" + Environment.Version.ToString(3);
            string theDir = Environment.ExpandEnvironmentVariables(dir);
            string exeName = "ngen.exe";
            string fullExeName = Path.Combine(theDir, exeName);
            string args;
            
            if (delete)
            {
                args = "/delete \"" + Path.GetFileNameWithoutExtension(fileName) + "\"";
            }
            else
            {
                args = "\"" + fileName + "\"";
            }
            
            ProcessStartInfo psi = new ProcessStartInfo(fullExeName, args);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) 
        {
            if (args.Length == 0)
            {
                return;
            }


            try
            {
                bool delete = false;
                string appDir = string.Empty;

                if (args.Length == 1)
                {
                    appDir = args[0];
                }

                if (args.Length == 2 && args[0] == "/d")
                {
                    appDir = args[1];
                    delete = true;
                }
            
                NgenAssembly(Path.Combine(appDir, "CpuCount.NET.dll"), delete);
                NgenAssembly(Path.Combine(appDir, "DotNetWidgets.dll"), delete);
                NgenAssembly(Path.Combine(appDir, "Interop.WIA.dll"), delete);
                NgenAssembly(Path.Combine(appDir, "ICSharpCode.SharpZipLib.dll"), delete);
                NgenAssembly(Path.Combine(appDir, "Skybound.VisualStyles.dll"), delete);
            }

            catch
            {
                // We don't want installation to fail if we can't ngen
            }
        }
    }
}
