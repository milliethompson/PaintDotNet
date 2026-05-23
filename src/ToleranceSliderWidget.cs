using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ToleranceSliderWidget.
	/// </summary>
	public class ToleranceSliderWidget : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TrackBar trackBar;
		private System.Windows.Forms.ToolTip toolTip;
		private System.ComponentModel.IContainer components;

		public int Tolerance
		{
			get
			{
				return trackBar.Value;
			}
		}

		public ToleranceSliderWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
            
			trackBar_ValueChanged(this,EventArgs.Empty);
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
            this.components = new System.ComponentModel.Container();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar
            // 
            this.trackBar.LargeChange = 20;
            this.trackBar.Location = new System.Drawing.Point(0, 0);
            this.trackBar.Maximum = 255;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(48, 45);
            this.trackBar.TabIndex = 0;
            this.trackBar.TickFrequency = 64;
            this.trackBar.Value = 20;
            this.trackBar.KeyUp += new System.Windows.Forms.KeyEventHandler(this.trackBar_KeyUp);
            this.trackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar_MouseUp);
            this.trackBar.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // ToleranceSliderWidget
            // 
            this.Controls.Add(this.trackBar);
            this.Name = "ToleranceSliderWidget";
            this.Size = new System.Drawing.Size(48, 32);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

		private void trackBar_ValueChanged(object sender, System.EventArgs e)
		{
			float currentValue = ((float)this.trackBar.Value / (float)this.trackBar.Maximum) * 100;
			toolTip.SetToolTip(this.trackBar,currentValue.ToString("F0") + "% Tolerance");
		}

		public event EventHandler ToleranceChanged;
		private void OnToleranceChanged()
		{
			if(ToleranceChanged != null)
			{
				ToleranceChanged(this,EventArgs.Empty);
			}	
		}

		private void trackBar_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			OnToleranceChanged();
		}

		private void trackBar_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnToleranceChanged();
		}
	}
}
