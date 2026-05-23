/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// New File Dialog By Chris Trevino
    /// </summary>
    public class NewFileDialog 
        : PdnBaseDialog
    {
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
                widthUpDown.Value = (int)Math.Ceiling((double)value);
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
                heightUpDown.Value = (int)Math.Ceiling((double)value);
                SetImageSizeLabel();
            }
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
            // baseOkButton
            // 
            this.baseOkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseOkButton.Location = new System.Drawing.Point(30, 96);
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.Click += new System.EventHandler(this.baseOkButton_Click);
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseCancelButton.Location = new System.Drawing.Point(110, 96);
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.Click += new System.EventHandler(this.baseCancelButton_Click);
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
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(192, 128);
            this.Controls.Add(this.fileSizeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "NewFileDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New";
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.fileSizeGroupBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).EndInit();
            this.fileSizeGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void baseOkButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void baseCancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void SetImageSizeLabel()
        {
            long fileSize = (long)NewWidth * (long)NewHeight * (long)ColorBgra.SizeOf;
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

            baseOkButton.Enabled = enabled;
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
