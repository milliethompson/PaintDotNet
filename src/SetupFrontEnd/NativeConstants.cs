/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Setup
{
    /// <summary>
    /// Summary description for NativeConstants.
    /// </summary>
    internal sealed class NativeConstants
    {
        internal const uint MB_ABORTRETRYIGNORE = 0x00000002;
        internal const uint MB_OK = 0x00000000;
        internal const uint MB_OKCANCEL = 0x00000001;
        internal const uint MB_RETRYCANCEL = 0x00000005;
        internal const uint MB_YESNO = 0x00000004;
        internal const uint MB_YESNOCANCEL = 0x00000003;

        internal const uint MB_ICONEXCLAMATION = 0x00000030;
        internal const uint MB_ICONWARNING = MB_ICONEXCLAMATION;
        internal const uint MB_ICONINFORMATION = MB_ICONASTERISK;
        internal const uint MB_ICONASTERISK = 0x00000040;
        internal const uint MB_ICONQUESTION = 0x00000020;
        internal const uint MB_ICONSTOP = MB_ICONHAND;
        internal const uint MB_ICONERROR = MB_ICONHAND;
        internal const uint MB_ICONHAND = 0x00000010;

        internal const uint MB_DEFBUTTON1 = 0x00000000;
        internal const uint MB_DEFBUTTON2 = 0x00000100;
        internal const uint MB_DEFBUTTON3 = 0x00000200;
        internal const uint MB_DEFBUTTON4 = 0x00000300;

        internal const uint INSTALLMESSAGE_FATALEXIT = 0x00000000;          // premature termination, possibly fatal OOM
        internal const uint INSTALLMESSAGE_ERROR = 0x01000000;              // formatted error message
        internal const uint INSTALLMESSAGE_WARNING = 0x02000000;            // formatted warning message
        internal const uint INSTALLMESSAGE_USER = 0x03000000;               // user request message
        internal const uint INSTALLMESSAGE_INFO = 0x04000000;               // informative message for log
        internal const uint INSTALLMESSAGE_FILESINUSE = 0x05000000;         // list of files in use that need to be replaced
        internal const uint INSTALLMESSAGE_RESOLVESOURCE = 0x06000000;      // request to determine a valid source location
        internal const uint INSTALLMESSAGE_OUTOFDISKSPACE = 0x07000000;     // insufficient disk space message
        internal const uint INSTALLMESSAGE_ACTIONSTART = 0x08000000;        // start of action: action name & description
        internal const uint INSTALLMESSAGE_ACTIONDATA = 0x09000000;         // formatted data associated with individual action item
        internal const uint INSTALLMESSAGE_PROGRESS = 0x0A000000;           // progress gauge info: units so far, total
        internal const uint INSTALLMESSAGE_COMMONDATA = 0x0B000000;         // product info for dialog: language Id, dialog caption
        internal const uint INSTALLMESSAGE_INITIALIZE = 0x0C000000;         // sent prior to UI initialization, no string data
        internal const uint INSTALLMESSAGE_TERMINATE = 0x0D000000;          // sent after UI termination, no string data
        internal const uint INSTALLMESSAGE_SHOWDIALOG = 0x0E000000;         // sent prior to display or authored dialog or wizard

        internal const uint INSTALLLOGMODE_FATALEXIT      = ((uint)1 << (int)(INSTALLMESSAGE_FATALEXIT      >> 24));
        internal const uint INSTALLLOGMODE_ERROR          = ((uint)1 << (int)(INSTALLMESSAGE_ERROR          >> 24));
        internal const uint INSTALLLOGMODE_WARNING        = ((uint)1 << (int)(INSTALLMESSAGE_WARNING        >> 24));
        internal const uint INSTALLLOGMODE_USER           = ((uint)1 << (int)(INSTALLMESSAGE_USER           >> 24));
        internal const uint INSTALLLOGMODE_INFO           = ((uint)1 << (int)(INSTALLMESSAGE_INFO           >> 24));
        internal const uint INSTALLLOGMODE_RESOLVESOURCE  = ((uint)1 << (int)(INSTALLMESSAGE_RESOLVESOURCE  >> 24));
        internal const uint INSTALLLOGMODE_OUTOFDISKSPACE = ((uint)1 << (int)(INSTALLMESSAGE_OUTOFDISKSPACE >> 24));
        internal const uint INSTALLLOGMODE_ACTIONSTART    = ((uint)1 << (int)(INSTALLMESSAGE_ACTIONSTART    >> 24));
        internal const uint INSTALLLOGMODE_ACTIONDATA     = ((uint)1 << (int)(INSTALLMESSAGE_ACTIONDATA     >> 24));
        internal const uint INSTALLLOGMODE_COMMONDATA     = ((uint)1 << (int)(INSTALLMESSAGE_COMMONDATA     >> 24));
        internal const uint INSTALLLOGMODE_PROPERTYDUMP   = ((uint)1 << (int)(INSTALLMESSAGE_PROGRESS       >> 24)); // log only
        internal const uint INSTALLLOGMODE_VERBOSE        = ((uint)1 << (int)(INSTALLMESSAGE_INITIALIZE     >> 24)); // log only
        internal const uint INSTALLLOGMODE_EXTRADEBUG     = ((uint)1 << (int)(INSTALLMESSAGE_TERMINATE      >> 24)); // log only
        internal const uint INSTALLLOGMODE_PROGRESS       = ((uint)1 << (int)(INSTALLMESSAGE_PROGRESS       >> 24)); // external handler only
        internal const uint INSTALLLOGMODE_INITIALIZE     = ((uint)1 << (int)(INSTALLMESSAGE_INITIALIZE     >> 24)); // external handler only
        internal const uint INSTALLLOGMODE_TERMINATE      = ((uint)1 << (int)(INSTALLMESSAGE_TERMINATE      >> 24)); // external handler only
        internal const uint INSTALLLOGMODE_SHOWDIALOG     = ((uint)1 << (int)(INSTALLMESSAGE_SHOWDIALOG     >> 24)); // external handler only

        internal const uint INSTALLUILEVEL_NOCHANGE = 0;           // UI level is unchanged
        internal const uint INSTALLUILEVEL_DEFAULT  = 1;           // default UI is used
        internal const uint INSTALLUILEVEL_NONE     = 2;           // completely silent installation
        internal const uint INSTALLUILEVEL_BASIC    = 3;           // simple progress and error handling
        internal const uint INSTALLUILEVEL_REDUCED  = 4;           // authored UI; wizard dialogs suppressed
        internal const uint INSTALLUILEVEL_FULL     = 5;           // authored UI with wizards; progress; errors
        internal const uint INSTALLUILEVEL_ENDDIALOG    = 0x80;    // display success/failure dialog at end of install
        internal const uint INSTALLUILEVEL_PROGRESSONLY = 0x40;    // display only progress dialog
        internal const uint INSTALLUILEVEL_HIDECANCEL   = 0x20;    // do not display the cancel button in basic UI
        internal const uint INSTALLUILEVEL_SOURCERESONLY = 0x100;  // force display of source resolution even if quiet

        internal const uint ERROR_SUCCESS = 0;
        internal const uint ERROR_SUCCESS_REBOOT_REQUIRED = 3010;
        internal const uint ERROR_SUCCESS_REBOOT_INITIATED = 1641;

        internal const int MAX_PATH = 260;
        internal const uint SHGFP_TYPE_CURRENT = 0;
        internal const uint SHGFP_TYPE_DEFAULT = 1;

        internal const uint CSIDL_PROGRAM_FILES = 0x0026;  // C:\Program Files
        internal const uint CSIDL_FLAG_CREATE = 0x8000;    // new for Win2K, or this in to force creation of folder


        internal const byte VER_EQUAL = 1;
        internal const byte VER_GREATER = 2;
        internal const byte VER_GREATER_EQUAL = 3;
        internal const byte VER_LESS = 4;
        internal const byte VER_LESS_EQUAL = 5;
        internal const byte VER_AND = 6;
        internal const byte VER_OR = 7;

        internal const uint VER_CONDITION_MASK = 7;
        internal const uint VER_NUM_BITS_PER_CONDITION_MASK = 3;

        internal const uint VER_MINORVERSION = 0x0000001;
        internal const uint VER_MAJORVERSION = 0x0000002;
        internal const uint VER_BUILDNUMBER = 0x0000004;
        internal const uint VER_PLATFORMID = 0x0000008;
        internal const uint VER_SERVICEPACKMINOR = 0x0000010;
        internal const uint VER_SERVICEPACKMAJOR = 0x0000020;
        internal const uint VER_SUITENAME = 0x0000040;
        internal const uint VER_PRODUCT_TYPE = 0x0000080;

        internal const uint VER_NT_WORKSTATION = 0x0000001;
        internal const uint VER_NT_DOMAIN_CONTROLLER = 0x0000002;
        internal const uint VER_NT_SERVER = 0x0000003;

        internal const uint VER_PLATFORM_WIN32s = 0;
        internal const uint VER_PLATFORM_WIN32_WINDOWS = 1;
        internal const uint VER_PLATFORM_WIN32_NT = 2;


        private NativeConstants()
        {
        }
    }
}
