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
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem rulersToggleButton;
        private DotNetWidgets.DotNetToolbarButtonItem drawGridToggleButton;
        private System.Windows.Forms.Label unitsLabel;
        private System.ComponentModel.IContainer components;
        private PaintDotNet.UnitsComboBox unitsComboBox;
        
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

        public MeasurementUnit Units
        {
            get
            {
                return this.unitsComboBox.Units;
            }

            set
            {
                this.unitsComboBox.Units = value;
            }
        }

        public WorkspaceOptionsConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            int gridIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuViewGridIcon.bmp"), imageList.TransparentColor);
            int rulersIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuViewRulersIcon.bmp"), imageList.TransparentColor);

            drawGridToggleButton.ImageIndex = gridIndex;
            rulersToggleButton.ImageIndex = rulersIndex;

            this.drawGridToggleButton.ToolTipText = PdnResources.GetString("WorkspaceOptionsConfigWidget.DrawGridToggleButton.ToolTipText");
            this.rulersToggleButton.ToolTipText = PdnResources.GetString("WorkspaceOptionsConfigWidget.RulersToggleButton.ToolTipText");
            this.unitsLabel.Text = PdnResources.GetString("WorkspaceOptionsConfigWidget.UnitsLabel.Text");
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
            this.drawGridToggleButton = ((DotNetWidgets.DotNetToolbarButtonItem)(new DotNetWidgets.DotNetToolbarButtonItem()));
            this.rulersToggleButton = ((DotNetWidgets.DotNetToolbarButtonItem)(new DotNetWidgets.DotNetToolbarButtonItem()));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.unitsLabel = new System.Windows.Forms.Label();
            this.unitsComboBox = new PaintDotNet.UnitsComboBox();
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.drawGridToggleButton);
            this.dotNetToolbar.Buttons.Add(this.rulersToggleButton);
            this.dotNetToolbar.Dock = System.Windows.Forms.DockStyle.None;
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
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // unitsLabel
            // 
            this.unitsLabel.Location = new System.Drawing.Point(60, 1);
            this.unitsLabel.Name = "unitsLabel";
            this.unitsLabel.Size = new System.Drawing.Size(47, 23);
            this.unitsLabel.TabIndex = 1;
            this.unitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // unitsComboBox
            // 
            this.unitsComboBox.Location = new System.Drawing.Point(107, 3);
            this.unitsComboBox.LowercaseStrings = false;
            this.unitsComboBox.Name = "unitsComboBox";
            this.unitsComboBox.UnitsDisplayType = PaintDotNet.UnitsDisplayType.Plural;
            this.unitsComboBox.Size = new System.Drawing.Size(85, 21);
            this.unitsComboBox.TabIndex = 3;
            this.unitsComboBox.Units = PaintDotNet.MeasurementUnit.Pixel;
            this.unitsComboBox.UnitsChanged += new System.EventHandler(this.unitsComboBox_UnitsChanged);
            // 
            // WorkspaceOptionsConfigWidget
            // 
            this.Controls.Add(this.unitsComboBox);
            this.Controls.Add(this.unitsLabel);
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "WorkspaceOptionsConfigWidget";
            this.Size = new System.Drawing.Size(201, 168);
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

        public event EventHandler RulersEnabledChanged;
        protected virtual void OnRulersEnabledChanged()
        {
            if (RulersEnabledChanged != null)
            {
                RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler UnitsChanged;
        protected virtual void OnUnitsChanged()
        {
            if (UnitsChanged != null)
            {
                UnitsChanged(this, EventArgs.Empty);
            }
        }

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == this.rulersToggleButton)
            {
                this.RulersEnabled = !this.RulersEnabled;
            }
            else if (e.Button == this.drawGridToggleButton)
            {
                this.DrawGrid = !this.DrawGrid;
            }
        }

        private void unitsComboBox_UnitsChanged(object sender, System.EventArgs e)
        {
            this.OnUnitsChanged();
        }
    }
}
