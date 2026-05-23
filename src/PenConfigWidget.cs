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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for PenConfigWidget.
	/// </summary>
	public class PenConfigWidget : System.Windows.Forms.UserControl
	{
        private System.Windows.Forms.ComboBox sizeComboBox;
		private DotNetWidgets.DotNetToolbar tbPenConfig;
		private DotNetWidgets.DotNetToolbarLabelItem lblBrushWidth;
		private System.Windows.Forms.ToolTip tooltipProvider;
		private System.ComponentModel.IContainer components;

		public PenConfigWidget()
		{
			InitializeComponent();
    	}

		public event EventHandler PenChanged;
		protected virtual void OnPenChanged()
		{
			if (PenChanged != null)
			{
				PenChanged(this, EventArgs.Empty);
			}
		}

		public void PerformPenChanged()
		{
			OnPenChanged();
		}

        public PenInfo PenInfo
        {
            get
            {
                return new PenInfo(DashStyle.Solid, float.Parse(this.sizeComboBox.Text));   
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
			this.components = new System.ComponentModel.Container();
			this.sizeComboBox = new System.Windows.Forms.ComboBox();
			this.tbPenConfig = new DotNetWidgets.DotNetToolbar();
			this.lblBrushWidth = new DotNetWidgets.DotNetToolbarLabelItem();
			this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// sizeComboBox
			// 
			this.sizeComboBox.ItemHeight = 13;
			this.sizeComboBox.Items.AddRange(new object[] {
															  "1",
															  "2",
															  "3",
															  "4",
															  "5",
															  "6",
															  "7",
															  "8",
															  "9",
															  "10",
															  "11",
															  "12",
															  "13",
															  "14",
															  "15",
															  "20",
															  "25",
															  "30",
															  "35",
															  "40",
															  "45",
															  "50",
															  "55",
															  "60",
															  "65",
															  "70",
															  "75",
															  "80",
															  "85",
															  "90",
															  "95",
															  "100"});
			this.sizeComboBox.Location = new System.Drawing.Point(87, 3);
			this.sizeComboBox.Name = "sizeComboBox";
			this.sizeComboBox.Size = new System.Drawing.Size(44, 21);
			this.sizeComboBox.TabIndex = 9;
			this.sizeComboBox.Text = "2";
			this.sizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.sizeComboBox_Validating);
			this.sizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
			// 
			// tbPenConfig
			// 
			this.tbPenConfig.Buttons.Add(this.lblBrushWidth);
			this.tbPenConfig.DrawGrabHandle = false;
			this.tbPenConfig.ImageList = null;
			this.tbPenConfig.Location = new System.Drawing.Point(0, 0);
			this.tbPenConfig.MenuProvider = null;
			this.tbPenConfig.Name = "tbPenConfig";
			this.tbPenConfig.Size = new System.Drawing.Size(133, 26);
			this.tbPenConfig.TabIndex = 12;
			// 
			// lblBrushWidth
			// 
			this.lblBrushWidth.BeginGroup = true;
			this.lblBrushWidth.Text = "Brush Width:";
			// 
			// PenConfigWidget
			// 
			this.Controls.Add(this.sizeComboBox);
			this.Controls.Add(this.tbPenConfig);
			this.Name = "PenConfigWidget";
			this.Size = new System.Drawing.Size(133, 26);
			this.ResumeLayout(false);

		}
		#endregion


		private void sizeComboBox_TextChanged(object sender, System.EventArgs e)
		{
			this.Validate();
		}

		private void sizeComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				bool invalid = false;

				try
				{
					float number = float.Parse(this.sizeComboBox.Text);
				}

				catch (FormatException)
				{
					invalid = true;
				}

				catch (OverflowException)
				{
					invalid = true;
				}

				if (invalid)
				{
					this.sizeComboBox.BackColor = Color.Red;
					this.tooltipProvider.SetToolTip(this.sizeComboBox, "ERROR: Invalid Number");
				}
				else
				{
					if (float.Parse(this.sizeComboBox.Text) < 1)
					{
						// Set the error if the size is too small.
						this.sizeComboBox.BackColor = Color.Red;
						this.tooltipProvider.SetToolTip(this.sizeComboBox, "ERROR: Size is smaller than 1");
					}
					else if ((float.Parse(this.sizeComboBox.Text) > 100 ))
					{
						// Set the error if the size is too large.
						this.sizeComboBox.BackColor = Color.Red;
						this.tooltipProvider.SetToolTip(this.sizeComboBox, "Size is larger than 100");
					}
					else 
					{
						// Clear the error, if any, in the error provider.
						this.sizeComboBox.BackColor = SystemColors.Window;
						this.tooltipProvider.RemoveAll();
						OnPenChanged();
					}
				}
			}

			catch (FormatException)
			{
			}
		}
	}
}
