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
using System.Security;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Summary description for SafeNativeMethods.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeNativeMethods
	{
		private SafeNativeMethods()
		{
		}

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowExW(
            IntPtr hwndParent, 
            IntPtr hwndChildAfter, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszClass, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszWindow
            );

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            IntPtr hWnd, 
            uint msg, 
            IntPtr wParam, 
            IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(
            ref NativeStructs.SYSTEM_INFO lpSystemInfo
            );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadIconW(
            IntPtr hInstance,
            IntPtr lpIconName
            );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(
            IntPtr hIcon
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandleW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpModuleName
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out ulong lpFrequency);

        [DllImport("msvcrt.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern unsafe void memcpy(void *dst, void *src, UIntPtr length);

        [DllImport("User32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
  
        [DllImport("kernel32.dll")]
        public static extern uint WaitForMultipleObjects(
            uint nCount,
            IntPtr[] lpHandles,
            [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
            uint dwMilliseconds
            );

        public static uint WaitForMultipleObjects(IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds)
        {
            return WaitForMultipleObjects((uint)lpHandles.Length, lpHandles, bWaitAll, dwMilliseconds);
        }

        [DllImport("wtsapi32.dll")]
        public static extern uint WTSRegisterSessionNotification(IntPtr hWnd, uint dwFlags);

        [DllImport("wtsapi32.dll")]
        public static extern uint WTSUnRegisterSessionNotification(IntPtr hWnd);

        [DllImport("Gdi32.dll", SetLastError = true)]
        public unsafe static extern uint GetRegionData(
            HandleRef hRgn,                    // handle to region
            uint dwCount,                      // size of region data buffer
            NativeStructs.RGNDATA *lpRgnData   // region data buffer
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static uint MoveToEx(
            IntPtr hdc,          // handle to device context
            int X,               // x-coordinate of new current position
            int Y,               // y-coordinate of new current position
            out NativeStructs.POINT lpPoint    // old current position
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static uint LineTo(
            IntPtr hdc,    // device context handle
            int nXEnd,     // x-coordinate of ending point
            int nYEnd      // y-coordinate of ending point
            );

        [DllImport("User32.dll", SetLastError = true)]
        public extern static int FillRect(
            IntPtr hDC,                   // handle to DC
            ref NativeStructs.RECT lprc,  // rectangle
            IntPtr hbr                    // handle to brush
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public unsafe extern static IntPtr ExtCreatePen(
            uint dwPenStyle,      // pen style
            uint dwWidth,         // pen width
            ref NativeStructs.LOGBRUSH lplb,  // brush attributes
            uint dwStyleCount,    // length of custom style array
            uint *lpStyle   // custom style array
            );

        public static IntPtr ExtCreatePen(
            uint dwPenStyle,
            uint dwWidth,
            ref NativeStructs.LOGBRUSH lplb,
            uint[] styles
            )
        {
            unsafe 
            {
                fixed (uint *lpStyle = styles)
                {
                    return ExtCreatePen(dwPenStyle, dwWidth, ref lplb, (uint)styles.Length, lpStyle);
                }
            }
        }

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static IntPtr CreatePen(
            int fnPenStyle,    // pen style
            int nWidth,        // pen width
            uint crColor       // pen color
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static IntPtr CreateSolidBrush(
            uint crColor
            );

        [DllImport("Gdi32.dll")]
        public extern static IntPtr SelectObject(
            IntPtr hdc,          // handle to DC
            IntPtr hgdiobj       // handle to object
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static uint DeleteObject(
            IntPtr hObject   // handle to graphic object
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static uint DeleteDC(
            IntPtr hdc       // handle to DC
            );

        [DllImport("Gdi32.Dll", SetLastError = true)]
        public extern static IntPtr CreateCompatibleDC(
            IntPtr hdc       // handle to DC
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        public extern static IntPtr CreateDC(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszDriver,        // driver name
            [MarshalAs(UnmanagedType.LPTStr)] string lpszDevice,        // device name
            [MarshalAs(UnmanagedType.LPTStr)] string lpszOutput,        // not used; should be NULL
            IntPtr lpInitData  // optional printer data. NOTE: this should be a CONST DEVMODE*, but that structure is huge and PDN does not use it, we just pass NULL
            );

        [DllImport("Gdi32.Dll", SetLastError = true)]
        public extern static uint BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest,     // x-coord of destination upper-left corner
            int nYDest,     // y-coord of destination upper-left corner
            int nWidth,     // width of destination rectangle
            int nHeight,    // height of destination rectangle
            IntPtr hdcSrc,  // handle to source DC
            int nXSrc,      // x-coordinate of source upper-left corner
            int nYSrc,      // y-coordinate of source upper-left corner
            uint dwRop      // raster operation code
            );

        [DllImport("Gdi32.Dll", SetLastError = true)]
        public extern static uint SetStretchBltMode(
            IntPtr hdc,           // handle to DC
            int iStretchMode      // bitmap stretching mode
            );
        
        [DllImport("Gdi32.Dll", SetLastError = true)]
        public extern static uint StretchBlt(
            IntPtr hdcDest,      // handle to destination DC
            int nXOriginDest,    // x-coord of destination upper-left corner
            int nYOriginDest,    // y-coord of destination upper-left corner
            int nWidthDest,      // width of destination rectangle
            int nHeightDest,     // height of destination rectangle
            IntPtr hdcSrc,       // handle to source DC
            int nXOriginSrc,     // x-coord of source upper-left corner
            int nYOriginSrc,     // y-coord of source upper-left corner
            int nWidthSrc,       // width of source rectangle
            int nHeightSrc,      // height of source rectangle
            uint dwRop           // raster operation code
            );

        [DllImport("Kernel32.dll")]
        public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        [DllImport("Kernel32.dll")]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll")]
        public static extern UIntPtr HeapSize(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetProcessHeap();

        [DllImport("Kernel32.dll")]
        public static extern IntPtr HeapCreate(
            uint flOptions,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwInitialSize,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwMaximumSize
            );

        [DllImport("Kernel32.dll")]
        public static extern uint HeapDestroy(IntPtr hHeap);

        [DllImport("Kernel32.Dll")]
        public unsafe static extern uint HeapSetInformation(
            IntPtr HeapHandle,
            int HeapInformationClass,
            void *HeapInformation,
            uint HeapInformationLength
            );

        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                                        
        public static extern int GdipGetAllPropertyItems(IntPtr image, uint totalSize, uint count, IntPtr buffer);  

        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                              
        public static extern int GdipGetPropertyCount(IntPtr image, out int count);                     
                                                                                                     
        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                              
        public static extern int GdipGetPropertyIdList(IntPtr image, int count, int[] list);            
                                                                                                     
        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                              
        public static extern int GdipGetPropertyItem(IntPtr image, int propid, int size, IntPtr buffer);
                                                                                                     
        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                              
        public static extern int GdipGetPropertyItemSize(IntPtr image, int propid, out int size);       
                                                                                                     
        [DllImport("gdiplus.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]                              
        public static extern int GdipGetPropertySize(IntPtr image, out uint totalSize, out uint count);   
    }
}
