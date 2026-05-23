/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Security related static methods and properties.
    /// </summary>
    public sealed class Security
    {
        private Security()
        {
        }

        private static bool isAdmin = GetIsAdministrator();

        private static bool GetIsAdministrator()
        {
            AppDomain domain = Thread.GetDomain();
            domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Gets a flag indicating whether the user has administrator-level privileges.
        /// </summary>
        /// <remarks>
        /// This is used to control access to actions that require the user to be an administrator.
        /// An example is checking for and installing updates, actions which are not normally able
        /// to be performed by normal or "limited" users. A user must also be an administrator in
        /// order to write to any Settings.SystemWide entries.
        /// </remarks>
        public static bool IsAdministrator
        {
            get
            {
                return isAdmin;
            }
        }

        /// <summary>
        /// Verifies that a file has a valid digital signature.
        /// </summary>
        /// <param name="owner">The parent/owner window for any UI that may be shown.</param>
        /// <param name="fileName">The path to the file to be validate.</param>
        /// <param name="showNegativeUI">Whether or not to show a UI in the case that the signature can not be found or validated.</param>
        /// <param name="showPositiveUI">Whether or not to show a UI in the case that the signature is successfully found and validated.</param>
        /// <returns>true if the file has a digital signature that validates up to a trusted root, or false otherwise</returns>
        public static bool VerifySignedFile(IWin32Window owner, string fileName, bool showNegativeUI, bool showPositiveUI)
        {
            unsafe
            {
                fixed (char *szFileName = fileName)
                {
                    Guid pgActionID = NativeConstants.WINTRUST_ACTION_GENERIC_VERIFY_V2;
                
                    NativeStructs.WINTRUST_FILE_INFO fileInfo = new NativeStructs.WINTRUST_FILE_INFO();
                    fileInfo.cbStruct = (uint)sizeof(NativeStructs.WINTRUST_FILE_INFO);
                    fileInfo.pcwszFilePath = szFileName;

                    NativeStructs.WINTRUST_DATA wintrustData = new NativeStructs.WINTRUST_DATA();
                    wintrustData.cbStruct = (uint)sizeof(NativeStructs.WINTRUST_DATA);

                    if (!showNegativeUI && !showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NONE;
                    }
                    else if (!showNegativeUI && showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NOBAD;
                    }
                    else if (showNegativeUI && !showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NOGOOD;
                    }
                    else // if (showNegativeUI && showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_ALL;
                    }

                    wintrustData.fdwRevocationChecks = NativeConstants.WTD_REVOKE_WHOLECHAIN;
                    wintrustData.dwUnionChoice = NativeConstants.WTD_CHOICE_FILE;
                    wintrustData.pInfo = (void *)&fileInfo;

                    IntPtr handle;

                    if (owner == null)
                    {
                        handle = IntPtr.Zero;
                    }
                    else
                    {
                        handle = owner.Handle;
                    }

                    int result = NativeMethods.WinVerifyTrust(handle, ref pgActionID, ref wintrustData);

                    GC.KeepAlive(owner);
                    return result >= 0;
                }
            }
        }
    }
}
