/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// This is the same as System.Windows.Forms.Panel except for three things:
    /// 1. It exposes a Scroll event.
    /// 2. It allows you to disable SetFocus.
    /// 3. It has a much simplified interface for AutoScrollPosition, exposed via the ScrollPosition property.
    /// </summary>
    public class ScrollPanel : 
        System.Windows.Forms.Panel
    {
        private bool ignoreSetFocus = false;

        /// <summary>
        /// Gets or sets whether the control ignores WM_SETFOCUS.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the scrollbar position.
        /// </summary>
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

        /// <summary>
        /// Occurs when the panel is scrolled.
        /// </summary>
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
                case NativeConstants.WM_HSCROLL:
                case NativeConstants.WM_VSCROLL:
                case NativeConstants.SBM_SETPOS:
                case NativeConstants.SBM_SETRANGE:
                case NativeConstants.SBM_SETRANGEREDRAW:
                case NativeConstants.SBM_SETSCROLLINFO:
                    base.WndProc(ref m);
                    OnScroll();
                    break;

                case NativeConstants.WM_SETFOCUS:
                    if (IgnoreSetFocus)
                    {
                        return;
                    }
                    else
                    {
                        goto default;
                    }

                default:
                    base.WndProc(ref m);
                    break;
            }
        }        
    }
}
