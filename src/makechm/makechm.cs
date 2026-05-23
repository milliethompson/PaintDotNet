/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace makechm
{
    /// <summary>
    /// It is necessary to use this short program to compile the help file
    /// because hhc.exe returns 1 on success and the VS2k3 build system
    /// interprets this as failure. Tools are SUPPOSED to return 0 on 
    /// success, not 1.
    /// Usage:
    ///    makechm [command] [arguments]
    ///  Example:
    ///    makechm ..\tools\hhc ..\chm\paintdotnet.chm
    /// </summary>
    public class makechm
    {
        [STAThread]
        public static int Main(string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo(args[0], args[1]);
            psi.UseShellExecute = false;
            Process process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
            return 0;
        }
    }
}
