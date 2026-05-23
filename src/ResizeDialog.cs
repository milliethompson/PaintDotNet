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
        
        private InterpolationMode interpolationMode;
        protected System.Windows.Forms.ComboBox interpolationModeComboBox;
        protected System.Windows.Forms.Label label3;

        private ResampleMethod lowQuality;
        private ResampleMethod highQuality;
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
                widthUpDown.Value = (decimal)value; //(int)(Math.Round((double)value));
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((double)layers * 4.0 * (double)ImageHeight * (double)ImageWidth);
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
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((double)layers * 4.0 * (double)ImageHeight * (double)ImageWidth);
            }
        }

        public double DocumentSize
        {
            set
            {
                currentImageSize.Text = Utility.SizeStringFromBytes(value);
            }
        }

        public InterpolationMode InterpMode
        {
            get
            {
                return interpolationMode;
            }
            
            set
            {
                interpolationMode = value;
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
                double initialSize = (double)layers * 4.0 * (double)ImageHeight * (double)ImageWidth;
                DocumentSize = initialSize;
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes(initialSize);
            }
        }

        #region Resample Method Private Class
        private class ResampleMethod
        {
            public InterpolationMode method;

            public override string ToString()
            {
                switch (method)
                {
                    case InterpolationMode.NearestNeighbor:
                        return "Nearest Neighbor";

                    case InterpolationMode.HighQualityBicubic:
                        return "Bicubic";

                    default:
                        return method.ToString();
                }
            }

            public ResampleMethod(InterpolationMode method)
            {
                this.method = method;
            }
        }
        #endregion

        public ResizeDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            ratioCheck.Checked = true;
            interpolationModeComboBox.Items.Clear();
            ratio = -1;

            upDownValueChangedDelegate = new EventHandler(upDown_ValueChanged);

            highQuality = new ResampleMethod(InterpolationMode.HighQualityBicubic);
            lowQuality = new ResampleMethod(InterpolationMode.NearestNeighbor);

            interpolationModeComboBox.Items.Add(highQuality);
            interpolationModeComboBox.Items.Add(lowQuality);

            interpolationModeComboBox.SelectedItem = highQuality;
            layers = 1;

            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuImageResizeIcon.bmp"), Color.FromArgb(192, 192, 192));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            this.widthUpDown.Select();
            this.widthUpDown.Select(0, widthUpDown.Text.Length);
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
            this.interpolationModeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.widthUpDown = new System.Windows.Forms.NumericUpDown();
            this.heightUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.currentImageSize = new System.Windows.Forms.Label();
            this.resizedImageGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.originalImageSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
            this.resizedImageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ratioCheck
            // 
            this.ratioCheck.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ratioCheck.Location = new System.Drawing.Point(11, 102);
            this.ratioCheck.Name = "ratioCheck";
            this.ratioCheck.Size = new System.Drawing.Size(136, 16);
            this.ratioCheck.TabIndex = 3;
            this.ratioCheck.Text = "Maintain Aspect Ratio";
            this.ratioCheck.CheckedChanged += new System.EventHandler(this.ratioCheck_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "New Width:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 76);
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
            this.okButton.Location = new System.Drawing.Point(80, 184);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(72, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(160, 184);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // interpolationModeComboBox
            // 
            this.interpolationModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.interpolationModeComboBox.Location = new System.Drawing.Point(78, 20);
            this.interpolationModeComboBox.Name = "interpolationModeComboBox";
            this.interpolationModeComboBox.Size = new System.Drawing.Size(130, 21);
            this.interpolationModeComboBox.TabIndex = 0;
            this.interpolationModeComboBox.Tag = "";
            this.interpolationModeComboBox.SelectedIndexChanged += new System.EventHandler(this.interpolationModeComboBox_SelectedIndexChanged);
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
            this.widthUpDown.Location = new System.Drawing.Point(78, 49);
            this.widthUpDown.Maximum = new System.Decimal(new int[] {
                                                                        65535,
                                                                        0,
                                                                        0,
                                                                        0});
            this.widthUpDown.Name = "widthUpDown";
            this.widthUpDown.Size = new System.Drawing.Size(72, 20);
            this.widthUpDown.TabIndex = 1;
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
            this.heightUpDown.Location = new System.Drawing.Point(78, 76);
            this.heightUpDown.Maximum = new System.Decimal(new int[] {
                                                                         65535,
                                                                         0,
                                                                         0,
                                                                         0});
            this.heightUpDown.Name = "heightUpDown";
            this.heightUpDown.Size = new System.Drawing.Size(72, 20);
            this.heightUpDown.TabIndex = 2;
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
            this.resizedImageGroupBox.Controls.Add(this.widthUpDown);
            this.resizedImageGroupBox.Controls.Add(this.heightUpDown);
            this.resizedImageGroupBox.Controls.Add(this.label6);
            this.resizedImageGroupBox.Controls.Add(this.label4);
            this.resizedImageGroupBox.Controls.Add(this.label2);
            this.resizedImageGroupBox.Controls.Add(this.label1);
            this.resizedImageGroupBox.Controls.Add(this.interpolationModeComboBox);
            this.resizedImageGroupBox.Controls.Add(this.label3);
            this.resizedImageGroupBox.Controls.Add(this.ratioCheck);
            this.resizedImageGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.resizedImageGroupBox.Location = new System.Drawing.Point(8, 40);
            this.resizedImageGroupBox.Name = "resizedImageGroupBox";
            this.resizedImageGroupBox.Size = new System.Drawing.Size(220, 128);
            this.resizedImageGroupBox.TabIndex = 15;
            this.resizedImageGroupBox.TabStop = false;
            this.resizedImageGroupBox.Text = "New Image Size group box";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(152, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "pixels";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(152, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 9;
            this.label4.Text = "pixels";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // originalImageSize
            // 
            this.originalImageSize.Location = new System.Drawing.Point(112, 8);
            this.originalImageSize.Name = "originalImageSize";
            this.originalImageSize.Size = new System.Drawing.Size(112, 16);
            this.originalImageSize.TabIndex = 17;
            this.originalImageSize.Text = "800 x 600";
            // 
            // ResizeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(238, 213);
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

        private void interpolationModeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ResampleMethod rm = (ResampleMethod)((ComboBox)sender).SelectedItem;
            InterpMode = rm.method;
        }

        private void FixWidthToRatio()
        {
            widthUpDown.ValueChanged -= upDownValueChangedDelegate;
            ImageWidth = (int)(Math.Round(ImageHeight* ratio));
            widthUpDown.ValueChanged += upDownValueChangedDelegate;         
        }

        private void FixHeightToRatio()
        {
            heightUpDown.ValueChanged -= upDownValueChangedDelegate;
            ImageHeight = (int)(Math.Round(ImageWidth * (1/ratio)));
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

            if (widthUpDown.Value != 0 && heightUpDown.Value != 0)
            {
                okButton.Enabled = true;
            }
            else
            {
                okButton.Enabled = false;
            }
        }

        private void upDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool numberIsOk = Utility.CheckNumericUpDown((NumericUpDown)sender);
            okButton.Enabled = numberIsOk;

            if (numberIsOk)
            {
                resizedImageGroupBox.Text = "New Size: " + Utility.SizeStringFromBytes((double)ImageHeight * (double)ImageWidth * 4.0 * (double)Layers);
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
    }
}
