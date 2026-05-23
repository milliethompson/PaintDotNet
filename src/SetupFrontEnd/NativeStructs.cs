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
    internal sealed class NativeStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO 
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };
    
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

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
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
