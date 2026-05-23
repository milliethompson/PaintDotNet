using System;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for ThreeAmountsConfigDialog.
	/// </summary>
	public class ThreeAmountsConfigDialog
        : TwoAmountsConfigDialog
	{
        private System.Windows.Forms.GroupBox amount3GroupBox;
        private System.Windows.Forms.Button amount3Reset;
        private System.Windows.Forms.NumericUpDown amount3UpDown;
        private System.Windows.Forms.TrackBar amount3Slider;
    
        private int amount3Default = 0;

        public int Amount3Default
        {
            get
            {
                return amount3Default;
            }

            set
            {
                amount3Default = value;
                amount3Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount3Minimum
        {
            get
            {
                return amount3Slider.Minimum;
            }

            set
            {
                amount3Slider.Minimum = value;
                amount3UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount3Maximum
        {
            get
            {
                return amount3Slider.Maximum;
            }

            set
            {
                amount3Slider.Maximum = value;
                amount3UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount3Label
        {
            get
            {
                return amount3GroupBox.Text;
            }

            set
            {
                amount3GroupBox.Text = value;
            }
        }

        protected override void InitialInitToken()
        {
            this.theEffectToken = new ThreeAmountsConfigToken(Amount1Default, Amount2Default, Amount3Default);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            base.InitDialogFromToken (effectToken);
            amount3Slider.Value = ((ThreeAmountsConfigToken)effectToken).Amount3;
        }

        protected override void InitTokenFromDialog()
        {
            base.InitTokenFromDialog ();
            ((ThreeAmountsConfigToken)theEffectToken).Amount3 = amount3Slider.Value;
        }

        private void InitializeComponent()
        {
            this.amount3GroupBox = new System.Windows.Forms.GroupBox();
            this.amount3Reset = new System.Windows.Forms.Button();
            this.amount3UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount3Slider = new System.Windows.Forms.TrackBar();
            this.amount3GroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.amount3UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount3Slider)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(86, 248);
            this.okButton.Name = "okButton";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(174, 248);
            this.cancelButton.Name = "cancelButton";
            // 
            // amount3GroupBox
            // 
            this.amount3GroupBox.Controls.Add(this.amount3Reset);
            this.amount3GroupBox.Controls.Add(this.amount3UpDown);
            this.amount3GroupBox.Controls.Add(this.amount3Slider);
            this.amount3GroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.amount3GroupBox.Location = new System.Drawing.Point(9, 168);
            this.amount3GroupBox.Name = "amount3GroupBox";
            this.amount3GroupBox.Size = new System.Drawing.Size(240, 70);
            this.amount3GroupBox.TabIndex = 9;
            this.amount3GroupBox.TabStop = false;
            this.amount3GroupBox.Text = "amount3";
            // 
            // amount3Reset
            // 
            this.amount3Reset.Location = new System.Drawing.Point(168, 41);
            this.amount3Reset.Name = "amount3Reset";
            this.amount3Reset.Size = new System.Drawing.Size(64, 20);
            this.amount3Reset.TabIndex = 7;
            this.amount3Reset.Text = "Reset";
            this.amount3Reset.Click += new System.EventHandler(this.amount3Reset_Click);
            // 
            // amount3UpDown
            // 
            this.amount3UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount3UpDown.Location = new System.Drawing.Point(168, 16);
            this.amount3UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount3UpDown.Name = "amount3UpDown";
            this.amount3UpDown.Size = new System.Drawing.Size(64, 20);
            this.amount3UpDown.TabIndex = 6;
            this.amount3UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount3UpDown.Enter += new System.EventHandler(this.amount3UpDown_Enter);
            this.amount3UpDown.ValueChanged += new System.EventHandler(this.amount3UpDown_ValueChanged);
            this.amount3UpDown.Leave += new System.EventHandler(this.amount3UpDown_Leave);
            // 
            // amount3Slider
            // 
            this.amount3Slider.LargeChange = 20;
            this.amount3Slider.Location = new System.Drawing.Point(8, 16);
            this.amount3Slider.Maximum = 100;
            this.amount3Slider.Minimum = -100;
            this.amount3Slider.Name = "amount3Slider";
            this.amount3Slider.Size = new System.Drawing.Size(152, 45);
            this.amount3Slider.TabIndex = 4;
            this.amount3Slider.TickFrequency = 10;
            this.amount3Slider.ValueChanged += new System.EventHandler(this.amount3Slider_ValueChanged);
            // 
            // ThreeAmountsConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(258, 280);
            this.Controls.Add(this.amount3GroupBox);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "ThreeAmountsConfigDialog";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.amount3GroupBox, 0);
            this.amount3GroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.amount3UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount3Slider)).EndInit();
            this.ResumeLayout(false);

        }
    
		public ThreeAmountsConfigDialog()
            : base()
		{
            InitializeComponent();
		}


        private void amount3Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount3UpDown.Value != (decimal)amount3Slider.Value)
            {
                amount3UpDown.Value = (decimal)amount3Slider.Value;
                UpdateToken();
            }
        }

        private void amount3UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount3Slider.Value != (int)amount3UpDown.Value)
            {
                amount3Slider.Value = (int)amount3UpDown.Value;
                UpdateToken();
            }
        }

        private void amount3UpDown_Enter(object sender, System.EventArgs e)
        {
            amount3UpDown.Select(0, amount3UpDown.Text.Length);        
        }

        private void amount3UpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(amount3UpDown))
            {
                amount3UpDown.Value = decimal.Parse(amount3UpDown.Text);
            }
        }

        private void amount3Reset_Click(object sender, System.EventArgs e)
        {
            this.amount3Slider.Value = amount3Default;
        }
	}
}
