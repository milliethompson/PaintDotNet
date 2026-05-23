using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class MotionBlurEffectConfigDialog 
        : EffectConfigDialog
    {
        private AngleChooserControl angleChooserControl;
        private System.Windows.Forms.NumericUpDown angleUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar distanceTrackBar;
        private System.Windows.Forms.NumericUpDown distanceUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.CheckBox centeredCheckBox;
        private System.Windows.Forms.GroupBox angleGroupBox;
        private System.Windows.Forms.GroupBox distanceGroupBox;
        private System.ComponentModel.IContainer components = null;

        public MotionBlurEffectConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new MotionBlurEffectConfigToken(25, 10, true);
        }

        protected override void InitTokenFromDialog()
        {
            ((MotionBlurEffectConfigToken)EffectToken).Angle = angleChooserControl.Value;
            ((MotionBlurEffectConfigToken)EffectToken).Distance = distanceTrackBar.Value;
            ((MotionBlurEffectConfigToken)EffectToken).Centered = centeredCheckBox.Checked;
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            MotionBlurEffectConfigToken token = (MotionBlurEffectConfigToken)effectToken;
            angleChooserControl.Value = token.Angle;
            distanceTrackBar.Value = token.Distance;
            centeredCheckBox.Checked = token.Centered;
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.angleChooserControl = new PaintDotNet.AngleChooserControl();
            this.angleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.distanceTrackBar = new System.Windows.Forms.TrackBar();
            this.distanceUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.centeredCheckBox = new System.Windows.Forms.CheckBox();
            this.angleGroupBox = new System.Windows.Forms.GroupBox();
            this.distanceGroupBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceUpDown)).BeginInit();
            this.angleGroupBox.SuspendLayout();
            this.distanceGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // angleChooserControl
            // 
            this.angleChooserControl.Location = new System.Drawing.Point(8, 16);
            this.angleChooserControl.Name = "angleChooserControl";
            this.angleChooserControl.Size = new System.Drawing.Size(56, 56);
            this.angleChooserControl.TabIndex = 0;
            this.angleChooserControl.TabStop = false;
            this.angleChooserControl.Value = 16;
            this.angleChooserControl.ValueChanged += new System.EventHandler(this.angleChooserControl_ValueChanged);
            // 
            // angleUpDown
            // 
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
            // distanceTrackBar
            // 
            this.distanceTrackBar.AutoSize = false;
            this.distanceTrackBar.Location = new System.Drawing.Point(8, 48);
            this.distanceTrackBar.Maximum = 200;
            this.distanceTrackBar.Minimum = 1;
            this.distanceTrackBar.Name = "distanceTrackBar";
            this.distanceTrackBar.Size = new System.Drawing.Size(136, 24);
            this.distanceTrackBar.TabIndex = 2;
            this.distanceTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.distanceTrackBar.Value = 1;
            this.distanceTrackBar.ValueChanged += new System.EventHandler(this.distanceTrackBar_ValueChanged);
            // 
            // distanceUpDown
            // 
            this.distanceUpDown.Location = new System.Drawing.Point(16, 24);
            this.distanceUpDown.Maximum = new System.Decimal(new int[] {
                                                                           200,
                                                                           0,
                                                                           0,
                                                                           0});
            this.distanceUpDown.Minimum = new System.Decimal(new int[] {
                                                                           1,
                                                                           0,
                                                                           0,
                                                                           0});
            this.distanceUpDown.Name = "distanceUpDown";
            this.distanceUpDown.Size = new System.Drawing.Size(72, 20);
            this.distanceUpDown.TabIndex = 1;
            this.distanceUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.distanceUpDown.Value = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         0});
            this.distanceUpDown.Enter += new System.EventHandler(this.angleUpDown_Enter);
            this.distanceUpDown.ValueChanged += new System.EventHandler(this.distanceUpDown_ValueChanged);
            this.distanceUpDown.Leave += new System.EventHandler(this.distanceUpDown_Leave);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(90, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "pixels";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(256, 104);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(168, 104);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // centeredCheckBox
            // 
            this.centeredCheckBox.Checked = true;
            this.centeredCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.centeredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.centeredCheckBox.Location = new System.Drawing.Point(16, 96);
            this.centeredCheckBox.Name = "centeredCheckBox";
            this.centeredCheckBox.TabIndex = 3;
            this.centeredCheckBox.Text = "Centered";
            this.centeredCheckBox.CheckedChanged += new System.EventHandler(this.centeredCheckBox_CheckedChanged);
            // 
            // angleGroupBox
            // 
            this.angleGroupBox.Controls.Add(this.angleUpDown);
            this.angleGroupBox.Controls.Add(this.label3);
            this.angleGroupBox.Controls.Add(this.angleChooserControl);
            this.angleGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.angleGroupBox.Location = new System.Drawing.Point(8, 8);
            this.angleGroupBox.Name = "angleGroupBox";
            this.angleGroupBox.Size = new System.Drawing.Size(160, 80);
            this.angleGroupBox.TabIndex = 11;
            this.angleGroupBox.TabStop = false;
            this.angleGroupBox.Text = "Angle";
            // 
            // distanceGroupBox
            // 
            this.distanceGroupBox.Controls.Add(this.distanceTrackBar);
            this.distanceGroupBox.Controls.Add(this.distanceUpDown);
            this.distanceGroupBox.Controls.Add(this.label4);
            this.distanceGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.distanceGroupBox.Location = new System.Drawing.Point(176, 8);
            this.distanceGroupBox.Name = "distanceGroupBox";
            this.distanceGroupBox.Size = new System.Drawing.Size(152, 80);
            this.distanceGroupBox.TabIndex = 12;
            this.distanceGroupBox.TabStop = false;
            this.distanceGroupBox.Text = "Distance";
            // 
            // MotionBlurEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(338, 135);
            this.ControlBox = false;
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.distanceGroupBox);
            this.Controls.Add(this.angleGroupBox);
            this.Controls.Add(this.centeredCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Name = "MotionBlurEffectConfigDialog";
            this.Text = "Motion Blur";
            this.Enter += new System.EventHandler(this.angleUpDown_Enter);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.centeredCheckBox, 0);
            this.Controls.SetChildIndex(this.angleGroupBox, 0);
            this.Controls.SetChildIndex(this.distanceGroupBox, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.distanceUpDown)).EndInit();
            this.angleGroupBox.ResumeLayout(false);
            this.distanceGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void angleChooserControl_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleUpDown.Value != (decimal)angleChooserControl.Value)
            {
                angleUpDown.Value = (decimal)angleChooserControl.Value;
                UpdateToken();
            }
        }

        private void angleUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (angleChooserControl.Value != (int)angleUpDown.Value)
            {
                angleChooserControl.Value = (int)angleUpDown.Value;
                UpdateToken();
            }
        }

        private void distanceUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (distanceTrackBar.Value != (int)distanceUpDown.Value)
            {
                distanceTrackBar.Value = (int)distanceUpDown.Value;
                UpdateToken();
            }
        }

        private void distanceTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (distanceUpDown.Value != (decimal)distanceTrackBar.Value)
            {
                distanceUpDown.Value = (decimal)distanceTrackBar.Value;
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

        private void centeredCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateToken();
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
        
        private void distanceUpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(distanceUpDown))
            {
                distanceUpDown.Value = decimal.Parse(distanceUpDown.Text);
            }
        }

    }
}

