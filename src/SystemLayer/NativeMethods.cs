/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Summary description for NativeMethods.
	/// </summary>
	internal sealed class NativeMethods
	{
		private NativeMethods()
		{
		}

        [DllImport("User32.dll")]
        public extern static unsafe uint SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            void *pvParam,
            uint fWinIni
            );   

        public static void ThrowOnWin32Error()
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            
            if (lastWin32Error != NativeConstants.ERROR_SUCCESS)
            {
                throw new Win32Exception(lastWin32Error);
            }
        }
    }
}
