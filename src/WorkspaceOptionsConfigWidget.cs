/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for WorkspaceOptionsConfigWidget.
    /// </summary>
    public class WorkspaceOptionsConfigWidget 
        : System.Windows.Forms.UserControl
    {
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem aaToggleButton;
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem rulersToggleButton;
		private DotNetWidgets.DotNetToolbarButtonItem drawGridToggleButton;
        private System.ComponentModel.IContainer components;
        
		public bool DrawGrid
		{
			get
			{
				return drawGridToggleButton.Pushed;
			}

			set
			{
				if (drawGridToggleButton.Pushed != value)
				{
					drawGridToggleButton.Pushed = value;
					this.OnDrawGridChanged();
				}
			}
		}
		
		public bool AntiAliasing
		{
			get
			{
				return aaToggleButton.Pushed;
			}

			set
			{
				if (aaToggleButton.Pushed != value)
				{
					aaToggleButton.Pushed = value;
					this.OnAntiAliasChanged();
				}
			}
		}

        public bool RulersEnabled
        {
            get
            {
                return rulersToggleButton.Pushed;
            }

            set
            {
                if (rulersToggleButton.Pushed != value)
                {
                    rulersToggleButton.Pushed = value;
                    this.OnRulersEnabledChanged();
                }
            }
        }

        public WorkspaceOptionsConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
			int gridIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuViewGridIcon.bmp"), imageList.TransparentColor);
			int aaIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuToolsAntiAliasingIcon.bmp"), imageList.TransparentColor);
            int rulersIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuViewRulersIcon.bmp"), imageList.TransparentColor);

			drawGridToggleButton.ImageIndex = gridIndex;
            aaToggleButton.ImageIndex = aaIndex;
            rulersToggleButton.ImageIndex = rulersIndex;
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
			this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
			this.drawGridToggleButton = new DotNetWidgets.DotNetToolbarButtonItem();
			this.aaToggleButton = new DotNetWidgets.DotNetToolbarButtonItem();
			this.rulersToggleButton = new DotNetWidgets.DotNetToolbarButtonItem();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// dotNetToolbar
			// 
			this.dotNetToolbar.Buttons.Add(this.drawGridToggleButton);
			this.dotNetToolbar.Buttons.Add(this.aaToggleButton);
			this.dotNetToolbar.Buttons.Add(this.rulersToggleButton);
			this.dotNetToolbar.DrawGrabHandle = false;
			this.dotNetToolbar.ImageList = this.imageList;
			this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
			this.dotNetToolbar.MenuProvider = null;
			this.dotNetToolbar.Name = "dotNetToolbar";
			this.dotNetToolbar.NegotiateToolTips = true;
			this.dotNetToolbar.Size = new System.Drawing.Size(120, 26);
			this.dotNetToolbar.TabIndex = 0;
			this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
			// 
			// drawGridToggleButton
			// 
			this.drawGridToggleButton.BeginGroup = true;
			this.drawGridToggleButton.ToolTipText = "Toggle Grid mode for zooming in";
			// 
			// aaToggleButton
			// 
			this.aaToggleButton.ToolTipText = "Toggle Anti-Aliasing";
			// 
			// rulersToggleButton
			// 
			this.rulersToggleButton.ToolTipText = "Toggle Rulers";
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// WorkspaceOptionsConfigWidget
			// 
			this.Controls.Add(this.dotNetToolbar);
			this.Name = "WorkspaceOptionsConfigWidget";
			this.Size = new System.Drawing.Size(120, 32);
			this.ResumeLayout(false);

		}
        #endregion

		public event EventHandler DrawGridChanged;
		protected virtual void OnDrawGridChanged()
		{
			if (DrawGridChanged != null)
			{
				DrawGridChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler AntiAliasChanged;
		protected virtual void OnAntiAliasChanged()
		{
			if (AntiAliasChanged != null)
			{
				AntiAliasChanged(this, EventArgs.Empty);
			}
		}

        public event EventHandler RulersEnabledChanged;
        protected virtual void OnRulersEnabledChanged()
        {
            if (RulersEnabledChanged != null)
            {
                RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
			if (e.Button == this.aaToggleButton)
			{
				this.AntiAliasing = !this.AntiAliasing;
			}
			else if (e.Button == this.rulersToggleButton)
			{
				this.RulersEnabled = !this.RulersEnabled;
			}
			else if (e.Button == this.drawGridToggleButton)
			{
				this.DrawGrid = !this.DrawGrid;
			}
        }
    }
}
