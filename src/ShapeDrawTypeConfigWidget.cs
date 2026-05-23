using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DotNetWidgets;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ShapeDrawTypeConfigWidget.
	/// </summary>
	public class ShapeDrawTypeConfigWidget : System.Windows.Forms.UserControl
	{
		private ImageList imageList;
		private ShapeDrawType shape;
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem outlineButton;
        private DotNetWidgets.DotNetToolbarButtonItem interiorButton;
        private DotNetWidgets.DotNetToolbarButtonItem bothButton;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ShapeDrawTypeConfigWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			// Create ImageList
			imageList = new ImageList();
			imageList.ImageSize = new Size(16, 16);
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);

            this.dotNetToolbar.ImageList = imageList;

			int outlineIndex = imageList.Images.Add(Utility.GetImageResource("Icons.ShapeOutlineIcon.bmp"), imageList.TransparentColor);
			int bothIndex = imageList.Images.Add(Utility.GetImageResource("Icons.ShapeBothIcon.bmp"), imageList.TransparentColor);
			int interiorIndex = imageList.Images.Add(Utility.GetImageResource("Icons.ShapeInteriorIcon.bmp"), imageList.TransparentColor);

            outlineButton.ImageIndex = outlineIndex;
            interiorButton.ImageIndex = interiorIndex;
            bothButton.ImageIndex = bothIndex;			
		}

		public event EventHandler ShapeDrawTypeChanged;
		protected virtual void OnShapeDrawTypeChanged()
		{
			if (ShapeDrawTypeChanged != null)
			{
				ShapeDrawTypeChanged(this, EventArgs.Empty);
			}
		}

        public void PerformShapeDrawTypeChanged()
        {
            OnShapeDrawTypeChanged();
        }

		public ShapeDrawType ShapeDrawType
		{
			get 
			{
				return shape;
			}
			set
			{
                if (shape != value)
                {
                    shape = value;
				
                    // if the user sets the shape the buttons must be updated
                    if(shape == ShapeDrawType.Outline)
                    {
                        this.outlineButton.Pushed = true;
                        this.bothButton.Pushed = false;
                        this.interiorButton.Pushed = false;
                    }
                    else if(shape == ShapeDrawType.Both)
                    {
                        this.outlineButton.Pushed = false;
                        this.bothButton.Pushed = true;
                        this.interiorButton.Pushed = false;
                    }
                    else if(shape == ShapeDrawType.Interior)
                    {
                        this.outlineButton.Pushed = false;
                        this.bothButton.Pushed = false;
                        this.interiorButton.Pushed = true;
                    }
                    else
                    {
                        // invalid shape
                        throw new InvalidOperationException("Shape draw type is invalid");
                    }

                    this.OnShapeDrawTypeChanged();
                }
			}
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.outlineButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.interiorButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.bothButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.outlineButton);
            this.dotNetToolbar.Buttons.Add(this.interiorButton);
            this.dotNetToolbar.Buttons.Add(this.bothButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.DrawSeparator = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.Size = new System.Drawing.Size(144, 26);
            this.dotNetToolbar.TabIndex = 1;
            this.dotNetToolbar.EnabledChanged += new System.EventHandler(this.dotNetToolbar_EnabledChanged);
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // outlineButton
            // 
            this.outlineButton.BeginGroup = true;
            this.outlineButton.ToolTipText = "Draw shape outline";
            // 
            // interiorButton
            // 
            this.interiorButton.ToolTipText = "Draw filled shape";
            // 
            // bothButton
            // 
            this.bothButton.ToolTipText = "Draw filled shape with outline";
            // 
            // ShapeDrawTypeConfigWidget
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "ShapeDrawTypeConfigWidget";
            this.Size = new System.Drawing.Size(144, 72);
            this.ResumeLayout(false);

        }
		#endregion

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == outlineButton)
            {
                this.ShapeDrawType = ShapeDrawType.Outline;
            }
            else if (e.Button == bothButton)
            {
                this.ShapeDrawType = ShapeDrawType.Both;
            }
            if (e.Button == interiorButton)
            {
                this.ShapeDrawType = ShapeDrawType.Interior;
            }
        }

        private void dotNetToolbar_EnabledChanged(object sender, System.EventArgs e)
        {
            foreach (DotNetToolbarItem dntbi in dotNetToolbar.Buttons)
            {
                dntbi.Enabled = this.Enabled;
            }
        }
	
	}
}
