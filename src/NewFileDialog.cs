using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;


namespace PaintDotNet
{
    /// <summary>
    /// New File Dialog By Chris Trevino
    /// Last Updated: 2/5/2004
    /// </summary>
    public class NewFileDialog 
        : PdnBaseForm
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown widthUpDown;
        public System.Windows.Forms.NumericUpDown heightUpDown;
        private System.Windows.Forms.GroupBox fileSizeGroupBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public int NewWidth 
        {
            get
            {
                return (int)widthUpDown.Value;
            }

            set
            {
                widthUpDown.Value = (int)Math.Ceiling(value);
                SetImageSizeLabel();
            }
        }

        public int NewHeight
        {
            get
            {
                return (int)heightUpDown.Value;
            }
            set
            {
                heightUpDown.Value = (int)Math.Ceiling(value);
                SetImageSizeLabel();
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

        public NewFileDialog()
        {
            InitializeComponent();
            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuFileNewIcon.bmp"), Color.FromArgb(192, 192, 192));
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.widthUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightUpDown = new System.Windows.Forms.NumericUpDown();
            this.fileSizeGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
            this.fileSizeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.okButton.Location = new System.Drawing.Point(32, 96);
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
            this.cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cancelButton.Location = new System.Drawing.Point(112, 96);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label1
            // 
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(8, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Height:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(8, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Width:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // widthUpDown
            // 
            this.widthUpDown.Location = new System.Drawing.Point(50, 20);
            this.widthUpDown.Maximum = new System.Decimal(new int[] {
                                                                        65535,
                                                                        0,
                                                                        0,
                                                                        0});
            this.widthUpDown.Name = "widthUpDown";
            this.widthUpDown.Size = new System.Drawing.Size(72, 20);
            this.widthUpDown.TabIndex = 0;
            this.widthUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.widthUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.widthUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.widthUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.widthUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // heightUpDown
            // 
            this.heightUpDown.Location = new System.Drawing.Point(50, 46);
            this.heightUpDown.Maximum = new System.Decimal(new int[] {
                                                                         65535,
                                                                         0,
                                                                         0,
                                                                         0});
            this.heightUpDown.Name = "heightUpDown";
            this.heightUpDown.Size = new System.Drawing.Size(72, 20);
            this.heightUpDown.TabIndex = 1;
            this.heightUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.heightUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.heightUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.heightUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.heightUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // fileSizeGroupBox
            // 
            this.fileSizeGroupBox.Controls.Add(this.label6);
            this.fileSizeGroupBox.Controls.Add(this.label4);
            this.fileSizeGroupBox.Controls.Add(this.label2);
            this.fileSizeGroupBox.Controls.Add(this.widthUpDown);
            this.fileSizeGroupBox.Controls.Add(this.heightUpDown);
            this.fileSizeGroupBox.Controls.Add(this.label1);
            this.fileSizeGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.fileSizeGroupBox.Location = new System.Drawing.Point(8, 8);
            this.fileSizeGroupBox.Name = "fileSizeGroupBox";
            this.fileSizeGroupBox.Size = new System.Drawing.Size(176, 76);
            this.fileSizeGroupBox.TabIndex = 10;
            this.fileSizeGroupBox.TabStop = false;
            this.fileSizeGroupBox.Text = "groupBox1";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(127, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 16);
            this.label6.TabIndex = 12;
            this.label6.Text = "pixels";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(127, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 11;
            this.label4.Text = "pixels";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NewFileDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(194, 125);
            this.Controls.Add(this.fileSizeGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewFileDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.fileSizeGroupBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).EndInit();
            this.fileSizeGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void okButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void SetImageSizeLabel()
        {
            double fileSize = ((double)NewWidth * (double)NewHeight * 4.0);
            fileSizeGroupBox.Text = "File Size: " + Utility.SizeStringFromBytes(fileSize);
        }

        private void upDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool numberIsOk = Utility.CheckNumericUpDown((NumericUpDown)sender);

            if (numberIsOk)
            {
                SetImageSizeLabel();
                SetOkEnable();
            }

            SetImageSizeLabel();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            SetOkEnable();
            this.widthUpDown.Select();
            this.widthUpDown.Select(0, widthUpDown.Text.Length);
        }

        /// <summary>
        /// Enables the OK button if width and height are valid and non-zero
        /// Otherwise disables it.
        /// </summary>
        private void SetOkEnable()
        {
            bool enabled = true;

            try
            {
                int width = int.Parse(widthUpDown.Text);
                int height = int.Parse(heightUpDown.Text);
                
                if (width > 0 && height > 0)
                {
                    enabled = true;
                }
                else
                {
                    enabled = false;
                }
            }

            catch
            {
                enabled = false;
            }

            okButton.Enabled = enabled;
        }

        private void upDown_ValueChanged(object sender, System.EventArgs e)
        {
            SetImageSizeLabel();
            SetOkEnable();
        }

        private void upDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void upDown_Leave(object sender, System.EventArgs e)
        {
            upDown_ValueChanged(sender, e);
        }
    }
}
