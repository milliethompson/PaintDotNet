/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class CanvasSizeDialog 
        : PaintDotNet.ResizeDialog
    {
        private AnchorChooserControl anchorChooserControl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label anchorLabel;
        private System.Windows.Forms.Label label8;
        private System.ComponentModel.IContainer components = null;

        public AnchorEdge AnchorEdge
        {
            get
            {
                return anchorChooserControl.AnchorEdge;
            }

            set
            {
                anchorChooserControl.AnchorEdge = value;
            }
        }

        public CanvasSizeDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            anchorChooserControl_AnchorEdgeChanged(anchorChooserControl, EventArgs.Empty);

            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuImageCanvasSizeIcon.bmp"), Color.FromArgb(192, 192, 192));
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.anchorChooserControl = new PaintDotNet.AnchorChooserControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.anchorLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
            this.resizedImageGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ratioCheck
            // 
            this.ratioCheck.Location = new System.Drawing.Point(38, 92);
            this.ratioCheck.Name = "ratioCheck";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(35, 42);
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(35, 68);
            this.label2.Name = "label2";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(96, 376);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 8;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(176, 376);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 9;
            // 
            // resamplingAlgorithmComboBox
            // 
            this.resamplingAlgorithmComboBox.Enabled = false;
            this.resamplingAlgorithmComboBox.Location = new System.Drawing.Point(440, 20);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.TabStop = false;
            this.resamplingAlgorithmComboBox.Visible = false;
            //
            // asteriskLabel
            //
            this.asteriskLabel.Visible = false;
            //
            // asteriskTextLabel
            //
            this.asteriskTextLabel.Visible = false;
            // 
            // label3
            // 
            this.label3.Enabled = false;
            this.label3.Location = new System.Drawing.Point(368, 22);
            this.label3.Name = "label3";
            this.label3.Visible = false;
            // 
            // widthUpDown
            // 
            this.widthUpDown.Location = new System.Drawing.Point(115, 42);
            this.widthUpDown.Name = "widthUpDown";
            // 
            // heightUpDown
            // 
            this.heightUpDown.Location = new System.Drawing.Point(115, 68);
            this.heightUpDown.Name = "heightUpDown";
            // 
            // label5
            // 
            this.label5.Name = "label5";
            // 
            // currentImageSize
            // 
            this.currentImageSize.Name = "currentImageSize";
            // 
            // resizedImageGroupBox
            // 
            this.resizedImageGroupBox.Name = "resizedImageGroupBox";
            this.resizedImageGroupBox.Size = new System.Drawing.Size(240, 150);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(189, 42);
            this.label4.Name = "label4";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(189, 68);
            this.label6.Name = "label6";
            // 
            // originalImageSize
            // 
            this.originalImageSize.Name = "originalImageSize";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(189, 121);
            this.label7.Name = "label7";
            // 
            // percentUpDown
            // 
            this.percentUpDown.Location = new System.Drawing.Point(115, 120);
            this.percentUpDown.Name = "percentUpDown";
            // 
            // absoluteRB
            // 
            this.absoluteRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.absoluteRB.Location = new System.Drawing.Point(11, 22);
            this.absoluteRB.Name = "absoluteRB";
            // 
            // percentRB
            // 
            this.percentRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.percentRB.Location = new System.Drawing.Point(11, 118);
            this.percentRB.Name = "percentRB";
            // 
            // anchorChooserControl
            // 
            this.anchorChooserControl.AnchorEdge = PaintDotNet.AnchorEdge.Middle;
            this.anchorChooserControl.Location = new System.Drawing.Point(120, 24);
            this.anchorChooserControl.Name = "anchorChooserControl";
            this.anchorChooserControl.Size = new System.Drawing.Size(96, 96);
            this.anchorChooserControl.TabIndex = 7;
            this.anchorChooserControl.TabStop = false;
            this.anchorChooserControl.AnchorEdgeChanged += new System.EventHandler(this.anchorChooserControl_AnchorEdgeChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.anchorLabel);
            this.groupBox1.Controls.Add(this.anchorChooserControl);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 198);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(240, 170);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Anchor";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 128);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(224, 32);
            this.label8.TabIndex = 20;
            this.label8.Text = "The new space will be filled with the currently selected background color.";
            // 
            // anchorLabel
            // 
            this.anchorLabel.Location = new System.Drawing.Point(8, 24);
            this.anchorLabel.Name = "anchorLabel";
            this.anchorLabel.Size = new System.Drawing.Size(88, 96);
            this.anchorLabel.TabIndex = 19;
            this.anchorLabel.Text = "label7";
            // 
            // CanvasSizeDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(258, 408);
            this.Controls.Add(this.groupBox1);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "CanvasSizeDialog";
            this.Text = "Canvas Size";
            this.Controls.SetChildIndex(this.groupBox1, 0);
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
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void anchorChooserControl_AnchorEdgeChanged(object sender, System.EventArgs e)
        {
            anchorLabel.Text = Utility.InsertSpaces(anchorChooserControl.AnchorEdge.ToString());
        }
    }
}

