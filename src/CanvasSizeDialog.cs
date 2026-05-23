using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
	public class CanvasSizeDialog : PaintDotNet.ResizeDialog
	{
        private PaintDotNet.AnchorChooserControl anchorChooserControl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label anchorLabel;
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

			// TODO: Add any initialization after the InitializeComponent call
            anchorChooserControl_AnchorEdgeChanged(anchorChooserControl, EventArgs.Empty);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
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
            this.anchorChooserControl = new PaintDotNet.AnchorChooserControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.anchorLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ratioCheck
            // 
            this.ratioCheck.Location = new System.Drawing.Point(11, 72);
            this.ratioCheck.Name = "ratioCheck";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(68, 294);
            this.okButton.Name = "okButton";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(148, 294);
            this.cancelButton.Name = "cancelButton";
            // 
            // interpolationModeComboBox
            // 
            this.interpolationModeComboBox.Location = new System.Drawing.Point(216, 104);
            this.interpolationModeComboBox.Name = "interpolationModeComboBox";
            this.interpolationModeComboBox.Visible = false;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(176, 104);
            this.label3.Name = "label3";
            this.label3.Visible = false;
            // 
            // widthUpDown
            // 
            this.widthUpDown.Location = new System.Drawing.Point(78, 22);
            this.widthUpDown.Name = "widthUpDown";
            // 
            // heightUpDown
            // 
            this.heightUpDown.Location = new System.Drawing.Point(78, 48);
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
            this.resizedImageGroupBox.Size = new System.Drawing.Size(208, 96);
            // 
            // label4
            // 
            this.label4.Name = "label4";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(152, 22);
            this.label6.Name = "label6";
            // 
            // originalImageSize
            // 
            this.originalImageSize.Name = "originalImageSize";
            // 
            // anchorChooserControl
            // 
            this.anchorChooserControl.AnchorEdge = PaintDotNet.AnchorEdge.Middle;
            this.anchorChooserControl.Location = new System.Drawing.Point(96, 24);
            this.anchorChooserControl.Name = "anchorChooserControl";
            this.anchorChooserControl.Size = new System.Drawing.Size(96, 96);
            this.anchorChooserControl.TabIndex = 18;
            this.anchorChooserControl.AnchorEdgeChanged += new System.EventHandler(this.anchorChooserControl_AnchorEdgeChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.anchorLabel);
            this.groupBox1.Controls.Add(this.anchorChooserControl);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 144);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 136);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Anchor";
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
            this.ClientSize = new System.Drawing.Size(226, 325);
            this.Controls.Add(this.groupBox1);
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

