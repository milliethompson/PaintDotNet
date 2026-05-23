/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// This class adds on to the functionality provided in System.Windows.Forms.ToolStrip.
    /// </summary>
    /// <remarks>
    /// The first aggravating thing I found out about ToolStrip is that it does not "click through."
    /// If the form that is hosting a ToolStrip is not active and you click on a button in the tool
    /// strip, it sets focus to the form but does NOT click the button. This makes sense in many
    /// situations, but definitely not for Paint.NET.
    /// </remarks>
    public class ToolStripEx
        : ToolStrip
    {
        private bool clickThrough = true;
        private bool managedFocus = true;
        private static int enteredComboBox = 0;

        public ToolStripEx()
        {
            System.Windows.Forms.ToolStripProfessionalRenderer tspr = this.Renderer as ToolStripProfessionalRenderer;

            if (tspr != null)
            {
                tspr.ColorTable.UseSystemColors = true;
            }

            this.ImageScalingSize = new System.Drawing.Size(UI.ScaleWidth(16), UI.ScaleHeight(16));
        }

        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus.
        /// </summary>
        /// <remarks>
        /// Default value is true, which is the opposite of the behavior provided by the base
        /// ToolStrip class.
        /// </remarks>
        public bool ClickThrough
        {
            get
            {
                return this.clickThrough;
            }

            set
            {
                this.clickThrough = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (this.clickThrough)
            {
                UI.ClickThroughWndProc(ref m);
            }
        }

        /// <summary>
        /// This event is raised when this toolstrip instance wishes to relinquish focuses.
        /// </summary>
        public event EventHandler RelinquishFocusRequest;

        private void OnRelinquishFocusRequest()
        {
            if (RelinquishFocusRequest != null)
            {
                RelinquishFocusRequest(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets whether the toolstrip manages focus.
        /// </summary>
        /// <remarks>
        /// If this is true, the toolstrip will capture focus when the mouse enters its client area. It will then
        /// relinquish focus (via the RelinquishFocusRequest event) when the mouse leaves. It will not capture or
        /// attempt to relinquish focus if MenuStripEx.IsAnyMenuActive returns true.
        /// </remarks>
        public bool ManagedFocus
        {
            get
            {
                return this.managedFocus;
            }

            set
            {
                this.managedFocus = value;
            }
        }

        private void OnAdded(Control c)
        {
            c.MouseEnter += OnItemMouseEnter;

            foreach (Control child in c.Controls)
            {
                OnAdded(child);
            }
        }

        private void OnRemoved(Control c)
        {
            c.MouseLeave -= OnItemMouseEnter;

            foreach (Control child in c.Controls)
            {
                OnRemoved(child);
            }
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e)
        {
            ToolStripComboBox tscb = e.Item as ToolStripComboBox;

            if (tscb == null)
            {
                e.Item.MouseEnter += OnItemMouseEnter;
            }
            else
            {
                tscb.DropDown += new EventHandler(tscb_DropDown);
                tscb.DropDownClosed += new EventHandler(tscb_DropDownClosed);
                tscb.SelectedIndexChanged += new EventHandler(tscb_SelectedIndexChanged);
                tscb.ComboBox.SelectedValueChanged += new EventHandler(ComboBox_SelectedValueChanged);
                tscb.Enter += new EventHandler(tscb_Enter);
                tscb.Leave += new EventHandler(tscb_Leave);
            }

            base.OnItemAdded(e);
        }

        void tscb_Leave(object sender, EventArgs e)
        {
            --enteredComboBox;
        }

        void tscb_Enter(object sender, EventArgs e)
        {
            ++enteredComboBox;
        }

        void ComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
        }

        void tscb_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        void tscb_DropDownClosed(object sender, EventArgs e)
        {
            OnRelinquishFocusRequest();
        }

        void tscb_DropDown(object sender, EventArgs e)
        {
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e)
        {
            ToolStripComboBox tscb = e.Item as ToolStripComboBox;

            if (tscb == null)
            {
                e.Item.MouseEnter -= OnItemMouseEnter;
            }
            else
            {
                tscb.DropDown -= new EventHandler(tscb_DropDown);
                tscb.DropDownClosed -= new EventHandler(tscb_DropDownClosed);
                tscb.SelectedIndexChanged -= new EventHandler(tscb_SelectedIndexChanged);
                tscb.ComboBox.SelectedValueChanged -= new EventHandler(ComboBox_SelectedValueChanged);
                tscb.Enter -= new EventHandler(tscb_Enter);
                tscb.Leave -= new EventHandler(tscb_Leave);
            }

            base.OnItemRemoved(e);
        }

        void OnItemMouseEnter(object sender, EventArgs e)
        {
            if (this.managedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive && enteredComboBox == 0)
            {
                this.Focus();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.managedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive && enteredComboBox == 0)
            {
                OnRelinquishFocusRequest();
            }

            base.OnMouseLeave(e);
        }
    }
}
