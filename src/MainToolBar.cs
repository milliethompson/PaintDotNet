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
using DotNetWidgets;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for MainToolBar.
    /// </summary>
	public class MainToolBar : System.Windows.Forms.UserControl
	{
		private ColorDisplayWidget colorDisplayWidget;
		private DotNetWidgets.DotNetToolbar[] dotNetToolbars;
		private ImageList imageList;
		private DotNetWidgets.DotNetToolbar.ButtonClickEventHandler toolClickedDelegate;
		private const int tbWidth = 2; // two buttons per line in the toolbars
		private ToleranceSliderControl  toleranceSlider;

		public ToleranceSliderControl ToleranceSlider
		{
			get
			{
				return toleranceSlider;
			}
		}
	
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public MainToolBar()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.toolClickedDelegate = new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(dotNetToolbar_ButtonClick);
        }

        public class DotNetToolbarButtonItemWithTag 
            : DotNetToolbarButtonItem
        {
            private object tag;
            public object Tag
            {
                get
                {
                    return tag;
                }

                set
                {
                    tag = value;
                }
            }
        }

        public event ToolClickedEventHandler ToolClicked;
        protected virtual void OnToolClicked(Type toolType)
        {
            if (ToolClicked != null)
            {
                ToolClicked(this, new ToolClickedEventArgs(toolType));
            }
        }

        public ColorDisplayWidget ColorDisplay
        {
            get
            {
                return colorDisplayWidget;
            }
        }

        public void SetTools(ToolInfo[] toolInfos, DocumentWorkspace workspace)
        {
            imageList = new ImageList();
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            int tbIndex = 0;

            if (dotNetToolbars != null)
            {
                foreach (DotNetToolbar tb in dotNetToolbars)
                {
                    tb.ButtonClick -= this.toolClickedDelegate;
                    this.Controls.Remove(tb);
                }
            }

            dotNetToolbars = new DotNetWidgets.DotNetToolbar[(toolInfos.Length + (tbWidth - 1)) / tbWidth];

            for (int i = 0; i < dotNetToolbars.Length; ++i)
            {
                dotNetToolbars[i] = new DotNetWidgets.DotNetToolbar();
                dotNetToolbars[i].Dock = DockStyle.Top;
                dotNetToolbars[i].ButtonClick += toolClickedDelegate;
                dotNetToolbars[i].DrawGrabHandle = false;
                dotNetToolbars[i].ImageList = imageList;
            }

            this.Controls.AddRange(dotNetToolbars);

            foreach (ToolInfo toolInfo in toolInfos)
            {
                int imageIndex = imageList.Images.Add((Image)toolInfo.Image.Clone(), imageList.TransparentColor);
                DotNetToolbarButtonItemWithTag tbb = new DotNetToolbarButtonItemWithTag();
                tbb.ImageIndex = imageIndex;
                tbb.Tag = toolInfo.ToolType;
                tbb.ToolTipText = toolInfo.Name + " (" + toolInfo.HotKey.ToString().ToUpper() + ")";
                dotNetToolbars[dotNetToolbars.Length - (tbIndex / tbWidth) - 1].Buttons.Add(tbb);

                ++tbIndex;
            }
        }

        public void SelectTool(Type toolType)
        {
            foreach (DotNetToolbar dotNetToolbar in dotNetToolbars)
            {
                foreach (DotNetToolbarButtonItemWithTag tbb in dotNetToolbar.Buttons)
                {
                    if ((Type)tbb.Tag == toolType)
                    {
                        dotNetToolbar_ButtonClick(this, new DotNetToolbarItemClickEventArgs(tbb));
                        return;
                    }
                }
            }

            throw new ArgumentException("Tool type not found");
        }

        public int ToolbarsHeight()
        {
            int total = 0;

            foreach (Control c in dotNetToolbars)
            {
                total += c.Height;
            }

            return total;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            this.ClientSize = new Size(dotNetToolbars[0].Width, colorDisplayWidget.Height + toleranceSlider.Height + ToolbarsHeight());
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
			this.colorDisplayWidget = new PaintDotNet.ColorDisplayWidget();
			this.toleranceSlider = new PaintDotNet.ToleranceSliderControl();
			this.SuspendLayout();
			// 
			// colorDisplayWidget
			// 
			this.colorDisplayWidget.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.colorDisplayWidget.Location = new System.Drawing.Point(0, 280);
			this.colorDisplayWidget.Name = "colorDisplayWidget";
			this.colorDisplayWidget.TabIndex = 1;
			// 
			// toleranceSlider
			// 
			this.toleranceSlider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.toleranceSlider.Location = new System.Drawing.Point(2, 264);
			this.toleranceSlider.Name = "toleranceSlider";
			this.toleranceSlider.Size = new System.Drawing.Size(44, 16);
			this.toleranceSlider.TabIndex = 0;
			this.toleranceSlider.Tolerance = 0.5f;
			// 
			// MainToolBar
			// 
			this.Controls.Add(this.toleranceSlider);
			this.Controls.Add(this.colorDisplayWidget);
			this.Name = "MainToolBar";
			this.Size = new System.Drawing.Size(48, 328);
			this.ResumeLayout(false);

		}
        #endregion

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            DotNetToolbarButtonItemWithTag button = (DotNetToolbarButtonItemWithTag)e.Button;

            foreach (DotNetToolbar dotNetToolbar in dotNetToolbars)
            {
                foreach (DotNetToolbarButtonItemWithTag tbb in dotNetToolbar.Buttons)
                {
                    if (tbb != button)
                    {
                        tbb.Pushed = false;
                    }
                }
            }

            button.Pushed = true;
            OnToolClicked((Type)button.Tag);
        }
    }
}

