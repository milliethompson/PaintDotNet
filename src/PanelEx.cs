using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PanelEx : 
        System.Windows.Forms.Panel
    {
        private sealed class NativeMethods
        {
            public const int SBM_SETSCROLLINFO = 0x00E9;
            public const int WM_HSCROLL = 0x115;
            public const int WM_VSCROLL = 0x114;
        }

        [Browsable(false)]
        public Point ScrollPosition
        {
            get { return new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y); }
            set { AutoScrollPosition = value; }
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
    }
}
