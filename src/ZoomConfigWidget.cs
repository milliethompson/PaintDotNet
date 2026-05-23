using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	public class ZoomConfigWidget : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ComboBox udZoom;
		private ScaleFactor scaleFactor;
		private DotNetWidgets.DotNetToolbar tbZoomConfig;
		private DotNetWidgets.DotNetToolbarButtonItem btnZoomIn;
		private DotNetWidgets.DotNetToolbarButtonItem btnZoomOut;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ToolTip tooltipProvider;
	
		private ZoomBasis zoomBasis;
		public ZoomBasis ZoomBasis 
		{
			get 
			{
				return zoomBasis;
			}
			set 
			{
				zoomBasis = value;
				/* Call OnZoomBasisChanged regardless of whether or not this is actually
				 * a new value. If this is not done, the document will not be re-fitted
				 * when this is assigned, as expected (Such as in MainForm's DoOpenFile)
				 */
				OnZoomBasisChanged();
			}
		}

		public ScaleFactor ScaleFactor 
		{
			get
			{
				return scaleFactor;
			}
			set 
			{
				if (scaleFactor.Ratio != value.Ratio) 
				{
					scaleFactor = value;
					OnZoomScaleChanged();
				}
			}
		}

		public ZoomConfigWidget()
		{
			InitializeComponent();
			ScaleFactor = new ScaleFactor(1.0f);
			udZoom.SelectedIndex = 4;
			udZoom.DropDownWidth = 1;

			
			Graphics g = Graphics.FromHwnd(udZoom.Handle);
			foreach (string str in udZoom.Items) 
			{
				udZoom.DropDownWidth = (int)Math.Max(udZoom.DropDownWidth, 2 + g.MeasureString(str, udZoom.Font).Width);
			}
			tbZoomConfig.ImageList = imageList;
			imageList.TransparentColor = Color.FromArgb(192, 192, 192);
			btnZoomIn.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuImageZoomInIcon.bmp"), imageList.TransparentColor);            
			btnZoomOut.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuImageZoomOutIcon.bmp"), imageList.TransparentColor);
			btnZoomIn.ToolTipText = "Zoom in";
			btnZoomOut.ToolTipText = "Zoom out";
			zoomBasis = ZoomBasis.Factor;
			ScaleFactor = ScaleFactor.OneToOne;
		}

		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
			this.components = new System.ComponentModel.Container();
			this.udZoom = new System.Windows.Forms.ComboBox();
			this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
			this.tbZoomConfig = new DotNetWidgets.DotNetToolbar();
			this.btnZoomIn = new DotNetWidgets.DotNetToolbarButtonItem();
			this.btnZoomOut = new DotNetWidgets.DotNetToolbarButtonItem();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// udZoom
			// 
			this.udZoom.DropDownWidth = 128;
			this.udZoom.ItemHeight = 13;
			this.udZoom.Items.AddRange(new object[] {
														"1600%",
														"800%",
														"400%",
														"200%",
														"100%",
														"50%",
														"25%",
														"10%",
														"5%",
														"2%",
														"Window",
														"Selection"});
			this.udZoom.Location = new System.Drawing.Point(57, 3);
			this.udZoom.MaxDropDownItems = 99;
			this.udZoom.Name = "udZoom";
			this.udZoom.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.udZoom.Size = new System.Drawing.Size(70, 21);
			this.udZoom.TabIndex = 1;
			this.udZoom.Text = "100%";
			this.udZoom.CausesValidation = true;
			this.udZoom.Validating += new CancelEventHandler(udZoom_Validating);
			this.udZoom.SelectedIndexChanged += new EventHandler(udZoom_SelectedIndexChanged);
			// 
			// tbZoomConfig
			// 
			this.tbZoomConfig.Buttons.Add(this.btnZoomIn);
			this.tbZoomConfig.Buttons.Add(this.btnZoomOut);
			this.tbZoomConfig.DrawGrabHandle = false;
			this.tbZoomConfig.ImageList = null;
			this.tbZoomConfig.Location = new System.Drawing.Point(0, 0);
			this.tbZoomConfig.MenuProvider = null;
			this.tbZoomConfig.Name = "tbZoomConfig";
			this.tbZoomConfig.Size = new System.Drawing.Size(120, 26);
			this.tbZoomConfig.TabIndex = 2;
			this.tbZoomConfig.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.tbZoomConfig_ButtonClick);
			// 
			// btnZoomIn
			// 
			this.btnZoomIn.BeginGroup = true;
			// 
			// imageList
			// 
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// ZoomConfigWidget
			// 
			this.Controls.Add(this.udZoom);
			this.Controls.Add(this.tbZoomConfig);
			this.Name = "ZoomConfigWidget";
			this.Size = new System.Drawing.Size(120, 26);
			this.ResumeLayout(false);

		}
		#endregion
		
		private void SetZoomText() 
		{
			this.udZoom.BackColor = SystemColors.Window;
			switch (zoomBasis) 
			{
				case ZoomBasis.Window: 
					udZoom.Text = "Window";
					break;
				case ZoomBasis.Selection:
					udZoom.Text = "Selection";
					break;
				case ZoomBasis.Factor:
					udZoom.Text = scaleFactor.ToString();
					break;
			}
		}

		public event EventHandler ZoomScaleChanged;
		protected void OnZoomScaleChanged() 
		{
			if (zoomBasis == ZoomBasis.Factor) 
			{
				SetZoomText();
				if (ZoomScaleChanged != null)
				{
					ZoomScaleChanged(this, EventArgs.Empty);
				}
			}
		}

		public void PerformZoomBasisChanged() 
		{
			OnZoomBasisChanged();
		}

		public event EventHandler ZoomBasisChanged;
		protected void OnZoomBasisChanged() 
		{
			SetZoomText();
			if (ZoomBasisChanged != null)
			{
				ZoomBasisChanged(this, EventArgs.Empty);
			}
		}	

		public void PerformZoomScaleChanged()
		{
			OnZoomScaleChanged();
		}

		private void udZoom_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				int val = 1;
				e.Cancel = false;
				if (udZoom.Text == "Window") 
				{
					ZoomBasis = ZoomBasis.Window;
				} 
				else if (udZoom.Text == "Selection")
				{
					ZoomBasis = ZoomBasis.Selection;
				}
				else 
				{
					try
					{
						string text = udZoom.Text;

						if (text[text.Length - 1] == '%')
							text = text.Substring(0, text.Length - 1);

						val = (int)Math.Round(double.Parse(text));
						ZoomBasis = ZoomBasis.Factor;
					}

					catch (FormatException)
					{
						e.Cancel = true;
					}

					catch (OverflowException)
					{
						e.Cancel = true;
					}

					if (e.Cancel)
					{
						this.udZoom.BackColor = Color.Red;
						this.tooltipProvider.SetToolTip(this.udZoom, "ERROR: Invalid number");
					}
					else
					{
						if (val < 1)
						{
							e.Cancel = true;
							this.udZoom.BackColor = Color.Red;
							this.tooltipProvider.SetToolTip(this.udZoom, "ERROR: Zoom must be at least 1%");
						}
						else if (val > 1600)
						{
							e.Cancel = true;
							this.udZoom.BackColor = Color.Red;
							this.tooltipProvider.SetToolTip(this.udZoom, "ERROR: Zoom must not exceed 1600%");
						}
						else 
						{
							// Clear the error, if any, in the error provider.
							e.Cancel = false;
							this.tooltipProvider.RemoveAll();
							this.udZoom.BackColor = SystemColors.Window;
							ScaleFactor = new ScaleFactor(val / 100.0f);
						}
					}
				}
			}
			catch (FormatException)
			{
				;
			}
		}

		private void tbZoomConfig_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
		{
			if (e.Button == btnZoomIn)
			{
				ZoomBasis = ZoomBasis.Factor;
				ScaleFactor = ScaleFactor.GetNextLarger();
			} 
			else if (e.Button == btnZoomOut) 
			{
				ZoomBasis = ZoomBasis.Factor;
				ScaleFactor = ScaleFactor.GetNextSmaller();
			}
		}

		private void udZoom_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.Validate();
		}
	}
}
