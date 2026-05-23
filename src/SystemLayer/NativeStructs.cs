/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public UIntPtr lpMinimumApplicationAddress;
            public UIntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public class LOGFONT
        {
            public int lfHeight = 0;
            public int lfWidth = 0;
            public int lfEscapement = 0;
            public int lfOrientation = 0;
            public int lfWeight = 0;
            public byte lfItalic = 0;
            public byte lfUnderline = 0;
            public byte lfStrikeOut = 0;
            public byte lfCharSet = 0;
            public byte lfOutPrecision = 0;
            public byte lfClipPrecision = 0;
            public byte lfQuality = 0;
            public byte lfPitchAndFamily = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
            public string lfFaceName = string.Empty;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct LOGBRUSH 
        { 
            public uint lbStyle; 
            public uint lbColor; 
            public int  lbHatch; 
        }; 
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RGNDATAHEADER 
        { 
            public uint dwSize; 
            public uint iType; 
            public uint nCount; 
            public uint nRgnSize; 
            public RECT rcBound; 
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RGNDATA
        {
            public RGNDATAHEADER rdh;

            public unsafe static RECT *GetRectsPointer(RGNDATA *me)
            {
                return (RECT *)((byte *)me + sizeof(RGNDATAHEADER));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct PropertyItem
        {
            public int id;
            public uint length;
            public short type;
            public void *value;
        }
	}
}
