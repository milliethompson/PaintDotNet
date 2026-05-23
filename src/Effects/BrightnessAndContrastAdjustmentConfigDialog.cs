using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class BrightnessAndContrastAdjustmentConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.TrackBar brightnessSlider;
        private System.Windows.Forms.NumericUpDown brightnessUpDown;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown contrastUpDown;
        private System.Windows.Forms.TrackBar contrastSlider;
        private System.Windows.Forms.Button contrastReset;
        private System.Windows.Forms.Button brightnessReset;
        private System.ComponentModel.IContainer components = null;

        public BrightnessAndContrastAdjustmentConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
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

        protected override void InitialInitToken()
        {
            theEffectToken = new BrightnessAndContrastAdjustmentConfigToken(0, 0);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            brightnessSlider.Value = ((BrightnessAndContrastAdjustmentConfigToken)effectToken).Brightness;
            contrastSlider.Value = ((BrightnessAndContrastAdjustmentConfigToken)effectToken).Contrast;                        
        }

        protected override void InitTokenFromDialog()
        {
            ((BrightnessAndContrastAdjustmentConfigToken)theEffectToken).Brightness = brightnessSlider.Value;
            ((BrightnessAndContrastAdjustmentConfigToken)theEffectToken).Contrast = contrastSlider.Value;
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.brightnessSlider = new System.Windows.Forms.TrackBar();
            this.brightnessUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.contrastReset = new System.Windows.Forms.Button();
            this.contrastUpDown = new System.Windows.Forms.NumericUpDown();
            this.contrastSlider = new System.Windows.Forms.TrackBar();
            this.brightnessReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.brightnessSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brightnessUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contrastUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.contrastSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(86, 170);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(174, 170);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // brightnessSlider
            // 
            this.brightnessSlider.LargeChange = 20;
            this.brightnessSlider.Location = new System.Drawing.Point(8, 16);
            this.brightnessSlider.Maximum = 100;
            this.brightnessSlider.Minimum = -100;
            this.brightnessSlider.Name = "brightnessSlider";
            this.brightnessSlider.Size = new System.Drawing.Size(152, 45);
            this.brightnessSlider.TabIndex = 4;
            this.brightnessSlider.TickFrequency = 10;
            this.brightnessSlider.ValueChanged += new System.EventHandler(this.brightnessSlider_ValueChanged);
            // 
            // brightnessUpDown
            // 
            this.brightnessUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.brightnessUpDown.Location = new System.Drawing.Point(168, 16);
            this.brightnessUpDown.Minimum = new System.Decimal(new int[] {
                                                                             100,
                                                                             0,
                                                                             0,
                                                                             -2147483648});
            this.brightnessUpDown.Name = "brightnessUpDown";
            this.brightnessUpDown.Size = new System.Drawing.Size(64, 20);
            this.brightnessUpDown.TabIndex = 6;
            this.brightnessUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.brightnessUpDown.Enter += new System.EventHandler(this.brightnessUpDown_Enter);
            this.brightnessUpDown.ValueChanged += new System.EventHandler(this.brightnessUpDown_ValueChanged);
            this.brightnessUpDown.Leave += new System.EventHandler(this.brightnessUpDown_Leave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.brightnessReset);
            this.groupBox1.Controls.Add(this.brightnessUpDown);
            this.groupBox1.Controls.Add(this.brightnessSlider);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(240, 70);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Brightness";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.contrastReset);
            this.groupBox2.Controls.Add(this.contrastUpDown);
            this.groupBox2.Controls.Add(this.contrastSlider);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Location = new System.Drawing.Point(9, 88);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(240, 70);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Contrast";
            // 
            // contrastReset
            // 
            this.contrastReset.Location = new System.Drawing.Point(168, 41);
            this.contrastReset.Name = "contrastReset";
            this.contrastReset.Size = new System.Drawing.Size(64, 20);
            this.contrastReset.TabIndex = 7;
            this.contrastReset.Text = "Reset";
            this.contrastReset.Click += new System.EventHandler(this.contrastReset_Click);
            // 
            // contrastUpDown
            // 
            this.contrastUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.contrastUpDown.Location = new System.Drawing.Point(168, 16);
            this.contrastUpDown.Minimum = new System.Decimal(new int[] {
                                                                           100,
                                                                           0,
                                                                           0,
                                                                           -2147483648});
            this.contrastUpDown.Name = "contrastUpDown";
            this.contrastUpDown.Size = new System.Drawing.Size(64, 20);
            this.contrastUpDown.TabIndex = 6;
            this.contrastUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.contrastUpDown.Enter += new System.EventHandler(this.contrastUpDown_Enter);
            this.contrastUpDown.ValueChanged += new System.EventHandler(this.contrastUpDown_ValueChanged);
            this.contrastUpDown.Leave += new System.EventHandler(this.contrastUpDown_Leave);
            // 
            // contrastSlider
            // 
            this.contrastSlider.LargeChange = 20;
            this.contrastSlider.Location = new System.Drawing.Point(8, 16);
            this.contrastSlider.Maximum = 100;
            this.contrastSlider.Minimum = -100;
            this.contrastSlider.Name = "contrastSlider";
            this.contrastSlider.Size = new System.Drawing.Size(152, 45);
            this.contrastSlider.TabIndex = 4;
            this.contrastSlider.TickFrequency = 10;
            this.contrastSlider.ValueChanged += new System.EventHandler(this.contrastSlider_ValueChanged);
            // 
            // brightnessReset
            // 
            this.brightnessReset.Location = new System.Drawing.Point(168, 41);
            this.brightnessReset.Name = "brightnessReset";
            this.brightnessReset.Size = new System.Drawing.Size(64, 20);
            this.brightnessReset.TabIndex = 8;
            this.brightnessReset.Text = "Reset";
            this.brightnessReset.Click += new System.EventHandler(this.brightnessReset_Click);
            // 
            // BrightnessAndContrastAdjustmentConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(258, 201);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupBox1);
            this.Name = "BrightnessAndContrastAdjustmentConfigDialog";
            this.Text = "Brightness / Contrast";
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.brightnessSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brightnessUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contrastUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.contrastSlider)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void brightnessSlider_ValueChanged(object sender, System.EventArgs e)
        {
            if (brightnessUpDown.Value != (decimal)brightnessSlider.Value)
            {
                brightnessUpDown.Value = (decimal)brightnessSlider.Value;
                UpdateToken();
            }
        }

        private void brightnessUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (brightnessSlider.Value != (int)brightnessUpDown.Value)
            {
                brightnessSlider.Value = (int)brightnessUpDown.Value;
                UpdateToken();
            }
        }

        private void brightnessUpDown_Enter(object sender, System.EventArgs e)
        {
            brightnessUpDown.Select(0, brightnessUpDown.Text.Length);        
        }

        private void brightnessUpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(brightnessUpDown))
            {
                brightnessUpDown.Value = decimal.Parse(brightnessUpDown.Text);
            }
        }

        private void contrastSlider_ValueChanged(object sender, System.EventArgs e)
        {
            if (contrastUpDown.Value != (decimal)contrastSlider.Value)
            {
                contrastUpDown.Value = (decimal)contrastSlider.Value;
                UpdateToken();
            }
        }

        private void contrastUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (contrastSlider.Value != (int)contrastUpDown.Value)
            {
                contrastSlider.Value = (int)contrastUpDown.Value;
                UpdateToken();
            }
        }

        private void contrastUpDown_Enter(object sender, System.EventArgs e)
        {
            contrastUpDown.Select(0, contrastUpDown.Text.Length);        
        }

        private void contrastUpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(contrastUpDown))
            {
                contrastUpDown.Value = decimal.Parse(contrastUpDown.Text);
            }
        }

        private void contrastReset_Click(object sender, System.EventArgs e)
        {
            this.contrastSlider.Value = 0;
        }

        private void brightnessReset_Click(object sender, System.EventArgs e)
        {
            this.brightnessSlider.Value = 0;
        }

    }
}

