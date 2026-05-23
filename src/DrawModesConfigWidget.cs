/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DotNetWidgets;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ShapeDrawTypeConfigWidget.
    /// </summary>
    public class DrawModesConfigWidget 
        : System.Windows.Forms.UserControl
    {
        private ImageList imageList;
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem alphaBlendingButton;
        private DotNetWidgets.DotNetToolbarButtonItem aaButton;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public DrawModesConfigWidget ()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Create ImageList
            imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);

            this.dotNetToolbar.ImageList = imageList;

            int aaIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuToolsAntiAliasingIcon.bmp"), imageList.TransparentColor);
            int alphaBlendingIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuToolsAlphaBlendingIcon.bmp"), imageList.TransparentColor);

            aaButton.ImageIndex = aaIndex;
            alphaBlendingButton.ImageIndex = alphaBlendingIndex;

            this.aaButton.ToolTipText = PdnResources.GetString("DrawModesConfigWidget.AAButton.ToolTipText");
            this.alphaBlendingButton.ToolTipText = PdnResources.GetString("DrawModesConfigWidget.AlphaBlendingButton.ToolTipText");
        }

        public event EventHandler AlphaBlendingChanged;
        protected virtual void OnAlphaBlendingChanged()
        {
            if (AlphaBlendingChanged != null)
            {
                AlphaBlendingChanged(this, EventArgs.Empty);
            }
        }

        public void PerformAlphaBlendingChanged()
        {
            OnAlphaBlendingChanged();
        }

        public bool AlphaBlending
        {
            get 
            {
                return alphaBlendingButton.Pushed;
            }

            set
            {
                alphaBlendingButton.Pushed = value;
            }
        }

        public event EventHandler AntiAliasingChanged;
        protected virtual void OnAntiAliasingChanged()
        {
            if (AntiAliasingChanged != null)
            {
                AntiAliasingChanged(this, EventArgs.Empty);
            }
        }

        public void PerformAntiAliasingChanged()
        {
            OnAntiAliasingChanged();
        }

        public bool AntiAliasing
        {
            get
            {
                return aaButton.Pushed;
            }

            set
            {
                aaButton.Pushed = value;
            }
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
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.alphaBlendingButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.aaButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.aaButton);
            this.dotNetToolbar.Buttons.Add(this.alphaBlendingButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.Size = new System.Drawing.Size(57, 26);
            this.dotNetToolbar.TabIndex = 1;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // aaButton
            // 
            this.aaButton.BeginGroup = true;
            // 
            // DrawModesConfigWidget 
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "DrawModesConfigWidget";
            this.Size = new System.Drawing.Size(57, 27);
            this.ResumeLayout(false);

        }
        #endregion

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == alphaBlendingButton)
            {
                alphaBlendingButton.Pushed = !alphaBlendingButton.Pushed;
                OnAlphaBlendingChanged();
            }
            else if (e.Button == aaButton)
            {
                aaButton.Pushed = !aaButton.Pushed;
                OnAntiAliasingChanged();
            }
        }
    }
}
