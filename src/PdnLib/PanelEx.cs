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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PanelEx : 
        PaintDotNet.SystemLayer.ScrollPanel
    {
        private bool hideHScroll = false;

        public bool HideHScroll
        {
            get
            {
                return this.hideHScroll;
            }

            set
            {
                this.hideHScroll = value;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.hideHScroll)
            {
                SystemLayer.UI.SetControlRedraw(this, false);
            }

            base.OnSizeChanged(e);

            if (this.hideHScroll)
            {
                SystemLayer.UI.HideHorizontalScrollBar(this);
                SystemLayer.UI.SetControlRedraw(this, true);
                Invalidate(true);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);
        }
    }
}
