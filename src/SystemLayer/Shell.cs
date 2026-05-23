/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public sealed class Shell
    {
        private Shell()
        {
        }

        /// <summary>
        /// Launches the default browser and opens the given URL.
        /// </summary>
        /// <param name="url"></param>
        public static bool OpenUrl(IWin32Window owner, string url)
        {
            string browser = GetDefaultBrowser();

            if (browser != null)
            {
                try
                {
                    Process.Start(browser, "\"" + url + "\"");
                }

                catch
                {
                    string message = PdnResources.GetString("LaunchLink.Error");
                    MessageBox.Show(owner, message, PdnInfo.GetProductName(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static string GetDefaultBrowser()
        {
            string returnVal = null;
            const string exeSuffix = ".exe";

            try
            {
                RegistryKey hkcr = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");

                if (hkcr != null)
                {
                    string path = hkcr.GetValue(null).ToString();
                    string pathSansQuotes = path.Replace("\"", "");
                    int exeIndex = pathSansQuotes.IndexOf(exeSuffix, StringComparison.InvariantCultureIgnoreCase);

                    if (exeIndex != -1)
                    {
                        returnVal = pathSansQuotes.Substring(0, exeIndex + exeSuffix.Length);
                    }
                }
            }

            catch
            {
                returnVal = null;
            }

            return returnVal;
        }

        public static void AddToRecentDocumentsList(string fileName)
        {
            IntPtr bstrFileName = IntPtr.Zero;

            try
            {
                bstrFileName = Marshal.StringToBSTR(fileName);
                NativeMethods.SHAddToRecentDocs(NativeConstants.SHARD_PATHW, bstrFileName);
            }

            finally
            {
                if (bstrFileName != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrFileName);
                    bstrFileName = IntPtr.Zero;
                }
            }
        }
    }
}
