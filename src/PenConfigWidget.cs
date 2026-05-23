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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox sizeComboBox;
        private DotNetWidgets.DotNetToolbar dotNetToolbar1;
        private DotNetWidgets.DotNetToolbarButtonItem dotNetToolbarButtonItem1;

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
            this.penSizeErrorProvider = new System.Windows.Forms.ErrorProvider();
            this.label1 = new System.Windows.Forms.Label();
            this.sizeComboBox = new System.Windows.Forms.ComboBox();
            this.dotNetToolbar1 = new DotNetWidgets.DotNetToolbar();
            this.dotNetToolbarButtonItem1 = new DotNetWidgets.DotNetToolbarButtonItem();
            this.SuspendLayout();
            // 
            // penSizeErrorProvider
            // 
            this.penSizeErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.penSizeErrorProvider.ContainerControl = this;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 1);
            this.label1.Name = "label1";
            this.label1.TabIndex = 11;
            this.label1.Text = "Brush Width:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.sizeComboBox.Location = new System.Drawing.Point(84, 3);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(44, 21);
            this.sizeComboBox.TabIndex = 9;
            this.sizeComboBox.Text = "2";
            this.sizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.sizeComboBox_Validating);
            this.sizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
            // 
            // dotNetToolbar1
            // 
            this.dotNetToolbar1.Buttons.Add(this.dotNetToolbarButtonItem1);
            this.dotNetToolbar1.Dock = System.Windows.Forms.DockStyle.None;
            this.dotNetToolbar1.DrawGrabHandle = false;
            this.dotNetToolbar1.ImageList = null;
            this.dotNetToolbar1.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar1.MenuProvider = null;
            this.dotNetToolbar1.Name = "dotNetToolbar1";
            this.dotNetToolbar1.Size = new System.Drawing.Size(32, 26);
            this.dotNetToolbar1.TabIndex = 12;
            // 
            // dotNetToolbarButtonItem1
            // 
            this.dotNetToolbarButtonItem1.BeginGroup = true;
            this.dotNetToolbarButtonItem1.Enabled = false;
            // 
            // PenConfigWidget
            // 
            this.Controls.Add(this.sizeComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dotNetToolbar1);
            this.Name = "PenConfigWidget";
            this.Size = new System.Drawing.Size(152, 48);
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
					if (float.Parse(this.sizeComboBox.Text) < 1)
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
