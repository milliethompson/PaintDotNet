using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for LayerForm.
	/// </summary>
	public class LayerForm
        : FloatingToolForm
	{
        private PaintDotNet.LayerControl layerControl;
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem addNewLayerButton;
        private DotNetWidgets.DotNetToolbarButtonItem deleteLayerButton;
        private DotNetWidgets.DotNetToolbarButtonItem moveLayerDownButton;
        private DotNetWidgets.DotNetToolbarButtonItem moveLayerUpButton;
        private DotNetWidgets.DotNetToolbarButtonItem duplicateLayerButton;
        private DotNetWidgets.DotNetToolbarButtonItem propertiesButton;
        private System.ComponentModel.IContainer components;

        public LayerControl LayerControl
        {
            get
            {
                return layerControl;
            }
        }

		public LayerForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            int addNewLayerIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersAddNewLayerIcon.bmp"), imageList.TransparentColor);
            int deleteLayerIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersDeleteLayerIcon.bmp"), imageList.TransparentColor);
            int moveLayerUpIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.bmp"), imageList.TransparentColor);
            int moveLayerDownIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersMoveLayerDownIcon.bmp"), imageList.TransparentColor);
            int duplicateLayerIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditCopyIcon.bmp"), imageList.TransparentColor);
            int propertiesIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.bmp"), imageList.TransparentColor);

            addNewLayerButton.ImageIndex = addNewLayerIndex;
            deleteLayerButton.ImageIndex = deleteLayerIndex;
            moveLayerUpButton.ImageIndex = moveLayerUpIndex;
            moveLayerDownButton.ImageIndex = moveLayerDownIndex;
            duplicateLayerButton.ImageIndex = duplicateLayerIndex;
            propertiesButton.ImageIndex = propertiesIndex;

            //
            layerControl.KeyUp += new KeyEventHandler(layerControl_KeyUp);
		}

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            if (layerControl != null)
            {
                layerControl.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - (dotNetToolbar.Height + (ClientRectangle.Height - ClientRectangle.Bottom)));
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.addNewLayerButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.deleteLayerButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.duplicateLayerButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.moveLayerUpButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.moveLayerDownButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.propertiesButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // layerControl
            // 
            this.layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControl.Location = new System.Drawing.Point(0, 0);
            this.layerControl.Name = "layerControl";
            this.layerControl.Size = new System.Drawing.Size(160, 158);
            this.layerControl.TabIndex = 5;
            this.layerControl.Workspace = null;
            this.layerControl.ClickedOnLayer += new PaintDotNet.LayerEventHandler(this.layerControl_ClickOnLayer);
            this.layerControl.SelectedLayerChanged += new PaintDotNet.LayerEventHandler(this.layerControl_ClickOnLayer);
            this.layerControl.DoubleClickedOnLayer += new PaintDotNet.LayerEventHandler(this.layerControl_DoubleClickedOnLayer);
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.addNewLayerButton);
            this.dotNetToolbar.Buttons.Add(this.deleteLayerButton);
            this.dotNetToolbar.Buttons.Add(this.duplicateLayerButton);
            this.dotNetToolbar.Buttons.Add(this.moveLayerUpButton);
            this.dotNetToolbar.Buttons.Add(this.moveLayerDownButton);
            this.dotNetToolbar.Buttons.Add(this.propertiesButton);
            this.dotNetToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = this.imageList;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 132);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.NegotiateToolTips = true;
            this.dotNetToolbar.Size = new System.Drawing.Size(160, 26);
            this.dotNetToolbar.TabIndex = 6;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // addNewLayerButton
            // 
            this.addNewLayerButton.ToolTipText = "Add New Layer";
            // 
            // deleteLayerButton
            // 
            this.deleteLayerButton.ToolTipText = "Delete Layer";
            // 
            // duplicateLayerButton
            // 
            this.duplicateLayerButton.ToolTipText = "Duplicate Layer";
            // 
            // moveLayerUpButton
            // 
            this.moveLayerUpButton.ToolTipText = "Move Layer Up";
            // 
            // moveLayerDownButton
            // 
            this.moveLayerDownButton.ToolTipText = "Move Layer Down";
            // 
            // propertiesButton
            // 
            this.propertiesButton.ToolTipText = "Layer Properties";
            // 
            // imageList
            // 
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // LayerForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(160, 158);
            this.Controls.Add(this.dotNetToolbar);
            this.Controls.Add(this.layerControl);
            this.Name = "LayerForm";
            this.Text = "Layers";
            this.ResumeLayout(false);

        }
		#endregion

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == addNewLayerButton)
            {
                this.OnNewLayerButtonClick();
            }
            else if (e.Button == deleteLayerButton)
            {
                this.OnDeleteLayerButtonClick();
            }
            else if (e.Button == duplicateLayerButton)
            {
                this.OnDuplicateLayerButtonClick();
            }
            else if (e.Button == moveLayerUpButton)
            {
                this.OnMoveLayerUpButtonClick();
            }
            else if (e.Button == moveLayerDownButton)
            {
                this.OnMoveLayerDownButtonClick();
            }
            else if (e.Button == propertiesButton)
            {
                this.OnPropertiesButtonClick();
            }
        }

        private void layerControl_ClickOnLayer(object sender, PaintDotNet.LayerEventArgs ce)
        {
            int index = layerControl.Workspace.Document.Layers.IndexOf(ce.Layer);

            // Find reasons to disable the Move Layer Up button
            if (index == 0 || index == 1)
            {
                moveLayerUpButton.Enabled = false;
            }
            else
            {
                moveLayerUpButton.Enabled = true;
            }

            // Find reasons to disable the Move Layer Down button
            if (index == 0 || index == (layerControl.Workspace.Document.Layers.Count - 1))
            {
                moveLayerDownButton.Enabled = false;
            }
            else
            {
                moveLayerDownButton.Enabled = true;
            }

            // Find reasons to disable the Delete Layer button
            if (index == 0)
            {
                deleteLayerButton.Enabled = false;
            }
            else
            {
                deleteLayerButton.Enabled = true;
            }
        }

        private void layerControl_DoubleClickedOnLayer(object sender, PaintDotNet.LayerEventArgs ce)
        {
            OnPropertiesButtonClick();
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
    }
}
