using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class AmountEffectConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar amountTrackBar;
        private System.Windows.Forms.Button cancelButton;
        private System.ComponentModel.IContainer components = null;

        public AmountEffectConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new AmountEffectConfigToken(1);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            if (effectToken != null)
            {
                this.amountTrackBar.Value = ((AmountEffectConfigToken)effectToken).Amount;
            }
        }

        protected override void InitTokenFromDialog()
        {
            if (theEffectToken != null)
            {
                ((AmountEffectConfigToken)theEffectToken).Amount = this.amountTrackBar.Value;
            }
        }

        [Browsable(false)]
        public int SliderMinimum
        {
            get
            {
                return amountTrackBar.Minimum;
            }

            set
            {
                amountTrackBar.Minimum = value;
            }
        }

        [Browsable(false)]
        public int SliderMaximum
        {
            get
            {
                return amountTrackBar.Maximum;
            }

            set
            {
                amountTrackBar.Maximum = value;
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.amountTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.amountTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(64, 72);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // amountTrackBar
            // 
            this.amountTrackBar.Location = new System.Drawing.Point(16, 24);
            this.amountTrackBar.Maximum = 32;
            this.amountTrackBar.Minimum = 1;
            this.amountTrackBar.Name = "amountTrackBar";
            this.amountTrackBar.Size = new System.Drawing.Size(192, 56);
            this.amountTrackBar.TabIndex = 0;
            this.amountTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.amountTrackBar.Value = 1;
            this.amountTrackBar.ValueChanged += new System.EventHandler(this.amountTrackBar_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Amount:";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(144, 72);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // AmountEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(224, 101);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.amountTrackBar);
            this.Name = "AmountEffectConfigDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "abc";
            this.Controls.SetChildIndex(this.amountTrackBar, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.amountTrackBar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void amountTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateToken();
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
    }
}

