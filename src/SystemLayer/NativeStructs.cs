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

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Summary description for NativeStructs.
    /// </summary>
    internal sealed class NativeStructs
    {
        private NativeStructs()
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORYSTATUSEX 
        {  
            internal uint dwLength;
            internal uint dwMemoryLoad;
            internal ulong ullTotalPhys;
            internal ulong ullAvailPhys;
            internal ulong ullTotalPageFile;
            internal ulong ullAvailPageFile;
            internal ulong ullTotalVirtual;
            internal ulong ullAvailVirtual;
            internal ulong ullAvailExtendedVirtual;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct OVERLAPPED 
        {
            internal UIntPtr Internal;  
            internal UIntPtr InternalHigh;  
            internal uint  Offset;  
            internal uint OffsetHigh;  
            internal IntPtr hEvent;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct RGBQUAD
        {
            internal byte rgbBlue;
            internal byte rgbGreen;
            internal byte rgbRed;
            internal byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPINFOHEADER
        {
            internal uint biSize;
            internal int biWidth;
            internal int biHeight;
            internal ushort biPlanes;
            internal ushort biBitCount;
            internal uint biCompression;
            internal uint biSizeImage;
            internal int biXPelsPerMeter;
            internal int biYPelsPerMeter;
            internal uint biClrUsed;
            internal uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BITMAPINFO
        {
            internal BITMAPINFOHEADER bmiHeader;
            internal RGBQUAD bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct MEMORY_BASIC_INFORMATION 
        {
            internal void *BaseAddress;  
            internal void *AllocationBase;  
            internal uint AllocationProtect;  
            internal UIntPtr RegionSize;  
            internal uint State;  
            internal uint Protect;  
            internal uint Type;
        };

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct SYSTEM_INFO
        {
            internal ushort wProcessorArchitecture;
            internal ushort wReserved;
            internal uint dwPageSize;
            internal UIntPtr lpMinimumApplicationAddress;
            internal UIntPtr lpMaximumApplicationAddress;
            internal UIntPtr dwActiveProcessorMask;
            internal uint dwNumberOfProcessors;
            internal uint dwProcessorType;
            internal uint dwAllocationGranularity;
            internal ushort wProcessorLevel;
            internal ushort wProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal class LOGFONT
        {
            internal int lfHeight = 0;
            internal int lfWidth = 0;
            internal int lfEscapement = 0;
            internal int lfOrientation = 0;
            internal int lfWeight = 0;
            internal byte lfItalic = 0;
            internal byte lfUnderline = 0;
            internal byte lfStrikeOut = 0;
            internal byte lfCharSet = 0;
            internal byte lfOutPrecision = 0;
            internal byte lfClipPrecision = 0;
            internal byte lfQuality = 0;
            internal byte lfPitchAndFamily = 0;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string lfFaceName = string.Empty;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LOGBRUSH 
        { 
            internal uint lbStyle; 
            internal uint lbColor; 
            internal int  lbHatch; 
        }; 
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct RGNDATAHEADER 
        { 
            internal uint dwSize; 
            internal uint iType; 
            internal uint nCount; 
            internal uint nRgnSize; 
            internal RECT rcBound; 
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct RGNDATA
        {
            internal RGNDATAHEADER rdh;

            internal unsafe static RECT *GetRectsPointer(RGNDATA *me)
            {
                return (RECT *)((byte *)me + sizeof(RGNDATAHEADER));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            internal int x;
            internal int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct PropertyItem
        {
            internal int id;
            internal uint length;
            internal short type;
            internal void *value;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct WINTRUST_DATA
        {
            internal uint cbStruct;
            internal IntPtr pPolicyCallbackData;
            internal IntPtr pSIPClientData;
            internal uint dwUIChoice;
            internal uint fdwRevocationChecks;
            internal uint dwUnionChoice;
            internal void *pInfo; // pFile, pCatalog, pBlob, pSgnr, or pCert
            internal uint dwStateAction;
            internal IntPtr hWVTStateData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct WINTRUST_FILE_INFO
        {
            internal uint cbStruct;
            internal char *pcwszFilePath;
            internal IntPtr hFile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
        {
            internal bool fAutoDetect;
            internal IntPtr lpszAutoConfigUrl;
            internal IntPtr lpszProxy;
            internal IntPtr lpszProxyBypass;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public UIntPtr Reserved;
        }
    }
}
