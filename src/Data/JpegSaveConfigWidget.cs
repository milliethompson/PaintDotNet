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
using System.Windows.Forms;

namespace PaintDotNet
{
	public class JpegSaveConfigWidget : PaintDotNet.SaveConfigWidget
	{
        private System.Windows.Forms.TrackBar qualitySlider;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown qualityUpDown;
		private System.ComponentModel.IContainer components = null;

		public JpegSaveConfigWidget()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

        protected override void InitFileType()
        {
            this.fileType = new JpegFileType();
        }

        protected override void InitTokenFromWidget()
        {
            ((JpegSaveConfigToken)this.Token).Quality = this.qualitySlider.Value;
        }

        protected override void InitWidgetFromToken(SaveConfigToken token)
        {
            this.qualitySlider.Value = ((JpegSaveConfigToken)token).Quality;
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.qualitySlider = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.qualityUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.qualitySlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // qualitySlider
            // 
            this.qualitySlider.Location = new System.Drawing.Point(0, 24);
            this.qualitySlider.Maximum = 100;
            this.qualitySlider.Minimum = 1;
            this.qualitySlider.Name = "qualitySlider";
            this.qualitySlider.Size = new System.Drawing.Size(168, 45);
            this.qualitySlider.TabIndex = 1;
            this.qualitySlider.TickFrequency = 10;
            this.qualitySlider.Value = 1;
            this.qualitySlider.ValueChanged += new System.EventHandler(this.qualitySlider_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Quality:";
            // 
            // qualityUpDown
            // 
            this.qualityUpDown.Location = new System.Drawing.Point(104, 0);
            this.qualityUpDown.Minimum = new System.Decimal(new int[] {
                                                                          1,
                                                                          0,
                                                                          0,
                                                                          0});
            this.qualityUpDown.Name = "qualityUpDown";
            this.qualityUpDown.Size = new System.Drawing.Size(56, 20);
            this.qualityUpDown.TabIndex = 0;
            this.qualityUpDown.Value = new System.Decimal(new int[] {
                                                                        1,
                                                                        0,
                                                                        0,
                                                                        0});
            this.qualityUpDown.Enter += new System.EventHandler(this.qualityUpDown_Enter);
            this.qualityUpDown.ValueChanged += new System.EventHandler(this.qualityUpDown_ValueChanged);
            this.qualityUpDown.Leave += new System.EventHandler(this.qualityUpDown_Leave);
            // 
            // JpegSaveConfigWidget
            // 
            this.Controls.Add(this.qualityUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.qualitySlider);
            this.Name = "JpegSaveConfigWidget";
            this.Size = new System.Drawing.Size(176, 80);
            ((System.ComponentModel.ISupportInitialize)(this.qualitySlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

        private void qualitySlider_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.qualityUpDown.Value != (decimal)this.qualitySlider.Value)
            {
                this.qualityUpDown.Value = (decimal)this.qualitySlider.Value;
            }

            UpdateToken();
        }

        private void qualityUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.qualitySlider.Value != (int)this.qualityUpDown.Value)
            {
                this.qualitySlider.Value = (int)this.qualityUpDown.Value;
            }
        }

        private void qualityUpDown_Leave(object sender, System.EventArgs e)
        {
            qualityUpDown_ValueChanged(sender, e);
        }

        private void qualityUpDown_Enter(object sender, System.EventArgs e)
        {
            qualityUpDown.Select(0, qualityUpDown.Text.Length);
        }
	}
}

