/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Contains static methods related to the user interface.
    /// </summary>
    public sealed class UI
    {
        private static bool initScales = false;
        private static float xScale;
        private static float yScale;

        private static void InitScaleFactors(Control c)
        {
            using (Graphics g = c.CreateGraphics())
            {
                xScale = g.DpiX / 96.0f;
                yScale = g.DpiY / 96.0f;
            }

            initScales = true;
        }

        public static void InitScaling(Control c)
        {
            if (!initScales)
            {
                InitScaleFactors(c);
            }
        }

        public static int ScaleWidth(int width)
        {
            return (int)Math.Round((float)width * GetXScaleFactor());
        }

        public static int ScaleHeight(int height)
        {
            return (int)Math.Round((float)height * GetYScaleFactor());
        }

        public static float GetXScaleFactor()
        {
            if (!initScales)
            {
                throw new InvalidOperationException("Must call InitScaling() first");
            }

            return xScale;
        }

        public static float GetYScaleFactor()
        {
            if (!initScales)
            {
                throw new InvalidOperationException("Must call InitScaling() first");
            }

            return yScale;
        }

        // This is a major hack to get the .NET's OFD to show with Thumbnail view by default!
        // Luckily for us this is a covert hack, and not one where we're working around a bug
        // in the framework or OS.
        // This hack works by retrieving a private property of the OFD class after it has shown
        // the dialog box.
        // Based off [cleaner] code found here: http://vbnet.mvps.org/index.html?code/hooks/fileopensavedlghooklvview.htm
        // They actually set up a hook procedure and do this the [more] proper way.
        private delegate void EtvDelegate(FileDialog ofd);
        private static void EnableThumbnailView(FileDialog ofd)
        {
            // HACK: Must verify this still works with each new revision of .NET
            Type ofdType = typeof(FileDialog);
            FieldInfo fi = ofdType.GetField("dialogHWnd", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fi != null)
            {
                object dialogHWndObject = fi.GetValue(ofd);
                IntPtr dialogHWnd = (IntPtr)dialogHWndObject;
                IntPtr hwndLV = SafeNativeMethods.FindWindowExW(dialogHWnd, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (hwndLV != IntPtr.Zero)
                {
                    SafeNativeMethods.SendMessageW(hwndLV, NativeConstants.WM_COMMAND, new IntPtr(NativeConstants.SHVIEW_THUMBNAIL), IntPtr.Zero);
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
        /// Note to implementors: This method may be implemented to simply call fd.ShowDialog(owner).
        /// </remarks>
        public static DialogResult ShowFileDialogWithThumbnailView(Control owner, FileDialog fd)
        {
            // Bug 1495: Since we do a little 'hackery' to get Thumbnails to be the default view,
            // and since the shortcut for File->Save As is Ctrl+Shift+S, and since Explorer hides
            // the filenames if you hold down Shift when opening a folder in Thumbnail view, we
            // simply spin until the user lets go of shift!
            Cursor.Current = Cursors.WaitCursor;
            while ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                System.Threading.Thread.Sleep(1);
                Application.DoEvents();
            }
            Cursor.Current = Cursors.Default;

            owner.BeginInvoke(new EtvDelegate(EnableThumbnailView), new object[] { fd });
            DialogResult result = fd.ShowDialog(owner);
            return result;
        }

        /// <summary>
        /// Enables or disables the marquee mode of a ProgressBar.
        /// </summary>
        /// <param name="progressBar">The ProgressBar to set the marquee mode on.</param>
        /// <param name="enabled">Whether to enable (true) or disable (false) marquee mode.</param>
        /// <param name="updateMs">Number of milliseconds between marquee updates.</param>
        /// <returns>true if the mode was set correctly, false if not.</returns>
        /// <remarks>
        /// Marquee mode is only available on Windows XP, Windows Server 2003, or later.
        /// Note to implementors: This method may be implemented as a no-op.
        /// TODO: .NET 2.0 does this for us, so we can remove this method when we upgrade to it.
        /// </remarks>
        public static bool SetMarqueeMode(ProgressBar progressBar, bool enabled)
        {
            uint oldStyle = SafeNativeMethods.GetWindowLongW(progressBar.Handle, NativeConstants.GWL_STYLE);
            uint newStyle = (oldStyle | (enabled ? NativeConstants.PBS_MARQUEE : 0)) & (enabled ? ~(uint)0 : ~NativeConstants.PBS_MARQUEE);
            SafeNativeMethods.SetWindowLongW(progressBar.Handle, NativeConstants.GWL_STYLE, newStyle);
            return true;
        }

        public enum ButtonState
        {
            Normal,
            Hot,
            Pressed,
            Disabled
        }

        private static IntPtr OpenTheme(Control hostControl)
        {
            IntPtr hTheme;
            IntPtr hModule = SafeNativeMethods.LoadLibraryW("uxtheme.dll");

            if (hModule == IntPtr.Zero)
            {
                hTheme = IntPtr.Zero;
            }
            else
            {
                hTheme = SafeNativeMethods.OpenThemeData(hostControl.Handle, "Button");
                SafeNativeMethods.FreeLibrary(hModule);
            }

            GC.KeepAlive(hostControl);
            return hTheme;
        }

        /// <summary>
        /// Draws a button in the appropriate system theme (Luna vs. Classic).
        /// </summary>
        /// <remarks>
        /// Note to implementors: This may be implemented as a simple thunk to ControlPaint.DrawButton().
        /// </remarks>
        public static void DrawThemedButton(Control hostControl, Graphics g, int x, int y, 
            int width, int height, UI.ButtonState state)
        {
            IntPtr hTheme = OpenTheme(hostControl);

            if (hTheme != IntPtr.Zero)
            {
                NativeStructs.RECT rect = new NativeStructs.RECT();
                rect.left = x;
                rect.top = y;
                rect.right = x + width;
                rect.bottom = y + height;

                int iState;
                switch (state)
                {
                    case UI.ButtonState.Disabled:
                        iState = NativeConstants.PBS_DISABLED;
                        break;

                    case UI.ButtonState.Hot:
                        iState = NativeConstants.PBS_HOT;
                        break;

                    default:
                    case UI.ButtonState.Normal:
                        iState = NativeConstants.PBS_NORMAL;
                        break;

                    case UI.ButtonState.Pressed:
                        iState = NativeConstants.PBS_PRESSED;
                        break;
                }

                IntPtr hdc = g.GetHdc();

                SafeNativeMethods.DrawThemeBackground(
                    hTheme,
                    hdc,
                    NativeConstants.BP_PUSHBUTTON,
                    iState,
                    ref rect,
                    ref rect);

                g.ReleaseHdc(hdc);

                SafeNativeMethods.CloseThemeData(hTheme);
                hTheme = IntPtr.Zero;
            }
            else
            {
                System.Windows.Forms.ButtonState swfState;

                switch (state)
                {
                    case UI.ButtonState.Disabled:
                        swfState = System.Windows.Forms.ButtonState.Inactive;
                        break;

                    default:
                    case UI.ButtonState.Hot:
                    case UI.ButtonState.Normal:
                        swfState = System.Windows.Forms.ButtonState.Normal;
                        break;

                    case UI.ButtonState.Pressed:
                        swfState = System.Windows.Forms.ButtonState.Pushed;
                        break;
                }

                ControlPaint.DrawButton(g, x, y, width, height, swfState);
            }

            GC.KeepAlive(hostControl);
        }

        /// <summary>
        /// Enables or disables a control from drawing.
        /// </summary>
        /// <param name="control">The control to modify.</param>
        /// <param name="enabled">Whether to enable or disable drawing.</param>
        /// <remarks>
        /// When enabled=false, this method causes all paint requests to be ignored. Invalidation rectangles
        /// are not accumulated during this period, so when redrawing is re-enabled, Invalidate() should be
        /// called.
        /// Note to implementors: This method may be implemented as a no-op.
        /// </remarks>
        public static void SetControlRedraw(Control control, bool enabled)
        {
            SafeNativeMethods.SendMessageW(control.Handle, NativeConstants.WM_SETREDRAW, enabled ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
            GC.KeepAlive(control);
        }

        private static IntPtr hRgn = SafeNativeMethods.CreateRectRgn(0, 0, 1, 1);

        /// <summary>
        /// This method retrieves the update region of a control.
        /// </summary>
        /// <param name="control">The control to retrieve the update region for.</param>
        /// <returns>
        /// An array of rectangles specifying the area that has been invalidated, or 
        /// null if this could not be determined.
        /// </returns>
        /// <remarks>
        /// Note to implementors: This method may be implemented as a no-op. In this case, just return null.
        /// </remarks>
        public static Rectangle[] GetUpdateRegion(Control control)
        {
            SafeNativeMethods.GetUpdateRgn(control.Handle, hRgn, false);
            Rectangle[] scans;
            int area;
            PdnGraphics.GetRegionScans(hRgn, out scans, out area);
            GC.KeepAlive(control);
            return scans;
        }

        /// <summary>
        /// Enables or disable the multiline text wrapping style on a button.
        /// </summary>
        /// <param name="button">The button to set the multiline text wrapping style on.</param>
        /// <remarks>
        /// This sets or clears the BS_MULTILINE class style bit, and then recreates the
        /// control's Handle if necessary.
        /// This method exists to work around a bug in many 3rd-party Windows themes where
        /// the text wrapping on buttons becomes completely baffling. I've seen buttons where
        /// 'OK' was wrapped on to two lines even though there was plenty of room for it.
        /// Note to implementors: This method may be implemented as a no-op, as it only affects 
        /// correctness of rendering under certain scenarios.
        /// </remarks>
        public static bool SetMultilineStyle(ButtonBase button, bool enabled)
        {
            uint dwStyle = SafeNativeMethods.GetWindowLongW(button.Handle, NativeConstants.GWL_STYLE);
            uint dwNewStyle = dwStyle;
            bool recreate = false;

            if (enabled)
            {
                if ((dwStyle & NativeConstants.BS_MULTILINE) != NativeConstants.BS_MULTILINE)
                {
                    dwNewStyle = dwStyle | NativeConstants.BS_MULTILINE;
                    recreate = true;
                }
            }
            else
            {
                if ((dwStyle & NativeConstants.BS_MULTILINE) != 0)
                {
                    dwNewStyle = dwStyle & ~NativeConstants.BS_MULTILINE;
                    recreate = true;
                }
            }

            if (recreate)
            {
                uint dwResult = SafeNativeMethods.SetWindowLongW(button.Handle, NativeConstants.GWL_STYLE, dwNewStyle);
            }

            GC.KeepAlive(button);
            return recreate;
        }

        /// <summary>
        /// Sets a form's opacity.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="opacity"></param>
        /// <remarks>
        /// Note to implementors: This may be implemented as just "form.Opacity = opacity".
        /// This method works around some visual clumsiness in .NET 2.0 related to
        /// transitioning between opacity == 1.0 and opacity != 1.0.</remarks>
        public static void SetFormOpacity(Form form, double opacity)
        {
            if (opacity < 0.0 || opacity > 1.0)
            {
                throw new ArgumentOutOfRangeException("opacity", "must be in the range [0, 1]");
            }

            if (System.Environment.OSVersion.Version >= OS.WindowsXP)
            {
                uint exStyle = SafeNativeMethods.GetWindowLongW(form.Handle, NativeConstants.GWL_EXSTYLE);

                byte bOldAlpha = 255;

                if ((exStyle & NativeConstants.GWL_EXSTYLE) != 0)
                {
                    uint dwOldKey;
                    uint dwOldFlags;
                    bool result = SafeNativeMethods.GetLayeredWindowAttributes(form.Handle, out dwOldKey, out bOldAlpha, out dwOldFlags);
                }

                byte bNewAlpha = (byte)(opacity * 255.0);
                uint newExStyle = exStyle;

                if (bNewAlpha != 255)
                {
                    newExStyle |= NativeConstants.WS_EX_LAYERED;
                }

                if (newExStyle != exStyle || (newExStyle & NativeConstants.WS_EX_LAYERED) != 0)
                {
                    if (newExStyle != exStyle)
                    {
                        SafeNativeMethods.SetWindowLongW(form.Handle, NativeConstants.GWL_EXSTYLE, newExStyle);
                    }

                    if ((newExStyle & NativeConstants.WS_EX_LAYERED) != 0)
                    {
                        SafeNativeMethods.SetLayeredWindowAttributes(form.Handle, 0, bNewAlpha, NativeConstants.LWA_ALPHA);
                    }
                }

                GC.KeepAlive(form);
            }
            else
            {
                form.Opacity = opacity;
            }
        }

        /// <summary>
        /// This WndProc implements click-through functionality. Some controls (MenuStrip, ToolStrip) will not
        /// recognize a click unless the form they are hosted in is active. So the first click will activate the
        /// form and then a second is required to actually make the click happen.
        /// </summary>
        /// <param name="m">The Message that was passed to your WndProv.</param>
        /// <returns>true if the message was processed, false if it was not</returns>
        /// <remarks>
        /// You should first call base.WndProc(), and then call this method. This method is only intended to
        /// change a return value, not to change actual processing before that.
        /// </remarks>
        internal static bool ClickThroughWndProc(ref Message m)
        {
            bool returnVal = false;

            if (m.Msg == NativeConstants.WM_MOUSEACTIVATE)
            {
                if (m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
                {
                    m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
                    returnVal = true;
                }
            }

            return returnVal;
        }

        public static bool IsOurAppActive
        {
            get
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form == Form.ActiveForm)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool HideHorizontalScrollBar(Control c)
        {
            return SafeNativeMethods.ShowScrollBar(c.Handle, NativeConstants.SB_HORZ, false);
        }

        private UI()
        {
        }
    }
}
