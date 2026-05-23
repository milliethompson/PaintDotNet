/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace PaintDotNet.Setup
{
    internal sealed class NativeDelegates
    {
        internal delegate int InstallUiHandler(
            IntPtr pvContext,
            uint iMessageType,
            [MarshalAs(UnmanagedType.LPWStr)] string szMessage
            );

        private NativeDelegates()
        {
        }
    }
}
