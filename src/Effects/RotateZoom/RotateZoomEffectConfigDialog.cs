/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for RotateZoomEffectConfigDialog.
    /// </summary>
    public class RotateZoomEffectConfigDialog 
        : EffectConfigDialog
    {
        private PaintDotNet.AngleChooserControl angleChooserControl;
        private System.Windows.Forms.TrackBar zoomSlider;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox keepBackgroundCheckBox;
        private System.Windows.Forms.NumericUpDown angleUpDown;
        private System.Windows.Forms.GroupBox angleGroupBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox zoomGroupBox;
        private System.Windows.Forms.Label zoomLabel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public RotateZoomEffectConfigDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

			this.Text = RotateZoomEffect.StaticName;
			this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.RotateZoomIcon.bmp"));
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new RotateZoomEffectConfigToken(0, 1.0f, false);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            angleChooserControl.ValueDouble = ((RotateZoomEffectConfigToken)effectToken).Angle;
            zoomSlider.Value = (int)(1.0f / ((RotateZoomEffectConfigToken)effectToken).Zoom * 100.0f);
            this.keepBackgroundCheckBox.Checked = ((RotateZoomEffectConfigToken)effectToken).SourceAsBackground;
        }

        protected override void InitTokenFromDialog()
        {
            ((RotateZoomEffectConfigToken)theEffectToken).Angle = (float)angleChooserControl.ValueDouble;
            ((RotateZoomEffectConfigToken)theEffectToken).Zoom = 1.0f / ((float)zoomSlider.Value / 100.0f);
            ((RotateZoomEffectConfigToken)theEffectToken).SourceAsBackground = keepBackgroundCheckBox.Checked;
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
			this.angleChooserControl = new PaintDotNet.AngleChooserControl();
			this.zoomSlider = new System.Windows.Forms.TrackBar();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.keepBackgroundCheckBox = new System.Windows.Forms.CheckBox();
			this.angleUpDown = new System.Windows.Forms.NumericUpDown();
			this.angleGroupBox = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.zoomGroupBox = new System.Windows.Forms.GroupBox();
			this.zoomLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.zoomSlider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).BeginInit();
			this.angleGroupBox.SuspendLayout();
			this.zoomGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// angleChooserControl
			// 
			this.angleChooserControl.Location = new System.Drawing.Point(8, 16);
			this.angleChooserControl.Name = "angleChooserControl";
			this.angleChooserControl.Size = new System.Drawing.Size(56, 56);
			this.angleChooserControl.TabIndex = 1;
			this.angleChooserControl.TabStop = false;
			this.angleChooserControl.Value = 0;
			this.angleChooserControl.ValueDouble = 0;
			this.angleChooserControl.ValueChanged += new System.EventHandler(this.angleChooserControl_ValueChanged);
			// 
			// zoomSlider
			// 
			this.zoomSlider.LargeChange = 50;
			this.zoomSlider.Location = new System.Drawing.Point(8, 24);
			this.zoomSlider.Maximum = 500;
			this.zoomSlider.Name = "zoomSlider";
			this.zoomSlider.Size = new System.Drawing.Size(177, 45);
			this.zoomSlider.TabIndex = 1;
			this.zoomSlider.TickFrequency = 100;
			this.zoomSlider.Value = 1;
			this.zoomSlider.ValueChanged += new System.EventHandler(this.zoomSlider_ValueChanged);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(256, 104);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(336, 104);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// keepBackgroundCheckBox
			// 
			this.keepBackgroundCheckBox.Location = new System.Drawing.Point(16, 96);
			this.keepBackgroundCheckBox.Name = "keepBackgroundCheckBox";
			this.keepBackgroundCheckBox.Size = new System.Drawing.Size(160, 24);
			this.keepBackgroundCheckBox.TabIndex = 2;
			this.keepBackgroundCheckBox.Text = "Don\'t erase background";
			this.keepBackgroundCheckBox.CheckedChanged += new System.EventHandler(this.keepBackgroundCheckBox_CheckedChanged);
			// 
			// angleUpDown
			// 
			this.angleUpDown.DecimalPlaces = 2;
			this.angleUpDown.Location = new System.Drawing.Point(72, 24);
			this.angleUpDown.Maximum = new System.Decimal(new int[] {
																		180,
																		0,
																		0,
																		0});
			this.angleUpDown.Minimum = new System.Decimal(new int[] {
																		180,
																		0,
																		0,
																		-2147483648});
			this.angleUpDown.Name = "angleUpDown";
			this.angleUpDown.Size = new System.Drawing.Size(72, 20);
			this.angleUpDown.TabIndex = 0;
			this.angleUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.angleUpDown.Enter += new System.EventHandler(this.angleUpDown_Enter);
			this.angleUpDown.ValueChanged += new System.EventHandler(this.angleUpDown_ValueChanged);
			this.angleUpDown.Leave += new System.EventHandler(this.angleUpDown_Leave);
			// 
			// angleGroupBox
			// 
			this.angleGroupBox.Controls.Add(this.label3);
			this.angleGroupBox.Controls.Add(this.angleChooserControl);
			this.angleGroupBox.Controls.Add(this.angleUpDown);
			this.angleGroupBox.Location = new System.Drawing.Point(8, 8);
			this.angleGroupBox.Name = "angleGroupBox";
			this.angleGroupBox.Size = new System.Drawing.Size(160, 80);
			this.angleGroupBox.TabIndex = 7;
			this.angleGroupBox.TabStop = false;
			this.angleGroupBox.Text = "Angle";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(144, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(8, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "°";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// zoomGroupBox
			// 
			this.zoomGroupBox.Controls.Add(this.zoomLabel);
			this.zoomGroupBox.Controls.Add(this.zoomSlider);
			this.zoomGroupBox.Location = new System.Drawing.Point(176, 8);
			this.zoomGroupBox.Name = "zoomGroupBox";
			this.zoomGroupBox.Size = new System.Drawing.Size(232, 80);
			this.zoomGroupBox.TabIndex = 8;
			this.zoomGroupBox.TabStop = false;
			this.zoomGroupBox.Text = "Zoom";
			// 
			// zoomLabel
			// 
			this.zoomLabel.Location = new System.Drawing.Point(192, 24);
			this.zoomLabel.Name = "zoomLabel";
			this.zoomLabel.Size = new System.Drawing.Size(32, 23);
			this.zoomLabel.TabIndex = 3;
			this.zoomLabel.Text = "1.00x";
			// 
			// RotateZoomEffectConfigDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(418, 135);
			this.Controls.Add(this.zoomGroupBox);
			this.Controls.Add(this.angleGroupBox);
			this.Controls.Add(this.keepBackgroundCheckBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Location = new System.Drawing.Point(0, 0);
			this.Name = "RotateZoomEffectConfigDialog";
			this.Text = "Rotate / Zoom";
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.keepBackgroundCheckBox, 0);
			this.Controls.SetChildIndex(this.angleGroupBox, 0);
			this.Controls.SetChildIndex(this.zoomGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.zoomSlider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).EndInit();
			this.angleGroupBox.ResumeLayout(false);
			this.zoomGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

        private void angleChooserControl_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleUpDown.Value != (decimal)angleChooserControl.ValueDouble)
            {
                angleUpDown.Value = (decimal)angleChooserControl.ValueDouble;
            }

            UpdateToken();
            Update();
        }

        private void zoomSlider_ValueChanged(object sender, System.EventArgs e)
        {
			// safety precaution such that sxul and syul in RotateZoomEffect can never 
			// reach infinity -- this is a quick fix due to the rapidly encroaching final
			// release date of PDN 2.0. MK
			if (zoomSlider.Value == 0)
			{
				zoomSlider.Value = 1;
			}
            UpdateToken();
			Update();
			zoomLabel.Text = (1.0f / ((RotateZoomEffectConfigToken)theEffectToken).Zoom).ToString("F2") + "x";
		}

        private void okButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void keepBackgroundCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateToken();
        }


        private void angleUpDown_Leave(object sender, System.EventArgs e)
        {
            Utility.ClipNumericUpDown(angleUpDown);

            if (Utility.CheckNumericUpDown(angleUpDown))
            {
                angleUpDown.Value = decimal.Parse(angleUpDown.Text);
            }
        }

        private void angleUpDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void angleUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleChooserControl.ValueDouble != (double)angleUpDown.Value)
            {
                angleChooserControl.ValueDouble = (double)angleUpDown.Value;
            }
        }
    }
}
