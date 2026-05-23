using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class TwoAmountsConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.TrackBar amount1Slider;
        private System.Windows.Forms.NumericUpDown amount1UpDown;
        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox amount1GroupBox;
        private System.Windows.Forms.GroupBox amount2GroupBox;
        private System.Windows.Forms.NumericUpDown amount2UpDown;
        private System.Windows.Forms.TrackBar amount2Slider;
        private System.Windows.Forms.Button amount2Reset;
        private System.Windows.Forms.Button amount1Reset;
        private System.ComponentModel.IContainer components = null;

        private int amount1Default = 0;
        private int amount2Default = 0;

        public int Amount1Default
        {
            get
            {
                return amount1Default;
            }

            set
            {
                amount1Default = value;
                amount1Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount1Minimum
        {
            get
            {
                return amount1Slider.Minimum;
            }

            set
            {
                amount1Slider.Minimum = value;
                amount1UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount1Maximum
        {
            get
            {
                return amount1Slider.Maximum;
            }

            set
            {
                amount1Slider.Maximum = value;
                amount1UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount1Label
        {
            get
            {
                return amount1GroupBox.Text;
            }

            set
            {
                amount1GroupBox.Text = value;
            }
        }

        public int Amount2Default
        {
            get
            {
                return amount2Default;
            }

            set
            {
                amount2Default = value;
                amount2Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount2Minimum
        {
            get
            {
                return amount2Slider.Minimum;
            }

            set
            {
                amount2Slider.Minimum = value;
                amount2UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount2Maximum
        {
            get
            {
                return amount2Slider.Maximum;
            }

            set
            {
                amount2Slider.Maximum = value;
                amount2UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount2Label
        {
            get
            {
                return amount2GroupBox.Text;
            }

            set
            {
                amount2GroupBox.Text = value;
            }
        }

        public TwoAmountsConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

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
            theEffectToken = new TwoAmountsConfigToken(Amount1Default, Amount2Default);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            amount1Slider.Value = ((TwoAmountsConfigToken)effectToken).Amount1;
            amount2Slider.Value = ((TwoAmountsConfigToken)effectToken).Amount2;                        
        }

        protected override void InitTokenFromDialog()
        {
            ((TwoAmountsConfigToken)theEffectToken).Amount1 = amount1Slider.Value;
            ((TwoAmountsConfigToken)theEffectToken).Amount2 = amount2Slider.Value;
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
            this.amount1Slider = new System.Windows.Forms.TrackBar();
            this.amount1UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount1GroupBox = new System.Windows.Forms.GroupBox();
            this.amount1Reset = new System.Windows.Forms.Button();
            this.amount2GroupBox = new System.Windows.Forms.GroupBox();
            this.amount2Reset = new System.Windows.Forms.Button();
            this.amount2UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount2Slider = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.amount1Slider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount1UpDown)).BeginInit();
            this.amount1GroupBox.SuspendLayout();
            this.amount2GroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.amount2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2Slider)).BeginInit();
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
            // amount1Slider
            // 
            this.amount1Slider.LargeChange = 20;
            this.amount1Slider.Location = new System.Drawing.Point(8, 16);
            this.amount1Slider.Maximum = 100;
            this.amount1Slider.Minimum = -100;
            this.amount1Slider.Name = "amount1Slider";
            this.amount1Slider.Size = new System.Drawing.Size(152, 45);
            this.amount1Slider.TabIndex = 4;
            this.amount1Slider.TickFrequency = 10;
            this.amount1Slider.ValueChanged += new System.EventHandler(this.amount1Slider_ValueChanged);
            // 
            // amount1UpDown
            // 
            this.amount1UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount1UpDown.Location = new System.Drawing.Point(168, 16);
            this.amount1UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount1UpDown.Name = "amount1UpDown";
            this.amount1UpDown.Size = new System.Drawing.Size(64, 20);
            this.amount1UpDown.TabIndex = 6;
            this.amount1UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount1UpDown.Enter += new System.EventHandler(this.amount1UpDown_Enter);
            this.amount1UpDown.ValueChanged += new System.EventHandler(this.amount1UpDown_ValueChanged);
            this.amount1UpDown.Leave += new System.EventHandler(this.amount1UpDown_Leave);
            // 
            // amount1GroupBox
            // 
            this.amount1GroupBox.Controls.Add(this.amount1Reset);
            this.amount1GroupBox.Controls.Add(this.amount1UpDown);
            this.amount1GroupBox.Controls.Add(this.amount1Slider);
            this.amount1GroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.amount1GroupBox.Location = new System.Drawing.Point(8, 8);
            this.amount1GroupBox.Name = "amount1GroupBox";
            this.amount1GroupBox.Size = new System.Drawing.Size(240, 70);
            this.amount1GroupBox.TabIndex = 7;
            this.amount1GroupBox.TabStop = false;
            this.amount1GroupBox.Text = "amount1";
            // 
            // amount1Reset
            // 
            this.amount1Reset.Location = new System.Drawing.Point(168, 41);
            this.amount1Reset.Name = "amount1Reset";
            this.amount1Reset.Size = new System.Drawing.Size(64, 20);
            this.amount1Reset.TabIndex = 8;
            this.amount1Reset.Text = "Reset";
            this.amount1Reset.Click += new System.EventHandler(this.amount1Reset_Click);
            // 
            // amount2GroupBox
            // 
            this.amount2GroupBox.Controls.Add(this.amount2Reset);
            this.amount2GroupBox.Controls.Add(this.amount2UpDown);
            this.amount2GroupBox.Controls.Add(this.amount2Slider);
            this.amount2GroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.amount2GroupBox.Location = new System.Drawing.Point(9, 88);
            this.amount2GroupBox.Name = "amount2GroupBox";
            this.amount2GroupBox.Size = new System.Drawing.Size(240, 70);
            this.amount2GroupBox.TabIndex = 8;
            this.amount2GroupBox.TabStop = false;
            this.amount2GroupBox.Text = "amount2";
            // 
            // amount2Reset
            // 
            this.amount2Reset.Location = new System.Drawing.Point(168, 41);
            this.amount2Reset.Name = "amount2Reset";
            this.amount2Reset.Size = new System.Drawing.Size(64, 20);
            this.amount2Reset.TabIndex = 7;
            this.amount2Reset.Text = "Reset";
            this.amount2Reset.Click += new System.EventHandler(this.amount2Reset_Click);
            // 
            // amount2UpDown
            // 
            this.amount2UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount2UpDown.Location = new System.Drawing.Point(168, 16);
            this.amount2UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount2UpDown.Name = "amount2UpDown";
            this.amount2UpDown.Size = new System.Drawing.Size(64, 20);
            this.amount2UpDown.TabIndex = 6;
            this.amount2UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount2UpDown.Enter += new System.EventHandler(this.amount2UpDown_Enter);
            this.amount2UpDown.ValueChanged += new System.EventHandler(this.amount2UpDown_ValueChanged);
            this.amount2UpDown.Leave += new System.EventHandler(this.amount2UpDown_Leave);
            // 
            // amount2Slider
            // 
            this.amount2Slider.LargeChange = 20;
            this.amount2Slider.Location = new System.Drawing.Point(8, 16);
            this.amount2Slider.Maximum = 100;
            this.amount2Slider.Minimum = -100;
            this.amount2Slider.Name = "amount2Slider";
            this.amount2Slider.Size = new System.Drawing.Size(152, 45);
            this.amount2Slider.TabIndex = 4;
            this.amount2Slider.TickFrequency = 10;
            this.amount2Slider.ValueChanged += new System.EventHandler(this.amount2Slider_ValueChanged);
            // 
            // TwoAmountsConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(258, 201);
            this.Controls.Add(this.amount2GroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.amount1GroupBox);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "TwoAmountsConfigDialog";
            this.Text = "amount1 / amount2";
            this.Controls.SetChildIndex(this.amount1GroupBox, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.amount2GroupBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.amount1Slider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount1UpDown)).EndInit();
            this.amount1GroupBox.ResumeLayout(false);
            this.amount2GroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.amount2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2Slider)).EndInit();
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

        private void amount1Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount1UpDown.Value != (decimal)amount1Slider.Value)
            {
                amount1UpDown.Value = (decimal)amount1Slider.Value;
                UpdateToken();
            }
        }

        private void amount1UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount1Slider.Value != (int)amount1UpDown.Value)
            {
                amount1Slider.Value = (int)amount1UpDown.Value;
                UpdateToken();
            }
        }

        private void amount1UpDown_Enter(object sender, System.EventArgs e)
        {
            amount1UpDown.Select(0, amount1UpDown.Text.Length);        
        }

        private void amount1UpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(amount1UpDown))
            {
                amount1UpDown.Value = decimal.Parse(amount1UpDown.Text);
            }
        }

        private void amount2Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount2UpDown.Value != (decimal)amount2Slider.Value)
            {
                amount2UpDown.Value = (decimal)amount2Slider.Value;
                UpdateToken();
            }
        }

        private void amount2UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount2Slider.Value != (int)amount2UpDown.Value)
            {
                amount2Slider.Value = (int)amount2UpDown.Value;
                UpdateToken();
            }
        }

        private void amount2UpDown_Enter(object sender, System.EventArgs e)
        {
            amount2UpDown.Select(0, amount2UpDown.Text.Length);        
        }

        private void amount2UpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(amount2UpDown))
            {
                amount2UpDown.Value = decimal.Parse(amount2UpDown.Text);
            }
        }

        private void amount2Reset_Click(object sender, System.EventArgs e)
        {
            this.amount2Slider.Value = amount2Default;
        }

        private void amount1Reset_Click(object sender, System.EventArgs e)
        {
            this.amount1Slider.Value = amount1Default;
        }
    }
}

