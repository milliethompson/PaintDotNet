using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for PanelEx.
	/// </summary>
	public class PanelEx : 
        System.Windows.Forms.Panel, 
        IDisposable        
	{
        private sealed class NativeMethods
        {
            public const int SB_HORZ = 0;
            public const int SB_VERT = 1;
            public const int SB_CTL = 2;
            public const int SB_BOTH = 3;

            public const int SB_LINEUP = 0;
            public const int SB_LINELEFT = 0;
            public const int SB_LINEDOWN = 1;
            public const int SB_LINERIGHT = 1;
            public const int SB_PAGEUP = 2;
            public const int SB_PAGELEFT = 2;
            public const int SB_PAGEDOWN = 3;
            public const int SB_PAGERIGHT = 3;
            public const int SB_THUMBPOSITION = 4;
            public const int SB_THUMBTRACK = 5;
            public const int SB_TOP = 6;
            public const int SB_LEFT = 6;
            public const int SB_BOTTOM = 7;
            public const int SB_RIGHT = 7;
            public const int SB_ENDSCROLL = 8;

            public const int SBM_SETPOS = 0x00E0;
            public const int SBM_GETPOS = 0x00E1;
            public const int SBM_SETRANGE = 0x00E2;
            public const int SBM_SETRANGEREDRAW = 0x00E6;
            public const int SBM_GETRANGE = 0x00E3;
            public const int SBM_ENABLE_ARROWS = 0x00E4;
            public const int SBM_SETSCROLLINFO = 0x00E9;
            public const int SBM_GETSCROLLINFO = 0x00EA;

            public const int WM_HSCROLL = 0x115;
            public const int WM_VSCROLL = 0x114;
            public const int WM_SETFOCUS = 7;

            public const uint SIF_RANGE = 0x0001;
            public const uint SIF_PAGE = 0x0002;
            public const uint SIF_POS = 0x0004;
            public const uint SIF_DISABLENOSCROLL = 0x0008;
            public const uint SIF_TRACKPOS = 0x0010;
            public const uint SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

            [StructLayout(LayoutKind.Sequential)]
            public struct SCROLLINFO
            {
                public uint cbSize; 
                public uint fMask; 
                public int nMin; 
                public int nMax; 
                public uint nPage; 
                public int nPos; 
                public int nTrackPos;
            }

            [DllImport("User32.dll")]
            public static extern int SetScrollInfo(IntPtr hWnd, int fnBar, ref SCROLLINFO lpsi, uint fRedraw);

            [DllImport("User32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        }

        [Browsable(false)]
        public Point ScrollPosition
        {
            get
            {
                return new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
            }

            set
            {
                AutoScrollPosition = value;
            }
        }

        public event EventHandler Scroll;

        protected virtual void OnScroll()
        {
            if (Scroll != null)
            {
                Scroll(this, EventArgs.Empty);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc (ref m);

            if (m.Msg == NativeMethods.WM_HSCROLL || 
                m.Msg == NativeMethods.WM_VSCROLL ||
                m.Msg == NativeMethods.SBM_SETSCROLLINFO)
            {
                OnScroll();
            }
        }

        #region IDisposable Members

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);
        }
        #endregion
    }
}
