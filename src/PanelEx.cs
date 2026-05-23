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
			public const int WM_SETFOCUS = 7;
		}

		private bool ignoreSetFocus = false;
		public bool IgnoreSetFocus
		{
			get
			{
				return ignoreSetFocus;
			}

			set
			{
				ignoreSetFocus = value;
			}
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
			switch (m.Msg)
			{
				case NativeMethods.WM_HSCROLL:
				case NativeMethods.WM_VSCROLL:
				case NativeMethods.SBM_SETSCROLLINFO:
					OnScroll();
					goto default;

				case NativeMethods.WM_SETFOCUS:
					if (IgnoreSetFocus)
					{
						return;
					}
					else
					{
						goto default;
					}

				default:
					base.WndProc (ref m);
					break;
			}
		}
    }
}
