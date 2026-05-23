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
    /// <summary>
    /// Summary description for NativeStructs.
    /// </summary>
    internal sealed class NativeStructs
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct OSVERSIONINFOEX 
        {
            public static int SizeOf
            {
                get
                {
                    return Marshal.SizeOf(typeof(OSVERSIONINFOEX));
                }
            }

            public uint dwOSVersionInfoSize;  
            public uint dwMajorVersion;  
            public uint dwMinorVersion;  
            public uint dwBuildNumber;  
            public uint dwPlatformId;  

            [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)]
            public string szCSDVersion;  

            public ushort wServicePackMajor;  

            public ushort wServicePackMinor;  
            public ushort wSuiteMask;  
            public byte wProductType;  
            public byte wReserved;
        };

        private NativeStructs()
        {
        }
    }
}
