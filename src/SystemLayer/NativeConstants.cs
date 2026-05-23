/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Summary description for NativeConstants.
	/// </summary>
	internal sealed class NativeConstants
	{
        private NativeConstants()
        {
        }

        public const uint SHVIEW_THUMBNAIL = 0x702d;
        public const uint WM_COMMAND = 0x111;

        public const uint IDI_APPLICATION = 32512;

        public const int ERROR_SUCCESS = 0;

        public const int SBM_SETPOS = 0x00E0;
        public const int SBM_SETRANGE = 0x00E2;
        public const int SBM_SETRANGEREDRAW = 0x00E6;
        public const int SBM_SETSCROLLINFO = 0x00E9;
        public const int WM_HSCROLL = 0x114;
        public const int WM_VSCROLL = 0x115;
        public const int WM_SETFOCUS = 7;

        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;

        public const uint ANSI_CHARSET = 0;
        public const uint DEFAULT_CHARSET = 1;
        public const uint SYMBOL_CHARSET = 2;
        public const uint SHIFTJIS_CHARSET = 128;
        public const uint HANGEUL_CHARSET = 129;
        public const uint HANGUL_CHARSET = 129;
        public const uint GB2312_CHARSET = 134;
        public const uint CHINESEBIG5_CHARSET = 136;
        public const uint OEM_CHARSET = 255;
        public const uint JOHAB_CHARSET = 130;
        public const uint HEBREW_CHARSET = 177;
        public const uint ARABIC_CHARSET = 178;
        public const uint GREEK_CHARSET = 161;
        public const uint TURKISH_CHARSET = 162;
        public const uint VIETNAMESE_CHARSET = 163;
        public const uint THAI_CHARSET = 222;
        public const uint EASTEUROPE_CHARSET = 238;
        public const uint RUSSIAN_CHARSET = 204;
        public const uint MAC_CHARSET = 77;
        public const uint BALTIC_CHARSET = 186;
        
        public const uint SPI_GETBEEP = 0x0001;
        public const uint SPI_SETBEEP = 0x0002;
        public const uint SPI_GETMOUSE = 0x0003;
        public const uint SPI_SETMOUSE = 0x0004;
        public const uint SPI_GETBORDER = 0x0005;
        public const uint SPI_SETBORDER = 0x0006;
        public const uint SPI_GETKEYBOARDSPEED = 0x000A;
        public const uint SPI_SETKEYBOARDSPEED = 0x000B;
        public const uint SPI_LANGDRIVER = 0x000C;
        public const uint SPI_ICONHORIZONTALSPACING = 0x000D;
        public const uint SPI_GETSCREENSAVETIMEOUT = 0x000E;
        public const uint SPI_SETSCREENSAVETIMEOUT = 0x000F;
        public const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
        public const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        public const uint SPI_GETGRIDGRANULARITY = 0x0012;
        public const uint SPI_SETGRIDGRANULARITY = 0x0013;
        public const uint SPI_SETDESKWALLPAPER = 0x0014;
        public const uint SPI_SETDESKPATTERN = 0x0015;
        public const uint SPI_GETKEYBOARDDELAY = 0x0016;
        public const uint SPI_SETKEYBOARDDELAY = 0x0017;
        public const uint SPI_ICONVERTICALSPACING = 0x0018;
        public const uint SPI_GETICONTITLEWRAP = 0x0019;
        public const uint SPI_SETICONTITLEWRAP = 0x001A;
        public const uint SPI_GETMENUDROPALIGNMENT = 0x001B;
        public const uint SPI_SETMENUDROPALIGNMENT = 0x001C;
        public const uint SPI_SETDOUBLECLKWIDTH = 0x001D;
        public const uint SPI_SETDOUBLECLKHEIGHT = 0x001E;
        public const uint SPI_GETICONTITLELOGFONT = 0x001F;
        public const uint SPI_SETDOUBLECLICKTIME = 0x0020;
        public const uint SPI_SETMOUSEBUTTONSWAP = 0x0021;
        public const uint SPI_SETICONTITLELOGFONT = 0x0022;
        public const uint SPI_GETFASTTASKSWITCH = 0x0023;
        public const uint SPI_SETFASTTASKSWITCH = 0x0024;
        public const uint SPI_SETDRAGFULLWINDOWS = 0x0025;
        public const uint SPI_GETDRAGFULLWINDOWS = 0x0026;
        public const uint SPI_GETNONCLIENTMETRICS = 0x0029;
        public const uint SPI_SETNONCLIENTMETRICS = 0x002A;
        public const uint SPI_GETMINIMIZEDMETRICS = 0x002B;
        public const uint SPI_SETMINIMIZEDMETRICS = 0x002C;
        public const uint SPI_GETICONMETRICS = 0x002D;
        public const uint SPI_SETICONMETRICS = 0x002E;
        public const uint SPI_SETWORKAREA = 0x002F;
        public const uint SPI_GETWORKAREA = 0x0030;
        public const uint SPI_SETPENWINDOWS = 0x0031;
        public const uint SPI_GETHIGHCONTRAST = 0x0042;
        public const uint SPI_SETHIGHCONTRAST = 0x0043;
        public const uint SPI_GETKEYBOARDPREF = 0x0044;
        public const uint SPI_SETKEYBOARDPREF = 0x0045;
        public const uint SPI_GETSCREENREADER = 0x0046;
        public const uint SPI_SETSCREENREADER = 0x0047;
        public const uint SPI_GETANIMATION = 0x0048;
        public const uint SPI_SETANIMATION = 0x0049;
        public const uint SPI_GETFONTSMOOTHING = 0x004A;
        public const uint SPI_SETFONTSMOOTHING = 0x004B;
        public const uint SPI_SETDRAGWIDTH = 0x004C;
        public const uint SPI_SETDRAGHEIGHT = 0x004D;
        public const uint SPI_SETHANDHELD = 0x004E;
        public const uint SPI_GETLOWPOWERTIMEOUT = 0x004F;
        public const uint SPI_GETPOWEROFFTIMEOUT = 0x0050;
        public const uint SPI_SETLOWPOWERTIMEOUT = 0x0051;
        public const uint SPI_SETPOWEROFFTIMEOUT = 0x0052;
        public const uint SPI_GETLOWPOWERACTIVE = 0x0053;
        public const uint SPI_GETPOWEROFFACTIVE = 0x0054;
        public const uint SPI_SETLOWPOWERACTIVE = 0x0055;
        public const uint SPI_SETPOWEROFFACTIVE = 0x0056;
        public const uint SPI_SETCURSORS = 0x0057;
        public const uint SPI_SETICONS = 0x0058;
        public const uint SPI_GETDEFAULTINPUTLANG = 0x0059;
        public const uint SPI_SETDEFAULTINPUTLANG = 0x005A;
        public const uint SPI_SETLANGTOGGLE = 0x005B;
        public const uint SPI_GETWINDOWSEXTENSION = 0x005C;
        public const uint SPI_SETMOUSETRAILS = 0x005D;
        public const uint SPI_GETMOUSETRAILS = 0x005E;
        public const uint SPI_SETSCREENSAVERRUNNING = 0x0061;
        public const uint SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING;
        public const uint SPI_GETFILTERKEYS = 0x0032;
        public const uint SPI_SETFILTERKEYS = 0x0033;
        public const uint SPI_GETTOGGLEKEYS = 0x0034;
        public const uint SPI_SETTOGGLEKEYS = 0x0035;
        public const uint SPI_GETMOUSEKEYS = 0x0036;
        public const uint SPI_SETMOUSEKEYS = 0x0037;
        public const uint SPI_GETSHOWSOUNDS = 0x0038;
        public const uint SPI_SETSHOWSOUNDS = 0x0039;
        public const uint SPI_GETSTICKYKEYS = 0x003A;
        public const uint SPI_SETSTICKYKEYS = 0x003B;
        public const uint SPI_GETACCESSTIMEOUT = 0x003C;
        public const uint SPI_SETACCESSTIMEOUT = 0x003D;
        public const uint SPI_GETSERIALKEYS = 0x003E;
        public const uint SPI_SETSERIALKEYS = 0x003F;
        public const uint SPI_GETSOUNDSENTRY = 0x0040;
        public const uint SPI_SETSOUNDSENTRY = 0x0041;
        public const uint SPI_GETSNAPTODEFBUTTON = 0x005F;
        public const uint SPI_SETSNAPTODEFBUTTON = 0x0060;
        public const uint SPI_GETMOUSEHOVERWIDTH = 0x0062;
        public const uint SPI_SETMOUSEHOVERWIDTH = 0x0063;
        public const uint SPI_GETMOUSEHOVERHEIGHT = 0x0064;
        public const uint SPI_SETMOUSEHOVERHEIGHT = 0x0065;
        public const uint SPI_GETMOUSEHOVERTIME = 0x0066;
        public const uint SPI_SETMOUSEHOVERTIME = 0x0067;
        public const uint SPI_GETWHEELSCROLLLINES = 0x0068;
        public const uint SPI_SETWHEELSCROLLLINES = 0x0069;
        public const uint SPI_GETMENUSHOWDELAY = 0x006A;
        public const uint SPI_SETMENUSHOWDELAY = 0x006B;
        public const uint SPI_GETSHOWIMEUI = 0x006E;
        public const uint SPI_SETSHOWIMEUI = 0x006F;
        public const uint SPI_GETMOUSESPEED = 0x0070;
        public const uint SPI_SETMOUSESPEED = 0x0071;
        public const uint SPI_GETSCREENSAVERRUNNING = 0x0072;
        public const uint SPI_GETDESKWALLPAPER = 0x0073;
        public const uint SPI_GETACTIVEWINDOWTRACKING = 0x1000;
        public const uint SPI_SETACTIVEWINDOWTRACKING = 0x1001;
        public const uint SPI_GETMENUANIMATION = 0x1002;
        public const uint SPI_SETMENUANIMATION = 0x1003;
        public const uint SPI_GETCOMBOBOXANIMATION = 0x1004;
        public const uint SPI_SETCOMBOBOXANIMATION = 0x1005;
        public const uint SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006;
        public const uint SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007;
        public const uint SPI_GETGRADIENTCAPTIONS = 0x1008;
        public const uint SPI_SETGRADIENTCAPTIONS = 0x1009;
        public const uint SPI_GETKEYBOARDCUES = 0x100A;
        public const uint SPI_SETKEYBOARDCUES = 0x100B;
        public const uint SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES;
        public const uint SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES;
        public const uint SPI_GETACTIVEWNDTRKZORDER = 0x100C;
        public const uint SPI_SETACTIVEWNDTRKZORDER = 0x100D;
        public const uint SPI_GETHOTTRACKING = 0x100E;
        public const uint SPI_SETHOTTRACKING = 0x100F;
        public const uint SPI_GETMENUFADE = 0x1012;
        public const uint SPI_SETMENUFADE = 0x1013;
        public const uint SPI_GETSELECTIONFADE = 0x1014;
        public const uint SPI_SETSELECTIONFADE = 0x1015;
        public const uint SPI_GETTOOLTIPANIMATION = 0x1016;
        public const uint SPI_SETTOOLTIPANIMATION = 0x1017;
        public const uint SPI_GETTOOLTIPFADE = 0x1018;
        public const uint SPI_SETTOOLTIPFADE = 0x1019;
        public const uint SPI_GETCURSORSHADOW = 0x101A;
        public const uint SPI_SETCURSORSHADOW = 0x101B;
        public const uint SPI_GETMOUSESONAR = 0x101C;
        public const uint SPI_SETMOUSESONAR = 0x101D;
        public const uint SPI_GETMOUSECLICKLOCK = 0x101E;
        public const uint SPI_SETMOUSECLICKLOCK = 0x101F;
        public const uint SPI_GETMOUSEVANISH = 0x1020;
        public const uint SPI_SETMOUSEVANISH = 0x1021;
        public const uint SPI_GETFLATMENU = 0x1022;
        public const uint SPI_SETFLATMENU = 0x1023;
        public const uint SPI_GETDROPSHADOW = 0x1024;
        public const uint SPI_SETDROPSHADOW = 0x1025;
        public const uint SPI_GETBLOCKSENDINPUTRESETS = 0x1026;
        public const uint SPI_SETBLOCKSENDINPUTRESETS = 0x1027;
        public const uint SPI_GETUIEFFECTS = 0x103E;
        public const uint SPI_SETUIEFFECTS = 0x103F;
        public const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        public const uint SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002;
        public const uint SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003;
        public const uint SPI_GETFOREGROUNDFLASHCOUNT = 0x2004;
        public const uint SPI_SETFOREGROUNDFLASHCOUNT = 0x2005;
        public const uint SPI_GETCARETWIDTH = 0x2006;
        public const uint SPI_SETCARETWIDTH = 0x2007;
        public const uint SPI_GETMOUSECLICKLOCKTIME = 0x2008;
        public const uint SPI_SETMOUSECLICKLOCKTIME = 0x2009;
        public const uint SPI_GETFONTSMOOTHINGTYPE = 0x200A;
        public const uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;
        public const uint SPI_GETFONTSMOOTHINGCONTRAST = 0x200C;
        public const uint SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
        public const uint SPI_GETFOCUSBORDERWIDTH = 0x200E;
        public const uint SPI_SETFOCUSBORDERWIDTH = 0x200F;
        public const uint SPI_GETFOCUSBORDERHEIGHT = 0x2010;
        public const uint SPI_SETFOCUSBORDERHEIGHT = 0x2011;
        public const uint SPI_GETFONTSMOOTHINGORIENTATION = 0x2012;
        public const uint SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;

        public const uint INFINITE = 0xffffffff;
        public const uint STATUS_WAIT_0 = 0;
        public const uint STATUS_ABANDONED_WAIT_0 = 0x80;
        public const uint WAIT_FAILED = 0xffffffff;
        public const uint WAIT_TIMEOUT = 258;
        public const uint WAIT_ABANDONED = STATUS_ABANDONED_WAIT_0 + 0;
        public const uint WAIT_OBJECT_0 = STATUS_WAIT_0 + 0;
        public const uint WAIT_ABANDONED_0 = STATUS_ABANDONED_WAIT_0 + 0;

        public const int SM_REMOTESESSION = 0x1000;
        public const int WM_WTSSESSION_CHANGE = 0x2b1;
        public const int WM_MOVING = 0x0216;
        public const uint NOTIFY_FOR_ALL_SESSIONS = 1;
        public const uint NOTIFY_FOR_THIS_SESSION = 0;
       

        public const int PS_SOLID = 0;
        public const int PS_DASH = 1;             /* -------  */
        public const int PS_DOT = 2;              /* .......  */
        public const int PS_DASHDOT = 3;          /* _._._._  */
        public const int PS_DASHDOTDOT = 4;       /* _.._.._  */
        public const int PS_NULL = 5;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_USERSTYLE = 7;
        public const int PS_ALTERNATE = 8;
        
        public const int PS_ENDCAP_ROUND = 0x00000000;
        public const int PS_ENDCAP_SQUARE = 0x00000100;
        public const int PS_ENDCAP_FLAT = 0x00000200;
        public const int PS_ENDCAP_MASK = 0x00000F00;

        public const int PS_JOIN_ROUND = 0x00000000;
        public const int PS_JOIN_BEVEL = 0x00001000;
        public const int PS_JOIN_MITER = 0x00002000;
        public const int PS_JOIN_MASK = 0x0000F000;

        public const int PS_COSMETIC = 0x00000000;
        public const int PS_GEOMETRIC = 0x00010000;
        public const int PS_TYPE_MASK = 0x000F0000;

        public const int BS_SOLID = 0;
        public const int BS_NULL = 1;
        public const int BS_HOLLOW = BS_NULL;
        public const int BS_HATCHED = 2;
        public const int BS_PATTERN = 3;
        public const int BS_INDEXED = 4;
        public const int BS_DIBPATTERN = 5;
        public const int BS_DIBPATTERNPT = 6;
        public const int BS_PATTERN8X8 = 7;
        public const int BS_DIBPATTERN8X8 = 8;
        public const int BS_MONOPATTERN = 9;

        public const uint SRCCOPY = 0x00CC0020;     /* dest = source  */
        public const uint SRCPAINT = 0x00EE0086;    /* dest = source OR dest */
        public const uint SRCAND = 0x008800C6;      /* dest = source AND dest */
        public const uint SRCINVERT = 0x00660046;   /* dest = source XOR dest */
        public const uint SRCERASE = 0x00440328;    /* dest = source AND (NOT dest ) */
        public const uint NOTSRCCOPY = 0x00330008;  /* dest = (NOT source) */
        public const uint NOTSRCERASE = 0x001100A6; /* dest = (NOT src) AND (NOT dest) */
        public const uint MERGECOPY = 0x00C000CA;   /* dest = (source AND pattern) */
        public const uint MERGEPAINT = 0x00BB0226;  /* dest = (NOT source) OR dest */
        public const uint PATCOPY = 0x00F00021;     /* dest = pattern  */
        public const uint PATPAINT = 0x00FB0A09;    /* dest = DPSnoo  */
        public const uint PATINVERT = 0x005A0049;   /* dest = pattern XOR dest */
        public const uint DSTINVERT = 0x00550009;   /* dest = (NOT dest) */
        public const uint BLACKNESS = 0x00000042;   /* dest = BLACK  */
        public const uint WHITENESS = 0x00FF0062;   /* dest = WHITE  */

        public const uint NOMIRRORBITMAP = 0x80000000; /* Do not Mirror the bitmap in this call */
        public const uint CAPTUREBLT = 0x40000000;     /* Include layered windows */

        // StretchBlt() Modes
        public const int BLACKONWHITE = 1;
        public const int WHITEONBLACK = 2;
        public const int COLORONCOLOR = 3;
        public const int HALFTONE = 4;
        public const int MAXSTRETCHBLTMODE = 4;

        public const int HeapCompatibilityInformation = 0;
        public const uint HEAP_NO_SERIALIZE = 0x00000001;
        public const uint HEAP_GROWABLE = 0x00000002;
        public const uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
        public const uint HEAP_ZERO_MEMORY = 0x00000008;
        public const uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;
        public const uint HEAP_TAIL_CHECKING_ENABLED = 0x00000020;
        public const uint HEAP_FREE_CHECKING_ENABLED = 0x00000040;
        public const uint HEAP_DISABLE_COALESCE_ON_FREE = 0x00000080;
        public const uint HEAP_CREATE_ALIGN_16 = 0x00010000;
        public const uint HEAP_CREATE_ENABLE_TRACING = 0x00020000;
        public const uint HEAP_MAXIMUM_TAG = 0x0FFF;
        public const uint HEAP_PSEUDO_TAG_FLAG = 0x8000;
        public const uint HEAP_TAG_SHIFT = 18;

        public const int SM_TABLETPC = 86;

        public const uint MONITOR_DEFAULTTONULL = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    }
}
