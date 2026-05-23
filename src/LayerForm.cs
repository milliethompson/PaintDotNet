/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class LayerForm
        : FloatingToolForm
    {
        private PaintDotNet.LayerControl layerControl;
        private System.Windows.Forms.ImageList imageList;
        private PaintDotNet.SystemLayer.ToolStripEx toolStrip;
        private ToolStripButton addNewLayerButton;
        private ToolStripButton deleteLayerButton;
        private ToolStripButton duplicateLayerButton;
        private ToolStripButton moveLayerUpButton;
        private ToolStripButton moveLayerDownButton;
        private ToolStripButton propertiesButton;
        private System.ComponentModel.IContainer components;

        public LayerControl LayerControl
        {
            get
            {
                return layerControl;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                foreach (LayerElement le in this.layerControl.Layers)
                {
                    le.RefreshPreview();
                }
            }

            base.OnVisibleChanged (e);
        }

        public LayerForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            imageList.TransparentColor = Utility.TransparentKey;

            toolStrip.ImageList = this.imageList;

            int addNewLayerIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuLayersAddNewLayerIcon.png"), imageList.TransparentColor);
            int deleteLayerIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuLayersDeleteLayerIcon.png"), imageList.TransparentColor);
            int moveLayerUpIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuLayersMoveLayerUpIcon.png"), imageList.TransparentColor);
            int moveLayerDownIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuLayersMoveLayerDownIcon.png"), imageList.TransparentColor);
            int duplicateLayerIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditCopyIcon.png"), imageList.TransparentColor);
            int propertiesIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuLayersLayerPropertiesIcon.png"), imageList.TransparentColor);

            addNewLayerButton.ImageIndex = addNewLayerIndex;
            deleteLayerButton.ImageIndex = deleteLayerIndex;
            moveLayerUpButton.ImageIndex = moveLayerUpIndex;
            moveLayerDownButton.ImageIndex = moveLayerDownIndex;
            duplicateLayerButton.ImageIndex = duplicateLayerIndex;
            propertiesButton.ImageIndex = propertiesIndex;

            layerControl.KeyUp += new KeyEventHandler(layerControl_KeyUp);

            this.Text = PdnResources.GetString("LayerForm.Text");
            this.addNewLayerButton.ToolTipText = PdnResources.GetString("LayerForm.AddNewLayerButton.ToolTipText");
            this.deleteLayerButton.ToolTipText = PdnResources.GetString("LayerForm.DeleteLayerButton.ToolTipText");
            this.duplicateLayerButton.ToolTipText = PdnResources.GetString("LayerForm.DuplicateLayerButton.ToolTipText");
            this.moveLayerUpButton.ToolTipText = PdnResources.GetString("LayerForm.MoveLayerUpButton.ToolTipText");
            this.moveLayerDownButton.ToolTipText = PdnResources.GetString("LayerForm.MoveLayerDownButton.ToolTipText");
            this.propertiesButton.ToolTipText = PdnResources.GetString("LayerForm.PropertiesButton.ToolTipText");

            this.MinimumSize = this.Size;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (layerControl != null)
            {
                layerControl.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - 
                    (this.toolStrip.Height + (ClientRectangle.Height - ClientRectangle.Bottom)));
            }
        }

        /// <summary>
        /// Event Handler for New Layer Button Click
        /// </summary>
        public event EventHandler NewLayerButtonClick;
        private void OnNewLayerButtonClick()
        {
            if (NewLayerButtonClick != null)
            {
                NewLayerButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event Handler for Delete Layer Button Click
        /// -CT
        /// </summary>
        public event EventHandler DeleteLayerButtonClick;
        private void OnDeleteLayerButtonClick()
        {
            if (DeleteLayerButtonClick != null)
            {
                DeleteLayerButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event Handler for Duplicate Layer Button Click
        /// -Rick
        /// </summary>
        public event EventHandler DuplicateLayerButtonClick;
        private void OnDuplicateLayerButtonClick()
        {
            if (DuplicateLayerButtonClick != null)
            {
                DuplicateLayerButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event Handler for Move Layer Up Button Click
        /// -CT
        /// </summary>
        public event EventHandler MoveLayerUpButtonClick;
        private void OnMoveLayerUpButtonClick()
        {
            if (MoveLayerUpButtonClick != null)
            {
                MoveLayerUpButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for move layer down button clicked
        /// -Ct
        /// </summary>
        public event EventHandler MoveLayerDownButtonClick;
        private void OnMoveLayerDownButtonClick()
        {
            if (MoveLayerDownButtonClick != null)
            {
                MoveLayerDownButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for when the properties button is clicked
        /// </summary>
        public event EventHandler PropertiesButtonClick;
        private void OnPropertiesButtonClick()
        {
            if (PropertiesButtonClick != null)
            {
                PropertiesButtonClick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Simulates pressing on the New Layer button
        /// </summary>
        public void PerformNewLayerClick()
        {
            this.OnNewLayerButtonClick();
        }

        public void PerformDeleteLayerClick()
        {
            this.OnDeleteLayerButtonClick();
        }

        public void PerformDuplicateLayerClick()
        {
            this.OnDuplicateLayerButtonClick();
        }

        public void PerformMoveLayerUpClick()
        {
            this.OnMoveLayerUpButtonClick();
        }

        public void PerformMoveLayerDownClick()
        {
            this.OnMoveLayerDownButtonClick();
        }

        public void PerformPropertiesClick()
        {
            this.OnPropertiesButtonClick();
        }

        private void newLayerButton_Click(object sender, System.EventArgs e)
        {
            OnNewLayerButtonClick();
        }

        private void deleteLayerButton_Click(object sender, System.EventArgs e)
        {
            OnDeleteLayerButtonClick();
        }

        private void duplicateLayerButton_Click(object sender, System.EventArgs e)
        {
            OnDuplicateLayerButtonClick();
        }

        private void moveUpButton_Click(object sender, System.EventArgs e)
        {
            OnMoveLayerUpButtonClick();
        }

        private void moveDownButton_Click(object sender, System.EventArgs e)
        {
            OnMoveLayerDownButtonClick();
        }

        private void propertiesButton_Click(object sender, System.EventArgs e)
        {
            OnPropertiesButtonClick();
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layerControl = new PaintDotNet.LayerControl();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip = new PaintDotNet.SystemLayer.ToolStripEx();
            this.addNewLayerButton = new System.Windows.Forms.ToolStripButton();
            this.deleteLayerButton = new System.Windows.Forms.ToolStripButton();
            this.duplicateLayerButton = new System.Windows.Forms.ToolStripButton();
            this.moveLayerUpButton = new System.Windows.Forms.ToolStripButton();
            this.moveLayerDownButton = new System.Windows.Forms.ToolStripButton();
            this.propertiesButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // layerControl
            // 
            this.layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControl.Document = null;
            this.layerControl.Location = new System.Drawing.Point(0, 0);
            this.layerControl.Name = "layerControl";
            this.layerControl.Size = new System.Drawing.Size(160, 158);
            this.layerControl.TabIndex = 5;
            this.layerControl.Workspace = null;
            this.layerControl.SelectedLayerChanged += new PaintDotNet.LayerEventHandler(this.layerControl_ClickOnLayer);
            this.layerControl.ClickedOnLayer += new PaintDotNet.LayerEventHandler(this.layerControl_ClickOnLayer);
            this.layerControl.DoubleClickedOnLayer += new PaintDotNet.LayerEventHandler(this.layerControl_DoubleClickedOnLayer);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                                                                        this.addNewLayerButton,
                                                                                        this.deleteLayerButton,
                                                                                        this.duplicateLayerButton,
                                                                                        this.moveLayerUpButton,
                                                                                        this.moveLayerDownButton,
                                                                                        this.propertiesButton
                                                                                   });
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new System.Drawing.Point(0, 132);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(160, 26);
            this.toolStrip.TabIndex = 7;
            this.toolStrip.TabStop = true;
            this.toolStrip.RelinquishFocusRequest += new EventHandler(toolStrip_RelinquishFocusRequest);
            // 
            // addNewLayerButton
            // 
            this.addNewLayerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addNewLayerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNewLayerButton.Name = "addNewLayerButton";
            this.addNewLayerButton.Size = new System.Drawing.Size(23, 4);
            this.addNewLayerButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // deleteLayerButton
            // 
            this.deleteLayerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteLayerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteLayerButton.Name = "deleteLayerButton";
            this.deleteLayerButton.Size = new System.Drawing.Size(23, 4);
            this.deleteLayerButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // duplicateLayerButton
            // 
            this.duplicateLayerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.duplicateLayerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.duplicateLayerButton.Name = "duplicateLayerButton";
            this.duplicateLayerButton.Size = new System.Drawing.Size(23, 4);
            this.duplicateLayerButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // moveLayerUpButton
            // 
            this.moveLayerUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveLayerUpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.moveLayerUpButton.Name = "moveLayerUpButton";
            this.moveLayerUpButton.Size = new System.Drawing.Size(23, 4);
            this.moveLayerUpButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // moveLayerDownButton
            // 
            this.moveLayerDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveLayerDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.moveLayerDownButton.Name = "moveLayerDownButton";
            this.moveLayerDownButton.Size = new System.Drawing.Size(23, 4);
            this.moveLayerDownButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // propertiesButton
            // 
            this.propertiesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.propertiesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new System.Drawing.Size(23, 4);
            this.propertiesButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // LayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(160, 158);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.layerControl);
            this.Name = "LayerForm";
            this.Controls.SetChildIndex(this.layerControl, 0);
            this.Controls.SetChildIndex(this.toolStrip, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        void toolStrip_RelinquishFocusRequest(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }

        private void DetermineButtonEnableStates()
        {
            DetermineButtonEnableStates(this.layerControl.SelectedLayer);
        }

        private void DetermineButtonEnableStates(int index)
        {
            // Find a reason to disable the Move Layer Down button
            if (index == 0)
            {
                moveLayerDownButton.Enabled = false;
            }
            else
            {
                moveLayerDownButton.Enabled = true;
            }

            // Find a reason to disable the Move Layer Up button
            if (index == (layerControl.Workspace.Document.Layers.Count - 1))
            {
                moveLayerUpButton.Enabled = false;
            }
            else
            {
                moveLayerUpButton.Enabled = true;
            }

            // Find reasons to disable the Delete Layer button
            if (layerControl.Workspace.Document.Layers.Count <= 1)
            {
                deleteLayerButton.Enabled = false;
            }
            else
            {
                deleteLayerButton.Enabled = true;
            }
        }
        
        private void layerControl_ClickOnLayer(object sender, PaintDotNet.LayerEventArgs ce)
        {
            int index = layerControl.Workspace.Document.Layers.IndexOf(ce.Layer);
            DetermineButtonEnableStates(index);
        }

        private void layerControl_DoubleClickedOnLayer(object sender, PaintDotNet.LayerEventArgs ce)
        {
            OnPropertiesButtonClick();
            this.OnRelinquishFocus();
        }

        private void layerControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
            {
                this.OnDeleteLayerButtonClick();
                e.Handled = true;
                return;
            }
        }

        private void OnToolStripButtonClick(object sender, EventArgs e)
        {
            SystemLayer.UI.SetControlRedraw(this.layerControl, false);

            if (sender == addNewLayerButton)
            {
                this.OnNewLayerButtonClick();
            }
            else if (sender == deleteLayerButton)
            {
                this.OnDeleteLayerButtonClick();
            }
            else if (sender == duplicateLayerButton)
            {
                this.OnDuplicateLayerButtonClick();
            }
            else if (sender == moveLayerUpButton)
            {
                this.OnMoveLayerUpButtonClick();
            }
            else if (sender == moveLayerDownButton)
            {
                this.OnMoveLayerDownButtonClick();
            }

            SystemLayer.UI.SetControlRedraw(this.layerControl, true);
            this.layerControl.Invalidate(true);

            if (sender == propertiesButton)
            {
                this.OnPropertiesButtonClick();
            }

            DetermineButtonEnableStates();
            OnRelinquishFocus();
        }
    }
}
