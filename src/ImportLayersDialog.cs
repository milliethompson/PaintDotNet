/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
	/// <summary>
	/// Summary description for ImportLayersDialog.
	/// </summary>
	public class ImportLayersDialog 
        : PdnBaseDialog
	{
        private PaintDotNet.DocumentView documentView;
        private PaintDotNet.LayerControl layerControl;
        private Surface renderSurface;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button allButton;
        private System.Windows.Forms.Button noneButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Size layerControlInitialSize;
        private Size documentViewInitialSize;

        [Browsable(false)]
        public Surface RenderSurface
        {
            set
            {
                if (this.renderSurface == null && this.Document == null)
                {
                    this.renderSurface = value;
                }
                else
                {
                    throw new InvalidOperationException("may only be set before the Document property");
                }
            }
        }

        [Browsable(false)]
        public Document Document
        {
            get
            {
                return this.layerControl.Document;
            }

            set
            {
                if (this.renderSurface != null)
                {
                    this.documentView.SetRenderSurface(this.renderSurface);
                }

                this.layerControl.Document = value;
                this.documentView.Document = value;
            }
        }

        [Browsable(false)]
        public bool[] SelectedLayers
        {
            get
            {
                bool[] selected;
                
                if (this.Document != null)
                {
                    selected = new bool[Document.Layers.Count];
                    int index = 0;

                    foreach (Layer layer in Document.Layers)
                    {
                        selected[index] = layer.Visible;
                        ++index;
                    }
                }
                else
                {
                    selected = new bool[0];
                }

                return selected;
            }
        }

		public ImportLayersDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            Image icon = Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp");
            this.Icon = Utility.ImageToIcon(icon);

            this.layerControlInitialSize = layerControl.Size;
            this.documentViewInitialSize = documentView.Size;
            this.EnableInstanceOpacity = false;
            this.MinimumSize = this.Size;
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            this.baseOkButton.Select();
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
            this.documentView = new PaintDotNet.DocumentView();
            this.layerControl = new PaintDotNet.LayerControl();
            this.allButton = new System.Windows.Forms.Button();
            this.noneButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // baseOkButton
            // 
            this.baseOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseOkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseOkButton.Location = new System.Drawing.Point(438, 284);
            this.baseOkButton.Name = "baseOkButton";
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseCancelButton.Location = new System.Drawing.Point(526, 284);
            this.baseCancelButton.Name = "baseCancelButton";
            // 
            // documentView
            // 
            this.documentView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.documentView.Document = null;
            this.documentView.DrawGrid = false;
            this.documentView.EnableOutlineAnimation = true;
            this.documentView.EnableSelectionInterior = true;
            this.documentView.EnableSelectionOutline = true;
            this.documentView.Location = new System.Drawing.Point(248, 28);
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.Size = new System.Drawing.Size(360, 248);
            this.documentView.TabStop = false;
            // 
            // layerControl
            // 
            this.layerControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.layerControl.Document = null;
            this.layerControl.Location = new System.Drawing.Point(8, 28);
            this.layerControl.Name = "layerControl";
            this.layerControl.Size = new System.Drawing.Size(224, 248);
            this.layerControl.TabIndex = 4;
            this.layerControl.Workspace = null;
            this.layerControl.TabStop = false;
            // 
            // allButton
            // 
            this.allButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.allButton.Location = new System.Drawing.Point(8, 284);
            this.allButton.Name = "allButton";
            this.allButton.TabIndex = 5;
            this.allButton.Text = "All";
            this.allButton.Click += new System.EventHandler(this.allButton_Click);
            // 
            // noneButton
            // 
            this.noneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.noneButton.Location = new System.Drawing.Point(96, 284);
            this.noneButton.Name = "noneButton";
            this.noneButton.TabIndex = 6;
            this.noneButton.Text = "None";
            this.noneButton.Click += new System.EventHandler(this.noneButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "Layers to insert:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(248, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Preview:";
            // 
            // ImportLayersDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(616, 315);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.noneButton);
            this.Controls.Add(this.allButton);
            this.Controls.Add(this.layerControl);
            this.Controls.Add(this.documentView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "ImportLayersDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Layers";
            this.Controls.SetChildIndex(this.documentView, 0);
            this.Controls.SetChildIndex(this.layerControl, 0);
            this.Controls.SetChildIndex(this.allButton, 0);
            this.Controls.SetChildIndex(this.noneButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.ResumeLayout(false);

        }
		#endregion

        protected override void OnLayout(LayoutEventArgs e)
        {
            Size delta = new Size(this.Width - this.MinimumSize.Width, this.Height - this.MinimumSize.Height);
            this.layerControl.Height = layerControlInitialSize.Height + delta.Height;

            this.documentView.Size = new Size(documentViewInitialSize.Width + delta.Width,
                documentViewInitialSize.Height + delta.Height);

            this.documentView.ZoomToWindow();
            
            base.OnLayout(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.documentView != null)
            {
                this.documentView.ZoomToWindow();
                this.documentView.PanelAutoScroll = true;
                this.documentView.PanelAutoScroll = false;
            }

            base.OnResize (e);
        }

        private void allButton_Click(object sender, System.EventArgs e)
        {
            foreach (LayerElement layer in layerControl.Layers)
            {
                layer.Layer.Visible = true;
            }
        }

        private void noneButton_Click(object sender, System.EventArgs e)
        {
            foreach (LayerElement layer in layerControl.Layers)
            {
                layer.Layer.Visible = false;
            }
        }	
    }
}
