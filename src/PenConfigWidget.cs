using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for PenConfigWidget.
	/// </summary>
	public class PenConfigWidget : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ErrorProvider penSizeErrorProvider;
		private DotNetWidgets.DotNetToolbar dotNetToolbar;
		private DotNetWidgets.DotNetToolbarLabelItem dotNetToolbarLabelItem1;
		private DotNetWidgets.DotNetToolbarIconButtonItem PlaceHolderButton;
		private DotNetWidgets.DotNetToolbarComboBoxItem sizeComboBoxTB;
		private DotNetWidgets.FlatComboBox sizeComboBox;
		private System.Windows.Forms.ErrorProvider errorProvider1;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PenConfigWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			// set the ErrorProvider for sizeComboBox data entry control.
			
			penSizeErrorProvider.SetIconAlignment (this.sizeComboBox, ErrorIconAlignment.MiddleRight);
			penSizeErrorProvider.SetIconPadding (this.sizeComboBox, 2);
			
			this.sizeComboBox = (DotNetWidgets.FlatComboBox)sizeComboBoxTB.ContainedControl;
			this.sizeComboBox.InitialText = "";
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
			this.sizeComboBox.Location = new System.Drawing.Point(32, 4);
			this.sizeComboBox.Name = "sizeComboBox";
			this.sizeComboBox.Size = new System.Drawing.Size(48, 21);
			this.sizeComboBox.TabIndex = 9;
			this.sizeComboBox.Text = "2";
			this.sizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.sizeComboBox_Validating);
			this.sizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
			sizeComboBoxTB.Text = "2";
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
            this.sizeComboBox = new DotNetWidgets.FlatComboBox();
            this.penSizeErrorProvider = new System.Windows.Forms.ErrorProvider();
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.dotNetToolbarLabelItem1 = new DotNetWidgets.DotNetToolbarLabelItem();
            this.sizeComboBoxTB = new DotNetWidgets.DotNetToolbarComboBoxItem();
            this.PlaceHolderButton = new DotNetWidgets.DotNetToolbarIconButtonItem();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
            this.SuspendLayout();
            // 
            // sizeComboBox
            // 
            this.sizeComboBox.InitialText = "";
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
            this.sizeComboBox.Location = new System.Drawing.Point(32, 4);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(48, 21);
            this.sizeComboBox.TabIndex = 9;
            this.sizeComboBox.Text = "2";
            this.sizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.sizeComboBox_Validating);
            this.sizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
            // 
            // penSizeErrorProvider
            // 
            this.penSizeErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.penSizeErrorProvider.ContainerControl = this;
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.dotNetToolbarLabelItem1);
            this.dotNetToolbar.Buttons.Add(this.sizeComboBoxTB);
            this.dotNetToolbar.Buttons.Add(this.PlaceHolderButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.Size = new System.Drawing.Size(168, 27);
            this.dotNetToolbar.TabIndex = 10;
            // 
            // dotNetToolbarLabelItem1
            // 
            this.dotNetToolbarLabelItem1.BeginGroup = true;
            this.dotNetToolbarLabelItem1.Text = "Brush Width:";
            // 
            // sizeComboBoxTB
            // 
            this.sizeComboBoxTB.ControlWidth = 48;
            this.sizeComboBoxTB.Text = "";
            // 
            // PlaceHolderButton
            // 
            this.PlaceHolderButton.Enabled = false;
            this.PlaceHolderButton.Icon = null;
            this.PlaceHolderButton.IdealSize = new System.Drawing.Size(16, 16);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // PenConfigWidget
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Controls.Add(this.sizeComboBox);
            this.Name = "PenConfigWidget";
            this.Size = new System.Drawing.Size(168, 27);
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
					penSizeErrorProvider.SetError(this.sizeComboBox, "Invalid number");
				}
				else
				{
					if ((float.Parse(this.sizeComboBox.Text) < 1))
					{
						// Set the error if the size is too small.
						penSizeErrorProvider.SetError(this.sizeComboBox, "Size is smaller than 1");
					}
					else if ((float.Parse(this.sizeComboBox.Text) > 100 ))
					{
						// Set the error if the size is too large.
						penSizeErrorProvider.SetError(this.sizeComboBox, "Size is larger than 100");
					}
					else 
					{
						// Clear the error, if any, in the error provider.
						penSizeErrorProvider.SetError(this.sizeComboBox, "");
						OnPenChanged();
					}
				}
			}
			catch (FormatException)
			{
				e.Cancel = true;
			}
		
		}
	}
}
