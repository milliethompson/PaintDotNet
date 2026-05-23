// AngleChooserConfigDialog.cs
// ui for configuring angle paramters. creates 
// an AngleChooserConfigToken for some of the effects
//
// Chris Crosetto

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for AngleChooserConfigDialog.
	/// </summary>
	public class AngleChooserConfigDialog 
        : EffectConfigDialog
	{

        private AngleChooserControl angleChooserControl;
        private System.Windows.Forms.GroupBox angleGroupBox;
        private System.Windows.Forms.NumericUpDown angleUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;

        private System.ComponentModel.Container components = null;

		public AngleChooserConfigDialog()
		{
			// Required for Windows Form Designer support
			InitializeComponent();
		}

        // create default config token with angle 45 degress
        protected override void InitialInitToken()
        {
            theEffectToken = new AngleChooserConfigToken(45);
        }

        protected override void InitTokenFromDialog()
        {
            ((AngleChooserConfigToken)EffectToken).Angle = angleChooserControl.ValueDouble;
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            AngleChooserConfigToken token = (AngleChooserConfigToken)effectToken;
            angleChooserControl.ValueDouble = token.Angle;
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
			this.okButton = new System.Windows.Forms.Button();
			this.angleGroupBox = new System.Windows.Forms.GroupBox();
			this.angleUpDown = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.angleChooserControl = new PaintDotNet.AngleChooserControl();
			this.cancelButton = new System.Windows.Forms.Button();
			this.angleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(16, 96);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// angleGroupBox
			// 
			this.angleGroupBox.Controls.Add(this.angleUpDown);
			this.angleGroupBox.Controls.Add(this.label3);
			this.angleGroupBox.Controls.Add(this.angleChooserControl);
			this.angleGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.angleGroupBox.Location = new System.Drawing.Point(8, 8);
			this.angleGroupBox.Name = "angleGroupBox";
			this.angleGroupBox.Size = new System.Drawing.Size(168, 80);
			this.angleGroupBox.TabIndex = 12;
			this.angleGroupBox.TabStop = false;
			this.angleGroupBox.Text = "Angle";
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
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(144, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(8, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "°";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// angleChooserControl
			// 
			this.angleChooserControl.Location = new System.Drawing.Point(8, 16);
			this.angleChooserControl.Name = "angleChooserControl";
			this.angleChooserControl.Size = new System.Drawing.Size(56, 56);
			this.angleChooserControl.TabIndex = 0;
			this.angleChooserControl.TabStop = false;
			this.angleChooserControl.Value = 16;
			this.angleChooserControl.ValueDouble = 16;
			this.angleChooserControl.ValueChanged += new System.EventHandler(this.angleChooserControl_ValueChanged);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(104, 96);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 13;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// AngleChooserConfigDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(186, 128);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.angleGroupBox);
			this.Controls.Add(this.okButton);
			this.Location = new System.Drawing.Point(0, 0);
			this.Name = "AngleChooserConfigDialog";
			this.Text = "AngleChooser";
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.angleGroupBox, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.angleGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

        private void angleChooserControl_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleUpDown.Value != (decimal)angleChooserControl.Value)
            {
                angleUpDown.Value = (decimal)angleChooserControl.ValueDouble;
                UpdateToken();
                Update();
            }
        }

        private void angleUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleChooserControl.ValueDouble != (double)angleUpDown.Value)
            {
                angleChooserControl.ValueDouble = (double)angleUpDown.Value;
                UpdateToken();
            }
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void angleUpDown_Leave(object sender, System.EventArgs e)
        {
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

    }
}
