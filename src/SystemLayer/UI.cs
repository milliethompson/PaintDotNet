/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Containts static methods related to the user interface.
    /// </summary>
    public sealed class UI
    {
        // This is a major hack to get the .NET's OFD to show with Thumbnail view by default!
        // Luckily for us this is a covert hack, and not one where we're working around a bug
        // in the framework or OS.
        // This hack works by retrieving a private property of the OFD class after it has shown
        // the dialog box.
        // Based off [cleaner] code found here: http://vbnet.mvps.org/index.html?code/hooks/fileopensavedlghooklvview.htm
        // They actually set up a hook procedure and do this the proper way.
        private delegate void EtvDelegate(FileDialog ofd);
        private static void EnableThumbnailView(FileDialog ofd)
        {
            Type ofdType = typeof(FileDialog);
            FieldInfo fi = ofdType.GetField("dialogHWnd", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fi != null)
            {
                object dialogHWndObject = fi.GetValue(ofd);
                IntPtr dialogHWnd = (IntPtr)dialogHWndObject;
                IntPtr hwndLV = SafeNativeMethods.FindWindowExW(dialogHWnd, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (hwndLV != IntPtr.Zero)
                {
                    SafeNativeMethods.SendMessage(hwndLV, NativeConstants.WM_COMMAND, new IntPtr(NativeConstants.SHVIEW_THUMBNAIL), IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Shows a FileDialog with the initial view set to Thumbnails.
        /// </summary>
        /// <param name="ofd">The FileDialog to show.</param>
        /// <remarks>
        /// This method may or may not show the OpenFileDialog with the initial view set to Thumbnails,
        /// depending on the implementation and available feature set of the underlying operating system
        /// or shell.
        /// </remarks>
        public static DialogResult ShowFileDialogWithThumbnailView(Control owner, FileDialog fd)
        {
            owner.BeginInvoke(new EtvDelegate(EnableThumbnailView), new object[] { fd });
            DialogResult result = fd.ShowDialog(owner);
            return result;
        }
        
        private UI()
        {
        }
    }
}
