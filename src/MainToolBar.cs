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
    /// Summary description for MainToolBar.
    /// </summary>
    public class MainToolBar 
        : System.Windows.Forms.UserControl
    {
        private ColorDisplayWidget colorDisplayWidget;
        private ToolStripEx toolStripEx;
        private ImageList imageList;
        private const int tbWidth = 2; // two buttons per line in the toolbars
        private ToleranceSliderControl  toleranceSlider;
        private int ignoreToolClicked = 0;
        private Control onePxSpacingLeft;

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
        }

        public event ToolClickedEventHandler ToolClicked;
        protected virtual void OnToolClicked(Type toolType)
        {
            if (this.ignoreToolClicked <= 0)
            {
                if (ToolClicked != null)
                {
                    ToolClicked(this, new ToolClickedEventArgs(toolType));
                }
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
            if (this.toolStripEx != null)
            {
                this.toolStripEx.Items.Clear();
            }

            this.imageList = new ImageList();
            this.imageList.ColorDepth = ColorDepth.Depth32Bit;
            this.imageList.TransparentColor = Utility.TransparentKey;

            this.toolStripEx.ImageList = this.imageList;

            ToolStripItem[] buttons = new ToolStripItem[toolInfos.Length];

            for (int i = 0; i < toolInfos.Length; ++i)
            {
                ToolInfo toolInfo = toolInfos[i];
                ToolStripButton button = new ToolStripButton();
                int imageIndex = imageList.Images.Add((Image)toolInfo.Image.Clone(), imageList.TransparentColor);
                button.ImageIndex = imageIndex;
                button.Tag = toolInfo.ToolType;
                button.ToolTipText = toolInfo.Name + " (" + char.ToUpperInvariant(toolInfo.HotKey) + ")";
                buttons[i] = button;
            }

            this.toolStripEx.Items.AddRange(buttons);
        }

        public void SelectTool(Type toolType)
        {
            SelectTool(toolType, true);
        }

        public void SelectTool(Type toolType, bool raiseEvent)
        {
            if (!raiseEvent)
            {
                ++this.ignoreToolClicked;
            }

            try
            {
                foreach (ToolStripButton button in this.toolStripEx.Items)
                {
                    if ((Type)button.Tag == toolType)
                    {
                        toolStripEx_ItemClicked(this, new ToolStripItemClickedEventArgs(button));
                        return;
                    }
                }

                throw new ArgumentException("Tool type not found");
            }

            finally
            {
                if (!raiseEvent)
                {
                    --this.ignoreToolClicked;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.colorDisplayWidget.Left = (this.ClientRectangle.Width - this.colorDisplayWidget.Width) / 2;

            this.toleranceSlider.Location = new Point(1, this.toolStripEx.Bottom + 1);
            this.toleranceSlider.Width = UI.ScaleWidth(45);
            this.toleranceSlider.Height = UI.ScaleHeight(16);

            this.ClientSize = new Size(
                this.toolStripEx.Width + this.onePxSpacingLeft.Width,
                colorDisplayWidget.Height + toleranceSlider.Height + this.toolStripEx.Height);
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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorDisplayWidget = new PaintDotNet.ColorDisplayWidget();
            this.toleranceSlider = new PaintDotNet.ToleranceSliderControl();
            this.toolStripEx = new ToolStripEx();
            this.onePxSpacingLeft = new Control();
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
            this.toleranceSlider.Dock = DockStyle.Bottom;
            this.toleranceSlider.Name = "toleranceSlider";
            this.toleranceSlider.Size = new System.Drawing.Size(44, 16);
            this.toleranceSlider.TabIndex = 0;
            this.toleranceSlider.Tolerance = 0.5f;
            //
            // toolStripEx
            //
            this.toolStripEx.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripEx.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripEx.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStripEx.ItemClicked += new ToolStripItemClickedEventHandler(toolStripEx_ItemClicked);
            this.toolStripEx.Name = "toolStripEx";
            this.toolStripEx.AutoSize = true;
            this.toolStripEx.RelinquishFocusRequest += new EventHandler(toolStripEx_RelinquishFocusRequest);
            //
            // onePxSpacingLeft
            //
            this.onePxSpacingLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.onePxSpacingLeft.Width = 1;
            this.onePxSpacingLeft.Name = "onePxSpacingLeft";
            // 
            // MainToolBar
            // 
            this.Controls.Add(this.toolStripEx);
            this.Controls.Add(this.onePxSpacingLeft);
            this.Controls.Add(this.toleranceSlider);
            this.Controls.Add(this.colorDisplayWidget);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Name = "MainToolBar";
            this.Size = new System.Drawing.Size(48, 328);
            this.ResumeLayout(false);
        }

        public event EventHandler RelinquishFocusRequest;

        private void OnRelinquishFocusRequest()
        {
            if (RelinquishFocusRequest != null)
            {
                RelinquishFocusRequest(this, EventArgs.Empty);
            }
        }

        void toolStripEx_RelinquishFocusRequest(object sender, EventArgs e)
        {
            OnRelinquishFocusRequest();
        }

        void toolStripEx_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripButton button in this.toolStripEx.Items)
            {
                button.Checked = (button == e.ClickedItem);
            }

            OnToolClicked((Type)e.ClickedItem.Tag);
        }
    }
}

