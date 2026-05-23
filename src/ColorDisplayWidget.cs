/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ColorDisplayWidget.
    /// </summary>
    public class ColorDisplayWidget : System.Windows.Forms.UserControl
    {
        private System.ComponentModel.IContainer components;

        private PaintDotNet.ColorRectangleControl foreColorRectangle;
        private PaintDotNet.ColorRectangleControl backColorRectangle;
        private IconBox blackAndWhiteIconBox;
        private System.Windows.Forms.ToolTip toolTip;
        private IconBox swapIconBox;
    
        protected override Size DefaultSize
        {
            get
            {
                return new Size(48, 48);
            }
        }

        public event EventHandler UserForeAndBackColorsChanged;
        protected virtual void OnUserForeAndBackColorsChanged()
        {
            if (UserForeAndBackColorsChanged != null)
            {
                UserForeAndBackColorsChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler UserForeColorChanged;
        protected virtual void OnUserForeColorChanged()
        {
            if (UserForeColorChanged != null)
            {
                UserForeColorChanged(this, EventArgs.Empty);
            }
        }

        private ColorBgra userForeColor;
        public ColorBgra UserForeColor
        {
            get
            {
                return userForeColor;
            }

            set
            {
                ColorBgra oldColor = userForeColor;
                userForeColor = value;
                foreColorRectangle.RectangleColor = value.ToColor();
                Invalidate();
                Update();
            }
        }

        public event EventHandler UserBackColorChanged;
        protected virtual void OnUserBackColorChanged()
        {
            if (UserBackColorChanged != null)
            {
                UserBackColorChanged(this, EventArgs.Empty);
            }
        }

        private ColorBgra userBackColor;
        public ColorBgra UserBackColor
        {
            get
            {
                return userBackColor;
            }

            set
            {
                ColorBgra oldColor = userBackColor;
                userBackColor = value;
                backColorRectangle.RectangleColor = value.ToColor();
                Invalidate();
                Update();
            }
        }

        public ColorDisplayWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            swapIconBox.Icon = new Bitmap(PdnResources.GetImage("Icons.SwapIcon.png"));
            blackAndWhiteIconBox.Icon = new Bitmap(PdnResources.GetImage("Icons.BlackAndWhiteIcon.png"));

            toolTip.SetToolTip(swapIconBox, PdnResources.GetString("ColorDisplayWidget.SwapIconBox.ToolTipText"));
            toolTip.SetToolTip(blackAndWhiteIconBox, PdnResources.GetString("ColorDisplayWidget.BlackAndWhiteIconBox.ToolTipText"));
            toolTip.SetToolTip(foreColorRectangle, PdnResources.GetString("ColorDisplayWidget.ForeColorRectangle.ToolTipText"));
            toolTip.SetToolTip(backColorRectangle, PdnResources.GetString("ColorDisplayWidget.BackColorRectangle.ToolTipText"));
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int ulX = (this.ClientRectangle.Width - UI.ScaleWidth(this.DefaultSize.Width)) / 2;
            int ulY = (this.ClientRectangle.Height - UI.ScaleHeight(this.DefaultSize.Height)) / 2;

            this.foreColorRectangle.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 2), UI.ScaleHeight(ulY + 2));
            this.backColorRectangle.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 18), UI.ScaleHeight(ulY + 18));
            this.swapIconBox.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 30), UI.ScaleHeight(ulY + 2));
            this.blackAndWhiteIconBox.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 2), UI.ScaleHeight(ulY + 31));

            base.OnLayout (levent);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.foreColorRectangle = new PaintDotNet.ColorRectangleControl();
            this.backColorRectangle = new PaintDotNet.ColorRectangleControl();
            this.swapIconBox = new PaintDotNet.IconBox();
            this.blackAndWhiteIconBox = new PaintDotNet.IconBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // foreColorRectangle
            // 
            this.foreColorRectangle.Name = "foreColorRectangle";
            this.foreColorRectangle.RectangleColor = System.Drawing.Color.FromArgb(((System.Byte)(0)), ((System.Byte)(0)), ((System.Byte)(192)));
            this.foreColorRectangle.Size = new System.Drawing.Size(28, 28);
            this.foreColorRectangle.TabIndex = 0;
            this.foreColorRectangle.Click += new System.EventHandler(this.foreColorRectangle_Click);
            this.foreColorRectangle.KeyUp += new System.Windows.Forms.KeyEventHandler(this.control_KeyUp);
            // 
            // backColorRectangle
            // 
            this.backColorRectangle.Name = "backColorRectangle";
            this.backColorRectangle.RectangleColor = System.Drawing.Color.Magenta;
            this.backColorRectangle.Size = new System.Drawing.Size(28, 28);
            this.backColorRectangle.TabIndex = 1;
            this.backColorRectangle.Click += new System.EventHandler(this.backColorRectangle_Click);
            this.backColorRectangle.KeyUp += new System.Windows.Forms.KeyEventHandler(this.control_KeyUp);
            // 
            // swapIconBox
            // 
            this.swapIconBox.Icon = null;
            this.swapIconBox.Name = "swapIconBox";
            this.swapIconBox.Size = new System.Drawing.Size(15, 15);
            this.swapIconBox.TabIndex = 2;
            this.swapIconBox.TabStop = false;
            this.swapIconBox.Click += new System.EventHandler(this.swapIconBox_Click);
            this.swapIconBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.control_KeyUp);
            this.swapIconBox.DoubleClick += new System.EventHandler(this.swapIconBox_Click);
            // 
            // blackAndWhiteIconBox
            // 
            this.blackAndWhiteIconBox.Icon = null;
            this.blackAndWhiteIconBox.Name = "blackAndWhiteIconBox";
            this.blackAndWhiteIconBox.Size = new System.Drawing.Size(15, 15);
            this.blackAndWhiteIconBox.TabIndex = 3;
            this.blackAndWhiteIconBox.TabStop = false;
            this.blackAndWhiteIconBox.Click += new System.EventHandler(this.blackAndWhiteIconBox_Click);
            this.blackAndWhiteIconBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.control_KeyUp);
            this.blackAndWhiteIconBox.DoubleClick += new System.EventHandler(this.blackAndWhiteIconBox_Click);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // ColorDisplayWidget
            // 
            this.Controls.Add(this.blackAndWhiteIconBox);
            this.Controls.Add(this.swapIconBox);
            this.Controls.Add(this.foreColorRectangle);
            this.Controls.Add(this.backColorRectangle);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Name = "ColorDisplayWidget";
            this.Size = new System.Drawing.Size(48, 48);
            this.ResumeLayout(false);

        }
        #endregion

        private void swapIconBox_Click(object sender, System.EventArgs e)
        {
            ColorBgra fore = UserForeColor;
            ColorBgra back = UserBackColor;
            UserForeColor = back;
            UserBackColor = fore;
            OnUserForeAndBackColorsChanged();
        }

        private void blackAndWhiteIconBox_Click(object sender, System.EventArgs e)
        {
            UserForeColor = ColorBgra.FromBgra(0, 0, 0, 255);
            UserBackColor = ColorBgra.FromBgra(255, 255, 255, 255);
            OnUserForeAndBackColorsChanged();
        }

        public event EventHandler UserForeColorClick;
        protected virtual void OnUserForeColorClick()
        {
            if (UserForeColorClick != null)
            {
                UserForeColorClick(this, EventArgs.Empty);
            }
        }

        private void foreColorRectangle_Click(object sender, System.EventArgs e)
        {
            OnUserForeColorClick();
        }

        public event EventHandler UserBackColorClick;
        protected virtual void OnUserBackColorClick()
        {
            if (UserBackColorClick != null)
            {
                UserBackColorClick(this, EventArgs.Empty);
            }
        }

        private void backColorRectangle_Click(object sender, System.EventArgs e)
        {
            OnUserBackColorClick();
        }

        private void control_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
    }
}
