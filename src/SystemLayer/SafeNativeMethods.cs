/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32.SafeHandles;
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowScrollBar(
            IntPtr hWnd, 
            int wBar, 
            [MarshalAs(UnmanagedType.Bool)] bool bShow);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetVersionEx(ref NativeStructs.OSVERSIONINFOEX lpVersionInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetLayeredWindowAttributes(
            IntPtr hwnd,
            out uint pcrKey,
            out byte pbAlpha,
            out uint pdwFlags
        );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetLayeredWindowAttributes(
            IntPtr hwnd,
            uint crKey,
            byte bAlpha,
            uint dwFlags);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFontW(
            int nHeight,               // height of font
            int nWidth,                // average character width
            int nEscapement,           // angle of escapement
            int nOrientation,          // base-line orientation angle
            int fnWeight,              // font weight
            uint fdwItalic,            // italic attribute option
            uint fdwUnderline,         // underline attribute option
            uint fdwStrikeOut,         // strikeout attribute option
            uint fdwCharSet,           // character set identifier
            uint fdwOutputPrecision,   // output precision
            uint fdwClipPrecision,     // clipping precision
            uint fdwQuality,           // output quality
            uint fdwPitchAndFamily,    // pitch and family
            string lpszFace            // typeface name
            );
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int DrawTextW(
            IntPtr hdc,
            string lpString,
            int nCount,
            ref NativeStructs.RECT lpRect,
            uint uFormat);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref NativeStructs.BITMAPINFO pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetEndOfFile(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFilePointerEx(
            IntPtr hFile,
            ulong liDistanceToMove,
            out ulong lpNewFilePointer,
            uint dwMoveMethod
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool WriteFile(
            IntPtr hFile,
            void *lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool WriteFile(
            IntPtr hFile,
            void *lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            ref NativeStructs.OVERLAPPED lpOverlapped
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool WriteFileEx(
            IntPtr hFile,
            void *lpBuffer,
            uint nNumberOfBytesToWrite,
            ref NativeStructs.OVERLAPPED lpOverlapped,
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeDelegates.OVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool GetOverlappedResult(
            IntPtr hFile,
            ref NativeStructs.OVERLAPPED lpOverlapped,
            out uint lpNumberOfBytesTransferred,
            [MarshalAs(UnmanagedType.Bool)] bool bWait
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool ReadFile(
            //IntPtr hFile,
            SafeFileHandle sfhFile,
            void *lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        internal static extern int GetUpdateRgn(IntPtr hWnd, IntPtr hRgn, bool bErase);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

        [DllImport("uxtheme.dll", PreserveSig = false)]
        internal static extern void DrawThemeBackground(
            IntPtr hTheme,
            IntPtr hdc,
            int iPartId,
            int iStateId,
            ref NativeStructs.RECT pRect,
            ref NativeStructs.RECT pClipRect
            );

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPWStr)] string pszClassList
            );

        [DllImport("uxtheme.dll", PreserveSig = false)]
        internal static extern void CloseThemeData(IntPtr hTheme);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindowExW(
            IntPtr hwndParent, 
            IntPtr hwndChildAfter, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszClass, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszWindow
            );

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessageW(
            IntPtr hWnd, 
            uint msg, 
            IntPtr wParam, 
            IntPtr lParam);

        [DllImport("user32.dll")]
        internal extern static bool PostMessageW(
            IntPtr handle, 
            int msg, 
            IntPtr wParam, 
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowLongW(
            IntPtr hWnd,
            int nIndex
            );

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SetWindowLongW(
            IntPtr hWnd,
            int nIndex,
            uint dwNewLong
            );

        [DllImport("kernel32.dll", SetLastError = false)]
        internal static extern void GetSystemInfo(
            ref NativeStructs.SYSTEM_INFO lpSystemInfo
            );

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr LoadIconW(
            IntPtr hInstance,
            IntPtr lpIconName
            );

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyIcon(
            IntPtr hIcon
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetModuleHandleW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpModuleName
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool QueryPerformanceFrequency(out ulong lpFrequency);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe void memcpy(void* dst, void* src, UIntPtr length);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe void memset(void* dst, int c, UIntPtr length);

        [DllImport("User32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(
            IntPtr hHandle,
            uint dwMilliseconds
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForMultipleObjects(
            uint nCount,
            IntPtr[] lpHandles,
            [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
            uint dwMilliseconds
            );

        internal static uint WaitForMultipleObjects(IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds)
        {
            return WaitForMultipleObjects((uint)lpHandles.Length, lpHandles, bWaitAll, dwMilliseconds);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForMultipleObjectsEx(
            uint nCount,
            IntPtr[] lpHandles,
            [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
            uint dwMilliseconds,
            [MarshalAs(UnmanagedType.Bool)] bool bAlertable
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateEventW(
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
            [MarshalAs(UnmanagedType.Bool)] bool bInitialState,
            string lpName
            );
        
        [DllImport("wtsapi32.dll")]
        internal static extern uint WTSRegisterSessionNotification(IntPtr hWnd, uint dwFlags);

        [DllImport("wtsapi32.dll")]
        internal static extern uint WTSUnRegisterSessionNotification(IntPtr hWnd);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal unsafe static extern uint GetRegionData(
            IntPtr hRgn,                       // handle to region
            uint dwCount,                      // size of region data buffer
            NativeStructs.RGNDATA *lpRgnData   // region data buffer
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static uint MoveToEx(
            IntPtr hdc,          // handle to device context
            int X,               // x-coordinate of new current position
            int Y,               // y-coordinate of new current position
            out NativeStructs.POINT lpPoint    // old current position
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static uint LineTo(
            IntPtr hdc,    // device context handle
            int nXEnd,     // x-coordinate of ending point
            int nYEnd      // y-coordinate of ending point
            );

        [DllImport("User32.dll", SetLastError = true)]
        internal extern static int FillRect(
            IntPtr hDC,                   // handle to DC
            ref NativeStructs.RECT lprc,  // rectangle
            IntPtr hbr                    // handle to brush
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal unsafe extern static IntPtr ExtCreatePen(
            uint dwPenStyle,      // pen style
            uint dwWidth,         // pen width
            ref NativeStructs.LOGBRUSH lplb,  // brush attributes
            uint dwStyleCount,    // length of custom style array
            uint *lpStyle   // custom style array
            );

        internal static IntPtr ExtCreatePen(
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
        internal extern static IntPtr CreatePen(
            int fnPenStyle,    // pen style
            int nWidth,        // pen width
            uint crColor       // pen color
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static IntPtr CreateSolidBrush(
            uint crColor
            );

        [DllImport("Gdi32.dll")]
        internal extern static IntPtr SelectObject(
            IntPtr hdc,          // handle to DC
            IntPtr hgdiobj       // handle to object
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool DeleteObject(
            IntPtr hObject   // handle to graphic object
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static uint DeleteDC(
            IntPtr hdc       // handle to DC
            );

        [DllImport("Gdi32.Dll", SetLastError = true)]
        internal extern static IntPtr CreateCompatibleDC(
            IntPtr hdc       // handle to DC
            );

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static IntPtr CreateDC(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszDriver,        // driver name
            [MarshalAs(UnmanagedType.LPTStr)] string lpszDevice,        // device name
            [MarshalAs(UnmanagedType.LPTStr)] string lpszOutput,        // not used; should be NULL
            IntPtr lpInitData                                           // optional printer data. NOTE: this should be a CONST DEVMODE*, but that structure is huge and PDN does not use it, we just pass NULL
            );

        [DllImport("Gdi32.Dll", SetLastError = true)]
        internal extern static uint BitBlt(
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
        internal extern static uint SetStretchBltMode(
            IntPtr hdc,           // handle to DC
            int iStretchMode      // bitmap stretching mode
            );
        
        [DllImport("Gdi32.Dll", SetLastError = true)]
        internal extern static uint StretchBlt(
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

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint VirtualQuery(IntPtr lpAddress, out NativeStructs.MEMORY_BASIC_INFORMATION lpBuffer, UIntPtr dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        [DllImport("Kernel32.dll")]
        internal static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll")]
        internal static extern UIntPtr HeapSize(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr GetProcessHeap();

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr HeapCreate(
            uint flOptions,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwInitialSize,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwMaximumSize
            );

        [DllImport("Kernel32.dll")]
        internal static extern uint HeapDestroy(IntPtr hHeap);

        [DllImport("Kernel32.Dll")]
        internal unsafe static extern uint HeapSetInformation(
            IntPtr HeapHandle,
            int HeapInformationClass,
            void *HeapInformation,
            uint HeapInformationLength
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibraryW(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("winhttp.dll", CharSet = CharSet.Unicode)]
        internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref NativeStructs.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG pProxyConfig);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GlobalFree(IntPtr hMem);
    }
}
