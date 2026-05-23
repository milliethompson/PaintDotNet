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
using System.Security;

namespace PaintDotNet
{
    /// <summary>
    /// All interop related stuff goes in here.
    /// </summary>
    internal sealed class NativeMethods
    {
        public sealed class WmConstants
        {
            private WmConstants()
            {
            }

            public const int WM_KEYDOWN = 0x100;
            public const int WM_KEYUP = 0x101;
            public const int WM_SETFOCUS = 7;
            public const int WM_PAINT = 0x000f;
            public const int WM_ERASEBKGND = 0x0014;
            public const int WM_PRINT = 0x0317;
            public const int WM_ACTIVATEAPP = 0x001c;
        }
    }
}
