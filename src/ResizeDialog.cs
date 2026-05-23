/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ResizeDialog.
    /// </summary>
    public class ResizeDialog 
        : PdnBaseForm
    {
        protected System.Windows.Forms.CheckBox ratioCheck;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;
        
        protected System.Windows.Forms.ComboBox resamplingAlgorithmComboBox;
        protected System.Windows.Forms.Label label3;

        protected System.Windows.Forms.NumericUpDown widthUpDown;
        protected System.Windows.Forms.NumericUpDown heightUpDown;

        private EventHandler upDownValueChangedDelegate;

        private double ratio;
        private bool isLocked;
        private int layers;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.Label currentImageSize;
        protected System.Windows.Forms.GroupBox resizedImageGroupBox;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label6;
        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        protected System.Windows.Forms.Label originalImageSize;
		protected System.Windows.Forms.Label label7;
		protected System.Windows.Forms.NumericUpDown percentUpDown;
        protected System.Windows.Forms.RadioButton absoluteRB;
        protected System.Windows.Forms.RadioButton percentRB;
        protected System.Windows.Forms.Label asteriskTextLabel;
        protected System.Windows.Forms.Label asteriskLabel;

        public int ImageWidth
        {
            get
            {
                return (int)widthUpDown.Value;
            }

            set
            {
                if (value <= 0)
                {
                    value = 0;
                }               

                if ((int)value > (int)widthUpDown.Maximum)
                {
                    value = (int)widthUpDown.Maximum;
                }

                widthUpDown.Value = (decimal)value;
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((long)layers * (long)ColorBgra.SizeOf * (long)ImageHeight * (long)ImageWidth);
            }
        }

        private Size originalSize = Size.Empty;
        public Size OriginalSize
        {
            get
            {
                return originalSize;
            }

            set
            {
				originalSize.Width = value.Width;
				originalSize.Height = value.Height;
                originalImageSize.Text = value.Width.ToString() + " x " + value.Height.ToString();
            }
        }

        public double AspectRatio
        {
            get
            {
                return ratio;
            }

            set
            {
                ratio = value;
            }
        }

        public int ImageHeight
        {
            get
            {
                return (int)heightUpDown.Value;
            }

            set
            {
                if (value <= 0)
                {
                    value = 0;
                }

                if ((int)value > (int)heightUpDown.Maximum)
                {
                    value = (int)heightUpDown.Maximum;
                }

                heightUpDown.Value = (decimal)value; //(int)(Math.Round((double)value));    
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((long)layers * (long)ColorBgra.SizeOf * (long)ImageHeight * (long)ImageWidth);
                PopulateAsteriskLabels();
            }
        }

        public long DocumentSize
        {
            set
            {
                currentImageSize.Text = Utility.SizeStringFromBytes((long)value);
            }
        }

        public ResamplingAlgorithm ResamplingAlgorithm
        {
            get
            {
                return ((ResampleMethod)this.resamplingAlgorithmComboBox.SelectedItem).method;
            }
            
            set
            {
                this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(value);
                PopulateAsteriskLabels();
            }
        }

        public bool IsLocked
        {
            get
            {
                return isLocked;
            }

            set
            {
                isLocked = value;
                ratioCheck.Checked = value;

                if (isLocked && ratio != 0.0)
                {
                    FixHeightToRatio();
                }
            }
        }

        public int Layers
        {
            get
            {
                return layers;
            }

            set
            {
                layers = value;
                long initialSize = (long)layers * ColorBgra.SizeOf * (long)ImageHeight * (long)ImageWidth;
                DocumentSize = initialSize;
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes(initialSize);
            }
        }

        private sealed class ResampleMethod
        {
            public ResamplingAlgorithm method;

            public override string ToString()
            {
                switch (method)
                {
                    case ResamplingAlgorithm.NearestNeighbor:
                        return "Nearest Neighbor";

                    case ResamplingAlgorithm.Bicubic:
                        return "Bicubic";

                    case ResamplingAlgorithm.SuperSampling:
                        return "Best Quality";

                    default:
                        return method.ToString();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is ResampleMethod && ((ResampleMethod)obj).method == this.method)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return this.method.GetHashCode();
            }

            public ResampleMethod(ResamplingAlgorithm method)
            {
                this.method = method;
            }
        }

        public ResizeDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            ratioCheck.Checked = true;
            resamplingAlgorithmComboBox.Items.Clear();
            ratio = -1;

            upDownValueChangedDelegate = new EventHandler(upDown_ValueChanged);

            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.Bicubic));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.Bilinear));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.NearestNeighbor));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.SuperSampling));

            resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(ResamplingAlgorithm.SuperSampling);
            layers = 1;

			this.percentUpDown.Enabled = false;

            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuImageResizeIcon.bmp"), Color.FromArgb(192, 192, 192));
            PopulateAsteriskLabels();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            this.widthUpDown.Select();
            this.widthUpDown.Select(0, widthUpDown.Text.Length);
            this.PopulateAsteriskLabels();
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
            this.ratioCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.resamplingAlgorithmComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.widthUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.currentImageSize = new System.Windows.Forms.Label();
            this.resizedImageGroupBox = new System.Windows.Forms.GroupBox();
            this.absoluteRB = new System.Windows.Forms.RadioButton();
            this.percentRB = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.percentUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.originalImageSize = new System.Windows.Forms.Label();
            this.asteriskTextLabel = new System.Windows.Forms.Label();
            this.asteriskLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
            this.resizedImageGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // ratioCheck
            // 
            this.ratioCheck.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ratioCheck.Location = new System.Drawing.Point(38, 118);
            this.ratioCheck.Name = "ratioCheck";
            this.ratioCheck.Size = new System.Drawing.Size(136, 16);
            this.ratioCheck.TabIndex = 4;
            this.ratioCheck.Text = "Maintain Aspect Ratio";
            this.ratioCheck.CheckedChanged += new System.EventHandler(this.ratioCheck_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(35, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "New Width:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(35, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "New Height:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(100, 248);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(72, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(180, 248);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // resamplingAlgorithmComboBox
            // 
            this.resamplingAlgorithmComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.resamplingAlgorithmComboBox.Location = new System.Drawing.Point(78, 20);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Size = new System.Drawing.Size(130, 21);
            this.resamplingAlgorithmComboBox.Sorted = true;
            this.resamplingAlgorithmComboBox.TabIndex = 0;
            this.resamplingAlgorithmComboBox.Tag = "";
            this.resamplingAlgorithmComboBox.SelectedIndexChanged += new System.EventHandler(this.resamplingAlgorithmComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "Resampling:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // widthUpDown
            // 
            this.widthUpDown.Location = new System.Drawing.Point(115, 68);
            this.widthUpDown.Maximum = new System.Decimal(new int[] {
                                                                        65535,
                                                                        0,
                                                                        0,
                                                                        0});
            this.widthUpDown.Name = "widthUpDown";
            this.widthUpDown.Size = new System.Drawing.Size(72, 20);
            this.widthUpDown.TabIndex = 2;
            this.widthUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.widthUpDown.Value = new System.Decimal(new int[] {
                                                                      797,
                                                                      0,
                                                                      0,
                                                                      0});
            this.widthUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.widthUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.widthUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.widthUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // heightUpDown
            // 
            this.heightUpDown.Location = new System.Drawing.Point(115, 94);
            this.heightUpDown.Maximum = new System.Decimal(new int[] {
                                                                         65535,
                                                                         0,
                                                                         0,
                                                                         0});
            this.heightUpDown.Name = "heightUpDown";
            this.heightUpDown.Size = new System.Drawing.Size(72, 20);
            this.heightUpDown.TabIndex = 3;
            this.heightUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.heightUpDown.Value = new System.Decimal(new int[] {
                                                                       595,
                                                                       0,
                                                                       0,
                                                                       0});
            this.heightUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.heightUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.heightUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.heightUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 16);
            this.label5.TabIndex = 13;
            this.label5.Text = "Current Image Size:";
            // 
            // currentImageSize
            // 
            this.currentImageSize.Location = new System.Drawing.Point(112, 24);
            this.currentImageSize.Name = "currentImageSize";
            this.currentImageSize.Size = new System.Drawing.Size(112, 16);
            this.currentImageSize.TabIndex = 14;
            this.currentImageSize.Text = "1.92 MB";
            // 
            // resizedImageGroupBox
            // 
            this.resizedImageGroupBox.Controls.Add(this.asteriskLabel);
            this.resizedImageGroupBox.Controls.Add(this.asteriskTextLabel);
            this.resizedImageGroupBox.Controls.Add(this.absoluteRB);
            this.resizedImageGroupBox.Controls.Add(this.percentRB);
            this.resizedImageGroupBox.Controls.Add(this.widthUpDown);
            this.resizedImageGroupBox.Controls.Add(this.heightUpDown);
            this.resizedImageGroupBox.Controls.Add(this.label6);
            this.resizedImageGroupBox.Controls.Add(this.label4);
            this.resizedImageGroupBox.Controls.Add(this.label2);
            this.resizedImageGroupBox.Controls.Add(this.label1);
            this.resizedImageGroupBox.Controls.Add(this.resamplingAlgorithmComboBox);
            this.resizedImageGroupBox.Controls.Add(this.label3);
            this.resizedImageGroupBox.Controls.Add(this.ratioCheck);
            this.resizedImageGroupBox.Controls.Add(this.percentUpDown);
            this.resizedImageGroupBox.Controls.Add(this.label7);
            this.resizedImageGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.resizedImageGroupBox.Location = new System.Drawing.Point(8, 40);
            this.resizedImageGroupBox.Name = "resizedImageGroupBox";
            this.resizedImageGroupBox.Size = new System.Drawing.Size(240, 200);
            this.resizedImageGroupBox.TabIndex = 15;
            this.resizedImageGroupBox.TabStop = false;
            this.resizedImageGroupBox.Text = "New Image Size group box";
            // 
            // absoluteRB
            // 
            this.absoluteRB.Checked = true;
            this.absoluteRB.Location = new System.Drawing.Point(11, 46);
            this.absoluteRB.Name = "absoluteRB";
            this.absoluteRB.Size = new System.Drawing.Size(152, 15);
            this.absoluteRB.TabIndex = 1;
            this.absoluteRB.TabStop = true;
            this.absoluteRB.Text = "By Absolute Size:";
            this.absoluteRB.Click += new System.EventHandler(this.absoluteRB_Click);
            this.absoluteRB.CheckedChanged += new System.EventHandler(this.percentRB_CheckedChanged);
            // 
            // percentRB
            // 
            this.percentRB.Location = new System.Drawing.Point(11, 144);
            this.percentRB.Name = "percentRB";
            this.percentRB.TabIndex = 5;
            this.percentRB.Text = "By Percentage:";
            this.percentRB.Click += new System.EventHandler(this.percentRB_Click);
            this.percentRB.CheckedChanged += new System.EventHandler(this.percentRB_CheckedChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(189, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "pixels";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(189, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 9;
            this.label4.Text = "pixels";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // percentUpDown
            // 
            this.percentUpDown.Location = new System.Drawing.Point(115, 146);
            this.percentUpDown.Maximum = new System.Decimal(new int[] {
                                                                          2000,
                                                                          0,
                                                                          0,
                                                                          0});
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.Size = new System.Drawing.Size(72, 20);
            this.percentUpDown.TabIndex = 6;
            this.percentUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.percentUpDown.Value = new System.Decimal(new int[] {
                                                                        100,
                                                                        0,
                                                                        0,
                                                                        0});
            this.percentUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.percentUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.percentUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.percentUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(189, 147);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 16);
            this.label7.TabIndex = 13;
            this.label7.Text = "%";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // originalImageSize
            // 
            this.originalImageSize.Location = new System.Drawing.Point(112, 8);
            this.originalImageSize.Name = "originalImageSize";
            this.originalImageSize.Size = new System.Drawing.Size(112, 16);
            this.originalImageSize.TabIndex = 17;
            this.originalImageSize.Text = "800 x 600";
            // 
            // asteriskTextLabel
            // 
            this.asteriskTextLabel.Location = new System.Drawing.Point(8, 176);
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            this.asteriskTextLabel.Size = new System.Drawing.Size(224, 16);
            this.asteriskTextLabel.TabIndex = 14;
            this.asteriskTextLabel.Text = "* ...";
            this.asteriskTextLabel.Visible = false;
            // 
            // asteriskLabel
            // 
            this.asteriskLabel.Location = new System.Drawing.Point(213, 21);
            this.asteriskLabel.Name = "asteriskLabel";
            this.asteriskLabel.Size = new System.Drawing.Size(13, 16);
            this.asteriskLabel.TabIndex = 15;
            this.asteriskLabel.Text = "*";
            this.asteriskLabel.Visible = false;
            // 
            // ResizeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(258, 277);
            this.Controls.Add(this.originalImageSize);
            this.Controls.Add(this.currentImageSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.resizedImageGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResizeDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resize";
            this.Controls.SetChildIndex(this.resizedImageGroupBox, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.currentImageSize, 0);
            this.Controls.SetChildIndex(this.originalImageSize, 0);
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).EndInit();
            this.resizedImageGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).EndInit();
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
            this.DialogResult = DialogResult.Cancel;
            this.Close();       
        }

        private void ratioCheck_CheckedChanged(object sender, System.EventArgs e)
        {
            if (ratioCheck.Checked != IsLocked)
            {
                IsLocked = ratioCheck.Checked;
            }
        }

        private void FixWidthToRatio()
        {
            widthUpDown.ValueChanged -= upDownValueChangedDelegate;
            ImageWidth = (int)Math.Round((double)ImageHeight * ratio);
			widthUpDown.ValueChanged += upDownValueChangedDelegate;         
        }

        private void FixHeightToRatio()
        {
            heightUpDown.ValueChanged -= upDownValueChangedDelegate;      
			ImageHeight = (int)Math.Round((double)ImageWidth * (1 / ratio));
			heightUpDown.ValueChanged += upDownValueChangedDelegate;
        }

		private void FixPercent()
		{
			widthUpDown.ValueChanged -= upDownValueChangedDelegate;
			heightUpDown.ValueChanged -= upDownValueChangedDelegate;

			ImageHeight = (int)((double)OriginalSize.Height * ((double)percentUpDown.Value / 100.0));
			ImageWidth = (int)((double)OriginalSize.Width * ((double)percentUpDown.Value / 100.0));
		
			widthUpDown.ValueChanged += upDownValueChangedDelegate;
			heightUpDown.ValueChanged += upDownValueChangedDelegate;
		}

        private void upDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (IsLocked)
            {
				if (sender == heightUpDown)
				{
					FixWidthToRatio();          
				}
				else if (sender == widthUpDown)
				{
					FixHeightToRatio();         
				}
            }

			if (sender == percentUpDown)
			{
				FixPercent();
			}

            if (widthUpDown.Value != 0 && heightUpDown.Value != 0)
            {
                okButton.Enabled = true;
            }
            else
            {
                okButton.Enabled = false;
            }

            PopulateAsteriskLabels();
        }

        private void upDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool numberIsOk = Utility.CheckNumericUpDown((NumericUpDown)sender);
            okButton.Enabled = numberIsOk;

            if (numberIsOk)
            {
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((long)ImageHeight * (long)ImageWidth * (long)ColorBgra.SizeOf * (long)Layers);
                upDown_ValueChanged(sender, e);
            }
        }

        private void upDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void upDown_Leave(object sender, System.EventArgs e)
        {
            //upDown_ValueChanged(sender, e);
        }

        private void percentRB_CheckedChanged(object sender, System.EventArgs e)
        {
            if (percentRB.Checked)
            {
                widthUpDown.Enabled = false;
                heightUpDown.Enabled = false;
                ratioCheck.Enabled = false;
                percentUpDown.Enabled = true;
            }
            else
            {
                widthUpDown.Enabled = true;
                heightUpDown.Enabled = true;
                ratioCheck.Enabled = true;
                percentUpDown.Enabled = false;
            }
        }

        private void absoluteRB_Click(object sender, System.EventArgs e)
        {
            absoluteRB.Checked = true;
            percentRB.Checked = false;
        }

        private void percentRB_Click(object sender, System.EventArgs e)
        {
            absoluteRB.Checked = false;
            percentRB.Checked = true;
            FixPercent();
        }

        private void PopulateAsteriskLabels()
        {
            ResampleMethod rm = (ResampleMethod)this.resamplingAlgorithmComboBox.SelectedItem;

            switch (rm.method)
            {
                default:
                    this.asteriskLabel.Visible = false;
                    this.asteriskTextLabel.Visible = false;
                    break;

                case ResamplingAlgorithm.SuperSampling:
                    if (this.ImageWidth < this.OriginalSize.Width &&
                        this.ImageHeight < this.OriginalSize.Height)
                    {
                        this.asteriskTextLabel.Text = "* Super Sampling will be used";
                    }
                    else
                    {
                        this.asteriskTextLabel.Text = "* Bicubic will be used";
                    }

                    if (this.resamplingAlgorithmComboBox.Visible)
                    {
                        this.asteriskLabel.Visible = true;
                        this.asteriskTextLabel.Visible = true;
                    }

                    break;
            }
        }

        private void resamplingAlgorithmComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            PopulateAsteriskLabels();
        }
    }
}
